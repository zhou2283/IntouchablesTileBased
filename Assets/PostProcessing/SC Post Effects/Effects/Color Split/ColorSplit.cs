using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class ColorSplit : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(ColorSplitRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Retro/Color Split", true)]
    public sealed class ColorSplit : PostProcessEffectSettings
    {
        public enum SplitMode
        {
            Single = 0,
            SingleBoxFiltered = 1,
            Double = 2,
            DoubleBoxFiltered = 3
        }

        [Serializable]
        public sealed class SplitModeParam : ParameterOverride<SplitMode> { }

        [DisplayName("Method"), Tooltip("")]
        public SplitModeParam mode = new SplitModeParam { value = SplitMode.Single };

        [Range(0f, 0.1f), Tooltip("Offset")]
        public FloatParameter offset = new FloatParameter { value = 0.01f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (offset == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class ColorSplitRenderer : PostProcessEffectRenderer<ColorSplit>
    {
        Shader ColorsplitShader;

        public override void Init()
        {
            ColorsplitShader = Shader.Find("Hidden/SC Post Effects/Color Split");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(ColorsplitShader);

            sheet.properties.SetFloat("_Offset", settings.offset);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

    }
}
#endif
