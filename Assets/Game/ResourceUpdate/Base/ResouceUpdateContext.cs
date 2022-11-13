using strange.extensions.context.api;
using strange.extensions.context.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class ResouceUpdateContext : MVCSContext
    {
        public ResouceUpdateContext(ContextView contextView) : base(contextView)
        { }

        protected override void mapBindings()
        {
            base.mapBindings();

            mediationBinder.Bind<CheckResView>().To<CheckResMediator>();

            commandBinder.Bind(ContextEvent.START).To<StartResourceUpdateCommand>().Once();

            //crossContextBridge.Bind(HotUpdateEvent.ContextInitialized);
        }

        public override void Launch()
        {
            base.Launch();
            
            //dispatcher.Dispatch(HotUpdateEvent.ContextInitialized);
        }
    }
}