using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Commands;
using UnityEditor.Build.Content;
using System.IO;

public class CommandBuilder 
{
    [MenuItem("Tools/Command/SetHybridCLR")]
    public static void SetHybridCLR()
    {
        //安装HybridCLR
        var installController = new InstallerController();
        if (!installController.HasInstalledHybridCLR())
        {
            #if UNITY_EDITOR_LINUX
                installController.Il2CppInstallDirectory = installController.Il2CppInstallDirectory.Replace("Unity/Contents/il2cpp", "Data/il2cpp");
            #endif
            installController.InitHybridCLR(installController.Il2CppBranch, installController.Il2CppInstallDirectory);
        }
        AssetDatabase.Refresh();
        //生成所有的的文件
        PrebuildCommand.GenerateAll();
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/Command/BuildPlayer - Android")]
    public static void BuildPlayer()
    {
        //EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        SetHybridCLR();

        if (!Directory.Exists("build"))
        {
            Directory.CreateDirectory("build");
        }

        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"build/{Application.productName}.apk", BuildTarget.Android, BuildOptions.None);
    }


}
