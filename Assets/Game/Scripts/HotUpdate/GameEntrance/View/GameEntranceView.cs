using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wanderer
{
    public class GameEntranceView : View
    {

        [Inject]
        public IEventDispatcher dispatcher { get; set; }

        protected override void Awake()
        {
            base.Awake();

            Log.Info($"GameEntranceView Awake");

            var menuPanel = transform.Find("MenuPanel");
            for (int i = 0; i < menuPanel.childCount; i++)
            {
                var menuButton = menuPanel.GetChild(i).GetComponent<Button>();
                menuButton.interactable = i == 0;
                menuButton.onClick.AddListener(() => {
                    OnSelectMenu(menuButton.name);
                });
            }
        }


        private void OnSelectMenu(string menu)
        {
            dispatcher.Dispatch("SelectMenu", menu);
        }

    }



    public class GameEntranceMediator : EventMediator
    {
        [Inject]
        public GameEntranceView entranceView { get; set; }

        [Inject]
        public IAssetsHelper assetsHelper { get; set; }

        [Inject("UICanvasRoot")]
        public GameObject uiRoot { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            entranceView.dispatcher.AddListener("SelectMenu", OnSelectMenu);

        }

        public override void OnRemove()
        {
            entranceView.dispatcher.RemoveListener("SelectMenu", OnSelectMenu);

            base.OnRemove();
        }

        private void OnSelectMenu(IEvent @event)
        {
            Log.Info("OnSelectMenu -- "+ @event.data);


            assetsHelper.LoadSceneAsync("Assets/Game/HotUpdate/GameTraining/Training.unity", LoadSceneMode.Single, (asyncHandle) => {
                //asyncHandle.completed += OnSceneAsyncHandleCompleted;
                dispatcher.Dispatch(HotUpdateEvent.ShowLoading, asyncHandle);
            });

            //assetsHelper.LoadAsset<GameObject>("Assets/Game/GameEntrance/UI/GameEntranceView.prefab", (uiPrefab) => {
            //    var entranceView = GameObject.Instantiate(uiPrefab, uiRoot.transform).AddComponent<GameEntranceView>();
            //});
        }
    }

}
