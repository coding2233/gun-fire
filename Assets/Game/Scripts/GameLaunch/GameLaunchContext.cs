using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.injector.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class GameLaunchContext : MVCSContext
    {

        public GameLaunchContext()
        {
            Log.Info("GameLaunchContext");
        }

        public GameLaunchContext(ContextView contextView, ContextStartupFlags flags) : base(contextView,flags)
        {
            Log.Info("GameLaunchContext");
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            //Debug.Log(contextView);
            crossContextBridge.Bind(GameLaunchEvent.RunHotUpdate);

            var tfContextView = (contextView as GameObject).transform;
            injectionBinder.Bind<GameObject>().ToValue(tfContextView.Find("HotUpdate").gameObject).ToName(GameContextKeys.HotUpdateRoot).CrossContext();
            injectionBinder.Bind<GameObject>().ToValue(tfContextView.Find("HotUpdate/CanvasRoot").gameObject).ToName(GameContextKeys.UICanvasRoot).CrossContext();

            injectionBinder.Bind<IXLuaHotFix>().To<XLuaHotFixService>().ToSingleton();

            //mediationBinder.BindView<CheckResView>().ToMediator<CheckResMediator>();

            commandBinder.Bind(ContextEvent.START).To<CheckResCommand>().Once();
            commandBinder.Bind(GameLaunchEvent.RunHotUpdate).To<RunHotUpdateCommand>();
        }

        public override void Launch()
        {
            base.Launch();
        }

    }

}