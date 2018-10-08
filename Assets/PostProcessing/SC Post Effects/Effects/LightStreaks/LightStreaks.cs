using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class LightStreaks : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(LightStreaksRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Rendering/Light Streaks", true)]
    public sealed class LightStreaks : PostProcessEffectSettings
    {
        public enum Quality
        {
            Performance,
            Appearance
        }

        [Serializable]
        public sealed class BlurMethodParameter : ParameterOverride<Quality> { }

        [DisplayName("Quality"), Tooltip("")]
        public BlurMethodParameter quality = new BlurMethodParameter { value = Quality.Performance };

        public BoolParameter debug = new BoolParameter { value = false };

        [Header("Anamorphic Lensfares")]
        [Range(0f, 1f), Tooltip("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 0.5f };

        [Range(0.01f, 3f), Tooltip("Luminance threshold")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 1f };

        [Range(-1f, 1f), Tooltip("Direction")]
        public FloatParameter direction = new FloatParameter { value = -1f };

        [Header("Blur")]
        [Range(0f, 10f), Tooltip("Blur")]
        public FloatParameter blur = new FloatParameter { value = 1f };

        [Range(1, 8), Tooltip("Iterations")]
        public IntParameter iterations = new IntParameter { value = 2 };

        [Range(1, 8), Tooltip("Downsampling")]
        public IntParameter downsamples = new IntParameter { value = 1 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (blur == 0 || intensity == 0 || direction == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    public sealed class LightStreaksRenderer : PostProcessEffectRenderer<LightStreaks>
    {
        Shader shader;
        private int emissionTex;
        RenderTexture aoRT;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Light Streaks");
            emissionTex = Shader.PropertyToID("_BloomTex");
        }

        public override void Release()
        {
            base.Release();
        }

        enum Pass
        {
            LuminanceDiff,
            BlurFast,
            Blur,
            Blend,
            Debug
        }


        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            int blurMode = (settings.quality.value == LightStreaks.Quality.Performance) ? (int)Pass.BlurFast : (int)Pass.Blur;

            //float luminanceThreshold = (context.isSceneView) ? settings.luminanceThreshold / 20f : settings.luminanceThreshold;
            float luminanceThreshold = Mathf.GammaToLinearSpace(settings.luminanceThreshold.value);

            sheet.properties.SetFloat("_Threshold", luminanceThreshold);
            sheet.properties.SetFloat("_Blur", settings.blur);
            sheet.properties.SetFloat("_Intensity", settings.intensity);

            // Create RT for storing edge detection in
            context.command.GetTemporaryRT(emissionTex, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat);

            //Luminance difference check on RT
            context.command.BlitFullscreenTriangle(context.source, emissionTex, sheet, (int)Pass.LuminanceDiff);

            int downSamples = settings.downsamples + 1;
            // get two smaller RTs
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.width/ downSamples, context.height/ downSamples, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.width/ downSamples, context.height/ downSamples, 0, FilterMode.Bilinear);

            //Pass into blur target texture
            cmd.Blit(emissionTex, blurredID);

            float ratio = Mathf.Clamp(settings.direction, -1, 1);
            float rw = ratio < 0 ? -ratio : 0f;
            float rh = ratio > 0 ? ratio : 0f;

            for (int i = 0; i < settings.iterations; i++)
            {
                // vertical blur 1
                cmd.SetGlobalVector("_BlurOffsets", new Vector4(rw * settings.blur / context.screenWidth, rh / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, blurMode);

                // vertical blur 2
                cmd.SetGlobalVector("_BlurOffsets", new Vector4((rw * (settings.blur * 2f)) / context.screenWidth, rh * 2f / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, blurMode);
            }

            context.command.SetGlobalTexture("_BloomTex", blurredID);

            //Blend AO tex with image
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (settings.debug) ? (int)Pass.Debug : (int)Pass.Blend);

            // release
            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(blurredID2);
            context.command.ReleaseTemporaryRT(emissionTex);
        }
    }
}
#endif