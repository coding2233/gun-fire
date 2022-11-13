//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using UnityEditor.AddressableAssets.Build;
//using UnityEditor.AddressableAssets.Build.DataBuilders;
//using UnityEditor.AddressableAssets.Settings;
//using UnityEditor.AddressableAssets.Settings.GroupSchemas;
//using UnityEditor.Build.Pipeline.Interfaces;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.AddressableAssets.Initialization;
//using UnityEngine.AddressableAssets.ResourceLocators;
//using UnityEngine.ResourceManagement.ResourceProviders;

//[CreateAssetMenu(fileName = "MyBuildScriptPackedMode.asset", menuName = "Addressable Assets/Data Builders/MyBuildScriptPackedMode")]
//public class MyBuildScriptPackedMode : BuildScriptPackedMode
//{
//    public override string Name
//    {
//        get { return "My Test Build"; }//自定义Build的名字
//    }

//    protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
//    {
//        return base.BuildDataImplementation<TResult>(builderInput);
//    }

//    protected override string ProcessAllGroups(AddressableAssetsBuildContext aaContext)
//    {
//        return base.ProcessAllGroups(aaContext);
//    }


//    protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
//    {
//        return base.DoBuild<TResult>(builderInput, aaContext);
//    }

//    protected override string ProcessBundledAssetSchema(BundledAssetGroupSchema schema, AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
//    {
//        return base.ProcessBundledAssetSchema(schema, assetGroup, aaContext);
//    }

//    //protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
//    //{
//    //    TResult result = default(TResult);

//    //    var timer = new Stopwatch();
//    //    timer.Start();
//    //    InitializeBuildContext(builderInput, out AddressableAssetsBuildContext aaContext);

//    //    using (m_Log.ScopedStep(LogLevel.Info, "ProcessAllGroups"))
//    //    {
//    //        var errorString = ProcessAllGroups(aaContext);
//    //        if (!string.IsNullOrEmpty(errorString))
//    //            result = AddressableAssetBuildResult.CreateResult<TResult>(null, 0, errorString);
//    //    }

//    //    /////START 初始化buildInData
//    //    AssetBundleManager.GetInstance().buildInData = new BuildInBundleData();
//    //    var targetDir = Path.Combine(Application.dataPath, "Resources");
//    //    if (!Directory.Exists(targetDir))
//    //        Directory.CreateDirectory(targetDir);

//    //    targetDir = Path.Combine(targetDir, "Version");
//    //    if (!Directory.Exists(targetDir))
//    //        Directory.CreateDirectory(targetDir);

//    //    var targetPath = Path.Combine(targetDir, "BuildInBundleName.bytes");
//    //    if (File.Exists(targetPath))
//    //    {
//    //        File.Delete(targetPath);
//    //    }
//    //    /////END 初始化buildInData 


//    //    if (result == null)
//    //    {
//    //        result = DoBuild<TResult>(builderInput, aaContext);
//    //    }

//    //    if (result != null)
//    //        result.Duration = timer.Elapsed.TotalSeconds;

//    //    ////START 序列化保存本次打包的内置包列表
//    //    var BuildInJson = JsonUtility.ToJson(AssetBundleManager.GetInstance().buildInData);
//    //    File.WriteAllText(targetPath, BuildInJson);
//    //    ////END 序列化保存本次打包的内置包列表

//    //    return result;
//    //}

//    //public override void PostProcessBundles(AddressableAssetGroup assetGroup, List<string> buildBundles, List<string> outputBundles, IBundleBuildResults buildResult, ResourceManagerRuntimeData runtimeData, List<ContentCatalogDataEntry> locations, FileRegistry registry, Dictionary<string, ContentCatalogDataEntry> primaryKeyToCatalogEntry, Dictionary<string, string> bundleRenameMap, List<Action> postCatalogUpdateCallbacks)
//    //{
//    //    var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
//    //    if (schema == null)
//    //        return;

//    //    var path = schema.BuildPath.GetValue(assetGroup.Settings);
//    //    if (string.IsNullOrEmpty(path))
//    //        return;

