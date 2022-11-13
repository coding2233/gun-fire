using strange.extensions.context.api;
using strange.extensions.context.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class HotUpdateContextView : ContextView
    {
        private void Awake()
        {
            Log.Info("HotUpdateContextView -- Awake");
            //热更里面加调试面板，方便更新
            gameObject.AddComponent<DebuggerManager>();

            context = new HotUpdateContext(this,ContextStartupFlags.MANUAL_LAUNCH);

            SetDynamicContext();

            context.Launch();
        }

        private void Start()
        {
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private void SetDynamicContext()
        {
            var allTypes = typeof(HotUpdateContextView).Assembly.GetTypes();
            foreach (var itemType in allTypes)
            {
                //Log.Info($"SetDynamicContext：{itemType}");
                if (itemType.BaseType == typeof(MVCSContext) && !itemType.IsAbstract)
                {
                    var attrs = itemType.GetCustomAttributes(false);
                    if(attrs!=null && attrs.Length>0)
                    {
                        foreach (var attr in attrs)
                        {
                            if (attr is HotUpdateContextAttribute)
                            {
                                CreateChildContext(itemType);
                                break;
                            }
                        }
                    }
                }
            }
        }


        private void CreateChildContext(Type type)
        {
            Log.Info($"CreateChildContext：{type}");
          
            MVCSContext childContext = Activator.CreateInstance(type, new object[] { this }) as MVCSContext;
            context.AddContext(childContext);
        }

    
    }
}
