using System;
using System.Threading.Tasks;

namespace Barebones.MasterServer
{
    public delegate void GetAccountCallback(IAccountInfoData accountInfo, string error);
    public delegate void GetPasswordResetCallback(IPasswordResetData passwordResetInfo, string error);
    public delegate void GetEmailConfirmationCodeCallback(string code, string error);

    public interface IAccountsDatabaseAccessor
    {
        /// <summary>
        /// Should create an empty object with account data.
        /// </summary>
        /// <returns></returns>
        IAccountInfoData CreateAccountInstance();
        /// <summary>
        /// Gets user account from database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        IAccountInfoData GetAccountByUsername(string username);
        /// <summary>
        /// Gets user account from database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        void GetAccountByUsernameAsync(string username, GetAccountCallback callback);
        /// <summary>
        /// Gets user account from database by token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        IAccountInfoData GetAccountByToken(string token);
        /// <summary>
        /// Gets user account from database by token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        void GetAccountByTokenAsync(string token, GetAccountCallback callback);
        /// <summary>
        /// Gets user account from database by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        IAccountInfoData GetAccountByEmail(string email);
        /// <summary>
        /// Gets user account from database by email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        void GetAccountByEmailAsync(string email, GetAccountCallback callback);
        /// <summary>
        /// Saves code that user gets when reset pasword request
        /// </summary>
        /// <param name="account"></param>
        /// <param name="code"></param>
        void SavePasswordResetCode(IAccountInfoData account, string code);
        /// <summary>
        /// Saves code that user gets when reset pasword request
        /// </summary>
        /// <param name="account"></param>
        /// <param name="code"></param>
        /// <param name="callback"></param>
        void SavePasswordResetCodeAsync(IAccountInfoData account, string code, Action<string> callback);
        /// <summary>
        /// Get data for password reset
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        IPasswordResetData GetPasswordResetData(string email);
        /// <summary>
        /// Get data for password reset
        /// </summary>
        /// <param name="email"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        void GetPasswordResetDataAsync(string email, GetPasswordResetCallback callback);
        /// <summary>
        /// Email confirmation code user gets after successful registration
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        void SaveEmailConfirmationCode(string email, string code);
        /// <summary>
        /// Email confirmation code user gets after successful registration
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <param name="callback"></param>
        void SaveEmailConfirmationCodeAsync(string email, string code, Action<string> callback);
        /// <summary>
        /// Get email confirmation code for user after successful registration
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        string GetEmailConfirmationCode(string email);
        /// <summary>
        /// Get email confirmation code for user after successful registration
        /// </summary>
        /// <param name="email"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        void GetEmailConfirmationCodeAsync(string email, GetEmailConfirmationCodeCallback callback);
        /// <summary>
        /// Update all account information in database
        /// </summary>
        /// <param name="account"></param>
        void UpdateAccount(IAccountInfoData account);
        /// <summary>
        /// Update all account information in database
        /// </summary>
        /// <param name="account"></param>
        /// <param name="callback"></param>
        void UpdateAccountAsync(IAccountInfoData account, Action<string> callback);
        /// <summary>
        /// Create new account in database
        /// </summary>
        /// <param name="account"></param>
        void InsertNewAccount(IAccountInfoData account);
        /// <summary>
        /// Create new account in database
        /// </summary>
        /// <param name="account"></param>
        /// <param name="callback"></param>
        void InsertNewAccountAsync(IAccountInfoData account, Action<string> callback);
        /// <summary>
        /// Insert account token to database
        /// </summary>
        /// <param name="account"></param>
        /// <param name="token"></param>
        void InsertToken(IAccountInfoData account, string token);
        /// <summary>
        /// Insert account token to database
        /// </summary>
        /// <param name="account"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        void InsertTokenAsync(IAccountInfoData account, string token, Action<string> callback);
    }
}