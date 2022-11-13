using HybridCLR;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class StartResourceUpdateCommand : EventCommand
    {
        [Inject("UICanvasRoot")]
        public GameObject tfCanvasRoot { get; set; }

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        private CheckResView m_checkResView;

        [Inject]
        public IAssetsHelper assetHelper { get; set; }


        public override void Execute()
        {
            Log.Info("StartResourceUpdateCommand");
            var ruc = GameObject.Instantiate(assetHelper.LoadAsset<GameObject>("Assets/Game/ResourceUpdate/DefaultLoading.prefab"), tfCanvasRoot.transform);
            m_checkResView = ruc.AddComponent<CheckResView>();
            //资源下载
            assetHelper.UpdateDefaultAssets(m_checkResView.OnDownloadProgress, OnDownloadComplete, m_checkResView.OnDownloadError);
        }

        private void OnDownloadComplete()
        {
            //资源下载完成， 通知运行热更主逻辑
            Log.Info("资源下载完成");
            dispatcher.Dispatch("RunHotUpdate");

            //销毁热更ContextView
            GameObject.Destroy(m_checkResView.gameObject);
            GameObject.Destroy(contextView);
        }

        //private void LoadMetadata(Action onComplete)
        //{
        //    int loadAOTMetadataCount = 0;
        //    foreach (var dll in aotMetadataDlls)
        //    {
        //        string assetPath = $"Assets/Game/Metadata/{dll}.bytes";
        //        assetHelper.LoadAsset<TextAsset>(assetPath, (textAsset) => {
        //            LoadMetadataForAOTAssembly(dll, textAsset.bytes);
        //            loadAOTMetadataCount++;
        //            if (loadAOTMetadataCount == aotMetadataDlls.Length)
        //            {
        //                onComplete?.Invoke();
        //            }
        //        });
        //    }
        //}
        

        ///// <summary>
        ///// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        ///// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        ///// </summary>
        //private unsafe void LoadMetadataForAOTAssembly(string dllName, byte[] dllBytes)
        //{
        //    // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        //    // 我们在BuildProcessor_xxx里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
        //    // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        //    // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        //    fixed (byte* ptr = dllBytes)
        //    {
        //        // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
        //        LoadImageErrorCode err = (LoadImageErrorCode)RuntimeApi.LoadMetadataForAOTAssembly((IntPtr)ptr, dllBytes.Length);
        //        Debug.Log($"LoadMetadataForAOTAssembly:{dllName}. ret:{err}");
        //    }
        //}

    }
}
