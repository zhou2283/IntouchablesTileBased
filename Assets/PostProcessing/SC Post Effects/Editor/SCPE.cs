// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using UnityEngine;
using System.IO;
using UnityEditor;
using System.Net;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.PackageManager;
#endif

namespace SCPE
{
    public class SCPE : Editor
    {
        //Asset specifics
        public const string ASSET_NAME = "SC Post Effects";
        public const string ASSET_ID = "108753";
        public const string ASSET_ABRV = "SCPE";
        public const string DEFINE_SYMBOL = "SCPE";
        private const string ASSET_UNITY_VERSION = "561";

        public const string INSTALLED_VERSION = "0.6.1 BETA";

        public const string MIN_UNITY_VERSION = "5.6.1";

        public const string VERSION_FETCH_URL = "http://www.staggart.xyz/backend/versions/scpe.php";
        public const string DOC_URL = "http://staggart.xyz/unity/sc-post-effects/scpe-docs/";
        public const string FORUM_URL = "https://forum.unity.com/threads/513191";

        public const string PP_LAYER_NAME = "PostProcessing";

        public const string CORRECT_BASE_FOLDER = "PostProcessing";
        public static string PACKAGE_BASE_FOLDER
        {
            get { return SessionState.GetString(SCPE.ASSET_ABRV + "_PATH", string.Empty); }
            set { SessionState.SetString(SCPE.ASSET_ABRV + "_PATH", value); }
        }

        public enum RenderPipeline
        {
            Legacy,
            Lightweight,
            HighDefinition
        };
        public static RenderPipeline pipeline = RenderPipeline.Legacy;

        public static bool IsSinglePassVR
        {
            get { return (PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass && PlayerSettings.virtualRealitySupported) ? true : false; }
        }

        public static RenderPipeline GetRenderPipeline()
        {
#if UNITY_2018_1_OR_NEWER
            UnityEngine.Experimental.Rendering.RenderPipelineAsset renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

            if (renderPipelineAsset)
            {
                if (renderPipelineAsset.name.Contains("Lightweight")) { pipeline = RenderPipeline.Lightweight; }
                else if (renderPipelineAsset.name.Contains("HD")) { pipeline = RenderPipeline.HighDefinition; }
            }
            else { pipeline = RenderPipeline.Legacy; }
#else
            pipeline = RenderPipeline.Legacy;
#endif
#if SCPE_DEV
            Debug.Log("<b>" + SCPE.ASSET_NAME + "</b> Pipeline used: " + pipeline.ToString());
#endif

            return pipeline;
        }


        [InitializeOnLoad]
        sealed class InitializeOnLoad : Editor
        {
            [InitializeOnLoadMethod]
            public static void Initialize()
            {
                if (EditorApplication.isPlaying) return;

                SCPE.GetRenderPipeline();
            }
        }

        public static string GetRootFolder()
        {
            //Get script path
            string[] res = System.IO.Directory.GetFiles("Assets", "SCPE.cs", SearchOption.AllDirectories);
            string scriptFilePath = res[0];

			#if UNITY_EDITOR_WIN
			string slash = "\\";
			#endif

			#if UNITY_EDITOR_OSX
			string slash = "/";
			#endif

            //Truncate to get relative path
            PACKAGE_BASE_FOLDER = scriptFilePath.Replace(slash + "Editor" + slash + "SCPE.cs", string.Empty);

            //Compose images path
            string bannerPath = SCPE.PACKAGE_BASE_FOLDER;
			bannerPath += slash + "Editor" + slash + "Images" + slash + SCPE.ASSET_ABRV + "_Banner.png";

            //Load banner image
            SCPE_GUI.HEADER_IMG_PATH = bannerPath;

			#if SCPE_DEV
			Debug.Log ("Banner img path: " + bannerPath);
			#endif

            return PACKAGE_BASE_FOLDER;
        }

        public static void OpenStorePage()
        {
            Application.OpenURL("com.unity3d.kharma:content/" + ASSET_ID);
        }

        public static int GetLayerID()
        {
            return LayerMask.NameToLayer(PP_LAYER_NAME);
        }
    }

