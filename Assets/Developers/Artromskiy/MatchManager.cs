using Mirror;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SyncListPlayerInfo: SyncList<PlayerInfo> {}
public class MatchManager : NetworkBehaviour
{
    private readonly Dictionary<string, List<MatchEvent>> matchLog = new Dictionary<string, List<MatchEvent>>();
    public readonly SyncListPlayerInfo PlayersStats = new SyncListPlayerInfo();

    [ServerCallback]
    public void LogConnection(uint id)
    {
        matchLog.Add(id.ToString("G"), new List<MatchEvent>());
    }

    [ServerCallback]
    public void LogDamage(float damage, string receiver, Weapon.ShootInfo shootInfo)
    {
        if (shootInfo == null || string.IsNullOrEmpty(receiver))
            return;
        if (shootInfo.AuthorInfo == null)
            shootInfo.AuthorInfo = "";
        if (shootInfo.WeaponInfo == null)
            shootInfo.WeaponInfo = "";
        MatchEvent matchEvent = new TakeDamageEvent(damage, shootInfo.AuthorInfo.Clone() as string, shootInfo.WeaponInfo.Clone() as string);//лог о получении урона
        AddEvent(receiver, matchEvent);
        matchEvent = new MakeDamageEvent(damage, receiver, shootInfo.WeaponInfo.Clone() as string);//лог о нанесении урона
        AddEvent(shootInfo.AuthorInfo.Clone() as string, matchEvent);
    }

    [ServerCallback]
    public void LogKill(string receiver, Weapon.ShootInfo shootInfo)
    {
        if (shootInfo == null || string.IsNullOrEmpty(receiver))
            return;
        if (shootInfo.AuthorInfo == null)
            shootInfo.AuthorInfo = "";
        if (shootInfo.WeaponInfo == null)
            shootInfo.WeaponInfo = "";
        MatchEvent matchEvent = new KillEvent(receiver.Clone() as string, shootInfo.WeaponInfo.Clone() as string);
        AddEvent(shootInfo.AuthorInfo.Clone() as string, matchEvent);
        matchEvent = new DeathEvent(shootInfo.WeaponInfo.Clone() as string);
        AddEvent(receiver,matchEvent);
    }

    [ServerCallback]
    public void LogHeadShot(string receiver, Weapon.ShootInfo shootInfo)
    {
        if (shootInfo == null || string.IsNullOrEmpty(receiver))
            return;
        if (shootInfo.AuthorInfo == null)
            shootInfo.AuthorInfo = "";
        if (shootInfo.WeaponInfo == null)
            shootInfo.WeaponInfo = "";
        MatchEvent matchEvent = new HeadShotEvent(receiver.Clone() as string, shootInfo.WeaponInfo.Clone() as string);
        AddEvent(shootInfo.AuthorInfo.Clone() as string, matchEvent);

    }

    [ServerCallback]
    public void LogRegen(string author, float hp)
    {
        if (author == null) author = "";
        MatchEvent matchEvent = new RegenerateEvent(hp);
        AddEvent(author,matchEvent);
    }

    [ServerCallback]
    private void AddEvent(string author, MatchEvent matchEvent)
    {
        if(matchLog.ContainsKey(author))
        {
            if (matchLog[author] == null)
            {
                var newList = new List<MatchEvent>();
                newList.Add(matchEvent);
                matchLog[author] = newList;
            }
            matchLog[author].Add(matchEvent);
        }
        else
        {
            var newList = new List<MatchEvent>
            {
                matchEvent
            };
            matchLog.Add(author, newList);
        }
        UpdateList(author, matchEvent);
    }

    [ServerCallback]
    private int FindCombos(string author)//начисление очков за убийство и помощь
    {
        List<MatchEvent> KillerEvents;
        int combos = 0;
        matchLog.TryGetValue(author, out KillerEvents);
        foreach(var Event in KillerEvents)
        {
            var death = Event as DeathEvent;
            if (death != null)
            {
                var kill = Event as KillEvent;
                if (kill != null) combos++;

            }
            else break;
        }
        return combos;
    }

