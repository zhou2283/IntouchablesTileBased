// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using System;
using System.IO;
using System.Linq;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace SCPE
{
    public class Installer : Editor
    {
        public class RunOnImport : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string str in importedAssets)
                {
                    if (str.Contains("Installer.cs"))
                    {
#if !SCPE
                        InstallerWindow.ShowWindow();
#endif
                    }
                }
            }
        }

        [InitializeOnLoad]
        sealed class InitializeOnLoad : Editor
        {
            [InitializeOnLoadMethod]
            public static void Initialize()
            {
                if (EditorApplication.isPlaying) return;
#if !SCPE
                //Package has been imported, but window may not show due to console errors
                //Force window to open after compilation is complete
                //InstallerWindow.ShowWindow();

                //For 2018.1+, after compiling the PostProcessing package, check for installaton again
                if (PostProcessingInstallation.IS_INSTALLED == false) PostProcessingInstallation.CheckInstallation();
#endif
            }
        }
        public static void Initialize()
        {
            IS_INSTALLED = false;
            Log.Clear();

            PackageVersionCheck.CheckForUpdate();
            UnityVersionCheck.CheckCompatibility();
            PostProcessingInstallation.CheckInstallation();
            //PPS Installation is checked before the folder is, so the installation type has been determined
            CheckBaseFolder();

            Demo.FindPackage();
        }

        public static void Install()
        {
            CURRENTLY_INSTALLING = true;
            IS_INSTALLED = false;

            //Install PPS
            {
                if (PostProcessingInstallation.IS_INSTALLED == false)
                {
                    PostProcessingInstallation.InstallPackage();
                }
            }


            //Unpack SCPE effects
            {
                if (PostProcessingInstallation.TYPE == PostProcessingInstallation.Type.PackageManager)
                {
                    SetShaderIncludePaths();
                }
            }

            //Define symbol
            {
                DefineSymbol.Add();
            }

            //Add Layer for project olders than 2018.1
            {
                SetupLayer();
            }

            //If option is chosen, unpack demo content
            {
                if (Settings.installDemoContent)
                {
                    Demo.InstallDemoPackage();
                }
            }

            Installer.Log.Write("Installation completed");
            CURRENTLY_INSTALLING = false;
            IS_INSTALLED = true;
        }

        public static void PostInstall()
        {
            if (Settings.deleteDemoContent)
            {
                AssetDatabase.DeleteAsset(Demo.PACKAGE_PATH);
            }
            if (Settings.setupCurrentScene)
            {
#if SCPE
                AutoSetup.SetupCamera();
                AutoSetup.SetupGlobalVolume();
#endif
            }
        }

        public static bool CURRENTLY_INSTALLING
        {
            get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_IS_INSTALLING", false); }
            set { SessionState.SetBool(SCPE.ASSET_ABRV + "_IS_INSTALLING", value); }
        }

        public static bool IS_INSTALLED
        {
            get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_IS_INSTALLED", false); }
            set { SessionState.SetBool(SCPE.ASSET_ABRV + "_IS_INSTALLED", value); }
        }

        //Folder Unity 2017.4 or older, the package must sit in this folder (No PackageManager version available)
        public const string CORRECT_BASE_FOLDER = "PostProcessing";

        public static bool IS_CORRECT_BASE_FOLDER
        {
            get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_CORRECT_BASE_FOLDER", false); }
            set { SessionState.SetBool(SCPE.ASSET_ABRV + "_CORRECT_BASE_FOLDER", value); }
        }

        //Check if SC Post Effects folder is placed inside PostProcessing folder
        public static void CheckBaseFolder()
        {
            SCPE.PACKAGE_BASE_FOLDER = SCPE.GetRootFolder();

            //Slashes are handled differently between Windows and macOS apparently
#if UNITY_EDITOR_WIN
            IS_CORRECT_BASE_FOLDER = SCPE.PACKAGE_BASE_FOLDER.Contains(SCPE.CORRECT_BASE_FOLDER + "\\" + SCPE.ASSET_NAME);
#endif

#if UNITY_EDITOR_OSX
            IS_CORRECT_BASE_FOLDER = SCPE.PACKAGE_BASE_FOLDER.Contains(SCPE.CORRECT_BASE_FOLDER + "/" + SCPE.ASSET_NAME);
#endif

#if UNITY_2018_1_OR_NEWER
            //For the package manager PPS version, asset folder can sit anywhere
            if (PostProcessingInstallation.TYPE == PostProcessingInstallation.Type.PackageManager)
            {
                IS_CORRECT_BASE_FOLDER = true;
            }
#endif

#if SCPE_DEV && !UNITY_2018_1_OR_NEWER
            Debug.Log("<b>Installer</b> Correct folder location: " + IS_CORRECT_BASE_FOLDER);
#endif
        }

