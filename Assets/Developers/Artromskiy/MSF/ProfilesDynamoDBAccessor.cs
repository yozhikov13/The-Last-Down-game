using System.Collections.Generic;
using Barebones.MasterServer;
using System;
using DB;
using GameData;
using Newtonsoft.Json;

public class ProfilesDynamoDBAccessor: ObservableProfile
{
    public void RestoreProfileAsync(ObservableServerProfile profile, SuccessCallback callback)
    {
        DBAccessor.Context.LoadAsync<Profile>(profile.Username, (res) =>
        {
            if (res.Exception == null)
            {
                profile = ConvertToObservable(res.Result);
                    // TODO => Попробовать перенести результат полученного профиля в формат наблюдаемого сервером профиля
                    callback(true, null);
            }
            else
            {
                callback(false, res.Exception.Message);
            }
        });

        throw new NotImplementedException();
    }
    public void UpdateProfileAsync(ObservableServerProfile profile, SuccessCallback callback)
    {
        //TODO => Попробовать конвертировать наблюдаемый серверром профиль в профиль для БД
        Profile p = ConvertToNotObservable(profile);
        DBAccessor.Context.SaveAsync(p, (res) =>
        {
            if (res.Exception == null)
            {
                callback(true, null);
            }
            else
            {
                callback(false, res.Exception.Message);
            }
        });
    }


    private ObservableServerProfile ConvertToObservable(Profile notObservable)
    {
        ObservableServerProfile observable = new ObservableServerProfile(notObservable.Username);

        if (!string.IsNullOrEmpty(notObservable.Account))
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Account), notObservable.Account));
        }

        if(notObservable.Inventory!=null)
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Inventory), JsonConvert.SerializeObject(notObservable.Inventory)));
        }
        else
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Inventory), JsonConvert.SerializeObject(new List<Item>())));
        }

        if(notObservable.Characters!=null)
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Characters), JsonConvert.SerializeObject(notObservable.Characters)));
        }
        else
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Characters), JsonConvert.SerializeObject(new List<string>())));
        }

        if (notObservable.Sets == null)
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Sets), JsonConvert.SerializeObject(notObservable.Sets)));
        }
        else
        {
            observable.Add(new ObservableString((short)(ProfileCodes.Sets), JsonConvert.SerializeObject(new Dictionary<int, WeaponSet>())));
        }

        observable.Add(new ObservableInt((short)(ProfileCodes.Experience), notObservable.Experience));
        observable.Add(new ObservableInt((short)(ProfileCodes.Money), notObservable.Money));
        observable.Add(new ObservableInt((short)(ProfileCodes.PremiumMoney), notObservable.PremiumMoney));
        observable.Add(new ObservableInt((short)(ProfileCodes.KillsCount), notObservable.KillsCount));
        observable.Add(new ObservableInt((short)(ProfileCodes.DeathsCount), notObservable.DeathsCount));
        observable.Add(new ObservableInt((short)(ProfileCodes.MatchesCount), notObservable.MathcesCount));

        return observable;
    }

    private Profile ConvertToNotObservable(ObservableServerProfile observable)
    {
        Profile notObservable = new Profile();
        if(!string.IsNullOrEmpty(observable.Username))
        {
            notObservable.Username = observable.Username;
        }
        ObservableString s = null;
        observable.TryGetProperty((short)(ProfileCodes.Account), out s);
        notObservable.Account = s?.GetValue();
        s = null;
        observable.TryGetProperty((short)(ProfileCodes.Inventory), out s);
        if (s != null)
            notObservable.Inventory = JsonConvert.DeserializeObject(s.GetValue()) as List<Item>;
        s = null;
        observable.TryGetProperty((short)(ProfileCodes.Characters), out s);
        if (s != null)
            notObservable.Characters = JsonConvert.DeserializeObject(s.GetValue()) as List<string>;
        s = null;
        observable.TryGetProperty((short)(ProfileCodes.Sets), out s);
        if (s != null)
            notObservable.Sets = JsonConvert.DeserializeObject(s.GetValue()) as Dictionary<int, WeaponSet>;
        ObservableInt i = null;
        observable.TryGetProperty((short)(ProfileCodes.Experience), out i);
        notObservable.Experience = i != null ? i.GetValue() : 0;
        i = null;
        observable.TryGetProperty((short)(ProfileCodes.Money), out i);
        notObservable.Money = i != null ? i.GetValue() : 0;
        i = null;
        observable.TryGetProperty((short)(ProfileCodes.PremiumMoney), out i);
        notObservable.PremiumMoney = i != null ? i.GetValue() : 0;
        i = null;
        observable.TryGetProperty((short)(ProfileCodes.KillsCount), out i);
        notObservable.KillsCount = i != null ? i.GetValue() : 0;
        i = null;
        observable.TryGetProperty((short)(ProfileCodes.DeathsCount), out i);
        notObservable.DeathsCount = i != null ? i.GetValue() : 0;
        i = null;
        observable.TryGetProperty((short)(ProfileCodes.MatchesCount), out i);
        notObservable.MathcesCount = i != null ? i.GetValue() : 0;

        return notObservable;
    }

    public void UpdateProfile(ObservableServerProfile profile)
    {
        throw new NotImplementedException();
    }
    public void RestoreProfile(ObservableServerProfile profile)
    {
        throw new NotImplementedException();
    }
}
