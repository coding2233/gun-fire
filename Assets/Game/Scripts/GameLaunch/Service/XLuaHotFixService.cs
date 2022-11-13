using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Wanderer
{

    public interface IXLuaHotFix
    {
        void DoLuaFix();
    }

    internal class XLuaHotFixService : MonoService,IXLuaHotFix
    {
        LuaEnv m_luaEnv;
        private Dictionary<string, byte[]> m_luaScripts;
        // lua 更新函数
        private Action<float> m_luaOnUpdate;
        //lua 关闭函数
        private Action m_luaOnClose;

        private float m_lastGCTime = 0;
        private const float m_gcInterval = 1;//1 second

        public XLuaHotFixService()
        {
            m_luaScripts = new Dictionary<string, byte[]>();
        }

        public void DoLuaFix()
        {
            if (m_luaEnv == null)
            {
                m_luaEnv = new LuaEnv();
                m_luaEnv.AddLoader(CustomLoader);

                var loadHotFixLua = m_luaEnv.NewTable();
                // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
                LuaTable meta = m_luaEnv.NewTable();
                meta.Set("__index", m_luaEnv.Global);
                loadHotFixLua.SetMetaTable(meta);
                meta.Dispose();

                loadHotFixLua.Set("self", this);
                m_luaEnv.DoString($"require \"HotFixMain\"", "HotFixMain", loadHotFixLua);
                Action onLuaStart = loadHotFixLua.Get<Action>("OnStart");
                //loadHotFixLua.Get("OnUpdate", out m_luaOnUpdate);
                loadHotFixLua.Get("OnClose", out m_luaOnClose);

                onLuaStart?.Invoke();
            }
        }

        public override void OnUpdate()
        {
            if (m_luaEnv != null)
            {
                //m_luaOnUpdate?.Invoke(Time.deltaTime);
                if (Time.time - m_lastGCTime > m_gcInterval)
                {
                    m_luaEnv.Tick();
                    m_lastGCTime = Time.time;
                }
            }
        }

        public override void OnMonoDestroy()
        {
            m_luaOnUpdate = null;
            m_luaOnClose?.Invoke();
            m_luaOnClose = null;
            if (m_luaEnv != null)
            {
                m_luaEnv?.Dispose();
                m_luaEnv = null;
            }
        }


        #region 内部函数
        //自定义加载
        private byte[] CustomLoader(ref string filePath)
        {
            byte[] data=null;
            //if (!m_luaScripts.TryGetValue(filePath, out data))
            //{
            //    string luaPath = $"Assets/Game/HotFix/{filePath}.lua.txt";
            //    if (!string.IsNullOrEmpty(luaPath))
            //    {
            //        TextAsset textAsset = Addressables.LoadAssetAsync<TextAsset>(luaPath).WaitForCompletion();
            //        if (textAsset != null)
            //        {
            //            data = textAsset.bytes;
            //            m_luaScripts.Add(filePath, data);
            //        }
            //        else
            //        {
            //            Log.Warn("No lua file was found. {0}", filePath);
            //        }
            //    }
               
            //}
            return data;
        }


     
        #endregion
    }
}
