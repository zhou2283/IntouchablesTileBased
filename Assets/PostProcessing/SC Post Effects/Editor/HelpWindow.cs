// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using UnityEngine;
using UnityEditor;

namespace SCPE
{
    public class HelpWindow : EditorWindow
    {

#if SCPE
        [MenuItem("Help/SC Post Effects", false, 0)]
#endif
        public static void ExecuteMenuItem()
        {
            HelpWindow.ShowWindow();
        }

        //Window properties
        private static int width = 440;
        private static int height = 500;

        //Tabs
        private bool isTabSetup = true;
        private bool isTabGettingStarted = false;
        private bool isTabSupport = false;

        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow<HelpWindow>(true, " ", true);
            editorWindow.titleContent = new GUIContent(SCPE.ASSET_NAME);
            editorWindow.autoRepaintOnSceneChange = true;

            //Open somewhat in the center of the screen
            editorWindow.position = new Rect((Screen.width) / 2f, 175, width, height);

            //Fixed size
            editorWindow.maxSize = new Vector2(width, height);
            editorWindow.minSize = new Vector2(width, 200);

            Init();

            editorWindow.Show();

        }

        private void SetWindowHeight(float height)
        {
            this.maxSize = new Vector2(width, height);
            this.minSize = new Vector2(width, height);
        }

        //Store values in the volatile SessionState
        static void Init()
        {
            SCPE.GetRootFolder();
        }

        void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(5);
            DrawTabs();
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (isTabSetup) DrawQuickSetup();

            if (isTabGettingStarted) DrawGettingStarted();

            if (isTabSupport) DrawSupport();

            //DrawActionButtons();

            EditorGUILayout.EndVertical();

            DrawFooter();
        }

        void DrawHeader()
        {
            SCPE_GUI.DrawHeader(width, height);

            GUILayout.Label("Version: " + SCPE.INSTALLED_VERSION, SCPE_GUI.Footer);
        }

        void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(isTabSetup, "Quick Setup", SCPE_GUI.Tab))
            {
                isTabSetup = true;
                isTabGettingStarted = false;
                isTabSupport = false;
            }

            if (GUILayout.Toggle(isTabGettingStarted, "Documentation", SCPE_GUI.Tab))
            {
                isTabSetup = false;
                isTabGettingStarted = true;
                isTabSupport = false;
            }

            if (GUILayout.Toggle(isTabSupport, "Support", SCPE_GUI.Tab))
            {
                isTabSetup = false;
                isTabGettingStarted = false;
                isTabSupport = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawQuickSetup()
        {
            SetWindowHeight(400f);

            EditorGUILayout.HelpBox("\nThese actions will automatically configure your scene for use with the Post Processing Stack.\n", MessageType.Info);

            EditorGUILayout.Space();

            //Camera setup
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Setup component on active camera");
                if (GUILayout.Button("Execute")) AutoSetup.SetupCamera();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            //Volume setup
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Create a new global Post Processing volume");
                if (GUILayout.Button("Execute")) AutoSetup.SetupGlobalVolume();
            }
            EditorGUILayout.EndHorizontal();

        }

        void DrawGettingStarted()
        {
            SetWindowHeight(335);

            EditorGUILayout.HelpBox("Please view the documentation for further details about this package and its workings.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("<b><size=12>Documentation</size></b>\n<i>Usage instructions</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL(SCPE.DOC_URL);
                }
                if (GUILayout.Button("<b><size=12>Effect details</size></b>\n<i>View effect examples</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL(SCPE.DOC_URL + "#effects");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawSupport()
        {
            SetWindowHeight(350f);

            EditorGUILayout.HelpBox("If you have any questions, or ran into issues, please get in touch.\n\nThis package is still in its Beta stage, feedback is greatly appreciated and will help improve it!", MessageType.Info);

            EditorGUILayout.Space();

            //Buttons box
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("<b><size=12>Email</size></b>\n<i>Contact</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL("mailto:contact@staggart.xyz");
                }
                if (GUILayout.Button("<b><size=12>Twitter</size></b>\n<i>Follow developments</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL("https://twitter.com/search?q=staggart%20creations");
                }
                if (GUILayout.Button("<b><size=12>Forum</size></b>\n<i>Join the discussion</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL(SCPE.FORUM_URL);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        //TODO: Implement after Beta
        private void DrawActionButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("<size=12>Rate</size>", SCPE_GUI.Button)) SCPE.OpenStorePage();

            if (GUILayout.Button("<size=12>Review</size>", SCPE_GUI.Button)) SCPE.OpenStorePage();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawFooter()
        {
            EditorGUILayout.LabelField("", UnityEngine.GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            GUILayout.Label("- Staggart Creations -", SCPE_GUI.Footer);
        }

        #region Styles
        private static GUIStyle _Header;
        public static GUIStyle Header
        {
            get
            {
                if (_Header == null)
                {
                    _Header = new GUIStyle(UnityEngine.GUI.skin.label)
                    {
                        richText = true,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        fontSize = 18,
                        fontStyle = FontStyle.Bold
                    };
                }

                return _Header;
            }
        }
        #endregion //Stylies

    }//SCPE_Window Class
}
