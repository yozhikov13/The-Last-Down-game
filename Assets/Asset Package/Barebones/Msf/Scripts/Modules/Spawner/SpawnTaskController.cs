using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class SpawnTaskController
    {
        private readonly IClientSocket _connection;
        public int SpawnId { get; private set; }
        public DictionaryOptions Options { get; private set; }

        public SpawnTaskController(int spawnId, DictionaryOptions options, IClientSocket connection)
        {
            _connection = connection;
            SpawnId = spawnId;
            Options = options;
        }

        /// <summary>
        /// Finalize spawn task
        /// </summary>
        public void FinalizeTask()
        {
            FinalizeTask(new DictionaryOptions());
        }

        /// <summary>
        /// Finalize spawn task
        /// </summary>
        /// <param name="finalizationData"></param>
        public void FinalizeTask(DictionaryOptions finalizationData)
        {
            FinalizeTask(finalizationData, () => { });
        }

        /// <summary>
        /// Finalize spawn task
        /// </summary>
        /// <param name="finalizationData"></param>
        /// <param name="callback"></param>
        public void FinalizeTask(DictionaryOptions finalizationData, Action callback)
        {
            Msf.Server.Spawners.FinalizeSpawnedProcess(SpawnId, finalizationData, (successful, error) =>
            {
                if (error != null)
                {
                    Logs.Error("Error while completing the spawn task: " + error);
                }

                callback.Invoke();
            }, _connection);
        }
    }
}