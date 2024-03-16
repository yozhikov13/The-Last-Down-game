using Barebones.Logging;
using Barebones.Networking;
using System;

namespace Barebones.MasterServer
{
    public interface ISpawnerController
    {
        event Action OnProcessStartedEvent;
        event Action OnProcessKilledEvent;
        SpawnerConfig SpawnSettings { get; }
        Logger Logger { get; }
        IClientSocket Connection { get; }
        int SpawnerId { get; }
        void SpawnRequestHandler(SpawnRequestPacket packet, IIncommingMessage message);
        void KillRequestHandler(int spawnId);
        void KillProcesses();
        int ProcessesCount();
    }
}