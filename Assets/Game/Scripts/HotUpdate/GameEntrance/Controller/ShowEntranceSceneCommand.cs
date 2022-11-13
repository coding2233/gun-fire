using strange.extensions.command.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wanderer
{
    public class ShowEntranceSceneCommand : EventCommand
    {
        [Inject("UICanvasRoot")]
        public GameObject uiRoot { get; set; }

        [Inject]
        public IAssetsHelper assetsHelper { get; set; }

        public override void Execute()
        {
            Log.Info("ShowEntranceSceneCommand");
            assetsHelper.LoadSceneAsync("Assets/Game/HotUpdate/GameEntrance/Scene/GameEntrance.unity", LoadSceneMode.Single, (asyncHandle) => { 
                //asyncHandle.completed += OnSceneAsyncHandleCompleted;
                dispatcher.Dispatch(HotUpdateEvent.ShowLoading, asyncHandle);
            });

            assetsHelper.LoadAsset<GameObject>("Assets/Game/HotUpdate/GameEntrance/UI/GameEntranceView.prefab", (uiPrefab) => {
               var entranceView =   GameObject.Instantiate(uiPrefab, uiRoot.transform).AddComponent<GameEntranceView>();
            });
        }


        private void OnSceneAsyncHandleCompleted(AsyncOperation asyncOperation)
        {
            //Addressables.Release(obj);
        }
    }
}