//    //    for (int i = 0; i < buildBundles.Count; ++i)
//    //    {
//    //        if (primaryKeyToCatalogEntry.TryGetValue(buildBundles[i], out ContentCatalogDataEntry dataEntry))
//    //        {
//    //            var info = buildResult.BundleInfos[buildBundles[i]];
//    //            var requestOptions = new AssetBundleRequestOptions
//    //            {
//    //                Crc = schema.UseAssetBundleCrc ? info.Crc : 0,
//    //                UseCrcForCachedBundle = schema.UseAssetBundleCrcForCachedBundles,
//    //                UseUnityWebRequestForLocalBundles = schema.UseUnityWebRequestForLocalBundles,
//    //                Hash = schema.UseAssetBundleCache ? info.Hash.ToString() : "",
//    //                ChunkedTransfer = schema.ChunkedTransfer,
//    //                RedirectLimit = schema.RedirectLimit,
//    //                RetryCount = schema.RetryCount,
//    //                Timeout = schema.Timeout,
//    //                BundleName = Path.GetFileNameWithoutExtension(info.FileName),
//    //                BundleSize = GetFileSize(info.FileName),
//    //                ClearOtherCachedVersionsWhenLoaded = schema.AssetBundledCacheClearBehavior == BundledAssetGroupSchema.CacheClearBehavior.ClearWhenWhenNewVersionLoaded
//    //            };
//    //            dataEntry.Data = requestOptions;

//    //            int extensionLength = Path.GetExtension(outputBundles[i]).Length;
//    //            string[] deconstructedBundleName = outputBundles[i].Substring(0, outputBundles[i].Length - extensionLength).Split('_');
//    //            string reconstructedBundleName = string.Join("_", deconstructedBundleName, 1, deconstructedBundleName.Length - 1) + ".bundle";

//    //            outputBundles[i] = ConstructAssetBundleName(assetGroup, schema, info, reconstructedBundleName);
//    //            dataEntry.InternalId = dataEntry.InternalId.Remove(dataEntry.InternalId.Length - buildBundles[i].Length) + outputBundles[i];
//    //            dataEntry.Keys[0] = outputBundles[i];
//    //            ReplaceDependencyKeys(buildBundles[i], outputBundles[i], locations);

//    //            if (!m_BundleToInternalId.ContainsKey(buildBundles[i]))
//    //                m_BundleToInternalId.Add(buildBundles[i], dataEntry.InternalId);

//    //            if (dataEntry.InternalId.StartsWith("http:\\"))
//    //                dataEntry.InternalId = dataEntry.InternalId.Replace("http:\\", "http://").Replace("\\", "/");
//    //            if (dataEntry.InternalId.StartsWith("https:\\"))
//    //                dataEntry.InternalId = dataEntry.InternalId.Replace("https:\\", "https://").Replace("\\", "/");
//    //        }
//    //        else
//    //        {
//    //            UnityEngine.Debug.LogWarningFormat("Unable to find ContentCatalogDataEntry for bundle {0}.", outputBundles[i]);
//    //        }

//    //        UnityEngine.Debug.Log(outputBundles[i]);
//    //        if (!AssetBundleManager.GetInstance().buildInData.BuildInBundleNames.Contains(outputBundles[i]))
//    //        {
//    //            //添加打包的Bundle记录,内置Bundle
//    //            AssetBundleManager.GetInstance().buildInData.BuildInBundleNames.Add(outputBundles[i]);
//    //        }

//    //        var targetPath = Path.Combine(path, outputBundles[i]);
//    //        var srcPath = Path.Combine(assetGroup.Settings.buildSettings.bundleBuildPath, buildBundles[i]);
//    //        bundleRenameMap.Add(buildBundles[i], outputBundles[i]);
//    //        CopyFileWithTimestampIfDifferent(srcPath, targetPath, m_Log);

//    //        AddPostCatalogUpdatesInternal(assetGroup, postCatalogUpdateCallbacks, dataEntry, targetPath, registry);

//    //        //复制到Library 打包到包里面
//    //        string RuntimePath = UnityEngine.AddressableAssets.Addressables.RuntimePath;
//    //        string destPath = Path.Combine(System.Environment.CurrentDirectory, RuntimePath, PlatformMappingService.GetPlatformPathSubFolder().ToString(), outputBundles[i]);
//    //        if (!Directory.Exists(Path.GetDirectoryName(destPath)))
//    //            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
//    //        if (!File.Exists(destPath))
//    //        {
//    //            File.Copy(targetPath, destPath);
//    //        }

//    //        registry.AddFile(targetPath);
//    //    }
//    //}





//}