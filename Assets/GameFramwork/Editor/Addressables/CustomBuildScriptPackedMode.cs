//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection.Metadata;
//using UnityEditor;
//using UnityEditor.AddressableAssets.Build;
//using UnityEditor.AddressableAssets.Build.DataBuilders;
//using UnityEditor.AddressableAssets.Settings;
//using UnityEngine;
//using Debug = UnityEngine.Debug;

//namespace Wanderer
//{
//    /// <summary>
//    /// Build scripts used for player builds and running with bundles in the editor.
//    /// </summary>
//    [CreateAssetMenu(fileName = "CustomBuildScriptPacked.asset", menuName = "Addressables/Content Builders/Custom Build Script")]
//    public class CustomBuildScriptPackedMode : BuildScriptPackedMode
//    {
//        public override string Name => "Custom Build Script";

//        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
//        {
//            var result = base.BuildDataImplementation<TResult>(builderInput);
//            var profileSettings = builderInput.AddressableSettings.profileSettings;
//            var remoteBuildPath = profileSettings.GetValueByName(builderInput.AddressableSettings.activeProfileId, "Remote.BuildPath");
//            var remoteLoadPath = profileSettings.GetValueByName(builderInput.AddressableSettings.activeProfileId, "Remote.LoadPath");
//            //var buildTargetName = profileSettings.GetValueByName(builderInput.AddressableSettings.activeProfileId, "BuildTarget");
//            //buildTargetName = buildTargetName.Substring(1, buildTargetName.Length - 2);
//            //int buildTargetTypeIndex = buildTargetName.LastIndexOf(".");
//            //string buildTargetType = buildTargetName.Substring(0, buildTargetTypeIndex);
//            //string filedName = buildTargetName.Substring(buildTargetTypeIndex+1, buildTargetName.Length - buildTargetTypeIndex-1);
//            //var tt = typeof(UnityEditor.EditorUserBuildSettings).Assembly.GetType(buildTargetType);
//            //var targetFiled = tt.GetField(filedName);

//            string buildTargetName = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
//            remoteBuildPath = remoteBuildPath.Replace("[BuildTarget]", buildTargetName);
//            remoteLoadPath = remoteLoadPath.Replace("[BuildTarget]", buildTargetName);
//            Debug.Log($"@资源打包完成等待处理. remoteBuildPath: {remoteBuildPath}  remoteLoadPath: {remoteLoadPath}");
//            if (Directory.Exists(remoteBuildPath))
//            {
//                byte[] encryptBuffer = new byte[4096];
//                foreach (var item in builderInput.Registry.GetFilePaths())
//                {
//                    string filePath = item.Replace("\\", "/");
//                    if (item.StartsWith("Library/") || item.StartsWith("Assets/"))
//                    {
//                        continue;
//                    }
                    
//                    if (item.Contains("catalog_"))
//                    {
//                        if (item.EndsWith(".hash") || item.EndsWith(".json"))
//                        {
//                            continue;
//                        }
//                    }

//                    if (File.Exists(filePath))
//                    {
//                        string originFilePath = $"{filePath}.origin";
//                        File.Copy(filePath, originFilePath,true);
//                        File.Delete(filePath);
//                        using (FileStream fileRead = File.OpenRead(originFilePath))
//                        {
//                            using (EncryptFileStream encryptFileStream = new EncryptFileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, encryptBuffer.Length, false))
//                            {
//                                int readSize = 1;
//                                while (readSize > 0)
//                                {
//                                    readSize = fileRead.Read(encryptBuffer, 0, encryptBuffer.Length);
//                                    encryptFileStream.Write(encryptBuffer, 0, readSize);
//                                }
                                
//                            }
//                        }
//                        File.Delete(originFilePath);
//                    }
//                }

//                Process.Start("scp", $"-r ./{remoteBuildPath}/ root@netlighting.cn:/usr/share/caddy/Gunfire/");
//            }
//            return result;
//        }


//        protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
//        {
//            string result = base.ProcessGroup(assetGroup, aaContext);

//            return result;
//        }
//    }
//}