    [ServerCallback]
    private List<KeyValuePair<string, float>> FindHelpers(string author,string reciever)//получение имени убийцы и убитого
    {
        List<KeyValuePair<string, float>> Damagers = new List<KeyValuePair<string, float>>();
        List<MatchEvent> RecieverEventList;//,HelperEventList;
        matchLog.TryGetValue(reciever, out RecieverEventList);//получение списка событий убитого
        int k = 1;

        foreach (var i in RecieverEventList)
        {
            var l = i as DeathEvent;
            if(l !=null && k !=0)
            {
                break;
            }
            k++;
        }// вычисление индекса предпоследней смерти

        RecieverEventList = RecieverEventList.GetRange(1,k);//получение событий в промежутке последней и предпоследней сметри, все события до этого уже были проанализированы

        for (int i = RecieverEventList.Count-1;i>=0;i--)//цикл добавляющий урон при нанесении и отнимающий при регене, в конце должны отстаться только те игроки у которых урон - реген >0
        {
            var takedamage = RecieverEventList[i] as TakeDamageEvent;
            if(takedamage !=null)
            {
                if(takedamage.author!= author)
                Damagers.Add(new KeyValuePair<string, float>(takedamage.author, takedamage.damage));
            }
            else
            {
                var regen = RecieverEventList[i] as RegenerateEvent;
                if(regen != null)
                {
                    float regenvalue = regen.hp;
                    for (int t =0; t< Damagers.Count;t++)
                    {
                        if(Damagers[t].Value<= regenvalue)
                        {
                            regenvalue -= Damagers[t].Value;
                            Damagers.RemoveAt(t);
                        }
                        else
                        {
                            Damagers[t] = new KeyValuePair<string, float>(Damagers[t].Key, Damagers[t].Value -regenvalue);
                            break;
                        }
                        if (regenvalue == 0) break;
                    }
                }
                else
                {
                    continue;
                }
            }
            


        }
        List<KeyValuePair<string, float>> Helpers = new List<KeyValuePair<string, float>>();

        foreach (var damagers in Damagers)//суммирование урона всем помощникам
        {
            //bool contains = false;
            //int i = 0;
            //foreach (var helpers in Helpers) //проверка на существование в списке помощников данного ключа
            //{
            //    i++;
            //    if(damagers.Key == helpers.Key)
            //    {
                    
            //        contains = true;
            //        break;
            //    }
            //}
            if (Helpers.Find(x=>x.Key == damagers.Key).Equals(default(KeyValuePair<string, float>)))//проверка на существование в списке помощников данного ключа
            {
                int i = Helpers.IndexOf(Helpers.Find(x => x.Key == damagers.Key));
                Helpers[i] = new KeyValuePair<string, float>(Helpers[i].Key, Helpers[i].Value + damagers.Value);
            }
            else Helpers.Add(damagers);
        }



        //int a = 0;
        //foreach (var i in Helpers)//добавление к статам всех помощников у которых урон> 40, очка помощи
        //{
        //    if( i.Value >= 40 )
        //    {
        //        if (PlayersStats.Exists(x => x.Name == i.Key))
        //        {
        //            PlayersStats[FindInList(i.Key)] = PlayersStats[FindInList(i.Key)] + new PlayerInfo("Helps", 1, PlayersStats[FindInList(i.Key)].Name);
        //        }
        //        else PlayersStats.Add(new PlayerInfo("Helps", 1,i.Key ));

        //    }
        //    else
        //    {
        //        Helpers.Remove(Helpers[a]);
        //    }
        //    a++;
        //}
        Helpers.RemoveAll(x => x.Value < 40);//удаление всех элементов с уроном <40
        return Helpers;
    }

