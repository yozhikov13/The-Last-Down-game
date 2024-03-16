using Aevien.Utilities;
using Barebones.MasterServer;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Barebones.MasterServer.Examples.BasicAuthorization
{
    public class AccountsDatabaseAccessor : IAccountsDatabaseAccessor
    {
        private readonly ILiteCollection<AccountInfoLiteDb> accounts;
        private readonly ILiteCollection<PasswordResetData> resetCodes;
        private readonly ILiteCollection<EmailConfirmationData> emailConfirmationCodes;

        private readonly LiteDatabase database;

        public AccountsDatabaseAccessor(LiteDatabase database)
        {
            this.database = database;

            accounts = this.database.GetCollection<AccountInfoLiteDb>("accounts");
            accounts.EnsureIndex(a => a.Username, true);
            accounts.EnsureIndex(a => a.Email, true);

            resetCodes = this.database.GetCollection<PasswordResetData>("resetCodes");
            resetCodes.EnsureIndex(a => a.Email, true);

            emailConfirmationCodes = this.database.GetCollection<EmailConfirmationData>("emailConfirmationCodes");
            emailConfirmationCodes.EnsureIndex(a => a.Email, true);
        }

        public IAccountInfoData CreateAccountInstance()
        {
            return new AccountInfoLiteDb();
        }

        public IAccountInfoData GetAccountByUsername(string username)
        {
            return accounts.FindOne(a => a.Username == username);
        }

        public async void GetAccountByUsernameAsync(string username, GetAccountCallback callback)
        {
            IAccountInfoData account = default;

            try
            {
                await Task.Run(() =>
                {
                    account = GetAccountByUsername(username);
                });

                callback?.Invoke(account, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(null, e.Message);
            }
        }

        public IAccountInfoData GetAccountByToken(string token)
        {
            return accounts.FindOne(a => a.Token == token);
        }

        public async void GetAccountByTokenAsync(string token, GetAccountCallback callback)
        {
            IAccountInfoData account = default;

            try
            {
                await Task.Run(() =>
                {
                    account = GetAccountByToken(token);
                });

                callback?.Invoke(account, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(null, e.Message);
            }
        }

        public IAccountInfoData GetAccountByEmail(string email)
        {
            return accounts.FindOne(i => i.Email == email.ToLower());
        }

        public async void GetAccountByEmailAsync(string email, GetAccountCallback callback)
        {
            IAccountInfoData account = default;

            try
            {
                await Task.Run(() =>
                {
                    account = GetAccountByEmail(email);
                });

                callback?.Invoke(account, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(null, e.Message);
            }
        }

        public void SavePasswordResetCode(IAccountInfoData account, string code)
        {
            resetCodes.DeleteMany(i => i.Email == account.Email.ToLower());
            resetCodes.Insert(new PasswordResetData()
            {
                Email = account.Email,
                Code = code
            });
        }

        public async void SavePasswordResetCodeAsync(IAccountInfoData account, string code, Action<string> callback)
        {
            try
            {
                await Task.Run(() => SavePasswordResetCode(account, code));
                callback?.Invoke(string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(e.Message);
            }
        }

        public IPasswordResetData GetPasswordResetData(string email)
        {
            return resetCodes.FindOne(i => i.Email == email.ToLower());
        }

        public async void GetPasswordResetDataAsync(string email, GetPasswordResetCallback callback)
        {
            IPasswordResetData passwordResetData = default;

            try
            {
                await Task.Run(() =>
                {
                    passwordResetData = GetPasswordResetData(email);
                });

                callback?.Invoke(passwordResetData, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(null, e.Message);
            }
        }

        public void SaveEmailConfirmationCode(string email, string code)
        {
            emailConfirmationCodes.DeleteMany(i => i.Email == email.ToLower());

            emailConfirmationCodes.Insert(new EmailConfirmationData()
            {
                Code = code,
                Email = email
            });
        }

        public async void SaveEmailConfirmationCodeAsync(string email, string code, Action<string> callback)
        {
            try
            {
                await Task.Run(() => SaveEmailConfirmationCode(email, code));
                callback?.Invoke(string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(e.Message);
            }
        }

        public string GetEmailConfirmationCode(string email)
        {
            var entry = emailConfirmationCodes.FindOne(i => i.Email == email);
            return entry != null ? entry.Code : string.Empty;
        }

        public async void GetEmailConfirmationCodeAsync(string email, GetEmailConfirmationCodeCallback callback)
        {
            string code = string.Empty;

            try
            {
                await Task.Run(() =>
                {
                    code = GetEmailConfirmationCode(email);
                });

                callback?.Invoke(code, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(string.Empty, e.Message);
            }
        }

        public void UpdateAccount(IAccountInfoData account)
        {
            accounts.Update(account as AccountInfoLiteDb);
        }

        public async void UpdateAccountAsync(IAccountInfoData account, Action<string> callback)
        {
            try
            {
                await Task.Run(() => UpdateAccount(account));
                callback?.Invoke(string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(e.Message);
            }
        }

        public void InsertNewAccount(IAccountInfoData account)
        {
            accounts.Insert(account as AccountInfoLiteDb);
        }

        public async void InsertNewAccountAsync(IAccountInfoData account, Action<string> callback)
        {
            try
            {
                await Task.Run(() => InsertNewAccount(account));
                callback?.Invoke(string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(e.Message);
            }
        }

        public void InsertToken(IAccountInfoData account, string token)
        {
            account.Token = token;
            accounts.Update(account as AccountInfoLiteDb);
        }

        public async void InsertTokenAsync(IAccountInfoData account, string token, Action<string> callback)
        {
            try
            {
                await Task.Run(() => InsertToken(account, token));
                callback?.Invoke(string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(e.Message);
            }
        }

        private class PasswordResetData : IPasswordResetData
        {
            [BsonId]
            public string Email { get; set; }
            public string Code { get; set; }
        }

        private class EmailConfirmationData
        {
            [BsonId]
            public string Email { get; set; }
            public string Code { get; set; }
        }

        /// <summary>
        /// LiteDB implementation of account data
        /// </summary>
        private class AccountInfoLiteDb : IAccountInfoData
        {
            [BsonId]
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public string Token { get; set; }
            public bool IsAdmin { get; set; }
            public bool IsGuest { get; set; }
            public bool IsEmailConfirmed { get; set; }
            public Dictionary<string, string> Properties { get; set; }

            public event Action<IAccountInfoData> OnChangedEvent;

            public AccountInfoLiteDb()
            {
                Username = string.Empty;
                Password = string.Empty;
                Email = string.Empty;
                Token = string.Empty;
                IsAdmin = false;
                IsGuest = false;
                IsEmailConfirmed = false;
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
}
