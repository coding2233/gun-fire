using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoslynCSharp;

public class CompileHotUpdateSource 
{
    [MenuItem("Tools/Compile HotUpdate Source Code")]
    public static void CompileSource()
    {
        var domain = ScriptDomain.CreateDomain("Game.HotUpdate");
        var ghAssembly = domain.CompileAndLoadCSharpProject("Game.HotUpdate.csproj");
    }

}
