using System.Collections.Generic;
using Barebones.MasterServer;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;
using DB;
using System;

public class AccountsDynamoDBAccessor : IAccountsDatabaseAccessor
{
    public TimeSpan ts = TimeSpan.FromSeconds(3);
    public IAccountInfoData CreateAccountInstance()
    {
        return new AccountInfoDynamoDB();
    }

    public async void GetAccountByUsernameAsync(string username, GetAccountCallback callback)
    {
        await Task.Run(() =>
        {
            DBAccessor.Context.LoadAsync<AccountInfoDynamoDB>(username, "main", (res) =>
            {
                if (res.Exception == null)
                {
                    callback(res.Result as AccountInfoDynamoDB, string.Empty);
                }
                else
                {
                    callback(null, res.Exception.Message);
                }
            });
        });
    }

    public async void GetAccountByTokenAsync(string token, GetAccountCallback callback)
    {
        // Получаем путь к аккаунту через токен
        await Task.Run(() =>
        {
            DBAccessor.Context.LoadAsync<WayToAccount>(token, "token", (way) =>
            {
                if (way.Exception == null)
                {
                    var wayToAccount = way.Result as WayToAccount;
                    // Получаем аккаунт через имя
                    GetAccountByUsernameAsync(wayToAccount.Login_Path, callback);
                }
                else
                {
                    callback(null, way.Exception.Message);
                }
            });
        });
    }

    public async void GetAccountByEmailAsync(string email, GetAccountCallback callback)
    {
        // Получаем путь к аккаунту через почту
        await Task.Run(() =>
        {
            DBAccessor.Context.LoadAsync<WayToAccount>(email, "email", (way) =>
            {
                if (way.Exception == null)
                {
                    var wayToAccount = way.Result as WayToAccount;
                    // Получаем аккаунт через имя
                    GetAccountByUsernameAsync(wayToAccount.Login_Path, callback);
                }
                else
                {
                    callback(null, way.Exception.Message);
                }
            });
        });
    }


    public async void SavePasswordResetCodeAsync(IAccountInfoData account, string code, Action<string> callback)
    {
        await Task.Run(() =>
        {
            PasswordResetData data = new PasswordResetData
            {
                Email = account.Email,
                Login_Type = "email",
                Code = code
            };
            DBAccessor.Context.SaveAsync(data, (res) =>
            {
                if (res.Exception == null)
                {
                    callback?.Invoke(string.Empty);
                }
                else
                {
                    callback?.Invoke(res.Exception.Message);
                }
            });
        });
    }


    public async void GetPasswordResetDataAsync(string email, GetPasswordResetCallback callback)
    {
        await Task.Run(() =>
        {
            DBAccessor.Context.LoadAsync<PasswordResetData>(email, "email", (res) =>
            {
                if (res.Exception == null)
                {
                    var resetData = res.Result as PasswordResetData;
                    callback?.Invoke(resetData, string.Empty);
                }
                else
                {
                    callback?.Invoke(null, res.Exception.Message);
                }
            });
        });
    }

    public async void SaveEmailConfirmationCodeAsync(string email, string code, Action<string> callback)
    {
        await Task.Run(() =>
        {
            EmailConfirmationData data = new EmailConfirmationData
            {
                Email = email,
                Login_Type = "email",
                Code = code
            };
            DBAccessor.Context.SaveAsync(data, (res) =>
            {
                if (res.Exception == null)
                {
                    callback?.Invoke(string.Empty);
                }
                else
                {
                    callback?.Invoke(res.Exception.Message);
                }
            });
        });
    }

    public async void GetEmailConfirmationCodeAsync(string email, GetEmailConfirmationCodeCallback callback)
    {
        await Task.Run(() =>
        {
            DBAccessor.Context.LoadAsync<EmailConfirmationData>(email, "email", (res) =>
            {
                if (res.Exception == null)
                {
                    var confirmationData = res.Result as EmailConfirmationData;
                    callback(confirmationData.Code, string.Empty);
                }
                else
                {
                    callback(null, res.Exception.Message);
                }
            });
        });
    }