#if SCPE_DEV
        [MenuItem("SCPE/Add layer")]
#endif
        public static void SetupLayer()
        {
            //PostProcessing layer already present by default in 2018.1+
#if UNITY_2018_1_OR_NEWER
            return;
#else
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty layers = tagManager.FindProperty("layers");

            bool hasLayer = false;

            //Skip default layers
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);

                if (layerSP.stringValue == SCPE.PP_LAYER_NAME)
                {
#if SCPE_DEV
                    Debug.Log("<b>SetupLayer</b> " + SCPE.PP_LAYER_NAME + " layer present");
#endif
                    hasLayer = true;
                }

                if (hasLayer) continue;

                if (layerSP.stringValue == String.Empty)
                {
#if SCPE_DEV
                    Debug.Log("<b>SetupLayer</b> " + SCPE.PP_LAYER_NAME + " layer present");
#endif
                    layerSP.stringValue = SCPE.PP_LAYER_NAME;
                    tagManager.ApplyModifiedProperties();
                    hasLayer = true;
                    Installer.Log.Write("Added \"" + SCPE.PP_LAYER_NAME + "\" layer to project");
                }
            }

            if (!hasLayer)
            {
                Debug.LogError("The layer \"" + SCPE.PP_LAYER_NAME + "\" could not be added, the maximum number of layers (32) has been exceeded");
                EditorApplication.ExecuteMenuItem("Edit/Project Settings/Tags and Layers");
            }
#endif

        }

        public static void UnpackEffects()
        {
            SetShaderIncludePaths();
        }

        public class Demo
        {
            public static string PACKAGE_PATH
            {
                get { return SessionState.GetString(SCPE.ASSET_ABRV + "_DEMO_PACKAGE_PATH", string.Empty); }
                set { SessionState.SetString(SCPE.ASSET_ABRV + "_DEMO_PACKAGE_PATH", value); }
            }

            public static bool HAS_PACKAGE
            {
                get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_HAS_DEMO_PACKAGE", false); }
                set { SessionState.SetBool(SCPE.ASSET_ABRV + "_HAS_DEMO_PACKAGE", value); }
            }

            public static bool INSTALLED
            {
                get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_DEMO_INSTALLED", false); }
                set { SessionState.SetBool(SCPE.ASSET_ABRV + "_DEMO_INSTALLED", value); }
            }

            public static void FindPackage()
            {
                string packageDir = SCPE.GetRootFolder();

                CheckInstallation();

                string[] assets = AssetDatabase.FindAssets("_DemoContent", new[] { packageDir });

                if (assets.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

                    PACKAGE_PATH = assetPath;
                    HAS_PACKAGE = true;
                }
                else
                {
                    Settings.installDemoContent = false;
                    HAS_PACKAGE = false;
                }
            }

            public static void CheckInstallation()
            {
#if UNITY_EDITOR_WIN
                string slash = "\\";
#endif

#if UNITY_EDITOR_OSX
            string slash = "/";
#endif
                INSTALLED = AssetDatabase.IsValidFolder(SCPE.PACKAGE_BASE_FOLDER + slash + "_Demo");

#if SCPE_DEV
                Debug.Log("<b>Demo</b> Installed: " + INSTALLED);
#endif
            }

#if SCPE_DEV
            [MenuItem("SCPE/Install/Demo content")]
#endif
            public static void InstallDemoPackage()
            {
                if (PACKAGE_PATH != null)
                {
                    AssetDatabase.ImportPackage(PACKAGE_PATH, false);

                    Installer.Log.Write("Unpacked demo scenes, profiles and samples");

                    AssetDatabase.Refresh();
                    AssetDatabase.DeleteAsset(PACKAGE_PATH);
                    INSTALLED = true;
                }
                else
                {
                    Debug.LogError("The \"_DemoContent\" package could not be found, please ensure all the package contents were imported from the Asset Store.");
                    INSTALLED = false;
                }
            }

        }


#if SCPE_DEV
        [MenuItem("SCPE/Fix shader includes")]
