using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wanderer
{
    public interface IAssetsHelper
    {
        /// <summary>
        /// 检查资源更新
        /// </summary>
        /// <param name="needUpdateCallback"></param>
        void CheckUpdate(Action<bool> needUpdateCallback);

        /// <summary>
        /// 更新默认的资源
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="downloadComplete"></param>
        /// <param name="errorCallback"></param>
        void UpdateDefaultAssets(Action<float, double, double, float> callback, Action downloadComplete, Action<string, string> errorCallback);

        /// <summary>
        /// 加载热更脚本
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onExtendAssemblyCallback"></param>
        void LoadExtendAssembly(string path, Action<Assembly> onExtendAssemblyCallback);

        /// <summary>
        /// 加载资源 -- 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        void LoadAsset<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object;

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        T LoadAsset<T>(string assetPath) where T : UnityEngine.Object;

        /// <summary>
        /// 卸载资源 -- 取消掉资源计数
        /// </summary>
        /// <param name="assetPath"></param>
        void UnloadAsset(string assetPath);

        /// <summary>
        /// 卸载资源 主要为卸载AssetBundle
        /// </summary>
        /// <param name="assetBundleName">资源名称</param>
        /// <param name="unload">是否卸载调所有资源</param>
        void UnloadAssetBunlde(string assetBundleName, bool unload = false);

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="sceneName"></param>
        void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action<AsyncOperation> callback);

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName"></param>
        AsyncOperation UnloadSceneAsync(string sceneName);

        /// <summary>
        /// 清理资源
        /// </summary>
        void Clear();

    }
}