    public async void UpdateAccountAsync(IAccountInfoData account, Action<string> callback)
    {
        await Task.Run(() =>
        {
            AccountInfoDynamoDB oldAccData = null;
            AccountInfoDynamoDB newAccData = account as AccountInfoDynamoDB;
            // Получить старые данные о данном пользователе из БД
            DBAccessor.Context.LoadAsync<AccountInfoDynamoDB>(newAccData.Username, "main", (acc) =>
            {
                if (acc.Exception == null)
                {
                    oldAccData = acc.Result as AccountInfoDynamoDB;
                    // Если имейл изменился, то заменить его в ключе
                    if (oldAccData.Email != newAccData.Email)
                    {
                        // Новый путь от почты к аккаунту
                        WayToAccount wayToAccount = new WayToAccount
                        {
                            Username = newAccData.Email,
                            Login_Type = "email",
                            Login_Path = newAccData.Username
                        };
                        // Сохраняем новый путь от почты к аккаунту
                        DBAccessor.Context.SaveAsync(wayToAccount, (res) =>
                        {
                            if (res.Exception != null)
                            {
                                callback?.Invoke(res.Exception.Message);
                            }
                        });
                        // Удаляем старый путь от почты к аккаунту
                        DBAccessor.Context.DeleteAsync(oldAccData.Email, (res) =>
                        {
                            if (res.Exception != null)
                            {
                                callback?.Invoke(res.Exception.Message);
                            }
                        });
                    }
                    // Если токен изменился, то заменить его в ключе
                    if (oldAccData.Token != newAccData.Token)
                    {
                        // Новый путь от токена к аккаунту
                        WayToAccount wayToAccount = new WayToAccount
                        {
                            Username = newAccData.Token,
                            Login_Type = "token",
                            Login_Path = newAccData.Username
                        };
                        // Сохраняем новый путь от токена к аккаунту
                        DBAccessor.Context.SaveAsync(wayToAccount, (res) =>
                        {
                            if (res.Exception != null)
                            {
                                callback?.Invoke(res.Exception.Message);
                            }
                        });
                        // Удаляем старый путь от токена к аккаунту
                        DBAccessor.Context.DeleteAsync(oldAccData.Token, (res) =>
                        {
                            if (res.Exception != null)
                            {
                                callback?.Invoke(res.Exception.Message);
                            }
                        });
                    }
                    // Сохраняем новые данные аккаунта
                    DBAccessor.Context.SaveAsync(newAccData, (res) =>
                    {
                        if (res.Exception != null)
                        {
                            callback?.Invoke(res.Exception.Message);
                        }
                    });
                    callback?.Invoke(string.Empty);
                }
                else
                {
                    callback?.Invoke(acc.Exception.Message);
                }
            });
        });
    }

    public async void InsertNewAccountAsync(IAccountInfoData account, Action<string> callback)
    {
        await Task.Run(() =>
        {
            if (account is AccountInfoDynamoDB newData)
            {
                DBAccessor.Context.SaveAsync(newData, (res) =>
                {
                    if (res.Exception != null)
                    {
                        callback?.Invoke(res.Exception.Message);
                    }
                });
                if (!string.IsNullOrEmpty(newData.Email))
                {
                    WayToAccount wayToAccount = new WayToAccount
                    {
                        Username = newData.Email,
                        Login_Type = "email",
                        Login_Path = newData.Username
                    };
                    DBAccessor.Context.SaveAsync(wayToAccount, (res) =>
                    {
                        if (res.Exception != null)
                        {
                            callback?.Invoke(res.Exception.Message);
                        }
                    });
                }
                if (!string.IsNullOrEmpty(newData.Token))
                {
                    WayToAccount wayToAccount = new WayToAccount
                    {
                        Username = newData.Token,
                        Login_Type = "token",
                        Login_Path = newData.Username
                    };
                    DBAccessor.Context.SaveAsync(wayToAccount, (res) =>
                    {
                        if (res.Exception != null)
                        {
                            callback?.Invoke(res.Exception.Message);
                        }
                    });

                }
            }
            else
            {
                callback?.Invoke("not DynamoDB account");
                return;
            }
            callback?.Invoke(string.Empty);
        });
    }

