//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Net;
//using System.Net.NetworkInformation;
//using System.Resources;
//using UnityEngine;
//using UnityEngine.Analytics;
//using UnityEngine.Networking;
//using UnityEngine.ResourceManagement;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using UnityEngine.ResourceManagement.Exceptions;
//using UnityEngine.ResourceManagement.ResourceLocations;
//using UnityEngine.ResourceManagement.ResourceProviders;
//using UnityEngine.ResourceManagement.Util;

//namespace Wanderer
//{
//    [DisplayName("Custom Asset Bundle Privider")]
//    public class CustomAssetBundlePrivider : ResourceProviderBase
//    {
//        public override void Provide(ProvideHandle providerInterface)
//        {
//            Debug.Log("CustomAssetBundlePrivider providerInterface");

//            new CustomAssetBundleResource().Start(providerInterface);
//        }

//        public override Type GetDefaultType(IResourceLocation location)
//        {
//            return typeof(IAssetBundleResource);
//        }

//        public override void Release(IResourceLocation location, object asset)
//        {
//            Debug.Log($"CustomAssetBundlePrivider Release {location} {asset}");

//            if (asset is CustomAssetBundleResource cabr)
//            {
//                cabr.Unload();
//            }
//        }

//    }

//    internal class CustomAssetBundleResource : IAssetBundleResource, IUpdateReceiver
//    {

//        ProvideHandle m_providerInterface;
//        AssetBundle m_assetBundle;
//        private UnityEngine.AsyncOperation m_asyncOperationHandle;
//        private bool m_completed;
//        //internal AssetBundleRequestOptions m_options;
//        private string m_downloadLocalPath;
//        private long m_bytesToDownload;
//        private long m_downloadedBytes;
//        private Stream m_fileStream;

//        public void Start(ProvideHandle providerInterface)
//        {
//            m_bytesToDownload = -1;

//            m_providerInterface = providerInterface;

//            providerInterface.SetProgressCallback(PercentComplete);
//            providerInterface.SetDownloadProgressCallbacks(GetDownloadStatus);
//            providerInterface.SetWaitForCompletionCallback(WaitForCompletionHandler);
//            //m_options = m_providerInterface.Location.Data as AssetBundleRequestOptions;

//            BeginOperation();
//        }

//        public void Unload()
//        {
//            if (m_assetBundle != null)
//            {
//                m_assetBundle.Unload(true);
//                m_assetBundle = null;
//            }

//            m_asyncOperationHandle = null;

//            if (m_fileStream != null)
//            {
//                m_fileStream.Close();
//                m_fileStream.Dispose();
//                m_fileStream = null;
//            }
//            //以及其他操作 类似 System.IDisposable
//        }

//        public AssetBundle GetAssetBundle()
//        {
//            if (m_assetBundle == null)
//            {
//                if (m_asyncOperationHandle != null && m_asyncOperationHandle.isDone)
//                {
//                    OnAsyncOperationHandleCompleted(m_asyncOperationHandle);
//                }
//            }
//            return m_assetBundle;
//        }

//        public void Update(float unscaledDeltaTime)
//        {
//            Log.Info($"CustomAssetBundleResource Update {unscaledDeltaTime}");
//        }


//        private void BeginOperation()
//        {
//            IResourceLocation location = m_providerInterface.Location;
//            //string path = m_providerInterface.Location.InternalId;
//            string path = m_providerInterface.ResourceManager.TransformInternalId(location);
//            string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(path));
//            if (File.Exists(localPath))
//            {
//                path = localPath;
//            }
//            m_downloadLocalPath = localPath;

