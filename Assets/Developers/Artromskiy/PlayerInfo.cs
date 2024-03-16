using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct PlayerInfo
{
    public string Name;
    public int Kills;
    public int Deaths;
    public float TotalDamageTaken;
    public float TotalDamageDone;
    public int Headshots;
    public int Helps;
    public int Score;
    public PlayerInfo(string parameter, float value, string name)
    {
        Name = name;
        Kills = 0;
        Deaths = 0;
        TotalDamageTaken = 0;
        TotalDamageDone = 0;
        Headshots = 0;
        Helps = 0;
        Score = 0;
        switch (parameter)
        {
            case "Kills":
                Kills = (int)value;
                break;
            case "Deaths":
                Deaths = (int)value;
                break;
            case "DamageTaken":
                TotalDamageTaken = value;
                break;
            case "DamageDone":
                TotalDamageDone = value;
                break;
            case "Headshots":
                Headshots = (int)value;
                break;
            case "Helps":
                Helps = (int)value;
                break;
        }
    }
    public PlayerInfo(string name, int kills, int deaths, float damagetaken, float damagedone, int headshots, int helps)
    {
        Name = name;
        Kills = kills;
        Deaths = deaths;
        TotalDamageTaken = damagetaken;
        TotalDamageDone = damagedone;
        Headshots = headshots;
        Helps = helps;
        Score = 0;
    }
    public PlayerInfo(string name)
    {
        Name = name;
        Kills = 0;
        Deaths = 0;
        TotalDamageTaken = 0;
        TotalDamageDone = 0;
        Headshots = 0;
        Helps = 0;
        Score = 0;
    }
    public static PlayerInfo operator +(PlayerInfo Current, PlayerInfo New)
    {
        return new PlayerInfo(Current.Name)
        {
            Kills = Current.Kills + New.Kills,
            Deaths = Current.Deaths + New.Deaths,
            TotalDamageTaken = Current.TotalDamageTaken + New.TotalDamageTaken,
            TotalDamageDone = Current.TotalDamageDone + New.TotalDamageDone,
            Headshots = Current.Headshots + New.Headshots,
            Helps = Current.Helps + New.Helps
        };
    }

}