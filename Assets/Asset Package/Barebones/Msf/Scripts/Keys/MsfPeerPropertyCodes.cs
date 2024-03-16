namespace Barebones.MasterServer
{
    public enum MsfPeerPropertyCodes : short
    {
        Start = 26000,

        // Rooms
        RegisteredRooms,

        // Spawners
        RegisteredSpawners,
        ClientSpawnRequest
    }
}