    // Вставляем новый токен и удаляем старый, при наличии
    public async void InsertTokenAsync(IAccountInfoData account, string token, Action<string> callback)
    {
        await Task.Run(() =>
        {
            AccountInfoDynamoDB newData = account as AccountInfoDynamoDB;
            newData.Token = token;
            AccountInfoDynamoDB oldData = null;
            // Загружаем старые данные аккаунта из БД
            DBAccessor.Context.LoadAsync<AccountInfoDynamoDB>(newData.Username, "main", (acc) =>
            {
                if (acc.Exception == null)
                {
                    oldData = acc.Result as AccountInfoDynamoDB;
                    // Если старый и новый токены не равны
                    if (oldData.Token != token)
                    {
                        // если старый токен существует
                        if (!string.IsNullOrEmpty(oldData.Token))
                        {
                            // удаляем старый путь от токена к аккаунту
                            DBAccessor.Context.DeleteAsync(oldData.Token, (res) =>
                            {
                                if (res.Exception != null)
                                {
                                    callback?.Invoke(res.Exception.Message);
                                }
                            });
                        }
                        WayToAccount wayToAccount = new WayToAccount
                        {
                            Username = newData.Token,
                            Login_Type = "token",
                            Login_Path = newData.Username
                        };
                        // Сохраняем новый путь от токена к аккаунту
                        DBAccessor.Context.SaveAsync(wayToAccount, (res) =>
                        {
                            if (res.Exception != null)
                            {
                                callback?.Invoke(res.Exception.Message);
                            }
                        });
                    }
                    // Сохраняем новые данные аккаунта
                    DBAccessor.Context.SaveAsync(newData, (res) =>
                    {
                        if (res.Exception != null)
                        {
                            callback?.Invoke(res.Exception.Message);
                        }
                    });
                }
                else
                {
                    callback?.Invoke(acc.Exception.Message);
                    return;
                }
                callback?.Invoke(string.Empty);
            });
        });
    }

    public IAccountInfoData GetAccountByUsername(string username)
    {
        IAccountInfoData data = null;
        Task task = Task.Run(() =>
        {
           GetAccountByUsernameAsync(username, (cal, err) =>
           {
               if (string.IsNullOrEmpty(err))
               {
                   data = cal;
               }
           });
        });
        task.Wait(ts);
        return data;
    }

    public IAccountInfoData GetAccountByToken(string token)
    {
        IAccountInfoData data = null;
        Task task = Task.Run(() =>
        {
            GetAccountByTokenAsync(token, (cal, err) =>
            {
                if (string.IsNullOrEmpty(err))
                {
                    data = cal;
                }
            });
        });
        task.Wait(ts);
        return data;
    }

    public IAccountInfoData GetAccountByEmail(string email)
    {
        IAccountInfoData data = null;
        Task task = Task.Run(() =>
        {
            GetAccountByEmailAsync(email, (cal, err) =>
            {
                if (string.IsNullOrEmpty(err))
                {
                    data = cal;
                }
            });
        });
        task.Wait(ts);
        return data;
    }

    public void SavePasswordResetCode(IAccountInfoData account, string code)
    {
        SavePasswordResetCodeAsync(account, code, null);
    }

    public IPasswordResetData GetPasswordResetData(string email)
    {
        IPasswordResetData data = null;
        Task task = Task.Run(() =>
        {
            GetPasswordResetDataAsync(email, (cal, err) =>
            {
                if (string.IsNullOrEmpty(err))
                {
                    data = cal;
                }
            });
        });
        task.Wait(ts);
        return data;
    }