#endif
        public static void SetShaderIncludePaths()
        {
            string packageDir = SCPE.GetRootFolder();

            //Find all shaders in the package folder
            string[] assets = AssetDatabase.FindAssets("*Shader t:Shader", new string[] { packageDir });

            for (int i = 0; i < assets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);

                Shader shaderFile = (Shader)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Shader));
                string shaderName = shaderFile.name.Replace("Hidden/SC Post Effects/", string.Empty);

                EditorUtility.DisplayProgressBar("Configuring shader library paths " + i + "/" + assets.Length, shaderName, (float)i / assets.Length);

                string fileContents = File.ReadAllText(assetPath);

#if !UNITY_2018_1_OR_NEWER
                fileContents = fileContents.Replace("PostProcessing", "../../../..");
#else
                fileContents = fileContents.Replace("../../../..", "PostProcessing");
#endif

                File.WriteAllText(assetPath, fileContents);

                AssetDatabase.ImportAsset(assetPath);

            }

            EditorUtility.ClearProgressBar();

            Installer.Log.Write("Modified shaders for PackageManager installation...");


        }

        /// <summary>
        /// Sub-classes
        /// </summary>

        sealed class DefineSymbol : Editor
        {
            public static void Add()
            {
#if !SCPE
                var targets = Enum.GetValues(typeof(BuildTargetGroup))
                   .Cast<BuildTargetGroup>()
                   .Where(x => x != BuildTargetGroup.Unknown)
                   .Where(x => !IsObsolete(x));

                foreach (var target in targets)
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Trim();

                    var list = defines.Split(';', ' ')
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    if (list.Contains(SCPE.DEFINE_SYMBOL))
                    {
                        continue;
                    }

                    list.Add(SCPE.DEFINE_SYMBOL);

                    defines = list.Aggregate((a, b) => a + ";" + b);

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);

                    Installer.Log.Write("Added \"SCPE\" define symbol to project");

                }
