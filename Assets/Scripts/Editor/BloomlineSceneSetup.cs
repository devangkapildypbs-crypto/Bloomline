// BloomlineSceneSetup.cs — Editor utility to create and configure game scenes
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace Bloomline.Editor
{
    /// <summary>
    /// Editor tool to automatically set up all Bloomline scenes.
    /// </summary>
    public class BloomlineSceneSetup
    {
        private const string SCENES_PATH = "Assets/Scenes";

        [MenuItem("Bloomline/Setup All Scenes", priority = 0)]
        public static void SetupAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Bloomline Scene Setup",
                "This will create and configure all 4 game scenes.\nExisting scenes will be overwritten.\n\nContinue?",
                "Create Scenes", "Cancel"))
            {
                return;
            }

            // Ensure Scenes folder exists
            if (!Directory.Exists(SCENES_PATH))
            {
                Directory.CreateDirectory(SCENES_PATH);
            }

            EditorUtility.DisplayProgressBar("Bloomline Setup", "Creating BootScene...", 0.1f);
            CreateBootScene();

            EditorUtility.DisplayProgressBar("Bloomline Setup", "Creating MainMenuScene...", 0.3f);
            CreateMainMenuScene();

            EditorUtility.DisplayProgressBar("Bloomline Setup", "Creating LevelSelectScene...", 0.5f);
            CreateLevelSelectScene();

            EditorUtility.DisplayProgressBar("Bloomline Setup", "Creating GameScene...", 0.7f);
            CreateGameScene();

            EditorUtility.DisplayProgressBar("Bloomline Setup", "Configuring build settings...", 0.9f);
            SetBuildSettings();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Bloomline Setup Complete",
                "All 4 scenes created and build settings configured!\n\n" +
                "• BootScene\n• MainMenuScene\n• LevelSelectScene\n• GameScene\n\n" +
                "Open BootScene and press Play to start.",
                "OK");
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Camera
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.10f, 0.22f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 0, -10);

            // Bootstrap
            var bootstrapObj = new GameObject("GameBootstrap");
            bootstrapObj.AddComponent<Core.GameBootstrap>();

            EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/BootScene.unity");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.10f, 0.22f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 0, -10);

            var menuObj = new GameObject("MainMenuUI");
            menuObj.AddComponent<UI.MainMenuUI>();

            EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/MainMenuScene.unity");
        }

        private static void CreateLevelSelectScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.10f, 0.22f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 0, -10);

            var selectObj = new GameObject("LevelSelectUI");
            selectObj.AddComponent<UI.LevelSelectUI>();

            EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/LevelSelectScene.unity");
        }

        private static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.10f, 0.22f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 0.5f, -10);

            var controllerObj = new GameObject("GameSceneController");
            controllerObj.AddComponent<UI.GameSceneController>();

            EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/GameScene.unity");
        }

        private static void SetBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{SCENES_PATH}/BootScene.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/MainMenuScene.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/LevelSelectScene.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/GameScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        [MenuItem("Bloomline/Open Boot Scene", priority = 10)]
        public static void OpenBootScene()
        {
            string path = $"{SCENES_PATH}/BootScene.unity";
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Scene Not Found",
                    "BootScene not found. Run 'Bloomline > Setup All Scenes' first.", "OK");
            }
        }

        [MenuItem("Bloomline/Play from Boot Scene", priority = 11)]
        public static void PlayFromBootScene()
        {
            string path = $"{SCENES_PATH}/BootScene.unity";
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
                EditorApplication.isPlaying = true;
            }
            else
            {
                EditorUtility.DisplayDialog("Scene Not Found",
                    "BootScene not found. Run 'Bloomline > Setup All Scenes' first.", "OK");
            }
        }
    }
}
#endif