    public string GetEmailConfirmationCode(string email)
    {
        string data = null;
        Task task = Task.Run(() =>
        {
            GetEmailConfirmationCodeAsync(email, (cal, err) =>
            {
                if (string.IsNullOrEmpty(err))
                {
                    data = cal;
                }
            });
        });
        task.Wait(ts);
        return data;
    }

    public void UpdateAccount(IAccountInfoData account)
    {
        UpdateAccountAsync(account, null);
    }

    public void InsertNewAccount(IAccountInfoData account)
    {
        InsertNewAccountAsync(account, null);
    }

    public void InsertToken(IAccountInfoData account, string token)
    {
        InsertTokenAsync(account, token, null);
    }

    public void SaveEmailConfirmationCode(string email, string code)
    {
        SaveEmailConfirmationCodeAsync(email, code, null);
    }

    [DynamoDBTable("Accounts")]
    public class PasswordResetData : IPasswordResetData
    {
        [DynamoDBHashKey("Login")]
        public string Email { get; set; }
        [DynamoDBRangeKey]
        public string Login_Type { get; set; }
        [DynamoDBProperty("PasswordResetCode")]
        public string Code { get; set; }
    }

    [DynamoDBTable("Accounts")]
    public class EmailConfirmationData
    {
        [DynamoDBHashKey("Login")]
        public string Email { get; set; }
        [DynamoDBRangeKey]
        public string Login_Type { get; set; }
        [DynamoDBProperty("EmailConfirmationCode")]
        public string Code { get; set; }
    }

    [DynamoDBTable("Accounts")]
    public class WayToAccount
    {
        [DynamoDBHashKey("Login")]
        public string Username { get; set; }
        [DynamoDBRangeKey]
        public string Login_Type { get; set; }
        [DynamoDBProperty]
        public string Login_Path { get; set; }
    }

    [DynamoDBTable("Accounts")]
    public class AccountInfoDynamoDB: IAccountInfoData
    {
        [DynamoDBHashKey("Login")]
        public string Username { get; set; }
        [DynamoDBRangeKey]
        public string Login_Type { get; set; }
        [DynamoDBProperty]
        public string Password { get; set; }
        [DynamoDBProperty]
        public string Email { get; set; }
        [DynamoDBProperty]
        public string Token { get; set; }
        [DynamoDBProperty]
        public bool IsPremium { get; set; }
        [DynamoDBProperty]
        public bool IsAdmin { get; set; }
        [DynamoDBProperty]
        public bool IsBanned { get; set; }
        [DynamoDBProperty]
        public bool IsEmailConfirmed { get; set; }
        [DynamoDBProperty]
        public bool IsLicenseConfirmed { get; set; }
        [DynamoDBProperty]
        public string BanDescription { get; set; }
        [DynamoDBProperty]
        public string Unban_Date { get; set; }
        [DynamoDBIgnore]
        public bool IsGuest { get; set; }
        
        public Dictionary<string, string> Properties { get; set; }

        public event Action<IAccountInfoData> OnChangedEvent;

        public AccountInfoDynamoDB()
        {
            Username = string.Empty;
            Login_Type = "main";
            Password = string.Empty;
            Email = string.Empty;
            Token = string.Empty;
            IsPremium = false;
            IsAdmin = false;
            IsBanned = false;
            IsEmailConfirmed = false;
            IsLicenseConfirmed = false;
            BanDescription = "This user has not received a ban yet";
            Unban_Date = string.Empty;
            IsGuest = false;
            Properties = new Dictionary<string, string>();
        }

        public void MarkAsDirty()
        {
            OnChangedEvent?.Invoke(this);
        }

        public bool HasToken()
        {
            return !string.IsNullOrEmpty(Token);
        }

        public bool IsTokenExpired()
        {
            throw new NotImplementedException();
        }
    }
}
