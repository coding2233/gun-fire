//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using UnityEditor.AddressableAssets.Settings.GroupSchemas;
//using UnityEditor.AddressableAssets.Settings;
//using UnityEngine.ResourceManagement.ResourceProviders;

//public class AddressableTool
//{
//    private static string PackPath = "Assets";// Config.PackPath; //图集碎片目录名字
//    private static string AssetDataPath = "Assets";// Config.AssetDataPath; //打包资源的目录名字
//    private static Dictionary<string, bool> originGroups;
//    private static Dictionary<string, AddressableAssetGroup> originGroupMaps;

//    //严禁一级目录下有需要打包文件，全用二级目录做好分配
//    public static void MarkAssets()
//    {
//        //AASUtility.RemoveAllGroups(); 不要移除所有的组 为改变guid 导致每次的bundle的hash都不一样

//        //记录原来的组
//        originGroups = new Dictionary<string, bool>();
//        originGroupMaps = new Dictionary<string, AddressableAssetGroup>();
//        var setting = AASUtility.GetSettings();
//        var groups = setting.groups;
//        if (groups != null)
//        {
//            foreach (var g in groups)
//            {
//                originGroups.Add(g.Name, false);
//                originGroupMaps.Add(g.Name, g);
//            }
//        }

//        string assetPath = Path.Combine(Application.dataPath, AssetDataPath);
//        DirectoryInfo[] levelsDataDirs = new DirectoryInfo(assetPath).GetDirectories();
//        foreach (var dir in levelsDataDirs)
//        {
//            //场景目录
//            MarkLevelDataDir(dir);
//        }

//        //对比移除无用的组
//        foreach (var iter in originGroups)
//        {
//            if (!iter.Value && originGroupMaps.ContainsKey(iter.Key))
//            {
//                setting.RemoveGroup(originGroupMaps[iter.Key]);
//            }
//        }

//        AssetDatabase.Refresh();
//    }

//    private static void MarkLevelDataDir(DirectoryInfo dir)
//    {
//        string parentName = dir.Name;
//        DirectoryInfo[] dirs = dir.GetDirectories();
//        foreach (var d in dirs)
//        {
//            //场景资源分类目录 一个打一个组
//            MarkDir(d, parentName);
//        }
//    }

//    private static void MarkDir(DirectoryInfo dir, string parentName)
//    {
//        if (dir.Name == PackPath) return;
//        bool isTipLabel = false;
//        if (parentName == "Src")//Lua脚本
//        {
//            isTipLabel = true;
//        }
//        string groudName = string.Format("{0}.{1}", parentName, dir.Name);
//        AASUtility.CreateGroup(groudName);
//        if (originGroups.ContainsKey(groudName))
//        {
//            originGroups[groudName] = true;
//        }

//        List<string> allFiles = new List<string>();
//        List<string> allFileGuids = new List<string>();
//        GetMarkFiles(allFiles, dir);
//        foreach (var fileStr in allFiles)
//        {
//            string uFilePath = fileStr;// UtilityTool.ChangePath(fileStr);
//            string guid = AssetDatabase.AssetPathToGUID(uFilePath);
//            var entry = AASUtility.AddAssetToGroup(guid, groudName);
//            //缩减 可寻址的长度 lua脚本的同步require的路径
//            entry.address = uFilePath.Replace("Assets/" + "LevelsData" + "/", "").Replace("Src" + "/", "");
//            allFileGuids.Add(guid);
//        }

//        if (isTipLabel)
//        {
//            AASUtility.SetLabelToAsset(allFileGuids, groudName, true);//脚本设置lable 用于预加载
//        }

//        AASUtility.SetLabelToAsset(allFileGuids, "default", true);//所有资源都设为default，以前是拿来检测更新的，现在好像没用了
//    }

//    private static void GetMarkFiles(List<string> list, DirectoryInfo dir)
//    {
//        if (dir.Name == PackPath) return;

//        FileInfo[] files = dir.GetFiles();
//        foreach (var file in files)
//        {
//            if (file.FullName.EndsWith(".meta")) continue;
//            list.Add(file.FullName);
//        }

//        DirectoryInfo[] dirs = dir.GetDirectories();
//        foreach (var d in dirs)
//        {
//            GetMarkFiles(list, d);
//        }
//    }
//}