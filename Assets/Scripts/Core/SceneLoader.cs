// SceneLoader.cs — Static scene loading utility for Bloomline
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bloomline.Core
{
    /// <summary>
    /// Static helper for scene loading. Wraps UnityEngine.SceneManagement
    /// with convenience methods for synchronous and asynchronous loads.
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>
        /// Returns the name of the currently active scene.
        /// </summary>
        public static string CurrentScene
        {
            get { return SceneManager.GetActiveScene().name; }
        }

        /// <summary>
        /// Load a scene synchronously by name.
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Load a scene asynchronously. Optionally provides a progress callback
        /// (0-1 float) invoked each frame via a coroutine.
        /// Start this coroutine from any MonoBehaviour.
        /// </summary>
        public static IEnumerator LoadSceneAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            Debug.Log($"[SceneLoader] Async loading scene: {sceneName}");
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncOp.isDone)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                onProgress?.Invoke(progress);
                yield return null;
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Reload the currently active scene.
        /// </summary>
        public static void ReloadCurrentScene()
        {
            LoadScene(CurrentScene);
        }
    }
}
