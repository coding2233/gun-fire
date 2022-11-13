//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MyAssetBundleProvider : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//    //MyAssetBundleProvider.cs

//    private void BeginOperation()
//    {
//        string path = m_ProvideHandle.Location.InternalId;
//        var url = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
//        string bundleName = Path.GetFileName(url);
//        // if a path starts with jar:file, it is an android embeded resource. The resource is a local file but cannot be accessed by 
//        // FileStream(called in LoadWithDataProc) directly
//        // Need to use webrequest's async call to get the content.
//        if (AssetBundleManager.GetInstance().buildInData != null && AssetBundleManager.GetInstance().buildInData.BuildInBundleNames.Contains(bundleName))//本地资源，内置包
//        {
//            string streamPath = UnityEngine.AddressableAssets.Addressables.RuntimePath + "/" + PlatformMappingService.GetPlatformPathSubFolder() + "/" + bundleName;
//            Debug.Log("LoadOne:" + streamPath);
//            var crc = m_Options == null ? 0 : m_Options.Crc;
//            CompleteBundleLoad(AssetBundle.LoadFromFile(streamPath));
//        }
//        else if (AssetBundleManager.GetInstance().IsCache(bundleName)) //已经下载过 缓存到本地的Bundle
//        {
//            string cachePath = Path.Combine(AssetBundleManager.GetInstance().GetBundleCachePath(), bundleName);
//            Debug.Log("LoadTwo:" + cachePath);
//            var crc = m_Options == null ? 0 : m_Options.Crc;
//            CompleteBundleLoad(AssetBundle.LoadFromFile(cachePath));
//        }
//        else if (ResourceManagerConfig.ShouldPathUseWebRequest(path)) //真正需要下载的Bundle
//        {
//            Debug.Log("DownloadThree：" + url);
//            var req = CreateWebRequest(m_ProvideHandle.Location);
//            req.disposeDownloadHandlerOnDispose = false;
//            m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(req);
//            if (m_WebRequestQueueOperation.IsDone)
//            {
//                m_RequestOperation = m_WebRequestQueueOperation.Result;
//                m_RequestOperation.completed += WebRequestOperationCompleted;
//            }
//            else
//            {
//                m_WebRequestQueueOperation.OnComplete += asyncOp =>
//                {
//                    m_RequestOperation = asyncOp;
//                    m_RequestOperation.completed += WebRequestOperationCompleted;
//                };
//            }
//        }
//        else
//        {
//            m_RequestOperation = null;
//            m_ProvideHandle.Complete<MyAssetBundleResource>(null, false, new Exception(string.Format("Invalid path in AssetBundleProvider: '{0}'.", path)));
//        }
//    }


//    private void WebRequestOperationCompleted(AsyncOperation op)
//    {
//        UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
//        var webReq = remoteReq.webRequest;
//        if (!UnityWebRequestUtilities.RequestHasErrors(webReq, out UnityWebRequestResult uwrResult))
//        {
//            m_downloadHandler = webReq.downloadHandler;
//            string path = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
//            string bundleName = Path.GetFileName(path);

//            AssetBundleManager.GetInstance().CacheBundle(bundleName, m_downloadHandler.data);//主要是在这里加了一个保存到本地的方法

//            if (!m_Completed)
//            {
//                m_ProvideHandle.Complete(this, true, null);
//                m_Completed = true;
//            }
//        }
//        else
//        {
//            m_downloadHandler = webReq.downloadHandler;
//            m_downloadHandler.Dispose();
//            m_downloadHandler = null;
//            string message = string.Format("Web request {0} failed with error '{1}', retrying ({2}/{3})...", webReq.url, webReq.error, m_Retries, m_Options.RetryCount);

//            if (m_Retries < m_Options.RetryCount)
//            {
//                Debug.LogFormat(message);
//                BeginOperation();
//                m_Retries++;
//            }
//            else
//            {
//                var exception = new Exception(string.Format(
//                    "RemoteAssetBundleProvider unable to load from url {0}, result='{1}'.", webReq.url,
//                    webReq.error));
//                m_ProvideHandle.Complete<MyAssetBundleResource>(null, false, exception);
//            }
//        }
//        webReq.Dispose();
//    }
//}//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MyAssetBundleProvider : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//    //MyAssetBundleProvider.cs

