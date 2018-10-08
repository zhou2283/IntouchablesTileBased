using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
using SCPE;

[PostProcessEditor(typeof(SCPE.AmbientOcclusion2D))]
public class AmbientOcclusion2DEditor : PostProcessEffectEditor<SCPE.AmbientOcclusion2D>
{
    SerializedParameterOverride aoOnly;
    SerializedParameterOverride intensity;
    SerializedParameterOverride luminanceThreshold;
    SerializedParameterOverride distance;
    SerializedParameterOverride blur;
    SerializedParameterOverride itterations;
    SerializedParameterOverride downsamples;

    private bool usingSinglePassRendering;


    public override void OnEnable()
    {
        usingSinglePassRendering = SCPE.SCPE.IsSinglePassVR;

        aoOnly = FindParameterOverride(x => x.aoOnly);
        intensity = FindParameterOverride(x => x.intensity);
        luminanceThreshold = FindParameterOverride(x => x.luminanceThreshold);
        distance = FindParameterOverride(x => x.distance);
        blur = FindParameterOverride(x => x.blur);
        itterations = FindParameterOverride(x => x.itterations);
        downsamples = FindParameterOverride(x => x.downsamples);
    }

    public override void OnInspectorGUI()
    {
        if (usingSinglePassRendering)
        {
            EditorGUILayout.HelpBox("Ambient Occlusion 2D is not supported in Single-Pass Stereo Rendering", MessageType.Warning);
            return;
        }

        PropertyField(aoOnly);
        PropertyField(intensity);
        PropertyField(luminanceThreshold);
        PropertyField(distance);
        PropertyField(intensity);
        PropertyField(blur);
        PropertyField(itterations);
        PropertyField(downsamples);

    }
}
#endif
