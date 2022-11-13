// 资源版本管理
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Wanderer
{
    [CreateAssetMenu(fileName = "BundleVersion.asset", menuName = "AssetBundle/BundleVersion")]
    public class ResourceVersion : ScriptableObject
    {
        //资源信息文本名称
        [SerializeField]
        private string m_assetVersionName = "version.json";
        //主资源的版本号
        [SerializeField]
        private string m_mainVersion = "1.0";

        //远程更新的路径
        [SerializeField]
        private List<string> m_remoteUpdatePath;
        [SerializeField]
        private int m_remoteUpdatePathActive;
        /// <summary>
        /// 资源主版本
        /// </summary>
        public string MainVersion => m_mainVersion;
        /// <summary>
        /// 当前激活的远程url
        /// </summary>
        internal string RemoteURL { get; private set; }
        /// <summary>
        /// 本地文件存储的路径
        /// </summary>
        internal string LocalPath 
        { 
            get 
            {
                return Application.persistentDataPath;
            } 
        }

        /// <summary>
        /// 远程的版本信息
        /// </summary>
        /// <value></value>
        internal AssetVersion RemoteVersion { get; private set; }

        /// <summary>
        /// 本地的版本信息
        /// </summary>
        /// <value></value>
        internal AssetVersion LocalVersion { get; private set; }

        /// <summary>
        /// 本地的只读版本信息
        /// </summary>
        /// <value></value>
        internal AssetVersion StreamingAssetsVersion { get; private set; }

        //在编辑器中使用ab包
        [SerializeField]
        internal bool UseAssetBundleInEditor;

        internal HashSet<AssetHashInfo> ForceUpdateAssets { get; private set; } = new HashSet<AssetHashInfo>();
        internal HashSet<AssetHashInfo> DefaultUpdateAssets { get; private set; } = new HashSet<AssetHashInfo>();

        internal void CheckUpdate(Action<bool,string> updateCallback)
        {
            LocalVersion = null;
            StreamingAssetsVersion = null;
            RemoteVersion = null;


            HashSet<UnityWebRequestAsyncOperation> asyncOperations = new HashSet<UnityWebRequestAsyncOperation>();

            //本地版本
            string localPath = Path.Combine(LocalPath, m_assetVersionName);
            if (File.Exists(localPath))
            {
                string content = File.ReadAllText(localPath);
                LocalVersion = JsonUtility.FromJson<AssetVersion>(content);
            }

            //StreamingAsset版本
            string streamingAssetPath = Path.Combine(Application.streamingAssetsPath, m_assetVersionName);
            var webRequest = UnityWebRequest.Get(streamingAssetPath);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            var localRequestHandler = webRequest.SendWebRequest();
            localRequestHandler.completed += (handle) => 
            {
                //本地可能不存在， 不抛出错误
                string downloadText = webRequest.downloadHandler.text;
                if (!string.IsNullOrEmpty(downloadText))
                {
                    StreamingAssetsVersion = JsonUtility.FromJson<AssetVersion>(downloadText);
                }
                asyncOperations.Add(localRequestHandler);
                OnAssetVersionRequestComplete(asyncOperations, updateCallback);
            };

            //远程版本
            string remoteURL = GetRemoteURL();
            if (!string.IsNullOrEmpty(remoteURL))
            {
                string remoteAssetPath = Path.Combine(remoteURL, m_assetVersionName);
                Log.Info(remoteAssetPath);
                var remoteWebRequest = UnityWebRequest.Get(remoteAssetPath);
                remoteWebRequest.downloadHandler = new DownloadHandlerBuffer();
                var remoteWebRequestHandle = remoteWebRequest.SendWebRequest();
                remoteWebRequestHandle.completed += (handle) =>
                {
                    //有错误
                    if (!string.IsNullOrEmpty(remoteWebRequest.error))
                    {
                        updateCallback?.Invoke(false, remoteWebRequest.error);
                        return;
                    }
                    else
                    {
                        string downloadText = remoteWebRequest.downloadHandler.text;
                        if (!string.IsNullOrEmpty(downloadText))
                        {
                            RemoteVersion = JsonUtility.FromJson<AssetVersion>(downloadText);
                        }
                        asyncOperations.Add(remoteWebRequestHandle);
                        OnAssetVersionRequestComplete(asyncOperations, updateCallback);
                    }
                };
            }
        }

        internal void UpdateLocalVersion(AssetHashInfo assetHashInfo)
        {
            if (assetHashInfo!=null && RemoteVersion != null)
            {
                string localVersionPath = Path.Combine(LocalPath, m_assetVersionName);
                if (LocalVersion == null)
                {
                    string jsonContent = JsonUtility.ToJson(RemoteVersion);
                    LocalVersion = JsonUtility.FromJson<AssetVersion>(jsonContent);
                    LocalVersion.AssetHashInfos.Clear();
                }

                var oldAssetHashInfo = LocalVersion.AssetHashInfos.Find(x => x.Name.Equals(assetHashInfo.Name));
                if (oldAssetHashInfo != null)
                {
                    LocalVersion.AssetHashInfos.Remove(oldAssetHashInfo);
                }
                LocalVersion.AssetHashInfos.Add(assetHashInfo);
                //更新本地信息
                File.WriteAllText(localVersionPath,JsonUtility.ToJson(LocalVersion));
            }
        }

        internal string AssetPathTransformBundlePath(string assetPath)
        {
            AssetHashInfo remoteAssetHash=null;
            AssetHashInfo localAssetHash=null;
            string assetBundlePath = null;
            if (RemoteVersion != null)
            {
                remoteAssetHash = RemoteVersion.AssetHashInfos.Find(x => x.Addressables.Contains(assetPath));
            }

            if (remoteAssetHash != null)
            {
                //找到本地对应的assetBundle
                if (StreamingAssetsVersion != null)
                {
                    localAssetHash = StreamingAssetsVersion.AssetHashInfos.Find(x => x.Equals(remoteAssetHash));
                    if (localAssetHash != null)
                    {
                        assetBundlePath = Path.Combine(Application.streamingAssetsPath, localAssetHash.Name);
                    }
                }

                if (localAssetHash == null)
                {
                    if (LocalVersion != null)
                    {
                        localAssetHash = LocalVersion.AssetHashInfos.Find(x => x.Equals(remoteAssetHash));
                        if (localAssetHash != null)
                        {
                            assetBundlePath = Path.Combine(LocalPath, localAssetHash.Name);
                        }
                    }
                }

                //本地没有下载返回整个url
                if (localAssetHash == null)
                {
                    assetBundlePath = Path.Combine(GetRemoteURL(), remoteAssetHash.Name);
                }
            }
            else
            {
                if (StreamingAssetsVersion != null)
                {
                    localAssetHash = StreamingAssetsVersion.AssetHashInfos.Find(x => x.Addressables.Contains(assetPath));
                    if (localAssetHash == null)
                    {
                        if (LocalVersion != null)
                        {
                            localAssetHash = LocalVersion.AssetHashInfos.Find(x => x.Addressables.Contains(assetPath));
                        }
                        else
                        {
                            assetBundlePath = Path.Combine(LocalPath, localAssetHash.Name);
                        }
                    }
                    else
                    {
                        assetBundlePath = Path.Combine(Application.streamingAssetsPath, localAssetHash.Name);
                    }
                }
            }

            if (!string.IsNullOrEmpty(assetBundlePath))
            {
                assetBundlePath = assetBundlePath.Replace("\\","/");
            }

            return assetBundlePath;
        }

        internal string BundleNameTransformBundlePath(string bundleName)
        {
            AssetHashInfo remoteAssetHash = null;
            AssetHashInfo localAssetHash = null;
            string assetBundlePath = null;
            if (RemoteVersion != null)
            {
                remoteAssetHash = RemoteVersion.AssetHashInfos.Find(x => x.Name.Contains(bundleName));
            }

            if (remoteAssetHash != null)
            {
                //找到本地对应的assetBundle
                if (StreamingAssetsVersion != null)
                {
                    localAssetHash = StreamingAssetsVersion.AssetHashInfos.Find(x => x.Equals(remoteAssetHash));
                    if (localAssetHash == null)
                    {
                        if (LocalVersion != null)
                        {
                            localAssetHash = LocalVersion.AssetHashInfos.Find(x => x.Equals(remoteAssetHash));
                        }
                        else
                        {
                            assetBundlePath = Path.Combine(LocalPath, localAssetHash.Name);
                        }
                    }
                    else
                    {
                        assetBundlePath = Path.Combine(Application.streamingAssetsPath, localAssetHash.Name);
                    }
                }

                //本地没有下载返回整个url
                if (localAssetHash == null)
                {
                    assetBundlePath = Path.Combine(GetRemoteURL(), localAssetHash.Name);
                }
            }
            else
            {
                if (StreamingAssetsVersion != null)
                {
                    localAssetHash = StreamingAssetsVersion.AssetHashInfos.Find(x => x.Name.Contains(bundleName));
                    if (localAssetHash == null)
                    {
                        if (LocalVersion != null)
                        {
                            localAssetHash = LocalVersion.AssetHashInfos.Find(x => x.Name.Contains(bundleName));
                        }
                        else
                        {
                            assetBundlePath = Path.Combine(LocalPath, localAssetHash.Name);
                        }
                    }
                    else
                    {
                        assetBundlePath = Path.Combine(Application.streamingAssetsPath, localAssetHash.Name);
                    }
                }
            }

            if (!string.IsNullOrEmpty(assetBundlePath))
            {
                assetBundlePath = assetBundlePath.Replace("\\", "/");
            }

            return assetBundlePath;
        }

        internal string GetManifest()
        {
            if (RemoteVersion != null)
            {
                return RemoteVersion.Manifest;
            }
            if (LocalVersion != null)
            {
                return LocalVersion.Manifest;
            }
            if(StreamingAssetsVersion!=null)
            {
                return StreamingAssetsVersion.Manifest;
            }

            return null;
        }

        private void OnAssetVersionRequestComplete(HashSet<UnityWebRequestAsyncOperation> asyncOperations , Action<bool,string> updateCallback)
        {
            if (asyncOperations != null)
            {
                if (m_remoteUpdatePath==null || m_remoteUpdatePath.Count <= m_remoteUpdatePathActive)
                {
                    updateCallback?.Invoke(true, null);
                }
                else
                {
                    if (asyncOperations.Count == 2)
                    {
                        ForceUpdateAssets = new HashSet<AssetHashInfo>();
                        DefaultUpdateAssets = new HashSet<AssetHashInfo>();

                        //必须有远程版本，检查资源是否有更新
                        if (RemoteVersion != null)
                        {
                            foreach (var item in RemoteVersion.AssetHashInfos)
                            {
                                if ((StreamingAssetsVersion != null && StreamingAssetsVersion.AssetHashInfos.Contains(item))
                                    || (LocalVersion != null && LocalVersion.AssetHashInfos.Contains(item)))
                                {
                                    continue;
                                }

                                //string assetURL = Path.Combine(GetRemoteURL(), item.Name).Replace("\\", "/");
                                //强制更新
                                if (item.UpdateTag.Equals("force"))
                                {
                                    ForceUpdateAssets.Add(item);
                                }
                                //默认更新
                                else if (item.UpdateTag.Equals("default"))
                                {
                                    DefaultUpdateAssets.Add(item);

                                }
                            }

                            //返回更新结果
                            updateCallback?.Invoke(ForceUpdateAssets.Count>0|| DefaultUpdateAssets.Count>0,null);
                        }
                        else
                        {
                            updateCallback?.Invoke(false, "Remote version is null!");
                        }

                    }
                }
            }
        }


        //获取远程激活的URL
        private string GetRemoteURL()
        {
            if (string.IsNullOrEmpty(RemoteURL))
            {
                if (m_remoteUpdatePath != null && m_remoteUpdatePath.Count > m_remoteUpdatePathActive)
                {
                    RemoteURL = Path.Combine(m_remoteUpdatePath[m_remoteUpdatePathActive], GetPlatform());
                }
            }
            return RemoteURL;
        }

        private string GetPlatform()
        {
            string platform = Application.platform.ToString().ToLower();
            if (platform.Contains("windows"))
            {
                platform = "windows";
            }
            else if (platform.Contains("osx"))
            {
                platform = "osx";
            }
            else if (platform.Contains("linux"))
            {
                platform = "linux";

            }
            return platform;
        }

        //        /// <summary>
        //        /// 检查资源
        //        /// </summary>
        //        /// <returns></returns>
        //        public async void CheckResource()
        //        {
        //            //var local = await RequestLocalVersion();
        //            //var remote = await RequestRemoteVersion();
        //            //bool result = false;
        //            //if (LocalVersion == null || !LocalVersion.Equals(RemoteVersion))
        //            //    result = true;
        //            ////检查资源
        //            //callback?.Invoke(result, local, remote);
        //        }

        //        /// <summary>
        //        /// 检查资源
        //        /// </summary>
        //        /// <returns></returns>
        //        public bool CheckResource()
        //        //{
        //        //    if (RemoteVersion == null)
        //        //    {
        //        //        throw new GameException("Request remote version information first!");
        //        //    }
        //        //    if (LocalVersion == null || !LocalVersion.Equals(RemoteVersion))
        //        //        return true;

        //            return false;
        //        }

        //        /// <summary>
        //        /// 更新资源
        //        /// </summary>
        //        /// <param name="callback">下载回调[进度(0-1)，大小(KB),速度(KB/S),剩余时间(s)]</param>
        //        /// <param name="downloadComplete">下载完成</param>
        //        /// <param name="errorCallback">下载错误</param>
        //        /// <param name="name">是否是更新单个资源,需要更新则传单个文件的名称</param>
        //        /// <returns></returns>
        //        public bool UpdateResource(Action<float, double, double, float> callback, Action downloadComplete, Action<string, string> errorCallback,string name=null)
        //        {
        //            //if (_updating)
        //            //    return false;

        //            //if (string.IsNullOrEmpty(name) && !CheckResource())
        //            //{
        //            //    return false;
        //            //}

        //            //_updating = true;
        //            ////整理下载资源
        //            // CollateDownloadResources((needDownloadFiles)=> {
        //            //     DownloadFiles(needDownloadFiles, callback, downloadComplete, errorCallback);
        //            // }, name);
        //            return true;
        //        }

        //        /// <summary>
        //        /// 检查某一个资源是否需要更新
        //        /// </summary>
        //        /// <param name="name"></param>
        //        /// <param name="callback">[本地是否存在文件，是否需要更新]</param>
        //        public async void CheckResource(string name, Action<bool,bool> callback)
        //        {

        ////#if UNITY_EDITOR
        ////            if (GameFrameworkMode.GetModule<ResourceManager>().ResUpdateType == ResourceUpdateType.Editor)
        ////            {
        ////                callback?.Invoke(true,false);
        ////                return;
        ////            }
        ////#endif
        ////            name = name.ToLower();
        ////            var ahif = RemoteVersion.AssetHashInfos.Find(x => x.Name.Equals(name));
        ////            if (ahif != null)
        ////            {
        ////                string md5 = await CheckFileMD5(ahif.Name);
        ////                callback?.Invoke(!string.IsNullOrEmpty(md5),!ahif.Hash.Equals(md5));
        ////            }
        ////            else
        ////            {
        ////                callback?.Invoke(false, true);
        ////                //throw new GameException($"There is no corresponding resource on the resource server: {name}");
        ////            }
        //        }

        //        #region 事件回调

        //        #endregion

        //        #region 内部函数
        //        /// <summary>
        //        /// 更新本地版本信息
        //        /// </summary>
        //        private void UpdateLocalVersion()
        //        {
        //            //if (CheckResource())
        //            //{
        //            //    LocalVersion = RemoteVersion;

        //            //    string versionAssetPath = Path.Combine(_localResourcePath, _assetVersionTxt);
        //            //    string content = JsonUtility.ToJson(LocalVersion).ToEncrypt();
        //            //    if (File.Exists(versionAssetPath))
        //            //    {
        //            //        File.Delete(versionAssetPath);
        //            //    }
        //            //    File.WriteAllText(versionAssetPath, content);
        //            //}
        //        }

        //        /// <summary>
        //        /// 整理下载资源
        //        /// </summary>
        //        /// <param name="name">资源名称</param>
        //        private async void CollateDownloadResources(Action<List<AssetHashInfo>> needDownloadCallback, string name = null)
        //        {
        //            List<AssetHashInfo> needDownloadFiles = new List<AssetHashInfo>();
        //            if (string.IsNullOrEmpty(name))
        //            {
        //                //遍历需要下载的文件
        //                foreach (var item in RemoteVersion.AssetHashInfos)
        //                {
        //                    if (!item.ForceUpdate)
        //                        continue;
        //                    string md5 = await CheckFileMD5(item.Name);
        //                    if (!item.Hash.Equals(md5))
        //                    {
        //                        needDownloadFiles.Add(item);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                name = name.ToLower();
        //                var ahif = RemoteVersion.AssetHashInfos.Find(x => x.Name.Equals(name));
        //                if (ahif != null)
        //                {
        //                    string md5 = await CheckFileMD5(ahif.Name);
        //                    if (!ahif.Hash.Equals(md5))
        //                    {
        //                        needDownloadFiles.Add(ahif);
        //                    }
        //                }
        //            }
        //            //需要下载的文件大小
        //            needDownloadCallback?.Invoke(needDownloadFiles);
        //        }

        //        /// <summary>
        //        /// 下载资源
        //        /// </summary>
        //        /// <param name="callback">[进度(0-1)，大小(KB),速度(KB/S),剩余时间(s)]</param>
        //        /// <param name="errorCallback">[文件路径，错误]</param>
        //        private void DownloadFiles(List<AssetHashInfo> needDownloadFiles,Action<float, double, double, float> callback, Action downloadComplete, Action<string, string> errorCallback)
        //        {
        //            //下载资源
        //            if (needDownloadFiles.Count > 0)
        //            {
        //                int downloadFileCount = 0;
        //                double totleFileSize = 0;
        //                Dictionary<string, AssetHashInfo> downloadFiles = new Dictionary<string, AssetHashInfo>();
        //                foreach (var item in needDownloadFiles)
        //                {
        //                    string remoteUrl = Path.Combine(_remoteUpdatePath, item.Name);
        //                    string localPath = Path.Combine(_localResourcePath, $"{item.Name}.download");
        //                    _webRequest.FileDownloader.AddDownloadFile(remoteUrl, localPath);
        //                    //整理文件大小
        //                    totleFileSize += item.Size;
        //                    downloadFiles.Add(localPath, item);
        //                }
        //                //下载文件
        //                _webRequest.FileDownloader.StartDownload((localPath, size, time, speed) =>
        //                {
        //                    float progress = Mathf.Clamp01((float)(size / totleFileSize));
        //                    float remainingTime = (float)((totleFileSize - size) / speed);
        //                    callback?.Invoke(progress, totleFileSize, speed, remainingTime);
        //                }, async (localPath) =>
        //                {
        //                    //验证文件的完整性
        //                    string md5 = await FileUtility.GetFileMD5(localPath);
        //                    var assetHashInfo = downloadFiles[localPath];
        //                    if (assetHashInfo.Hash.Equals(md5))
        //                    {
        //                        int index = localPath.LastIndexOf('.');
        //                        string targetPath = localPath.Substring(0, index);
        //                        if (File.Exists(targetPath))
        //                        {
        //                            File.Delete(targetPath);
        //                        }
        //                        File.Move(localPath, targetPath);
        //                        downloadFiles.Remove(localPath);
        //                        downloadFileCount++;
        //                        //下载完成
        //                        if (downloadFiles.Count == 0)
        //                        {
        //                            //更新本地资源
        //                            if (downloadFileCount == needDownloadFiles.Count)
        //                            {
        //                                if (CheckResource())
        //                                {
        //                                    UpdateLocalVersion();
        //                                }
        //                            }
        //                            needDownloadFiles = null;
        //                            _updating = false;
        //                            downloadComplete?.Invoke();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        File.Delete(localPath);
        //                        _webRequest.FileDownloader.StopDownload();
        //                        _updating = false;
        //                        //throw new GameException($"File integrity verification failed. {localPath}");
        //                        errorCallback?.Invoke(localPath, "File integrity verification failed.");

        //                    }
        //                }, (localPath, error) =>
        //                {
        //                    _webRequest.FileDownloader.StopDownload();
        //                    _updating = false;
        //                    errorCallback?.Invoke(localPath, error);
        //                });
        //            }
        //            else
        //            {
        //                //下载完成
        //                downloadComplete?.Invoke();
        //                _updating = false;
        //            }
        //        }

        //        //获取文件的md5
        //        private Task<string> CheckFileMD5(string fileName)
        //        {
        //            string localFilePath = Path.Combine(_localResourcePath, fileName);
        //            return FileUtility.GetFileMD5(localFilePath);
        //        }
        //        #endregion

        //	}
    }
}
