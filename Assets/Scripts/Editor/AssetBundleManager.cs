using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class BuildInBundleData
{
    public List<string> BuildInBundleNames = new List<string>();
}

public class AssetBundleManager
{
    private static AssetBundleManager _instance = null;
    private string cachePath = "";
    public BuildInBundleData buildInData;

    public static AssetBundleManager GetInstance()
    {
        if (_instance == null)
            _instance = new AssetBundleManager();

        return _instance;
    }

    AssetBundleManager()
    {
        cachePath = Path.Combine(Application.persistentDataPath, "ab");
        if (!Directory.Exists(cachePath))
        {
            Directory.CreateDirectory(cachePath);
        }
    }

    public void Init()
    {
#if !UNITY_EDITOR
            var json = Resources.Load<TextAsset>("Version/BuildInBundleName");
            buildInData = JsonUtility.FromJson<BuildInBundleData>(json.text);
#endif
    }

    public String GetBundleCachePath()
    {
        return cachePath;
    }

    public bool IsCache(string bundleName)
    {
        string filePath = Path.Combine(AssetBundleManager.GetInstance().GetBundleCachePath(), bundleName);
        return File.Exists(filePath);
    }

    public void CacheBundle(string bundlename, byte[] bytes)
    {
        string filePath = Path.Combine(GetBundleCachePath(), bundlename);
        if (File.Exists(filePath))
            File.Delete(filePath);

        File.WriteAllBytes(filePath, bytes);

        string realName = GetRealBundleName(bundlename);
        string oldPath = PlayerPrefs.GetString(realName, "");
        if (oldPath != "")
            File.Delete(oldPath);

        PlayerPrefs.SetString(realName, filePath);
    }

    public string GetRealBundleName(string bundlename)
    {
        if (string.IsNullOrEmpty(bundlename))
            return "";

        int index = bundlename.LastIndexOf("_");
        return bundlename.Substring(0, index);
    }
}