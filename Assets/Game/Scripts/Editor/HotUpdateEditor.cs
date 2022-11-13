using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wanderer
{
	public class HotUpdateEditor
	{
		private static string[] m_srcDllPath = new string[] { 
			"Library/ScriptAssemblies/Game.HotUpdate.dll" ,
            "Library/ScriptAssemblies/Game.HotUpdate.pdb" ,
            "Library/ScriptAssemblies/Resouce.HotUpdate.dll",
            "Library/ScriptAssemblies/Resouce.HotUpdate.pdb"
        };
		//pdb...
		private static string[] m_destDllPath = new string[] {
		"Assets/Game/HotUpdate/Code/Game.HotUpdate.dll.bytes",
        "Assets/Game/HotUpdate/Code/Game.HotUpdate.pdb.bytes",
        "Assets/Game/ResourceUpdate/Resouce.HotUpdate.dll.bytes",
        "Assets/Game/ResourceUpdate/Resouce.HotUpdate.pdb.bytes"
    }; 



		//这个只针对编辑器开发有效
		//hybridclr发布的时候， 需要使用官方的方案
		[InitializeOnLoadMethod]
		static void CompileAutoCopyHotUpdateDLL()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
                for (int i = 0; i < m_srcDllPath.Length; i++)
                {
					//复制dll
					File.Copy(m_srcDllPath[i], m_destDllPath[i], true);
					Debug.Log($"[Only Editor]  {m_srcDllPath[i]} --> {m_destDllPath[i]}");
				}

				//刷新资源
				AssetDatabase.Refresh();

			}
		}
	}
}
