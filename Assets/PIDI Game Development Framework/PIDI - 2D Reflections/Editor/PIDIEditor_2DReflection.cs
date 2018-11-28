using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System; 

[CustomEditor(typeof(PIDI_2DReflection))]
public class PIDIEditor_2DReflection : Editor {

	public AnimBool[] animFolds;
	public GUISkin pidiSkin;

	private void OnEnable() {
		animFolds = new AnimBool[4];
		for ( int i = 0; i < animFolds.Length; i++ ){
			animFolds[i] = new AnimBool();
			animFolds[i].valueChanged.AddListener(Repaint);
		}

		var mTarget = (PIDI_2DReflection)target;

		if ( mTarget.GetComponent<Renderer>() && mTarget.GetComponent<Renderer>().sharedMaterial && mTarget.GetComponent<Renderer>().sharedMaterial.HasProperty("_ReflectionTex") ){
			mTarget.surfaceLevel = mTarget.GetComponent<Renderer>().sharedMaterial.GetFloat("_SurfaceLevel");
		}

	}


	public override void OnInspectorGUI(){

        

		var mTarget = (PIDI_2DReflection)target;
		Undo.RecordObject(mTarget,"2DReflection"+mTarget.GetInstanceID());
        SceneView.RepaintAll();
        var tSkin = GUI.skin;
		GUI.skin = pidiSkin;

		GUILayout.BeginHorizontal();GUILayout.BeginVertical(pidiSkin.box);
        GUILayout.Space(8);

        #if UNITY_2018_1_OR_NEWER

        if ( mTarget.srpMode ){
            GUILayout.BeginHorizontal();GUILayout.Space(8);
            EditorGUILayout.HelpBox("SRPs are a new rendering system introduced by Unity in newer versions. It is still in development and support for this pipeline is still in Beta, with not all features supported. Make sure you import all the required libraries to unity before enabling SRP support", MessageType.Info);
            GUILayout.Space(8);GUILayout.EndHorizontal();

            GUILayout.Space(4);
        }
        PDEditor_Toggle( new GUIContent( "Lightweight SRP Mode", "Enable support for Lightweight SRP in Unity 2018.1 and above" ), ref mTarget.srpMode );
		
        GUILayout.Space(8);

        #endif

		if ( PDEditor_BeginFold( "Reflection Settings", ref animFolds[0] ) ){
            PDEditor_Toggle( new GUIContent("BETA - Out of Screen Reflections", "Designed for side-scrolling reflections, an improved algorithm that reflects objects partially or totally out of the screen bounds to produce better quality reflections"), ref mTarget.improvedReflection );

            PDEditor_Toggle( new GUIContent("Local Reflection Mode", "Enable the horizotal local reflction mode, used to track a specific transform object with an object specific reflection"), ref mTarget.isLocalReflection );

            GUILayout.Space(4);
            PDEditor_Color( new GUIContent("Reflection Color","The color that will tint the reflection on this object"), ref mTarget.refColor );
			
            if ( mTarget.GetComponent<Renderer>() && mTarget.GetComponent<Renderer>().sharedMaterial && mTarget.GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection") ){
            PDEditor_Color( new GUIContent("Back Reflection Color", "The color of the reflected parallax background"), ref mTarget.backgroundColor );
            }            
            PDEditor_Slider( new GUIContent("Surface Level", "Adjust the offset of the reflection to make it match the desired point in the surface"), ref mTarget.surfaceLevel, -5, 5 );
			
            GUILayout.Space(6);
		}
		PDEditor_EndFold();

		if ( PDEditor_BeginFold( "Rendering Settings", ref animFolds[1] ) ){

			PDEditor_Toggle( new GUIContent( "Erase Background", "In selected shaders, it removes the background around the reflected objects. Useful for local reflections" ), ref mTarget.alphaBackground );
			PDEditor_IntSlider( new GUIContent( "Reflection Downscaling","Factor in which the reflection's resolution will be downscaled to improve the performance" ), ref mTarget.downScaleValue, 1, 5 );
			PDEditor_LayerMaskField( new GUIContent("Reflect Layers","The layers that this object will reflect"), ref mTarget.renderLayers );
			
            if ( mTarget.srpMode && mTarget.advancedParallax ){
                GUILayout.Space(8);
                 GUILayout.BeginHorizontal();GUILayout.Space(8);
                EditorGUILayout.HelpBox("SRP rendering pipelines might not be fully compatible with the new advanced parallax method. These features are still in development for SRP rendering paths", MessageType.Info);
                GUILayout.Space(8);GUILayout.EndHorizontal();
                GUILayout.Space(8);
            }

            PDEditor_Toggle( new GUIContent("Advanced Parallax Support","Enables the new advanced parallax support method"), ref mTarget.advancedParallax );
        
            if ( !mTarget.advancedParallax && mTarget.GetComponent<Renderer>() && mTarget.GetComponent<Renderer>().sharedMaterial && mTarget.GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection") ){
				GUILayout.Space(4);
				GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox("You are using a parallax composition shader. Make sure to define both a foreground and background cameras as well as their rendering layers", MessageType.Info );
				GUILayout.Space(8);GUILayout.EndHorizontal();
				GUILayout.Space(8);
				PDEditor_ObjectField<Camera>( new GUIContent("Foreground Camera","The camera that will render the foreground in a parallax composition setup"), ref mTarget.foregroundCam, true );
				PDEditor_ObjectField<Camera>( new GUIContent("Background Camera","The camera that will render the background in a parallax composition setup"), ref mTarget.backgroundCam, true );
                PDEditor_LayerMaskField( new GUIContent("Background Layers","Only the background layers you define will be reflected"), ref mTarget.drawOverLayers );
                GUILayout.Space(6);
            }

            if ( mTarget.advancedParallax ){
                GUILayout.Space(8);
                var tempCameras = mTarget.cameras;
                int size = mTarget.cameras.Length;
                PDEditor_IntSlider( new GUIContent("Cameras (Back to Front)","The amount of cameras that will be mixed into a single reflection"), ref size, 2, 24 );
                mTarget.cameras = new Camera[size];
                for ( int i = 0; i < Mathf.Min(tempCameras.Length,size); i++ ){
                    mTarget.cameras[i] = tempCameras[i];
                    PDEditor_ObjectField( new GUIContent("Camera "+i), ref mTarget.cameras[i], true );
                }
                GUILayout.Space(8);
            }

			if ( mTarget.GetComponent<Renderer>() && mTarget.GetComponent<Renderer>().sharedMaterial && mTarget.GetComponent<Renderer>().sharedMaterial.HasProperty("_ReflectionMask") ){
				GUILayout.Space(4);
				GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox("You are using a masked reflection shader. Make sure to set which layers should this reflection draw over", MessageType.Info );
				GUILayout.Space(8);GUILayout.EndHorizontal();
				GUILayout.Space(8);
                PDEditor_LayerMaskField( new GUIContent("Draw Over Layers","The reflection will be drawn only over the layers you select"), ref mTarget.drawOverLayers );
			}
			GUILayout.Space(6);

			
		}
		PDEditor_EndFold();
        if ( mTarget.isLocalReflection ){
            if ( PDEditor_BeginFold( "Local Reflection Settings", ref animFolds[2] ) ){
                
                if ( mTarget.isLocalReflection ){
                    GUILayout.Space(4);
                    PDEditor_ObjectField( new GUIContent("Target Object","The object that this local reflection will track. Remember to make this reflection a child object of the target object and to place it exactly where you want it once it starts following your target object"), ref mTarget.reflectAsLocal, true );
                    GUILayout.Space(4);
                    PDEditor_FloatField(  new GUIContent("Ground level line","The local reflection will not be able to move above this horizontal line"), ref mTarget.waterSurfaceLine, false );
                    PDEditor_FloatField( new GUIContent("Left Side Limit","The left side bounds that limit the movement of this local reflection"), ref mTarget.horizontalLimits.x, false );
                    PDEditor_FloatField( new GUIContent("Right Side Limit","The left side bounds that limit the movement of this local reflection"), ref mTarget.horizontalLimits.y, false );
                    GUILayout.Space(6);				
                }
                GUILayout.Space(4);
            } 
            PDEditor_EndFold();
        }
		
        GUILayout.Space(2);

        var tempStyle = new GUIStyle();
        tempStyle.normal.textColor = new Color(0.75f,0.75f,0.75f);
        tempStyle.fontSize = 9;
        tempStyle.fontStyle = FontStyle.Italic;
        GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
        GUILayout.Label("PIDI - 2D Reflections©. Version 1.4", tempStyle );
        GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

		GUILayout.Space(8);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUI.skin = tSkin;
	}

	
	#region GENERIC PIDI EDITOR FUNCTIONS

