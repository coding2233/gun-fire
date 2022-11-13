//using System.Collections.Generic;
//using UnityEditor;
//using UnityEditor.AddressableAssets.Settings;
//using UnityEngine;

//public class AASUtility : UnityEditor.Editor
//{
//    public static UnityEditor.AddressableAssets.Settings.AddressableAssetSettings GetSettings()
//    {
//        //アドレサブルアセットセッティング取得
//        var d = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.AddressableAssets.Settings.AddressableAssetSettings>(
//            "Assets/AddressableAssetsData/AddressableAssetSettings.asset"
//            );
//        return d;
//    }


//    public static UnityEditor.AddressableAssets.Settings.AddressableAssetGroup CreateGroup(string groupName, bool setAsDefaultGroup = false)
//    {
//        //アドレサブルアセットセッティング取得
//        var s = GetSettings();
//        //スキーマ生成
//        List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema> schema = new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>() {
//             ScriptableObject.CreateInstance<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>(),
//             ScriptableObject.CreateInstance<UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema>(),
//        };
//        //グループの作成
//        var f = s.groups.Find((g) => {
//            return g.name == groupName;
//        });
//        if (f == null)
//        {
//            f = s.CreateGroup(groupName, setAsDefaultGroup, false, true, schema);
//        }

//        return f;
//    }

//    public static AddressableAssetEntry AddAssetToGroup(string assetGuid, string groupName)
//    {
//        if (assetGuid.Equals(""))
//        {
//            Debug.Log($"assetGuid is empty, groupName: {groupName}");
//            return null;
//        }
//        var s = GetSettings();
//        var g = CreateGroup(groupName);
//        var entry = s.CreateOrMoveEntry(assetGuid, g);
//        return entry;
//    }
//    public static void SetLabelToAsset(List<string> assetGuidList, string label, bool flag)
//    {
//        var s = GetSettings();
//        //ラベルを追加するように呼んでおく。追加されていないと設定されない。
//        s.AddLabel(label);
//        List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry> assetList = new List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry>();
//        s.GetAllAssets(assetList, true);
//        foreach (var assetGuid in assetGuidList)
//        {
//            var asset = assetList.Find((a) => { return a.guid == assetGuid; });
//            if (asset != null)
//            {
//                asset.SetLabel(label, flag);
//            }
//        }
//    }
//    public static void RemoveAssetFromGroup(string assetGuid)
//    {
//        var s = GetSettings();
//        s.RemoveAssetEntry(assetGuid);
//    }

//    public static void RemoveAllGroups()
//    {
//        var s = GetSettings();
//        var list = s.groups;
//        List<AddressableAssetGroup> temp_list = new List<AddressableAssetGroup>();
//        for (int i = list.Count - 1; i >= 0; i--)
//        {
//            temp_list.Add(list[i]);
//        }
//        for (int i = temp_list.Count - 1; i >= 0; i--)
//        {
//            s.RemoveGroup(temp_list[i]);
//        }
//    }

//    //public static void AddGroup(string groupName, bool setAsDefaultGroup, bool readOnly, bool postEvent, List<AddressableAssetGroupSchema> schemasToCopy, params Type[] types)
//    //{
//    //    var s = GetSettings();
//    //    s.CreateGroup(groupName, setAsDefaultGroup,readOnly,postEvent,schemasToCopy,types);
//    //}

//    public static void BuildPlayerContent()
//    {
//        //System.Threading.Thread.Sleep(30000);
//        var d = GetSettings();
//        d.ActivePlayerDataBuilderIndex = 3;
//        //AddressableAssetSettings.CleanPlayerContent(d.ActivePlayerDataBuilder);
//        AddressableAssetSettings.BuildPlayerContent();
//    }

//    public static void CleanPlayerContent()
//    {
//        // var d = GetSettings();
//        // d.ActivePlayerDataBuilderIndex = 3;
//        //AddressableAssetSettings.CleanPlayerContent(d.ActivePlayerDataBuilder);
//        AddressableAssetSettings.CleanPlayerContent();
//        UnityEditor.Build.Pipeline.Utilities.BuildCache.PurgeCache(false);
//        // AssetImportMgr.OnDataBuilderComplete();
//    }

//    [MenuItem("Tools/Test")]
//    static public void Test()
//    {
//        var d = GetSettings();

//        var matguid = UnityEditor.AssetDatabase.AssetPathToGUID("Assets/Data/hogeMat.mat");
//        AddAssetToGroup(matguid, "CreatedGroup");
//        ////List<string> assetGuidList = new List<string>() { matguid };
//        ////SetLabelToAsset(assetGuidList, "mat", true);
//        //CreateGroup("CreatedGroup");
//    }

    

//}