using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class KuwuharaEditor : Editor {} }
#else
    [PostProcessEditor(typeof(Kuwahara))]
    public class KuwuharaEditor : PostProcessEffectEditor<Kuwahara>
    {
        SerializedParameterOverride mode;
        SerializedParameterOverride radius;
        SerializedParameterOverride fadeDist;
        SerializedParameterOverride fadeFalloff;

        private bool isOrthographic = false;

        public override void OnEnable()
        {
            mode = FindParameterOverride(x => x.mode);
            radius = FindParameterOverride(x => x.radius);
            fadeDist = FindParameterOverride(x => x.fadeDist);
            fadeFalloff = FindParameterOverride(x => x.fadeFalloff);

            if (Camera.main) isOrthographic = Camera.main.orthographic;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(isOrthographic);
            PropertyField(mode);
            EditorGUI.EndDisabledGroup();

            if (isOrthographic)
            {
                mode.value.intValue = 0;
                EditorGUILayout.HelpBox("Depth fade is disabled for orthographic cameras", MessageType.Info);
            }
            PropertyField(radius);
            if (!isOrthographic)
            {
                PropertyField(fadeDist);
                PropertyField(fadeFalloff);
            }
        }
    }
}
#endif
