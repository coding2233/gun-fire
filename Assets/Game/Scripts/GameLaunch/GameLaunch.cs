using strange.extensions.context.api;
using strange.extensions.context.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Wanderer
{
    public class GameLaunch : ContextView
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);

            //重定向日志输出
            //UnitySystemConsoleRedirector.Redirect();
            //框架初始化成功
            context = new FrameworkContext(this);
            Debug.Log("FrameworkContext Init.");

            GameLaunchContext gameLaunchContext = new GameLaunchContext(this,ContextStartupFlags.MANUAL_LAUNCH);
            context.AddContext(gameLaunchContext);

            gameLaunchContext.Launch();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            context = null;
        }
        //private void Start()
        //{
        //    (context as FrameworkContext).GameEntrance();
        //    Log.Info("GameEntrance");
        //}
    }
}