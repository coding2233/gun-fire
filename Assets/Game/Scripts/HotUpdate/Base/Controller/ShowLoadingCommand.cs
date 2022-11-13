using strange.extensions.command.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class ShowLoadingCommand : EventCommand
    {
        [Inject("UICanvasRoot")]
        public GameObject tfCanvasRoot { get; set; }

        [Inject]
        public IAssetsHelper assetsHelper { get; set; }

        public override void Execute()
        {
            if (evt.data != null)
            {
                if (evt.data is AsyncOperation asyncOperation)
                {
                    var loadingGo = GameObject.Instantiate(assetsHelper.LoadAsset<GameObject>("Assets/Game/ResourceUpdate/DefaultLoading.prefab"), tfCanvasRoot.transform);
                    var loadingView = loadingGo.AddComponent<LoadingView>();
                    loadingView.Setup((AsyncOperation)evt.data);
                }
            }

        }
    }
}