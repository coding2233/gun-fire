using strange.extensions.command.impl;
using strange.extensions.context.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    [HotUpdateContext]
    public class GameEntranceContext : MVCSContext
    {
        public GameEntranceContext(ContextView view):base(view)
        {
            Log.Info($"GameEntranceContext: {view}");
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            Log.Info($"GameEntranceContext,mapBindings {contextView}");


            mediationBinder.Bind<GameEntranceView>().To<GameEntranceMediator>();

            commandBinder.Bind(HotUpdateEvent.ContextInitialized).To<ShowEntranceSceneCommand>();
        }
    }
}