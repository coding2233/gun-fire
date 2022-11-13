//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.ResourceManagement.ResourceProviders;
//using UnityEngine.ResourceManagement.Util;

//namespace Wanderer
//{
//    [System.ComponentModel.DisplayName("Custom Assets from Bundles Provider")]
//    public class CustomBundledAssetProvider : ResourceProviderBase
//    {
//        internal class InternalOp
//        {
//            AssetBundle m_AssetBundle;
//            AssetBundleRequest m_PreloadRequest;
//            AssetBundleRequest m_RequestOperation;
//            object m_Result;
//            ProvideHandle m_ProvideHandle;
//            string subObjectName = null;

//            internal static T LoadBundleFromDependecies<T>(IList<object> results) where T : class, IAssetBundleResource
//            {
//                if (results == null || results.Count == 0)
//                    return default(T);

//                IAssetBundleResource bundle = null;
//                bool firstBundleWrapper = true;
//                for (int i = 0; i < results.Count; i++)
//                {
//                    var abWrapper = results[i] as IAssetBundleResource;
//                    if (abWrapper != null)
//                    {
//                        //only use the first asset bundle, even if it is invalid
//                        abWrapper.GetAssetBundle();
//                        if (firstBundleWrapper)
//                            bundle = abWrapper;
//                        firstBundleWrapper = false;
//                    }
//                }

//                return bundle as T;
//            }

//            public void Start(ProvideHandle provideHandle)
//            {
//                provideHandle.SetProgressCallback(ProgressCallback);
//                provideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
//                subObjectName = null;
//                m_ProvideHandle = provideHandle;
//                m_RequestOperation = null;
//                List<object> deps = new List<object>(); // TODO: garbage. need to pass actual count and reuse the list
//                m_ProvideHandle.GetDependencies(deps);
//                var bundleResource = LoadBundleFromDependecies<IAssetBundleResource>(deps);
//                if (bundleResource == null)
//                {
//                    m_ProvideHandle.Complete<AssetBundle>(null, false, new System.Exception("Unable to load dependent bundle from location " + m_ProvideHandle.Location));
//                }
//                else
//                {
//                    m_AssetBundle = bundleResource.GetAssetBundle();
//                    if (m_AssetBundle == null)
//                    {
//                        m_ProvideHandle.Complete<AssetBundle>(null, false, new System.Exception("Unable to load dependent bundle from location " + m_ProvideHandle.Location));
//                        return;
//                    }

//                    var assetBundleResource = bundleResource as CustomAssetBundleResource;
//                    //if (assetBundleResource != null)
//                    //    m_PreloadRequest = assetBundleResource.GetAssetPreloadRequest();
//                    if (m_PreloadRequest == null || m_PreloadRequest.isDone)
//                        BeginAssetLoad();
//                    else
//                        m_PreloadRequest.completed += operation => BeginAssetLoad();
//                }
//            }

//            private void BeginAssetLoad()
//            {
//                if (m_AssetBundle == null)
//                {
//                    m_ProvideHandle.Complete<AssetBundle>(null, false, new System.Exception("Unable to load dependent bundle from location " + m_ProvideHandle.Location));
//                }
//                else
//                {
//                    var assetPath = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
//                    if (m_ProvideHandle.Type.IsArray)
//                    {
//                        m_RequestOperation = m_AssetBundle.LoadAssetWithSubAssetsAsync(assetPath, m_ProvideHandle.Type.GetElementType());
//                    }
//                    else if (m_ProvideHandle.Type.IsGenericType && typeof(IList<>) == m_ProvideHandle.Type.GetGenericTypeDefinition())
//                    {
//                        m_RequestOperation = m_AssetBundle.LoadAssetWithSubAssetsAsync(assetPath, m_ProvideHandle.Type.GetGenericArguments()[0]);
//                    }
//                    else
//                    {
//                        if (ExtractKeyAndSubKey(assetPath, out string mainPath, out string subKey))
//                        {
//                            subObjectName = subKey;
//                            m_RequestOperation = m_AssetBundle.LoadAssetWithSubAssetsAsync(mainPath, m_ProvideHandle.Type);
//                        }
//                        else
//                        {
//                            m_RequestOperation = m_AssetBundle.LoadAssetAsync(assetPath, m_ProvideHandle.Type);
//                        }
//                    }