	public bool PDEditor_BeginFold( string label, ref AnimBool fold ){
            if ( GUILayout.Button(label, pidiSkin.button ) ){
                fold.target = !fold.target;
            }

            var b = EditorGUILayout.BeginFadeGroup( fold.faded );
            if ( b ){ 
                GUILayout.Space(8);}
            return b;
        }


        public void PDEditor_EndFold( ){
            EditorGUILayout.EndFadeGroup();
        }

	public void PDEditor_Toggle( GUIContent label, ref bool value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle( value, "", GUILayout.Width(16) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_TextField( GUIContent label, ref string value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField( value, GUILayout.MinWidth(64), GUILayout.MaxWidth(180) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public Enum PDEditor_EnumPopup( GUIContent label, Enum value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            var x = EditorGUILayout.EnumPopup( value, pidiSkin.button, GUILayout.MinWidth(64), GUILayout.MaxWidth(120) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return x;
        }


        public void PDEditor_ObjectField<T> ( GUIContent label, ref T value, bool fromScene )where T:UnityEngine.Object{
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(180));
            GUILayout.FlexibleSpace();
            var t = GUI.skin;
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = (T)EditorGUILayout.ObjectField( value, typeof(T), fromScene, GUILayout.MinWidth(64), GUILayout.MaxWidth(180) );
            GUI.skin = t;
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Slider( GUIContent label, ref float value, float min, float max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


		public void PDEditor_IntSlider( GUIContent label, ref int value, int min, int max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntSlider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Vector2( GUIContent label, ref Vector2 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector2Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Vector3( GUIContent label, ref Vector3 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector3Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Vector4( GUIContent label, ref Vector4 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector4Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Color( GUIContent label, ref Color value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = EditorGUILayout.ColorField( "", value, GUILayout.MinWidth(100) );
            GUI.skin = pidiSkin;
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Popup( GUIContent label, ref int value, params GUIContent[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_CenteredLabel( string label ){
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        public void PDEditor_FloatField( GUIContent label, ref float value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.FloatField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


		public void PDEditor_LayerMaskField ( GUIContent label, ref LayerMask selected) {
		
		List<string> layers = null;
		string[] layerNames = null;
		
		if (layers == null) {
			layers = new List<string>();
			layerNames = new string[4];
		} else {
			layers.Clear ();
		}
		
		int emptyLayers = 0;
		for (int i=0;i<32;i++) {
			string layerName = LayerMask.LayerToName (i);
			
			if (layerName != "") {
				
				for (;emptyLayers>0;emptyLayers--) layers.Add ("Layer "+(i-emptyLayers));
				layers.Add (layerName);
			} else {
				emptyLayers++;
			}
		}
		
		if (layerNames.Length != layers.Count) {
			layerNames = new string[layers.Count];
		}
		for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
		
		GUILayout.BeginHorizontal();
        GUILayout.Space(12);
        GUILayout.Label(label, GUILayout.Width(175));
        GUILayout.FlexibleSpace();

		selected.value =  EditorGUILayout.MaskField (selected.value,layerNames, GUILayout.MaxWidth(256) );
		
		GUILayout.Space(4);
        GUILayout.EndHorizontal();
		
		}


	#endregion
}
