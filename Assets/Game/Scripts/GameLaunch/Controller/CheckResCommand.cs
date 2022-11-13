using HybridCLR;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Wanderer
{
    public class CheckResCommand : EventCommand
    {
        [Inject(GameContextKeys.UICanvasRoot)]
        public GameObject tfCanvasRoot { get; set; }

        [Inject(GameContextKeys.HotUpdateRoot)]
        public GameObject tfHotUpdate { get; set; }

        [Inject]
        public IAssetsHelper assetHelper { get; set; }

        [Inject]
        public IXLuaHotFix xluaHotFix { get; set; }

        private GameObject m_defaultLoading;

        private string[] aotMetadataDlls = new string[] { "mscorlib.dll", "System.dll", "System.Core.dll" };

        public override void Execute()
        {
            Log.Info("CheckResCommand");

            Toast.Info("检查资源中...");
            //检查更新以及强制更新hotfix
            assetHelper.CheckUpdate((needUpdate) => {
#if HOTFIX_ENABLE
            //执行hotfix
            xluaHotFix.DoLuaFix();
            //测试热修复
            resource.TestPublicHotFix();
#endif
                //加载AOT Meta数据后再执行其他的
                LoadMetadata(() =>
                {
                    //销毁默认加载界面
                    GameObject.Destroy(tfCanvasRoot.transform.Find("DefaultLoading").gameObject);

                    if (needUpdate)
                    {
                        Toast.Info("准备资源更新....");
                        //资源更新
                        RunResourceUpdate();
                    }
                    else
                    {
                        Toast.Info("开始游戏");
                        //直接开始游戏逻辑
                        dispatcher.Dispatch(GameLaunchEvent.RunHotUpdate);
                    }

                });

              
            });
        }

        private void RunResourceUpdate()
        {
            Log.Info("RunResourceUpdate");

            assetHelper.LoadExtendAssembly("Assets/Game/ResourceUpdate/Resouce.HotUpdate.dll.bytes", (hotUpdateAssembly) => 
            {
                if (hotUpdateAssembly == null)
                {
                    Log.Error("hotUpdateAssembly is null!");
                }
                var hotUpdateContextView = hotUpdateAssembly.GetType("Wanderer.ResouceUpdateContextView");
                var resouceHotUpdate = new GameObject("Run Resouce.HotUpdate");
                resouceHotUpdate.transform.SetParent(tfHotUpdate.transform);
                resouceHotUpdate.AddComponent(hotUpdateContextView);
            });
           
        }

        private void LoadMetadata(Action onComplete)
        {
#if UNITY_EDITOR
            onComplete?.Invoke();
            return;
#endif
            int loadAOTMetadataCount = 0;
            foreach (var dll in aotMetadataDlls)
            {
                string assetPath = $"Assets/Game/Metadata/{dll}.bytes";
                assetHelper.LoadAsset<TextAsset>(assetPath, (textAsset) => {
                    LoadMetadataForAOTAssembly(dll, textAsset.bytes);
                    loadAOTMetadataCount++;
                    if (loadAOTMetadataCount == aotMetadataDlls.Length)
                    {
                        onComplete?.Invoke();
                    }
                });
            }
        }


        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private unsafe void LoadMetadataForAOTAssembly(string dllName, byte[] dllBytes)
        {
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessor_xxx里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            try
            {
                LoadImageErrorCode err = (LoadImageErrorCode)RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{dllName}. ret:{err}");
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            //fixed (byte* ptr = dllBytes)
            //{
                
            //}
        }


    }
}
