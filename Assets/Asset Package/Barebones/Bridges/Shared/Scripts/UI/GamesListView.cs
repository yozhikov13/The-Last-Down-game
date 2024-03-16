using Aevien.UI;
using Barebones.Logging;
using Barebones.MasterServer;
using Barebones.Networking;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.Games
{
    public class GamesListView : UIView
    {
        [Header("Components"), SerializeField]
        private GameItem gameItemPrefab;
        [SerializeField]
        private RectTransform listContainer;
        [SerializeField]
        private TextMeshProUGUI statusInfoText;

        public UnityEvent OnStartGameEvent;

        protected override void OnShow()
        {
            FindGames();
        }

        public void FindGames()
        {
            ClearGamesList();

            canvasGroup.interactable = false;

            if (statusInfoText)
            {
                statusInfoText.text = "Finding rooms... Please wait!";
                statusInfoText.gameObject.SetActive(true);
            }

            MsfTimer.WaitForSeconds(0.2f, () =>
            {
                Msf.Client.Matchmaker.FindGames((games) =>
                {
                    canvasGroup.interactable = true;

                    if (games.Count == 0)
                    {
                        statusInfoText.text = "No games found! Try to create your own.";
                        return;
                    }

                    statusInfoText.gameObject.SetActive(false);
                    DrawGamesList(games);
                });
            });
        }

        private void DrawGamesList(IEnumerable<GameInfoPacket> games)
        {
            if (listContainer && gameItemPrefab)
            {
                foreach (GameInfoPacket game in games)
                {
                    var gameItemInstance = Instantiate(gameItemPrefab, listContainer, false);
                    gameItemInstance.SetInfo(game, this);

                    Logs.Info(game);
                }
            }
            else
            {
                Logs.Error("Not all components are setup");
            }
        }

        private void ClearGamesList()
        {
            if (listContainer)
            {
                foreach (Transform tr in listContainer)
                {
                    Destroy(tr.gameObject);
                }
            }
        }

        public void StartGame(GameInfoPacket gameInfo)
        {
            Msf.Options.Set(MsfDictKeys.roomId, gameInfo.Id);

            if (gameInfo.IsPasswordProtected)
            {
                Msf.Events.Invoke(MsfEventKeys.showPasswordDialogBox,
                    new PasswordInputDialoxBoxEventMessage("Room is required the password. Please enter room password below",
                    () =>
                    {
                        OnStartGameEvent?.Invoke();
                    }));
            }
            else
            {
                OnStartGameEvent?.Invoke();
            }
        }
    }
}