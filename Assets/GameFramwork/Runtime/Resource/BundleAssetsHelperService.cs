using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


namespace Wanderer
{
    public sealed class BundleAssetsHelperService : MonoService, IAssetsHelper
    {
        //资源下载
        [Inject]
        public IWebRequest webRequest { get; set; }

        //版本信息
        private ResourceVersion m_resourceVersion;

        //是否正在更新中
        private bool m_update;

        //资源引用管理
        private AssetBundleManifest m_mainfest;

        /// <summary>
        /// 预加载的所有资源
        /// </summary>
        private Dictionary<string, UnityEngine.Object> m_preloadAssets = new Dictionary<string, UnityEngine.Object>();

        ////资源对应的AssetBundle
        //private readonly Dictionary<string, BundleProvider> m_assetsFromBundleProviderMap = new Dictionary<string, BundleProvider>();

        private Dictionary<string, BundleProvider> m_assetBundlePathForBundleProviderMap = new Dictionary<string, BundleProvider>();

        //加载BundleProvider的异步锁
        private HashSet<string> m_lockAssetBundlePathForBundleProvider = new HashSet<string>();

        public BundleAssetsHelperService()
        {
            m_resourceVersion = UnityEngine.Resources.Load<ResourceVersion>("BundleVersion");
        }

        public void CheckUpdate(Action<bool> needUpdateCallback)
        {
            Toast.Info("检查资源中...");
            m_resourceVersion.CheckUpdate((result, errorMessage) =>
            {
                if (result)
                {
                    if (m_resourceVersion.ForceUpdateAssets.Count > 0)
                    {
                        Toast.Info("更新版本信息...");
                        //强制下载
                        DownloadAssets(m_resourceVersion.ForceUpdateAssets, null, () => {
                            SetNeedUpdateCallback(needUpdateCallback, m_resourceVersion.DefaultUpdateAssets.Count > 0);

                        }, (localPath, error) => {
                            Toast.Error($"下载失败,{error} {localPath}");
                        });
                    }
                    else
                    {
                        SetNeedUpdateCallback(needUpdateCallback, m_resourceVersion.DefaultUpdateAssets.Count > 0);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Toast.Error(errorMessage);
                    }
                    else
                    {
                        SetNeedUpdateCallback(needUpdateCallback,false);
                    }
                }
            });
        }

        //设置mainfest 以及返回是否更新

        private void SetNeedUpdateCallback(Action<bool> needUpdateCallback, bool needUpdate)
        {
            var bytes = Convert.FromBase64String(m_resourceVersion.GetManifest());
            var manifestAB = AssetBundle.LoadFromMemory(bytes);
            m_mainfest = manifestAB.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            needUpdateCallback?.Invoke(needUpdate);
        }

        /// <summary>
        /// 异步加载Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="callback"></param>
        public void LoadAsset<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object
        {
            assetPath = assetPath.ToLower();
            if (m_preloadAssets.TryGetValue(assetPath, out UnityEngine.Object asset))
            {
                callback?.Invoke((T)asset);
            }
            else
            {
                LoadBundleProviderFromAssetPath(assetPath, (bundleProvider) =>
                {
                    bundleProvider.GetBundle(ab =>
                    {
                        if (ab != null)
                        {
                            bundleProvider.SetRefCount(+1);
                            ab.LoadAssetAsync<T>(assetPath).completed += (asyncHandle) =>
                            {
                                AssetBundleRequest abRequest = asyncHandle as AssetBundleRequest;
                                callback?.Invoke((T)abRequest.asset);
                            };
                        }
                        else
                        {
                            Log.Error("Cannot find assetbundle from: {0}", assetPath);
                            callback?.Invoke(null);
                        }
                    });
                });
            }
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string assePath) where T : UnityEngine.Object
        {
            assePath = assePath.ToLower();
            if (m_preloadAssets.TryGetValue(assePath, out UnityEngine.Object asset))
            {
                return (T)asset;
            }

            var bundleProvider = LoadBundleProviderFromAssetPath(assePath);
            var loadAsset = bundleProvider.GetBundle().LoadAsset<T>(assePath);
            bundleProvider.SetRefCount(+1);
            return loadAsset;
        }

