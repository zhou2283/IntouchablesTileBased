using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Fog : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(FogRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Environment/Fog")]
    public sealed class Fog : PostProcessEffectSettings
    {
        [Tooltip("Use the settings of the current active scene found under the Lighting tab\n\nThis is also advisable for third-party scripts that modify fog settings")]
        public BoolParameter useSceneSettings = new BoolParameter { value = false };
        [Serializable]
        public sealed class FogModeParameter : ParameterOverride<FogMode> { }
        [DisplayName("Mode")]
        public FogModeParameter fogMode = new FogModeParameter { value = FogMode.Exponential };

        [Range(0f,1f)]
        public FloatParameter globalDensity = new FloatParameter { value = 0.2f };
        [DisplayName("Start")]
        public FloatParameter fogStartDistance = new FloatParameter { value = 170f };
        [DisplayName("End")]
        public FloatParameter fogEndDistance = new FloatParameter { value = 600f };

        public enum FogColorSource
        {
            Color,
            Gradient
        }

        [Serializable]
        public sealed class FogColorSourceParameter : ParameterOverride<FogColorSource> { }
        [Space]
        public FogColorSourceParameter colorMode = new FogColorSourceParameter { value = FogColorSource.Color };

        [DisplayName("Color")]
        public ColorParameter fogColor = new ColorParameter { value = new Color(0.76f, 0.94f, 1f, 1f) };
        [DisplayName("Texture")]
        public TextureParameter fogColorGradient = new TextureParameter { value = null };
        [Tooltip("Automatic mode uses the current camera's far clipping plane to set the max distance\n\nOtherwise, a fixed value may be used instead")]
        public FloatParameter gradientDistance = new FloatParameter { value = 1000f };
        public BoolParameter gradientUseFarClipPlane = new BoolParameter { value = true };


        [Header("Distance")]
        [DisplayName("Enable")]
        public BoolParameter distanceFog = new BoolParameter { value = true };
        [Range(0.001f, 1.0f)]
        [DisplayName("Density")]
        public FloatParameter distanceDensity = new FloatParameter { value = 1f };
        [Tooltip("Distance based on radial distance from viewer, rather than parrallel")]
        public BoolParameter useRadialDistance = new BoolParameter { value = true };

        [Header("Skybox")]
        [Tooltip("Disables fog rendering on the skybox")]
        public BoolParameter excludeSkybox = new BoolParameter { value = false };


        [Header("Height")]
        [DisplayName("Enable")]
        public BoolParameter heightFog = new BoolParameter { value = true };

        [Tooltip("Height relative to 0 world height position")]
        public FloatParameter height = new FloatParameter { value = 10f };

        [Range(0.001f, 1.0f)]
        [DisplayName("Density")]
        public FloatParameter heightDensity = new FloatParameter { value = 0.75f };

        [Header("Height noise (2D)")]
        [DisplayName("Enable")]
        public BoolParameter heightFogNoise = new BoolParameter { value = false };
        [DisplayName("Texture (R)")]
        public TextureParameter heightNoiseTex = new TextureParameter { value = null };
        [Range(0f, 1f)]
        [DisplayName("Size")]
        public FloatParameter heightNoiseSize = new FloatParameter { value = 0.25f };
        [Range(0f, 1f)]
        [DisplayName("Strength")]
        public FloatParameter heightNoiseStrength = new FloatParameter { value = 1f };
        [Range(0f, 10f)]
        [DisplayName("Speed")]
        public FloatParameter heightNoiseSpeed = new FloatParameter { value = 2f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }

    }

    internal sealed class FogRenderer : PostProcessEffectRenderer<Fog>
    {

        Shader shader;
        //int skyboxTexID = Shader.PropertyToID("_SkyboxTex");
        //int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        //int blurredID = Shader.PropertyToID("_Temp1");
        //int blurredID2 = Shader.PropertyToID("_Temp2");
        CommandBuffer cmd2;

        enum Pass
        {
            Blend
        }

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Fog");

        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            Camera cam = context.camera;

            /*
            CommandBuffer cmd2 = new CommandBuffer();
            cmd2.name = "[SCPE] Render skybox to texture";
            cmd2.GetTemporaryRT(skyboxTexID, -1, -1, 0, FilterMode.Bilinear);
            cam.AddCommandBuffer(CameraEvent.AfterSkybox, cmd2);
            cmd2.Blit(BuiltinRenderTextureType.CurrentActive, skyboxTexID);

            cmd2.SetGlobalTexture("_SkyboxTex", skyboxTexID);
            */

            if (settings.heightNoiseTex.value) sheet.properties.SetTexture("_NoiseTex", settings.heightNoiseTex);
            if (settings.fogColorGradient.value) sheet.properties.SetTexture("_ColorGradient", settings.fogColorGradient);

			//OpenVR.System.GetProjectionMatrix(vrEye, mainCamera.nearClipPlane, mainCamera.farClipPlane, EGraphicsAPIConvention.API_DirectX)
			
            //Clip-space to world-space camera matrix conversion
            var p = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            var clipToWorld = Matrix4x4.Inverse(p * cam.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
            sheet.properties.SetMatrix("clipToWorld", clipToWorld);

            float FdotC = cam.transform.position.y - settings.height;
            float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
            float excludeSkybox = (settings.excludeSkybox ? 1.0f : 2.0f);
            float distanceFog = (settings.distanceFog) ? 1.0f : 0.0f;
            float heightFog = (settings.heightFog) ? 1.0f : 0.0f;

            int colorSource = (settings.useSceneSettings) ? 0 : (int)settings.colorMode.value;
            var sceneMode = (settings.useSceneSettings) ? RenderSettings.fogMode : settings.fogMode;
            bool linear = (sceneMode == FogMode.Linear);
            var sceneDensity = (settings.useSceneSettings) ? RenderSettings.fogDensity : settings.globalDensity /100;
            var sceneStart = (settings.useSceneSettings) ? RenderSettings.fogStartDistance : settings.fogStartDistance;
            var sceneEnd = (settings.useSceneSettings) ? RenderSettings.fogEndDistance : settings.fogEndDistance;
            Vector4 sceneParams;

            float diff = linear ? sceneEnd - sceneStart : 0.0f;
            float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
            sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
            sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
            sceneParams.z = linear ? -invDiff : 0.0f;
            sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;

            float gradientDistance = (settings.gradientUseFarClipPlane.value) ? settings.gradientDistance : context.camera.farClipPlane;

            sheet.properties.SetFloat("_FarClippingPlane", gradientDistance);
            sheet.properties.SetVector("_SceneFogParams", sceneParams);
            sheet.properties.SetVector("_SceneFogMode", new Vector4((int)sceneMode, settings.useRadialDistance ? 1 : 0, colorSource, settings.heightFogNoise ? 1: 0));
            sheet.properties.SetVector("_NoiseParams", new Vector4(settings.heightNoiseSize * 0.01f, settings.heightNoiseSpeed * 0.01f, settings.heightNoiseStrength, 0));
            sheet.properties.SetVector("_DensityParams", new Vector4(settings.distanceDensity, settings.heightNoiseStrength, 0, 0));
            sheet.properties.SetVector("_HeightParams", new Vector4(settings.height, FdotC, paramK, settings.heightDensity * 0.5f));
            sheet.properties.SetVector("_DistanceParams", new Vector4(-sceneStart, excludeSkybox, distanceFog, heightFog));
            sheet.properties.SetColor("_FogColor", (settings.useSceneSettings) ? RenderSettings.fogColor : settings.fogColor);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)Pass.Blend);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }


    }
}
#endif