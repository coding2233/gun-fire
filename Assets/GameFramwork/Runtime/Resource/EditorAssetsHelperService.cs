#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace Wanderer
{
    public class EditorAssetsHelperService : IAssetsHelper
    {
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action<AsyncOperation> callback)
        {
            AsyncOperation asyncLoadScene = EditorSceneManager.LoadSceneAsyncInPlayMode(sceneName, new LoadSceneParameters(mode));
            callback?.Invoke(asyncLoadScene);
        }


        public AsyncOperation UnloadSceneAsync(string sceneName)
        {
            return UnitySceneManager.UnloadSceneAsync(sceneName);
        }


        public void LoadAsset<T>(string assetName, Action<T> callback) where T : Object
        {
            callback(AssetDatabase.LoadAssetAtPath<T>(assetName));
        }

        public void UnloadAsset(string assetName)
        {

        }

        public void UnloadAssetBunlde(string assetBundleName, bool unload = false)
        {

        }

        public void Clear()
        {
        }

		public T LoadAsset<T>(string assetName) where T : Object
		{
            return AssetDatabase.LoadAssetAtPath<T>(assetName);
        }

		public void Preload(Action<float> progressCallback)
		{
            progressCallback?.Invoke(1.0f);
        }

        public void CheckUpdate(Action<bool> needUpdateCallback)
        {
            needUpdateCallback?.Invoke(false);
        }

        public void UpdateDefaultAssets(Action<float, double, double, float> callback, Action downloadComplete, Action<string, string> errorCallback)
        {
            downloadComplete?.Invoke();
        }

        public void LoadExtendAssembly(string path, Action<Assembly> onExtendAssemblyCallback)
        {
            LoadAsset<TextAsset>(path, (textAsset) => {
                onExtendAssemblyCallback?.Invoke(Assembly.Load(textAsset.bytes));
            });
        }
    }
}

#endif
