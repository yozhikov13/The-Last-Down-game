using Aevien.UI;
using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Barebones.Games
{
    public class MatchmakingBehaviour : BaseClientBehaviour
    {
        private CreateNewRoomView createNewRoomView;

        [SerializeField]
        protected string startRoomScene = "Room";

        public UnityEvent OnRoomStartedEvent;

        protected override void OnInitialize()
        {
            // Set cliet mode
            Msf.Client.Rooms.ForceClientMode = true;

            // Set MSF global options
            Msf.Options.Set(MsfDictKeys.autoStartRoomClient, true);
            Msf.Options.Set(MsfDictKeys.offlineSceneName, SceneManager.GetActiveScene().name);

            createNewRoomView = ViewsManager.GetView<CreateNewRoomView>("CreateNewRoomView");
        }

        public void CreateNewRoom()
        {
            if (!createNewRoomView)
            {
                ViewsManager.NotifyNoViewFound("CreateNewRoomView");
                return;
            }

            createNewRoomView.Hide();

            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Starting room... Please wait!");

            // Spawn options for spawner controller
            var spawnOptions = new DictionaryOptions();
            spawnOptions.Add(MsfDictKeys.maxPlayers, createNewRoomView.MaxConnections);
            spawnOptions.Add(MsfDictKeys.roomName, createNewRoomView.RoomName);
            spawnOptions.Add(MsfDictKeys.roomPassword, createNewRoomView.Password);

            // Custom options that will be given to room directly
            var customSpawnOptions = new DictionaryOptions();
            customSpawnOptions.Add(Msf.Args.Names.StartClientConnection, string.Empty);

            Msf.Client.Spawners.RequestSpawn(spawnOptions, customSpawnOptions, createNewRoomView.RegionName, (controller, error) =>
            {
                if (controller == null)
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);
                    Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(error, null));
                    return;
                }

                Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Room started. Finalizing... Please wait!");

                MsfTimer.WaitWhile(() =>
                {
                    return controller.Status != SpawnStatus.Finalized;
                }, (isSuccess) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (!isSuccess)
                    {
                        Msf.Client.Spawners.AbortSpawn(controller.SpawnTaskId);
                        logger.Error("Failed spawn new room. Time is up!");
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage("Failed spawn new room. Time is up!", null));
                        return;
                    }

                    OnRoomStartedEvent?.Invoke();

                    logger.Info("You have successfully spawned new room");
                }, 60f);
            });
        }

        public void StartMatch()
        {
            ScenesLoader.LoadSceneByName(startRoomScene, (progressValue) =>
            {
                Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, $"Loading scene {Mathf.RoundToInt(progressValue * 100f)}% ... Please wait!");
            }, null);
        }
    }
}