//                    if (m_RequestOperation != null)
//                    {
//                        if (m_RequestOperation.isDone)
//                            ActionComplete(m_RequestOperation);
//                        else
//                            m_RequestOperation.completed += ActionComplete;
//                    }
//                }
//            }

//            private bool WaitForCompletionHandler()
//            {
//                if (m_PreloadRequest != null && !m_PreloadRequest.isDone)
//                    return m_PreloadRequest.asset == null;
//                if (m_Result != null)
//                    return true;
//                if (m_RequestOperation == null)
//                    return false;
//                if (m_RequestOperation.isDone)
//                    return true;
//                return m_RequestOperation.asset != null;
//            }

//            private void ActionComplete(AsyncOperation obj)
//            {
//                if (m_RequestOperation != null)
//                {
//                    if (m_ProvideHandle.Type.IsArray)
//                        GetArrayResult(m_RequestOperation.allAssets);
//                    else if (m_ProvideHandle.Type.IsGenericType && typeof(IList<>) == m_ProvideHandle.Type.GetGenericTypeDefinition())
//                        GetListResult(m_RequestOperation.allAssets);
//                    else if (string.IsNullOrEmpty(subObjectName))
//                        GetAssetResult(m_RequestOperation.asset);
//                    else
//                        GetAssetSubObjectResult(m_RequestOperation.allAssets);
//                }
//                CompleteOperation();
//            }

//            private void GetArrayResult(Object[] allAssets)
//            {
//                m_Result = ResourceManagerConfig.CreateArrayResult(m_ProvideHandle.Type, allAssets);
//            }

//            private void GetListResult(Object[] allAssets)
//            {
//                m_Result = ResourceManagerConfig.CreateListResult(m_ProvideHandle.Type, allAssets);
//            }

//            private void GetAssetResult(Object asset)
//            {
//                m_Result = (asset != null && m_ProvideHandle.Type.IsAssignableFrom(asset.GetType())) ? asset : null;
//            }

//            private void GetAssetSubObjectResult(Object[] allAssets)
//            {
//                foreach (var o in allAssets)
//                {
//                    if (o.name == subObjectName)
//                    {
//                        if (m_ProvideHandle.Type.IsAssignableFrom(o.GetType()))
//                        {
//                            m_Result = o;
//                            break;
//                        }
//                    }
//                }
//            }

//            void CompleteOperation()
//            {
//                System.Exception e = m_Result == null
//                    ? new System.Exception($"Unable to load asset of type {m_ProvideHandle.Type} from location {m_ProvideHandle.Location}.")
//                    : null;
//                m_ProvideHandle.Complete(m_Result, m_Result != null, e);
//            }

//            public float ProgressCallback() { return m_RequestOperation != null ? m_RequestOperation.progress : 0.0f; }


//            /// <summary>
//            /// Extracts main and subobject keys if properly formatted
//            /// </summary>
//            /// <param name="keyObj">The key as an object.</param>
//            /// <param name="mainKey">The key of the main asset.  This will be set to null if a sub key is not found.</param>
//            /// <param name="subKey">The key of the sub object.  This will be set to null if not found.</param>
//            /// <returns></returns>
//            bool ExtractKeyAndSubKey(object keyObj, out string mainKey, out string subKey)
//            {
//                var key = keyObj as string;
//                if (key != null)
//                {
//                    var i = key.IndexOf('[');
//                    if (i > 0)
//                    {
//                        var j = key.LastIndexOf(']');
//                        if (j > i)
//                        {
//                            mainKey = key.Substring(0, i);
//                            subKey = key.Substring(i + 1, j - (i + 1));
//                            return true;
//                        }
//                    }
//                }
//                mainKey = null;
//                subKey = null;
//                return false;
//            }
//        }

//        public override void Provide(ProvideHandle provideHandle)
//        {
//            new InternalOp().Start(provideHandle);
//        }
//    }
//}