//    private void BeginOperation()
//    {
//        string path = m_ProvideHandle.Location.InternalId;
//        var url = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
//        string bundleName = Path.GetFileName(url);
//        // if a path starts with jar:file, it is an android embeded resource. The resource is a local file but cannot be accessed by 
//        // FileStream(called in LoadWithDataProc) directly
//        // Need to use webrequest's async call to get the content.
//        if (AssetBundleManager.GetInstance().buildInData != null && AssetBundleManager.GetInstance().buildInData.BuildInBundleNames.Contains(bundleName))//本地资源，内置包
//        {
//            string streamPath = UnityEngine.AddressableAssets.Addressables.RuntimePath + "/" + PlatformMappingService.GetPlatformPathSubFolder() + "/" + bundleName;
//            Debug.Log("LoadOne:" + streamPath);
//            var crc = m_Options == null ? 0 : m_Options.Crc;
//            CompleteBundleLoad(AssetBundle.LoadFromFile(streamPath));
//        }
//        else if (AssetBundleManager.GetInstance().IsCache(bundleName)) //已经下载过 缓存到本地的Bundle
//        {
//            string cachePath = Path.Combine(AssetBundleManager.GetInstance().GetBundleCachePath(), bundleName);
//            Debug.Log("LoadTwo:" + cachePath);
//            var crc = m_Options == null ? 0 : m_Options.Crc;
//            CompleteBundleLoad(AssetBundle.LoadFromFile(cachePath));
//        }
//        else if (ResourceManagerConfig.ShouldPathUseWebRequest(path)) //真正需要下载的Bundle
//        {
//            Debug.Log("DownloadThree：" + url);
//            var req = CreateWebRequest(m_ProvideHandle.Location);
//            req.disposeDownloadHandlerOnDispose = false;
//            m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(req);
//            if (m_WebRequestQueueOperation.IsDone)
//            {
//                m_RequestOperation = m_WebRequestQueueOperation.Result;
//                m_RequestOperation.completed += WebRequestOperationCompleted;
//            }
//            else
//            {
//                m_WebRequestQueueOperation.OnComplete += asyncOp =>
//                {
//                    m_RequestOperation = asyncOp;
//                    m_RequestOperation.completed += WebRequestOperationCompleted;
//                };
//            }
//        }
//        else
//        {
//            m_RequestOperation = null;
//            m_ProvideHandle.Complete<MyAssetBundleResource>(null, false, new Exception(string.Format("Invalid path in AssetBundleProvider: '{0}'.", path)));
//        }
//    }


//    private void WebRequestOperationCompleted(AsyncOperation op)
//    {
//        UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
//        var webReq = remoteReq.webRequest;
//        if (!UnityWebRequestUtilities.RequestHasErrors(webReq, out UnityWebRequestResult uwrResult))
//        {
//            m_downloadHandler = webReq.downloadHandler;
//            string path = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
//            string bundleName = Path.GetFileName(path);

//            AssetBundleManager.GetInstance().CacheBundle(bundleName, m_downloadHandler.data);//主要是在这里加了一个保存到本地的方法

//            if (!m_Completed)
//            {
//                m_ProvideHandle.Complete(this, true, null);
//                m_Completed = true;
//            }
//        }
//        else
//        {
//            m_downloadHandler = webReq.downloadHandler;
//            m_downloadHandler.Dispose();
//            m_downloadHandler = null;
//            string message = string.Format("Web request {0} failed with error '{1}', retrying ({2}/{3})...", webReq.url, webReq.error, m_Retries, m_Options.RetryCount);

//            if (m_Retries < m_Options.RetryCount)
//            {
//                Debug.LogFormat(message);
//                BeginOperation();
//                m_Retries++;
//            }
//            else
//            {
//                var exception = new Exception(string.Format(
//                    "RemoteAssetBundleProvider unable to load from url {0}, result='{1}'.", webReq.url,
//                    webReq.error));
//                m_ProvideHandle.Complete<MyAssetBundleResource>(null, false, exception);
//            }
//        }
//        webReq.Dispose();
//    }
//}