        public void LoadExtendAssembly(string path, Action<Assembly> onExtendAssemblyCallback)
        {
            LoadAsset<TextAsset>(path, (textAsset) =>
            {
                var assembly = Assembly.Load(textAsset.bytes);
                onExtendAssemblyCallback?.Invoke(assembly);
            });
        }
      
        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetName"></param>
        public void UnloadAsset(string assetPath)
        {
            assetPath = assetPath.ToLower();
            string abPath = m_resourceVersion.AssetPathTransformBundlePath(assetPath);
            if (m_assetBundlePathForBundleProviderMap.TryGetValue(abPath, out BundleProvider bundleProvider))
            {
                bundleProvider.SetRefCount(-1);
            }
        }
       
        /// <summary>
        /// 卸载assetBundle
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="unload"></param>
        public void UnloadAssetBunlde(string assetBundleName, bool unload = false)
        {
            assetBundleName = assetBundleName.ToLower();
            string abPath = m_resourceVersion.BundleNameTransformBundlePath(assetBundleName);
            if (m_assetBundlePathForBundleProviderMap.TryGetValue(abPath, out BundleProvider bundleProvider))
            {
                m_assetBundlePathForBundleProviderMap.Remove(abPath);
                bundleProvider.Dispose();
            }
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadSceneAsync(string scenePath, LoadSceneMode mode, Action<AsyncOperation> callback)
        {
            AsyncOperation asyncOperation = null;
            try
            {
                LoadBundleProviderFromAssetPath(scenePath, (bundleProvider) =>
                {
                    bundleProvider.GetBundle(ab =>
                    {
                        if (ab != null)
                        {
                            asyncOperation = SceneManager.LoadSceneAsync(scenePath, mode);
                            callback?.Invoke(asyncOperation);
                        }
                        else
                        {
                            Log.Error("Cannot find assetbundle from: {0}", scenePath);
                            callback?.Invoke(null);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName"></param>
        public AsyncOperation UnloadSceneAsync(string scenePath)
        {
            var asyncHandle = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scenePath);
            asyncHandle.completed += (handle) => {
                LoadBundleProviderFromAssetPath(scenePath, (bundleProvider) =>
                {
                    m_assetBundlePathForBundleProviderMap.Remove(scenePath);
                    bundleProvider.Dispose();
                    bundleProvider = null;
                });
            };
            return asyncHandle;
        }
     
        public void UpdateDefaultAssets(Action<float, double, double, float> callback, Action downloadComplete, Action<string, string> errorCallback)
        {
            DownloadAssets(m_resourceVersion.DefaultUpdateAssets, callback, downloadComplete, errorCallback);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Clear()
        {
            m_mainfest = null;
            foreach (var item in m_assetBundlePathForBundleProviderMap.Values)
            {
                item.Dispose();
            }
            m_assetBundlePathForBundleProviderMap.Clear();
            m_assetBundlePathForBundleProviderMap = null;

        }

        #region 加载AssetBunlde，以及处理版本、路径、引用关系
        private void LoadBundleProvider(string abName, Action<BundleProvider> getAssetBundleCallback)
        {
            var abPath = m_resourceVersion.BundleNameTransformBundlePath(abName);
            if (!string.IsNullOrEmpty(abPath))
            {
                Log.Error("Cannot find assetbundle path: {0}", abName);
            }
            else
            {
                LoadBundleProviderFromPath(abPath, getAssetBundleCallback);
            }
        }

        private BundleProvider LoadBundleProvider(string abName)
        {
            var abPath = m_resourceVersion.BundleNameTransformBundlePath(abName);
            if (!string.IsNullOrEmpty(abPath))
            {
                Log.Error("Cannot find assetbundle path: {0}", abName);
            }
            else
            {
                return LoadBundleProviderFromPath(abPath);
            }

            return null;
        }


        private void LoadBundleProviderFromPath(string abPath, Action<BundleProvider> getAssetBundleACallback)
        {
            if (m_lockAssetBundlePathForBundleProvider.Contains(abPath))
            {
                StartCoroutine(WaitLoadBundleProviderFromPath(abPath, getAssetBundleACallback));
                return;
            }

            Log.Info("LoadBundleProviderFromPath. {0}", abPath);

            BundleProvider bundleProvider = null;
            if (!m_assetBundlePathForBundleProviderMap.TryGetValue(abPath, out bundleProvider))
            {
                bundleProvider = new BundleProvider(abPath);
                m_lockAssetBundlePathForBundleProvider.Add(abPath);
                bool waitForAllDependencies = false;
                AssetBundle loadAssetBunlde = null;
                //处理引用
                var allDependencies = m_mainfest.GetAllDependencies(bundleProvider.BundleName);
                if (allDependencies != null && allDependencies.Length > 0)
                {
                    waitForAllDependencies = true;
                    int dependenciesCount = 0;
                    foreach (var bundleName in allDependencies)
                    {
                        LoadBundleProvider(bundleName, (bp) =>
                        {
                            dependenciesCount++;
                            if (dependenciesCount == allDependencies.Length)
                            {
                                waitForAllDependencies = true;
                                if (loadAssetBunlde != null)
                                {
                                    m_lockAssetBundlePathForBundleProvider.Remove(abPath);
                                    getAssetBundleACallback?.Invoke(bundleProvider);
                                }
                            }
                        });
                    }
                }

                //加载自己的ab
                bundleProvider.GetBundle(ab =>
                {
                    if (ab == null)
                    {
                        Log.Error("BundleProvider GetBundle is null.");
                    }
                    loadAssetBunlde = ab;
                    if (!waitForAllDependencies)
                    {
                        m_lockAssetBundlePathForBundleProvider.Remove(abPath);
                        getAssetBundleACallback?.Invoke(bundleProvider);
                    }
                });

                //资源路径对应Bundle处理
                m_assetBundlePathForBundleProviderMap.Add(abPath, bundleProvider);
            }
            else
            {
                getAssetBundleACallback?.Invoke(bundleProvider);
            }
        }

        private BundleProvider LoadBundleProviderFromPath(string abPath)
        {
            BundleProvider bundleProvider = null;
            if (!m_assetBundlePathForBundleProviderMap.TryGetValue(abPath, out bundleProvider))
            {
                bundleProvider = new BundleProvider(abPath);
                m_lockAssetBundlePathForBundleProvider.Add(abPath);
                //处理引用
                var allDependencies = m_mainfest.GetAllDependencies(bundleProvider.BundleName);

                if (allDependencies != null && allDependencies.Length > 0)
                {
                    foreach (var bundleName in allDependencies)
                    {
                        var depAb = LoadBundleProvider(bundleName);
                        if (depAb == null)
                        {
                            Log.Error("同步加载错误");
                        }
                    }
                }

                AssetBundle loadAssetBunlde = bundleProvider.GetBundle();
                if (loadAssetBunlde != null)
                {
                    return bundleProvider;
                }
                else
                {
                    Log.Error("同步加载错误");
                }

                //资源路径对应Bundle处理
                m_assetBundlePathForBundleProviderMap.Add(abPath, bundleProvider);
            }

            return bundleProvider;
        }

        private IEnumerator WaitLoadBundleProviderFromPath(string abPath, Action<BundleProvider> getAssetBundleACallback)
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
            while (m_lockAssetBundlePathForBundleProvider.Contains(abPath))
            {
                yield return waitForSeconds;
            }

            if (m_assetBundlePathForBundleProviderMap.TryGetValue(abPath, out BundleProvider bundleProvider))
            {
                getAssetBundleACallback?.Invoke(bundleProvider);
            }
            else
            {
                Log.Error("异步加载失败");
            }
        }

        private void LoadBundleProviderFromAssetPath(string assetPath, Action<BundleProvider> getAssetBundleCallback)
        {
            Log.Info("LoadBundleProviderFromAssetPath. assetPath: {0}", assetPath);
            var abPath = m_resourceVersion.AssetPathTransformBundlePath(assetPath);
            if (string.IsNullOrEmpty(abPath))
            {
                Log.Error("Cannot find assetbundle path: {0}", assetPath);
            }
            else
            {
                LoadBundleProviderFromPath(abPath, getAssetBundleCallback);
            }
        }


        private BundleProvider LoadBundleProviderFromAssetPath(string assetPath)
        {
            var abPath = m_resourceVersion.AssetPathTransformBundlePath(assetPath);
            if (string.IsNullOrEmpty(abPath))
            {
                Log.Error("Cannot find assetbundle path: {0}", assetPath);
            }
            else
            {
                return LoadBundleProviderFromPath(abPath);
            }

            return null;
        }

        #endregion


        #region 内部函数

        private void DownloadAssets(HashSet<AssetHashInfo> needDownloadAssets, Action<float, double, double, float> callback, Action downloadComplete, Action<string, string> errorCallback)
        {
            if (m_update)
            {
                Log.Warn("资源正在更新中， 不应该再调用下载");
                return;
            }

            m_update = true;

            if (needDownloadAssets != null && needDownloadAssets.Count > 0)
            {
                string remoteURLRoot = m_resourceVersion.RemoteURL;
                string localPathRoot = m_resourceVersion.LocalPath;
                var fileDownloader = webRequest.GetFileDownloader();

                int downloadFileCount = 0;
                double totleFileSize = 0;
                Dictionary<string, AssetHashInfo> downloadFiles = new Dictionary<string, AssetHashInfo>();
                foreach (var item in needDownloadAssets)
                {
                    string remoteUrl = Path.Combine(remoteURLRoot, item.Name);
                    string localPath = Path.Combine(localPathRoot, $"{item.Name}.download");

                    fileDownloader.AddDownloadFile(remoteUrl, localPath);
                    //整理文件大小
                    totleFileSize += item.Size;
                    downloadFiles.Add(localPath, item);
                }
                //下载文件
                fileDownloader.StartDownload((localPath, size, time, speed) =>
                {
                    float progress = Mathf.Clamp01((float)(size / totleFileSize));
                    float remainingTime = (float)((totleFileSize - size) / speed);
                    callback?.Invoke(progress, totleFileSize, speed, remainingTime);
                }, async (localPath) =>
                {
                    //验证文件的完整性
                    string md5 = FileUtility.GetFileMD5(localPath);
                    var assetHashInfo = downloadFiles[localPath];
                    if (assetHashInfo.MD5.Equals(md5))
                    {
                        int index = localPath.LastIndexOf('.');
                        string targetPath = localPath.Substring(0, index);
                        if (File.Exists(targetPath))
                        {
                            File.Delete(targetPath);
                        }
                        File.Move(localPath, targetPath);
                        downloadFiles.Remove(localPath);
                        downloadFileCount++;
                        //更新本地版本信息
                        m_resourceVersion.UpdateLocalVersion(assetHashInfo);
                        //下载完成
                        if (downloadFiles.Count == 0)
                        {
                            //更新本地资源
                            if (downloadFileCount == needDownloadAssets.Count)
                            {
                                //if (CheckResource())
                                //{
                                //    UpdateLocalVersion();
                                //}
                            }
                            else
                            {
                                Log.Error("资源下载失败...");
                            }
                            needDownloadAssets = null;
                            m_update = false;
                            fileDownloader.StopDownload();
                            downloadComplete?.Invoke();
                        }
                    }
                    else
                    {
                        File.Delete(localPath);
                        fileDownloader.StopDownload();
                        m_update = false;
                        //throw new GameException($"File integrity verification failed. {localPath}");
                        errorCallback?.Invoke(localPath, "File integrity verification failed.");

                    }
                }, (localPath, error) =>
                {
                    fileDownloader.StopDownload();
                    m_update = false;
                    errorCallback?.Invoke(localPath, error);
                });
            }
            else
            {
                //下载完成
                downloadComplete?.Invoke();
                m_update = false;
            }
        }


        
        #endregion



    }



    class BundleProvider : System.IDisposable
    {
        private string m_abPath;
        private AssetBundle m_bundle;
        private Stream m_fileStream;
        private AsyncOperation m_asyncHandle;
        private Action<AssetBundle> m_getABCallback;
        private int m_refCount;
        internal string BundleName { get; private set; }

        internal BundleProvider(string abPath)
        {
            BundleName = Path.GetFileName(abPath);
            LoadAssetBundle(abPath);
        }

        internal void GetBundle(Action<AssetBundle> onABCallback)
        {
            if (m_bundle != null)
            {
                onABCallback?.Invoke(m_bundle);
            }
            else
            {
                m_getABCallback += onABCallback;
            }
        }
        internal AssetBundle GetBundle()
        {
            while (m_bundle == null)
            {
                Thread.Sleep(100);
            }

            return m_bundle;
        }

        internal void SetRefCount(int refCount)
        {
            if (m_bundle == null)
                return;

            m_refCount += refCount;
            if (m_refCount <= 0)
            {
                m_bundle.Unload(false);
            }
        }

        private void LoadAssetBundle(string abPath)
        {
            if (abPath.StartsWith("http"))
            {
                //这里是网络下载的处理
            }
            else if (Application.platform == RuntimePlatform.Android && abPath.StartsWith("jar:"))
            {
                var unityRequest = UnityWebRequest.Get(abPath);
                unityRequest.downloadHandler = new DownloadHandlerBuffer();
                m_asyncHandle = unityRequest.SendWebRequest();
            }
            else
            {
                Debug.Log($"BundleProvider.EncryptFileStream: {abPath}");
                m_fileStream = new EncryptFileStream(abPath, FileMode.Open,FileAccess.Read,FileShare.Read,1024*10,false);
                SetBundleComplete(AssetBundle.LoadFromStream(m_fileStream));
            }

            if (m_asyncHandle != null)
            {
                m_asyncHandle.completed += OnAssetBundleReadComplete;
            }
        }

        private void OnAssetBundleReadComplete(AsyncOperation asyncHandle)
        {
            asyncHandle.completed -= OnAssetBundleReadComplete;
            if (asyncHandle is AssetBundleCreateRequest abCreateRequest)
            {
                SetBundleComplete(abCreateRequest.assetBundle);
            }
            else if (asyncHandle is UnityWebRequestAsyncOperation unityWebRequest)
            {
                //android从StreamingAssets中读取ab包
                if (Application.platform == RuntimePlatform.Android && m_abPath.StartsWith("jar:"))
                {
                    m_fileStream = new EncryptMemoryStream(unityWebRequest.webRequest.downloadHandler.data, m_abPath);
                    SetBundleComplete(AssetBundle.LoadFromStream(m_fileStream));
                }
                //从http下载的文件
                else
                {
                    //这里还未实现
                }
            }
        }

        //设置回调
        private void SetBundleComplete(AssetBundle assetBundle)
        {
            m_bundle = assetBundle;
            //异步回调
            if (m_getABCallback != null)
            {
                m_getABCallback?.Invoke(m_bundle);
                m_getABCallback = null;
            }
        }

        public void Dispose()
        {
            m_abPath = null;
            m_asyncHandle = null;
            if (m_bundle != null)
            {
                m_bundle.Unload(true);
                m_bundle = null;
            }
            if (m_fileStream != null)
            {
                m_fileStream.Dispose();
                m_fileStream = null;
            }
        }
    }
}
