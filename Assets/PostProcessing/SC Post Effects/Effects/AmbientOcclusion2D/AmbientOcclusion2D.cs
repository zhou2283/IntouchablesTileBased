using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public class AmbientOcclusion2D : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(AmbientOcclusion2DRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Rendering/Ambient Occlusion 2D", true)]
    public sealed class AmbientOcclusion2D : PostProcessEffectSettings
    {
        public BoolParameter aoOnly = new BoolParameter { value = false };

        [Header("LBAO")]
        [Range(0f, 1f), Tooltip("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 0.5f };

        [Range(0.01f, 1f), Tooltip("Luminance threshold")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 0.05f };

        [Range(0f, 3f), Tooltip("Distance")]
        public FloatParameter distance = new FloatParameter { value = 1f };

        [Header("Blur")]
        [Range(0f, 3f), Tooltip("Blur")]
        public FloatParameter blur = new FloatParameter { value = 1f };

        [Range(1, 8), Tooltip("Itterations")]
        public IntParameter itterations = new IntParameter { value = 2 };

        [Range(1, 8), Tooltip("Downsampling")]
        public IntParameter downsamples = new IntParameter { value = 1 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class AmbientOcclusion2DRenderer : PostProcessEffectRenderer<AmbientOcclusion2D>
    {
        Shader shader;
        private int aoTexID;
        private int screenCopyID;
        RenderTexture aoRT;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Ambient Occlusion 2D");
            aoTexID = Shader.PropertyToID("_AO");
        }

        public override void Release()
        {
            base.Release();
        }

        enum Pass
        {
            LuminanceDiff,
            Blur,
            Blend,
            Debug
        }


        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            sheet.properties.SetFloat("_SampleDistance", settings.distance);
            sheet.properties.SetFloat("_Threshold", settings.luminanceThreshold);
            sheet.properties.SetFloat("_Blur", settings.blur);
            sheet.properties.SetFloat("_Intensity", settings.intensity);

            // Create RT for storing edge detection in
            context.command.GetTemporaryRT(aoTexID, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat);

            //Luminance difference check on RT
            context.command.BlitFullscreenTriangle(context.source, aoTexID, sheet, (int)Pass.LuminanceDiff);

            // get two smaller RTs
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.screenWidth / settings.downsamples, context.screenHeight / settings.downsamples, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.screenWidth / settings.downsamples, context.screenHeight / settings.downsamples, 0, FilterMode.Bilinear);

            //Pass AO into blur target texture
            cmd.Blit(aoTexID, blurredID);

            for (int i = 0; i < settings.itterations; i++)
            {
                // horizontal blur
                cmd.SetGlobalVector("_BlurOffsets", new Vector4((settings.blur) / context.screenWidth, 0, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.Blur);

                // vertical blur
                cmd.SetGlobalVector("_BlurOffsets", new Vector4(0, (settings.blur) / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.Blur);
            }

            context.command.SetGlobalTexture("_AO", blurredID);

            //Blend AO tex with image
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (settings.aoOnly) ? (int)Pass.Debug : (int)Pass.Blend);

            // release
            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(blurredID2);
            context.command.ReleaseTemporaryRT(aoTexID);
        }
    }
}
#endif