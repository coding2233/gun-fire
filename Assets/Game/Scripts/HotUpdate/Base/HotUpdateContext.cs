using strange.extensions.context.api;
using strange.extensions.context.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class HotUpdateContext : MVCSContext
    {
        public HotUpdateContext(ContextView contextView, ContextStartupFlags flags) : base(contextView, flags)
        { }

        protected override void mapBindings()
        {
            base.mapBindings();

            crossContextBridge.Bind(HotUpdateEvent.ContextInitialized);
            crossContextBridge.Bind(HotUpdateEvent.ShowLoading);

            commandBinder.Bind(HotUpdateEvent.ShowLoading).To<ShowLoadingCommand>();

            mediationBinder.Bind<LoadingView>().To<LoadingMediator>();
        }

        public override void Launch()
        {
            base.Launch();
            dispatcher.Dispatch(HotUpdateEvent.ContextInitialized);
        }
    }
}