//            //Android以及StreamingAssets中
//            if (Application.platform == RuntimePlatform.Android && path.StartsWith("jar:"))
//            {
//                //UnityWebRequest 读取Android StreamingAssets中的文件
//                //注意解密
//                UnityWebRequest request = UnityWebRequest.Get(path);
//                request.downloadHandler = new DownloadHandlerBuffer();
//                m_asyncOperationHandle = request.SendWebRequest();
//            }
//            else if (File.Exists(path))
//            {
//                //解密
//                //FileStream fs = File.OpenRead(path);
//                //m_asyncOperationHandle = AssetBundle.LoadFromStreamAsync(fs);
//                //var abRequest = AssetBundle.LoadFromFileAsync(path);
//                //m_asyncOperationHandle = AssetBundle.LoadFromFileAsync(path);
//                ReadFromEncryptFileStream(path);
//            }
//            else
//            {
//                //网络文件下载后
//                UnityWebRequest request = UnityWebRequest.Get(path);
//                var downloadHandler = new DownloadHandlerFile(localPath, false);
//                //downloadHandler.removeFileOnAbort = true;

//                request.downloadHandler = downloadHandler;
//                m_asyncOperationHandle = request.SendWebRequest();

//                var sizeData = location.Data as ILocationSizeData;
//                if (sizeData != null)
//                {
//                    m_bytesToDownload = sizeData.ComputeSize(location, m_providerInterface.ResourceManager);
//                }

//                SetAsyncOperationHandleCompleted();
//                //使用AssetBundle.LoadFromStreamAsync解密
//                //AssetBundle.LoadFromStreamAsync()
//            }

//            Log.Info($"CustomAssetBundleResource download url: {path}");

//            //UnityWebRequest.Get
//        }

//        private void OnAsyncOperationHandleCompleted(UnityEngine.AsyncOperation obj)
//        {
//            Log.Info($"OnAsyncOperationHandleCompleted: {obj.GetType()}");
//            //var ab = GetAssetBundle();
//            //m_providerInterface.Complete(this, true, null);
//            obj.completed -= OnAsyncOperationHandleCompleted;

//            if (m_asyncOperationHandle is UnityWebRequestAsyncOperation requestAsyncOperation)
//            {
//                var webRequest = requestAsyncOperation.webRequest;
//                //文件下载
//                if (webRequest.downloadHandler is DownloadHandlerFile)
//                {
//                    //m_asyncOperationHandle = AssetBundle.LoadFromFileAsync(m_internalPath);
//                    ReadFromEncryptFileStream(m_downloadLocalPath);
//                }
//                //StreamingAssets中读取
//                //if (webRequest.downloadHandler is DownloadHandlerBuffer)
//                else
//                {
//                    //解密
//                    //加载ab包
//                    //m_asyncOperationHandle = AssetBundle.LoadFromMemoryAsync(webRequest.downloadHandler.data);
//                    ReadFromEncryptMemoryStream(webRequest.downloadHandler.data, m_downloadLocalPath);
//                }
//            }
//            else if (m_asyncOperationHandle is AssetBundleCreateRequest assetBundleHandle)
//            {
//                m_completed = true;
//                m_assetBundle = assetBundleHandle.assetBundle;
//                Log.Info("GetAssetBundle is null: {0}", m_assetBundle == null);
//                m_providerInterface.Complete(this, m_assetBundle != null, null);
//            }
//        }

//        float PercentComplete()
//        {
//            float progress = m_asyncOperationHandle == null ? 0.0f : m_asyncOperationHandle.progress;
//            Log.Info($"PercentComplete: {progress}");
//            return progress;
//        }

//        DownloadStatus GetDownloadStatus()
//        {
//            //if (m_Options == null)
//            //    return default;
//            //var status = new DownloadStatus() { TotalBytes = BytesToDownload, IsDone = PercentComplete() >= 1f };
//            //if (BytesToDownload > 0)
//            //{
//            //    if (m_WebRequestQueueOperation != null && string.IsNullOrEmpty(m_WebRequestQueueOperation.m_WebRequest.error))
//            //        m_DownloadedBytes = (long)(m_WebRequestQueueOperation.m_WebRequest.downloadedBytes);
//            //    else if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation operation && string.IsNullOrEmpty(operation.webRequest.error))
//            //        m_DownloadedBytes = (long)operation.webRequest.downloadedBytes;
//            //}