#endif
            }

            private static bool IsObsolete(BuildTargetGroup group)
            {
                var attrs = typeof(BuildTargetGroup)
                    .GetField(group.ToString())
                    .GetCustomAttributes(typeof(ObsoleteAttribute), false);

                return attrs != null && attrs.Length > 0;
            }
        }

        public class Settings
        {
            public static bool upgradeShaders = true;
            public static bool installDemoContent
            {
                get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_INSTALL_DEMO", false); }
                set { SessionState.SetBool(SCPE.ASSET_ABRV + "_INSTALL_DEMO", value); }
            }
            public static bool deleteDemoContent = false;
            public static bool setupCurrentScene = false;
        }

        public static class Log
        {
            public static string Read(int index)
            {
                return SessionState.GetString(SCPE.ASSET_ABRV + "_LOG_ITEM_" + index, string.Empty);
            }

            public static string ReadNext()
            {
                return SessionState.GetString(SCPE.ASSET_ABRV + "_LOG_ITEM_" + NumItems, string.Empty);
            }

            public static int NumItems
            {
                get { return SessionState.GetInt(SCPE.ASSET_ABRV + "_LOG_INDEX", 0); }
                set { SessionState.SetInt(SCPE.ASSET_ABRV + "_LOG_INDEX", value); }
            }

            public static void Write(string text)
            {
                SessionState.SetString(SCPE.ASSET_ABRV + "_LOG_ITEM_" + NumItems, text);
                NumItems++;
            }

            internal static void Clear()
            {
                for (int i = 0; i < NumItems; i++)
                {
                    SessionState.EraseString(SCPE.ASSET_ABRV + "_LOG_ITEM_" + i);
                }
                NumItems = 0;
            }

#if SCPE_DEV
            [MenuItem("SCPE/Test install log")]
            public static void Test()
            {
                Installer.CURRENTLY_INSTALLING = true;

                Installer.Log.Write("Installed Post Processing Stack v2");
                Installer.Log.Write("Upgraded shader library paths");
                Installer.Log.Write("Enabled SCPE scripts");
                Installer.Log.Write("Adding \"PostProcessing\" layer to next available slot");
                Installer.Log.Write("Unpacked demo scenes and samples");
                Installer.Log.Write("<b>Installation completed</b>");

                Installer.CURRENTLY_INSTALLING = false;
            }
#endif
        }
    }

    public class PostProcessingInstallation
    {
        public enum Type
        {
            GitHub,
            PackageManager
        }
        public static Type TYPE
        {
            get { return (Type)SessionState.GetInt("PPS_TYPE", 0); }
            set { SessionState.SetInt("PPS_TYPE", (int)value); }
        }

        public static string ppPackageID = "com.unity.postprocessing";
        public static string ppInstallID = "com.unity.postprocessing@2.0.12-preview";
        public const string PP_DOWNLOAD_URL = "http://staggart.xyz/public/PostProcessingStack_v2.unitypackage";

        public static bool IS_INSTALLED
        {
            get { return SessionState.GetBool("PPS_INSTALLED", false); }
            set { SessionState.SetBool("PPS_INSTALLED", value); }
        }

        public static string PACKAGE_PATH
        {
            get { return SessionState.GetString("PPS_PACKAGE_PATH", string.Empty); }
            set { SessionState.SetString("PPS_PACKAGE_PATH", value); }
        }

        public static void CheckInstallation()
        {
            IsInstalled();
        }

        private static bool IsInstalled()
        {
            //Unity 2017.3 and older, check for define symbol
#if !UNITY_2018_1_OR_NEWER
#if UNITY_POST_PROCESSING_STACK_V2
            TYPE = Type.GitHub;
            IS_INSTALLED = true;

#if SCPE_DEV
            Debug.Log("<b>PostProcessingInstallation</b> " + TYPE + " version is installed");
#endif
#else
#if SCPE_DEV
                Debug.Log("<b>PostProcessingInstallation</b> Post Processing Stack v2 is not installed");
#endif
                TYPE = Type.GitHub;
                IS_INSTALLED = false;
#endif //UNITY_POST_PROCESSING_STACK_V2
            return IS_INSTALLED;
#endif //!UNITY_2018_1_OR_NEWER

            //Unity 2018.1+ check in Package Manager

#if UNITY_2018_1_OR_NEWER
                string packagesFolder = Application.dataPath + "/../Packages/";
                string manifestFile = packagesFolder + "manifest.json";

                string manifestContents = File.ReadAllText(manifestFile);

                if (manifestContents.Contains(ppPackageID))
                {
                    IS_INSTALLED = true;
                    TYPE = Type.PackageManager;
#if SCPE_DEV
                Debug.Log("<b>PostProcessingInstallation</b> " + TYPE + " version is installed");
#endif
                    return IS_INSTALLED;
                }
                else
                {
                    //Install PM version
                    TYPE = Type.PackageManager;

                    IS_INSTALLED = false;
#if SCPE_DEV
                Debug.Log("<b>PostProcessingInstallation</b> Not installed");
#endif
                    return IS_INSTALLED;
                }
#endif

        }

        public static void InstallPackage()
        {
            if (TYPE == Type.GitHub)
            {
                if (PACKAGE_PATH != null)
                {
                    AssetDatabase.ImportPackage(PACKAGE_PATH, false);
                    AssetDatabase.Refresh();

                    Installer.Log.Write("Installed Post Processing Stack v2 package");
                    IS_INSTALLED = true;
#if SCPE_DEV
                    Debug.Log("<b>PostProcessingInstallation</b> Installed Post Processing Stack v2 package");
#endif
                }

            }
            else if (TYPE == Type.PackageManager)
            {
#if UNITY_2018_1_OR_NEWER
                Client.Add(ppInstallID);

                Installer.Log.Write("Installed Post Processing from Package Manager");
#if SCPE_DEV
                Debug.Log("<b>PostProcessingInstallation</b> Installed from Package Manager");
#endif
                //Installer.ForceRecompile();

                IS_INSTALLED = true;
#endif
            }
        }
    }

    public class UnityVersionCheck
    {
        public static string UnityVersion
        {
            get { return Application.unityVersion; }
        }

        public static bool COMPATIBLE
        {
            get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_COMPATIBLE_VERSION", true); }
            set { SessionState.SetBool(SCPE.ASSET_ABRV + "_COMPATIBLE_VERSION", value); }
        }
        public static bool UNTESTED
        {
            get { return SessionState.GetBool(SCPE.ASSET_ABRV + "_UNTESTED_VERSION", false); }
            set { SessionState.SetBool(SCPE.ASSET_ABRV + "_UNTESTED_VERSION", value); }
        }

        public static void CheckCompatibility()
        {
            //Defaults
            COMPATIBLE = true;
            UNTESTED = false;

            //Positives
#if UNITY_5_6_OR_NEWER && !UNITY_2018_4_OR_NEWER
            COMPATIBLE = true;
            UNTESTED = false;
#endif
            //Negatives
#if !UNITY_5_5_OR_NEWER // < 5.6
            COMPATIBLE = false;
            UNTESTED = false;
#endif
#if UNITY_2018_4_OR_NEWER // >= 2018.4
            UNTESTED = true;
#endif

#if SCPE_DEV
            Debug.Log("<b>UnityVersionCheck</b> [Compatible: " + COMPATIBLE + "] - [Untested: " + UNTESTED + "]");
#endif
        }
    }
}
