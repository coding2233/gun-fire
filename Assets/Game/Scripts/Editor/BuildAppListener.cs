#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using HybridCLR;
using System.IO;
using Wanderer;

public class BuildAppListener : IPostprocessBuildWithReport, IPreprocessBuildWithReport, IPostBuildPlayerScriptDLLs
{
    public int callbackOrder => 100;

    public void OnPostBuildPlayerScriptDLLs(BuildReport report)
    {
        //Debug.LogFormat("OnPostBuildPlayerScriptDLLs:{0}", report.name);

    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("OnPostprocessBuild:");
        //BuildTarget target = report.summary.platform;
        ////CompileDllHelper.CompileDll(target);
        //var hotfixDllDir = UtilityExt.Path.GetCombinePath(Application.dataPath, ConstBuiltin.HOT_FIX_DLL_DIR);
        //try
        //{
        //    if (!Directory.Exists(hotfixDllDir))
        //    {
        //        Directory.CreateDirectory(hotfixDllDir);
        //    }
        //    else
        //    {
        //        var dllFils = Directory.GetFiles(hotfixDllDir);
        //        for (int i = dllFils.Length - 1; i >= 0; i--)
        //        {
        //            File.Delete(dllFils[i]);
        //        }
        //    }
        //    CopyHotfixDllTo(target, hotfixDllDir);
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogErrorFormat("生成热更新dll文件失败:{0}", e.Message);
        //    throw;
        //}

    }

    public void OnPreprocessBuild(BuildReport report)
    {

        Debug.Log("OnPreprocessBuild:");
    }
    public static void CopyHotfixDllTo(BuildTarget target, string desDir, bool copyAotMeta = true)
    {
        //string hotfixDllSrcDir = BuildConfig.GetHotFixDllsOutputDirByTarget(target);
        //foreach (var dll in BuildConfig.AllHotUpdateDllNames)
        //{
        //    string dllPath = UtilityExt.Path.GetCombinePath(hotfixDllSrcDir, dll);
        //    if (File.Exists(dllPath))
        //    {
        //        string dllBytesPath = UtilityExt.Path.GetCombinePath(desDir, Utility.Text.Format("{0}.bytes", dll));
        //        File.Copy(dllPath, dllBytesPath, true);
        //    }
        //}
        //if (copyAotMeta)
        //{
        //    string aotDllDir = BuildConfig.GetAssembliesPostIl2CppStripDir(target);
        //    foreach (var dll in BuildConfig.AOTMetaDlls)
        //    {
        //        string dllPath = UtilityExt.Path.GetCombinePath(aotDllDir, dll);
        //        if (!File.Exists(dllPath))
        //        {
        //            Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
        //            continue;
        //        }
        //        string dllBytesPath = UtilityExt.Path.GetCombinePath(desDir, Utility.Text.Format("{0}.bytes", dll));
        //        File.Copy(dllPath, dllBytesPath, true);
        //    }
        //}
        //AssetDatabase.Refresh();
    }
}
#endif