//            //status.DownloadedBytes = m_DownloadedBytes;
//            //return status;

//            //if (m_asyncOperationHandle == null || !(m_asyncOperationHandle is ))
//            //{
//            //    return default;
//            //}

//            var status = new DownloadStatus() { TotalBytes = m_bytesToDownload, IsDone = PercentComplete() >= 1f };
//            if (m_bytesToDownload > 0)
//            {
//                if (m_asyncOperationHandle != null)
//                {
//                    if (m_asyncOperationHandle is UnityWebRequestAsyncOperation uwrao)
//                    {
//                        status.DownloadedBytes = (long)uwrao.webRequest.downloadedBytes;
//                    }
//                }
//            }
//            return status;
//        }

//        private bool WaitForCompletionHandler()
//        {

//            //if (m_RequestOperation == null)
//            //{
//            //    if (m_WebRequestQueueOperation == null)
//            //        return false;
//            //    else
//            //        WebRequestQueue.WaitForRequestToBeActive(m_WebRequestQueueOperation, k_WaitForWebRequestMainThreadSleep);
//            //}

//            ////We don't want to wait for request op to complete if it's a LoadFromFileAsync. Only UWR will complete in a tight loop like this.
//            //if (m_RequestOperation is UnityWebRequestAsyncOperation op)
//            //{
//            //    while (!UnityWebRequestUtilities.IsAssetBundleDownloaded(op))
//            //        System.Threading.Thread.Sleep(k_WaitForWebRequestMainThreadSleep);
//            //}

//            //if (m_RequestOperation is UnityWebRequestAsyncOperation && !m_WebRequestCompletedCallbackCalled)
//            //{
//            //    WebRequestOperationCompleted(m_RequestOperation);
//            //    m_RequestOperation.completed -= WebRequestOperationCompleted;
//            //}

//            //var assetBundle = GetAssetBundle();
//            //if (!m_Completed && m_RequestOperation.isDone)
//            //{
//            //    m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
//            //    m_Completed = true;
//            //}

//            //return m_Completed;

//            if (m_completed)
//            {
//                return true;
//            }

//            if (m_asyncOperationHandle == null)
//                return false;

//            //if (m_asyncOperationHandle is UnityWebRequestAsyncOperation op)
//            //{
//            //    while (!op.isDone)
//            //    {
//            //        System.Threading.Thread.Sleep(1);
//            //    }
//            //}

//            if (m_asyncOperationHandle is AssetBundleCreateRequest op)
//            {
//                while (!op.isDone)
//                {
//                    System.Threading.Thread.Sleep(10);
//                }
//            }

//            Log.Info($"WaitForCompletionHandler: {m_asyncOperationHandle.progress}");


//            if (!m_completed && m_asyncOperationHandle.isDone)
//            {
//                var ab = GetAssetBundle();
//                m_providerInterface.Complete(this, m_assetBundle != null, null);

//                m_completed = true;
//            }

//            return m_completed;
//        }


//        private void ReadFromEncryptFileStream(string filePath)
//        {
//            m_fileStream = new EncryptFileStream(filePath, FileMode.Open);
//            m_asyncOperationHandle = AssetBundle.LoadFromStreamAsync(m_fileStream);
//            SetAsyncOperationHandleCompleted();

//        }

//        private void ReadFromEncryptMemoryStream(byte[] data, string filePath)
//        {
//            m_fileStream = new EncryptMemoryStream(data, filePath);
//            m_asyncOperationHandle = AssetBundle.LoadFromStreamAsync(m_fileStream);
//            SetAsyncOperationHandleCompleted();
//        }


//        private void SetAsyncOperationHandleCompleted()
//        {
//            if (m_asyncOperationHandle != null)
//            {
//                m_asyncOperationHandle.completed += OnAsyncOperationHandleCompleted;
//                if (m_asyncOperationHandle.isDone)
//                {
//                    OnAsyncOperationHandleCompleted(m_asyncOperationHandle);
//                }
//            }

//        }
//    }





//}