    public class AutoSetup
    {
        [MenuItem("CONTEXT/Camera/Add Post Processing Layer")]
        public static void SetupCamera()
        {
#if SCPE //Avoid missing PostProcessing scripts
            Camera cam = (Camera.main) ? Camera.main : GameObject.FindObjectOfType<Camera>();
            GameObject mainCamera = cam.gameObject;

            if (!mainCamera)
            {
                Debug.LogError("<b>SC Post Effects</b> No camera found in scene to configure");
                return;
            }

            //Add PostProcessLayer component if not already present
            if (mainCamera.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>() == false)
            {
                UnityEngine.Rendering.PostProcessing.PostProcessLayer ppLayer = mainCamera.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
                ppLayer.volumeLayer = LayerMask.GetMask(LayerMask.LayerToName(SCPE.GetLayerID()));
                ppLayer.fog.enabled = false;
                Debug.Log("<b>PostProcessLayer</b> component was added to <b>" + mainCamera.name + "</b>");
                cam.allowMSAA = false;
                cam.allowHDR = true;

                //Enable AA by default
                ppLayer.antialiasingMode = UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.FastApproximateAntialiasing;

                Selection.objects = new[] { mainCamera };
                EditorUtility.SetDirty(mainCamera);
            }
#endif
        }


        //Create a global post processing volume and assign the correct layer and default profile
        public static void SetupGlobalVolume()
        {
#if SCPE //Avoid missing PostProcessing scripts
            GameObject volumeObject = new GameObject("Global Post-process Volume");
            UnityEngine.Rendering.PostProcessing.PostProcessVolume volume = volumeObject.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();

            volumeObject.layer = SCPE.GetLayerID();
            volume.isGlobal = true;

            //Find default profile
            string[] assets = AssetDatabase.FindAssets("SC Default Profile");

            if (assets.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

                UnityEngine.Rendering.PostProcessing.PostProcessProfile defaultProfile = (UnityEngine.Rendering.PostProcessing.PostProcessProfile)AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Rendering.PostProcessing.PostProcessProfile));
                volume.sharedProfile = defaultProfile;
            }
            else
            {
                Debug.Log("The default \"SC Post Effects\" profile could not be found. Add a new profile to the volume to get started.");
            }

            Selection.objects = new[] { volumeObject };
            EditorUtility.SetDirty(volumeObject);
#endif
        }
    }

    public class PackageVersionCheck : Editor
    {
        public static bool IS_UPDATED
        {
            get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_IS_UPDATED", false); }
            set { SessionState.SetBool(SCPE.ASSET_ABRV + "_IS_UPDATED", value); }
        }
        public static string fetchedVersion;

#if SCPE_DEV
        [MenuItem("SCPE/Check for update")]
#endif
        public static void GetLatestVersionPopup()
        {
            CheckForUpdate();

            if (!IS_UPDATED)
            {
                if (EditorUtility.DisplayDialog(SCPE.ASSET_NAME + ", version " + SCPE.INSTALLED_VERSION, "A new version is available: " + fetchedVersion, "Open store page", "Close"))
                {
                    SCPE.OpenStorePage();
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog(SCPE.ASSET_NAME + ", version " + SCPE.INSTALLED_VERSION, "Installed version is up-to-date!", "Close")) { }
            }
        }


        public static void CheckForUpdate()
        {
            WebClient webClient = new WebClient();
            try
            {
                //Fetching latest version
                fetchedVersion = webClient.DownloadString(SCPE.VERSION_FETCH_URL);

                //Success
                IS_UPDATED = (SCPE.INSTALLED_VERSION == fetchedVersion) ? true : false;

#if SCPE_DEV
                Debug.Log("<b>PackageVersionCheck</b> Updated = " + IS_UPDATED);
#endif
                return;
            }
            catch (WebException ex)
            {
                Debug.LogWarning("[" + SCPE.ASSET_NAME + "] Contacting update server failed: " + ex.Status);

                //When failed, assume installation is up-to-date
                fetchedVersion = SCPE.INSTALLED_VERSION;
                return;
            }
        }

    }

}//namespace