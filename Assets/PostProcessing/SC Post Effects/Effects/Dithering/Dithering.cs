using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Dithering : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(DitheringRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Retro/Dithering", true)]
    public sealed class Dithering : PostProcessEffectSettings
    {

        [Range(0.1f, 0.5f), Tooltip("Size")]
        public FloatParameter size = new FloatParameter { value = 0.5f };

        [Range(0f, 1f), Tooltip("Luminance threshold")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 1f };

        [Range(0f, 1f), Tooltip("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0) return false;
                return true;
            }

            return false;
        }

    }

    internal sealed class DitheringRenderer : PostProcessEffectRenderer<Dithering>
    {

        Shader ditheringShader;

        public override void Init()
        {

            ditheringShader = Shader.Find("Hidden/SC Post Effects/Dithering");

        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            //context.camera.depthTextureMode = DepthTextureMode.DepthNormals;
            var sheet = context.propertySheets.Get(ditheringShader);

            Vector4 ditherParams = new Vector4(settings.size, settings.size, settings.luminanceThreshold, settings.intensity);
            sheet.properties.SetVector("_Dithering_Coords", ditherParams);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

    }
}
#endif