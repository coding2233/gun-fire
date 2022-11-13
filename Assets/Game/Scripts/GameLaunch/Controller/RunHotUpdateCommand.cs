using strange.extensions.command.impl;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Wanderer
{
    public class RunHotUpdateCommand : EventCommand
    {
        [Inject(GameContextKeys.HotUpdateRoot)]
        public GameObject tfHotUpdate { get; set; }

        [Inject]
        public IAssetsHelper assetHelper { get; set; }

        public override void Execute()
        {
            RunHotUpdate();
        }

        private void RunHotUpdate()
        {
            Debug.Log("Run hot update");

            assetHelper.LoadExtendAssembly("Assets/Game/HotUpdate/Code/Game.HotUpdate.dll.bytes", (hotUpdateAssembly) =>
            {
                var hotUpdateContextView = hotUpdateAssembly.GetType("Wanderer.HotUpdateContextView");

                tfHotUpdate.AddComponent(hotUpdateContextView);
            });

           
        }

    }
}