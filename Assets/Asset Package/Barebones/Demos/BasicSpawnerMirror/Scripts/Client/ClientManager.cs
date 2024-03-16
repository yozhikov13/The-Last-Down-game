using Aevien.UI;
using Barebones.Bridges.Mirror;
using Barebones.Games;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Barebones.MasterServer.Examples.BasicSpawnerMirror
{
    public class ClientManager : BaseClientBehaviour
    {
        [Header("Components"), SerializeField]
        private ClientToMasterConnector clientToMasterConnector;

        private CreateNewRoomView createNewRoomView;
        private GamesListView gamesListView;

        protected override void OnInitialize()
        {
            if (!clientToMasterConnector)
                clientToMasterConnector = FindObjectOfType<ClientToMasterConnector>();

            // Set cliet mode
            Msf.Client.Rooms.ForceClientMode = true;

            // Set MSF global options
            Msf.Options.Set(MsfDictKeys.autoStartRoomClient, true);
            Msf.Options.Set(MsfDictKeys.offlineSceneName, SceneManager.GetActiveScene().name);

            createNewRoomView = ViewsManager.GetView<CreateNewRoomView>("CreateNewRoomView");
            gamesListView = ViewsManager.GetView<GamesListView>("GamesListView");
        }

        public void CreateNewRoom()
        {
            createNewRoomView.Hide();

            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Starting room... Please wait!");

            // Spawn options for spawner controller
            var spawnOptions = new DictionaryOptions();
            spawnOptions.Add(MsfDictKeys.maxPlayers, createNewRoomView.MaxConnections);
            spawnOptions.Add(MsfDictKeys.roomName, createNewRoomView.RoomName);

            // Custom options that will be given to room directly
            var customSpawnOptions = new DictionaryOptions();
            customSpawnOptions.Add(Msf.Args.Names.StartClientConnection, string.Empty);

            Msf.Client.Spawners.RequestSpawn(spawnOptions, customSpawnOptions, createNewRoomView.RegionName, (controller, error) =>
            {
                if (controller == null) {
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

                    gamesListView.Show();

                    logger.Info("You have successfully spawned new room");
                }, 60f);
            });
        }

        public void Quit()
        {
            Msf.Runtime.Quit();
        }
    }
}