    [ServerCallback]
    private void UpdateList(string author, MatchEvent matchEvent)
    {
        var kill = matchEvent as KillEvent;
        if(kill != null)
        {
            int k = FindInList(author);

            if (k != -1)
            {
                PlayersStats[k] = PlayersStats[k] + new PlayerInfo("Kills", 1, PlayersStats[k].Name);
                
            }
            else PlayersStats.Add(new PlayerInfo("Kills", 1, ""));//добавление нового элемнта списка с именем убившего
            k = FindInList(author);
            PlayersStats[k] += new PlayerInfo("Score", 100, "");//обновление счета
            int combos = FindCombos(author);
            switch (combos)
            {
                case 2:
                    PlayersStats[k] += new PlayerInfo("Score", 2, "");
                    break;
                case 3:
                    PlayersStats[k] += new PlayerInfo("Score", 10, "");
                    break;
                case 5:
                    PlayersStats[k] += new PlayerInfo("Score", 15, "");
                    break;
                case 8:
                    PlayersStats[k] += new PlayerInfo("Score", 20, "");
                    break;
                case 10:
                    PlayersStats[k] += new PlayerInfo("Score", 50, "");
                    break;
                default:
                    if (combos > 10) PlayersStats[k] += new PlayerInfo("Score", 50, author);
                    break;
            }
            if(kill.weapon =="Knife"|| kill.weapon == "Turret"|| kill.weapon == "Drone") PlayersStats[k] += new PlayerInfo("Score", 2, author);
            List<KeyValuePair<string, float>> helpers= FindHelpers(author,kill.receiver);
            foreach(var i in helpers)
            {
                int index = FindInList(i.Key);
                if (index != -1)
                {
                    PlayersStats[index] = PlayersStats[index] + new PlayerInfo("Helps", 1, PlayersStats[index].Name);
                    PlayersStats[index] += new PlayerInfo("Score", 50, "");
                }
                else
                {
                    PlayersStats.Add(new PlayerInfo("Helps",1,i.Key));
                    index = FindInList(i.Key);
                    PlayersStats[index] += new PlayerInfo("Score", 50, "");
                }
                
                
            }

            k = FindInList(kill.receiver);

            if (k != -1) PlayersStats[k] = PlayersStats[k] + new PlayerInfo("Deaths", 1, PlayersStats[k].Name);
            else PlayersStats.Add(new PlayerInfo("Deaths", 1, kill.receiver));//добавление нового элемнта списка с именем убитого
        }
        else
        {
            var headshot = matchEvent as HeadShotEvent;

            if(headshot != null)
            {
                int h = FindInList(author);

                if (h != -1) PlayersStats[h] = PlayersStats[h] + new PlayerInfo("Headshots", 1, PlayersStats[h].Name);
                else PlayersStats.Add(new PlayerInfo("Headshots", 1, author));// //добавление нового элемнт  а списка с именем автора шедшота
                h = FindInList(author);
                PlayersStats[h] += new PlayerInfo("Score", 2, "");

            }
            else
            {
                var damagetaken = matchEvent as TakeDamageEvent;//получить урон
                if (damagetaken != null)
                {
                    int d = FindInList(author);

                    if (d != -1) PlayersStats[d] = PlayersStats[d] + new PlayerInfo("DamageTaken", damagetaken.damage, PlayersStats[d].Name);
                    else PlayersStats.Add(new PlayerInfo("DamageTaken", damagetaken.damage, author));//добавление нового элемнта списка с именем автора нанесения урона
                }
                else
                {
                    var damagedone = matchEvent as MakeDamageEvent;// нанести урон

                    if (damagedone != null)
                    {


                        int d = FindInList(author);

                        if (d != 1) PlayersStats[d] = PlayersStats[d] + new PlayerInfo("DamageDone", damagedone.damage, PlayersStats[d].Name);
                        else PlayersStats.Add(new PlayerInfo("DamageTaken", damagedone.damage, author));
                    }
                }
            }
        }



    }

    [ServerCallback]
    int FindInList(string name)
    {
        return PlayersStats.IndexOf(PlayersStats.Find(x => x.Name == name ));
    }

    abstract class MatchEvent
    {
        public float eventTime;
    }

    class TakeDamageEvent: MatchEvent//получение урона
    {
        public float damage;
        public string weapon;
        public string author;
        public TakeDamageEvent(float damage, string author, string weapon)
        {
            this.damage = damage;
            this.weapon = weapon;
            this.author = author;
            eventTime = Time.time;
        }
    }

    class MakeDamageEvent: MatchEvent//нанесение урона
    {
        public float damage;
        public string weapon;
        public string reciever;
        public MakeDamageEvent(float damage,string reciever, string weapon)
        {
            this.damage = damage;
            this.weapon = weapon;
            this.reciever = reciever;
            eventTime = Time.time;
        }

    }

    class RegenerateEvent: MatchEvent
    {
        public float hp;
        public RegenerateEvent(float hp)
        {
            this.hp = hp;
            eventTime = Time.time;
        }
    }

    class KillEvent: MatchEvent
    {
        public string receiver;
        public string weapon;
        public KillEvent(string receiver, string weapon)
        {
            this.weapon = weapon;
            this.receiver = receiver;
            eventTime = Time.time;
        }
    }

    class DeathEvent: MatchEvent
    {
        public string weapon;
        public DeathEvent(string weapon)
        {
            this.weapon = weapon;
            eventTime = Time.time;
        }
    }

    class HeadShotEvent: MatchEvent
    {
        public string receiver;
        public string weapon;
        public HeadShotEvent(string receiver, string weapon)
        {
            this.weapon = weapon;
            this.receiver = receiver;
            eventTime = Time.time;
        }
    }

    [ContextMenu("Visualize MatchLog")]
    public void Visualize()
    {
        Debug.Log(JsonConvert.SerializeObject(matchLog));
    }
}
