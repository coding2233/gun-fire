using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

public class BuildProcessorSetEnv 
{
    [InitializeOnLoadMethod]
    public static void Setup()
    {
        /// unity允许使用UNITY_IL2CPP_PATH环境变量指定il2cpp的位置，因此我们不再直接修改安装位置的il2cpp，
        Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", $"{Application.dataPath}/hybrid_clr_data/libil2cpp");
        
    }

    [MenuItem("Tools/Build")]
    public static void CompileWindow()
    {
        CompileDll("build/", BuildTarget.StandaloneWindows64);
    }

    public static void CompileDll(string buildDir, BuildTarget target)
    {
        var group = BuildPipeline.GetBuildTargetGroup(target);

        ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
        scriptCompilationSettings.group = group;
        scriptCompilationSettings.target = target;
        Directory.CreateDirectory(buildDir);
        ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
        foreach (var ass in scriptCompilationResult.assemblies)
        {
            Debug.LogFormat("compile assemblies:{1}/{0}", ass, buildDir);
        }
    }
}
