using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Pixelize : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(PixelizeRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Retro/Pixelize", true)]
    public sealed class Pixelize : PostProcessEffectSettings
    {

        [Range(1, 2048), Tooltip("Resolution")]
        public IntParameter resolution = new IntParameter { value = 270 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (resolution == 2048) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class PixelizeRenderer : PostProcessEffectRenderer<Pixelize>
    {
        Shader pixelizeShader;

        public override void Init()
        {
            pixelizeShader = Shader.Find("Hidden/SC Post Effects/Pixelize");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(pixelizeShader);

            sheet.properties.SetFloat("_Resolution", 1f / settings.resolution);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif