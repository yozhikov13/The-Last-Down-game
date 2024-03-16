﻿using Aevien.Utilities;
using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Authentication module, which handles logging in and registration of accounts
    /// </summary>
    public class AuthModule : BaseServerModule
    {
        /// <summary>
        /// Censor module for bad words checking :)
        /// </summary>
        protected CensorModule censorModule;

        /// <summary>
        /// Unique ID for guest name postfix
        /// </summary>
        protected int nextGuestId;

        /// <summary>
        /// Min number of username characters
        /// </summary>
        [Header("Settings"), SerializeField]
        protected int usernameMinChars = 4;

        /// <summary>
        /// Max number of username characters
        /// </summary>
        [SerializeField]
        protected int usernameMaxChars = 12;

        [SerializeField]
        protected bool useEmailConfirmation = true;

        [Header("Guest Settings")]
        [SerializeField, Tooltip("If true, players will be able to log in as guests")]
        protected bool enableGuestLogin = true;

        [SerializeField, Tooltip("Guest names will start with this prefix")]
        protected string guestPrefix = "Guest";

        [SerializeField, Tooltip("Guest names will be generated as normal human names")]
        protected bool useGuestFriendlyName = true;

        [SerializeField, Tooltip("Minimal permission level, required to retrieve peer account information")]
        protected int getPeerDataPermissionsLevel = 0;

        [Header("E-Mail Settings"), SerializeField]
        protected Mailer mailer;

        [SerializeField, TextArea(3, 10)]
        public string emailAddressValidationTemplate = @"^[a-z0-9][-a-z0-9._]+@([-a-z0-9]+\.)+[a-z]{2,5}$";

        /// <summary>
        /// Collection of users who are currently logged in
        /// </summary>
        public Dictionary<string, IUserPeerExtension> LoggedInUsers { get; protected set; }

        /// <summary>
        /// Invoked, when user logedin
        /// </summary>
        public event Action<IUserPeerExtension> OnUserLoggedInEvent;

        /// <summary>
        /// Invoked, when user logs out
        /// </summary>
        public event Action<IUserPeerExtension> OnUserLoggedOutEvent;

        /// <summary>
        /// Invoked, when user successfully registers an account
        /// </summary>
        public event Action<IPeer, IAccountInfoData> OnRegisteredEvent;

        /// <summary>
        /// Invoked, when user successfully confirms his e-mail
        /// </summary>
        public event Action<IAccountInfoData> OnEmailConfirmedEvent;

        protected override void Awake()
        {
            base.Awake();

            // Optional dependancy to CensorModule
            AddOptionalDependency<CensorModule>();
        }

        public override void Initialize(IServer server)
        {
            censorModule = server.GetModule<CensorModule>();
            mailer = mailer ?? FindObjectOfType<Mailer>();

            // Init logged in users list
            LoggedInUsers = new Dictionary<string, IUserPeerExtension>();

            // Set handlers
            server.SetHandler((short)MsfMessageCodes.SignInRequest, SignInRequestHandler);
            server.SetHandler((short)MsfMessageCodes.SignUpRequest, SignUpRequestHandler);

            server.SetHandler((short)MsfMessageCodes.PasswordResetCodeRequest, PasswordResetRequestHandler);
            server.SetHandler((short)MsfMessageCodes.ChangePasswordRequest, ChangePasswordRequestHandler);

            server.SetHandler((short)MsfMessageCodes.EmailConfirmationCodeRequest, EmailConfirmationCodeRequestHandler);
            server.SetHandler((short)MsfMessageCodes.EmailConfirmationRequest, EmailConfirmationRequestHandler);

            server.SetHandler((short)MsfMessageCodes.GetLoggedInUsersCountRequest, GetLoggedInUsersCountRequestHandler);

            server.SetHandler((short)MsfMessageCodes.GetPeerAccountInfoRequest, GetPeerAccountInfoRequestHandler);
        }

        /// <summary>
        /// Generate new guest username
        /// </summary>
        /// <returns></returns>
        public string GenerateGuestUsername()
        {
            return $"{guestPrefix}_{nextGuestId++}";
        }

        /// <summary>
        /// Get logged in user by Username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public IUserPeerExtension GetLoggedInUser(string username)
        {
            if (LoggedInUsers.TryGetValue(username.ToLower(), out IUserPeerExtension user))
            {
                return user;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Check if given user is logged in
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsUserLoggedIn(string username)
        {
            return LoggedInUsers.ContainsKey(username.ToLower());
        }

        /// <summary>
        /// Check if given peer has permission to get peer info
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public bool HasGetPeerInfoPermissions(IPeer peer)
        {
            var extension = peer.GetExtension<SecurityInfoPeerExtension>();
            return extension.PermissionLevel >= getPeerDataPermissionsLevel;
        }

        /// <summary>
        /// Create instance of <see cref="UserPeerExtension"/>
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public virtual IUserPeerExtension CreateUserPeerExtension(IPeer peer)
        {
            return new UserPeerExtension(peer);
        }

        /// <summary>
        /// Fired when any user disconected from server
        /// </summary>
        /// <param name="peer"></param>
        private void OnUserDisconnectedEventListener(IPeer peer)
        {
            peer.OnPeerDisconnectedEvent -= OnUserDisconnectedEventListener;

            var extension = peer.GetExtension<IUserPeerExtension>();

            if (extension == null)
            {
                return;
            }

            LoggedInUsers.Remove(extension.Username.ToLower());
            OnUserLoggedOutEvent?.Invoke(extension);
        }

        /// <summary>
        /// Check if Username is valid. Whether it is not empty or has no white spaces
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        protected virtual bool IsUsernameValid(string username)
        {
            string lowerUserName = username.ToLower();
            return !string.IsNullOrWhiteSpace(lowerUserName) && !lowerUserName.Contains(" ");
        }

        /// <summary>
        /// Check if Email is valid
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        protected virtual bool IsEmailValid(string email)
        {
            return Regex.IsMatch(email, emailAddressValidationTemplate);
        }

        #region MESSAGE HANDLERS

        /// <summary>
        /// Handles client's request to change password
        /// </summary>
        /// <param name="message"></param>
        protected virtual void ChangePasswordRequestHandler(IIncommingMessage message)
        {
            var data = new Dictionary<string, string>().FromBytes(message.AsBytes());

            if (!data.ContainsKey("code") || !data.ContainsKey("password") || !data.ContainsKey("email"))
            {
                message.Respond("Invalid request", ResponseStatus.Unauthorized);
                return;
            }

            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();
            var passwordResetData = authDbAccessor.GetPasswordResetData(data["email"]);

            if (passwordResetData == null || passwordResetData.Code == null || passwordResetData.Code != data["code"])
            {
                message.Respond("Invalid code provided", ResponseStatus.Unauthorized);
                return;
            }

            var account = authDbAccessor.GetAccountByEmail(data["email"]);

            // Delete (overwrite) code used
            authDbAccessor.SavePasswordResetCode(account, null);

            account.Password = Msf.Security.CreateHash(data["password"]);
            authDbAccessor.UpdateAccount(account);

            message.Respond(ResponseStatus.Success);
        }

        /// <summary>
        /// Handles a request to retrieve a number of logged in users
        /// </summary>
        /// <param name="message"></param>
        protected virtual void GetLoggedInUsersCountRequestHandler(IIncommingMessage message)
        {
            message.Respond(LoggedInUsers.Count, ResponseStatus.Success);
        }

        /// <summary>
        /// Handles password reset request
        /// </summary>
        /// <param name="message"></param>
        protected virtual void PasswordResetRequestHandler(IIncommingMessage message)
        {
            var userEmail = message.AsString();
            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();
            var userAccount = authDbAccessor.GetAccountByEmail(userEmail);

            if (userAccount == null)
            {
                message.Respond("No such e-mail in the system", ResponseStatus.Unauthorized);
                return;
            }

            var passwordResetCode = Msf.Helper.CreateRandomString(6);
            authDbAccessor.SavePasswordResetCode(userAccount, passwordResetCode);

            StringBuilder emailBody = new StringBuilder();
            emailBody.Append($"<h3>You have requested reset password</h3>");
            emailBody.Append($"<p>Here is your reset code</p>");
            emailBody.Append($"<h1>{passwordResetCode}</h1>");
            emailBody.Append($"<p>Copy this code and paste it to your reset password form</p>");

            if (!mailer.SendMail(userAccount.Email, "Password Reset Code", emailBody.ToString()))
            {
                message.Respond("Couldn't send an activation code to your e-mail");
                return;
            }

            message.Respond(ResponseStatus.Success);
        }

        /// <summary>
        /// Handles e-mail confirmation request
        /// </summary>
        /// <param name="message"></param>
        protected virtual void EmailConfirmationRequestHandler(IIncommingMessage message)
        {
            var confirmationCode = message.AsString();
            var userExtension = message.Peer.GetExtension<IUserPeerExtension>();

            if (userExtension == null || userExtension.Account == null)
            {
                message.Respond("Invalid session", ResponseStatus.Unauthorized);
                return;
            }

            if (userExtension.Account.IsGuest)
            {
                message.Respond("Guests cannot confirm e-mails", ResponseStatus.Unauthorized);
                return;
            }

            if (userExtension.Account.IsEmailConfirmed)
            {
                // We still need to respond with "success" in case
                // response is handled somehow on the client
                message.Respond("Your email is already confirmed", ResponseStatus.Success);
                return;
            }

            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();
            var requiredCode = authDbAccessor.GetEmailConfirmationCode(userExtension.Account.Email);

            if (requiredCode != confirmationCode)
            {
                message.Respond("Invalid activation code", ResponseStatus.Error);
                return;
            }

            // Confirm e-mail
            userExtension.Account.IsEmailConfirmed = true;

            // Update account
            authDbAccessor.UpdateAccount(userExtension.Account);

            // Respond with success
            message.Respond(ResponseStatus.Success);

            // Invoke the event
            OnEmailConfirmedEvent?.Invoke(userExtension.Account);
        }

        /// <summary>
        /// Handles request to get email conformation code
        /// </summary>
        /// <param name="message"></param>
        protected virtual void EmailConfirmationCodeRequestHandler(IIncommingMessage message)
        {
            var userExtension = message.Peer.GetExtension<IUserPeerExtension>();

            if (userExtension == null || userExtension.Account == null)
            {
                message.Respond("Invalid session", ResponseStatus.Unauthorized);
                return;
            }

            if (userExtension.Account.IsGuest)
            {
                message.Respond("Guests cannot confirm e-mails", ResponseStatus.Unauthorized);
                return;
            }

            var newEmailConfirmationCode = Msf.Helper.CreateRandomString(6);
            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();

            authDbAccessor.SaveEmailConfirmationCode(userExtension.Account.Email, newEmailConfirmationCode);

            if(mailer == null)
            {
                message.Respond("Couldn't send a confirmation code to your e-mail. Please contact support", ResponseStatus.Error); 
                return;
            }

            StringBuilder emailBody = new StringBuilder();
            emailBody.Append($"<h3>You have requested email activation</h3>");
            emailBody.Append($"<p>Here is your email activation code</p>");
            emailBody.Append($"<h1>{newEmailConfirmationCode}</h1>");
            emailBody.Append($"<p>Copy this code and paste it to your account activation form</p>");

            if (!mailer.SendMail(userExtension.Account.Email, "E-mail confirmation", emailBody.ToString()))
            {
                message.Respond("Couldn't send a confirmation code to your e-mail. Please contact support", ResponseStatus.Error);
                return;
            }

            // Respond with success
            message.Respond(ResponseStatus.Success);
        }

        /// <summary>
        /// Handles account registration request
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SignUpRequestHandler(IIncommingMessage message)
        {
            var encryptedData = message.AsBytes();

            var securityExt = message.Peer.GetExtension<SecurityInfoPeerExtension>();
            var aesKey = securityExt.AesKey;

            if (aesKey == null)
            {
                // There's no aesKey that client and master agreed upon
                message.Respond("Insecure request".ToBytes(), ResponseStatus.Unauthorized);
                return;
            }

            var decryptedBytesData = Msf.Security.DecryptAES(encryptedData, aesKey);
            var userCredentials = new Dictionary<string, string>().FromBytes(decryptedBytesData);

            if (!userCredentials.ContainsKey("username") || !userCredentials.ContainsKey("password") || !userCredentials.ContainsKey("email"))
            {
                message.Respond("Invalid registration request".ToBytes(), ResponseStatus.Error);
                return;
            }

            var userName = userCredentials["username"];
            var userPassword = userCredentials["password"];
            var userEmail = userCredentials["email"].ToLower();

            var userExtension = message.Peer.GetExtension<IUserPeerExtension>();

            if (userExtension != null && !userExtension.Account.IsGuest)
            {
                // Fail, if user is already logged in, and not with a guest account
                message.Respond("Invalid registration request".ToBytes(), ResponseStatus.Error);
                return;
            }

            // Check if username is valid
            if (!IsUsernameValid(userName))
            {
                message.Respond("Invalid Username".ToBytes(), ResponseStatus.Error);
                return;
            }

            // Check if there's a forbidden word in username
            if (censorModule != null && censorModule.HasCensoredWord(userName))
            {
                message.Respond("Forbidden word used in username".ToBytes(), ResponseStatus.Error);
                return;
            }

            // Check if username length is good
            if ((userName.Length < usernameMinChars) || (userName.Length > usernameMaxChars))
            {
                message.Respond($"Invalid usernanme length. Min length is {usernameMinChars} and max length is {usernameMaxChars}".ToBytes(), ResponseStatus.Error);

                return;
            }

            // Check if email is valid
            if (!IsEmailValid(userEmail))
            {
                message.Respond("Invalid Email".ToBytes(), ResponseStatus.Error);
                return;
            }

            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();

            var userAccount = authDbAccessor.CreateAccountInstance();
            userAccount.Username = userName;
            userAccount.Email = userEmail;
            userAccount.Password = Msf.Security.CreateHash(userPassword);

            try
            {
                authDbAccessor.InsertNewAccount(userAccount);

                if (OnRegisteredEvent != null)
                {
                    OnRegisteredEvent.Invoke(message.Peer, userAccount);
                }

                message.Respond(ResponseStatus.Success);
            }
            catch (Exception e)
            {
                Logs.Error(e);
                message.Respond("Username or E-mail is already registered".ToBytes(), ResponseStatus.Error);
            }
        }

        /// <summary>
        /// Handles a request to retrieve account information
        /// </summary>
        /// <param name="message"></param>
        protected virtual void GetPeerAccountInfoRequestHandler(IIncommingMessage message)
        {
            if (!HasGetPeerInfoPermissions(message.Peer))
            {
                message.Respond("Unauthorized", ResponseStatus.Unauthorized);
                return;
            }

            var peerId = message.AsInt();
            var peer = Server.GetPeer(peerId);

            if (peer == null)
            {
                message.Respond("Peer with a given ID is not in the game", ResponseStatus.Error);
                return;
            }

            var userExtension = peer.GetExtension<IUserPeerExtension>();

            if (userExtension == null)
            {
                message.Respond("Peer has not been authenticated", ResponseStatus.Failed);
                return;
            }

            var userAccount = userExtension.Account;

            var userAccountPacket = new PeerAccountInfoPacket()
            {
                PeerId = peerId,
                CustomOptions = new DictionaryOptions(userAccount.Properties),
                Username = userExtension.Username
            };

            message.Respond(userAccountPacket, ResponseStatus.Success);
        }

        /// <summary>
        /// Handles a request to log in
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SignInRequestHandler(IIncommingMessage message)
        {
            logger.Debug($"Signing in user {message.Peer.Id}...");

            if (message.Peer.HasExtension<IUserPeerExtension>())
            {
                logger.Debug("You are already logged in");
                message.Respond("You are already logged in", ResponseStatus.Unauthorized);
                return;
            }

            var encryptedData = message.AsBytes();
            var securityExt = message.Peer.GetExtension<SecurityInfoPeerExtension>();
            var aesKey = securityExt.AesKey;

            if (aesKey == null)
            {
                // There's no aesKey that client and master agreed upon
                message.Respond("Insecure request".ToBytes(), ResponseStatus.Unauthorized);
                return;
            }

            var decryptedBytesData = Msf.Security.DecryptAES(encryptedData, aesKey);
            var userCredentials = new DictionaryOptions(new Dictionary<string, string>().FromBytes(decryptedBytesData));
            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();

            IAccountInfoData userAccount = null;

            // ---------------------------------------------
            // Guest Authentication
            if (userCredentials.Has("guest") && enableGuestLogin)
            {
                userAccount = authDbAccessor.CreateAccountInstance();
                userAccount.Username = GenerateGuestUsername();
                userAccount.IsGuest = true;
                userAccount.IsAdmin = false;
            }

            // ----------------------------------------------
            // Token Authentication
            if (userCredentials.Has("token") && userAccount == null)
            {
                userAccount = authDbAccessor.GetAccountByToken(userCredentials.AsString("token"));

                if (userAccount == null)
                {
                    message.Respond("Your session token has expired".ToBytes(), ResponseStatus.Unauthorized);
                    return;
                }

                if (userAccount.Properties.ContainsKey("expires"))
                {
                    long filetime = Convert.ToInt64(userAccount.Properties["expires"]);
                    
                    if(DateTime.FromFileTime(filetime) <= DateTime.Now)
                    {
                        message.Respond("Your session token has expired".ToBytes(), ResponseStatus.Unauthorized);
                        return;
                    }
                }

                // If another session found
                if (IsUserLoggedIn(userAccount.Username))
                {
                    // And respond to requester
                    message.Respond("This account is already logged in".ToBytes(), ResponseStatus.Unauthorized);
                    return;
                }

                userAccount.Properties["expires"] = DateTime.Now.AddDays(7).ToFileTime().ToString();
                authDbAccessor.UpdateAccount(userAccount);
            }

            // ----------------------------------------------
            // Username / Password authentication

            if (userCredentials.Has("username") && userCredentials.Has("password") && userAccount == null)
            {
                var userName = userCredentials.AsString("username");
                var userPassword = userCredentials.AsString("password");

                userAccount = authDbAccessor.GetAccountByUsername(userName);

                if (userAccount == null)
                {
                    // Couldn't find an account with this name
                    message.Respond("Invalid Credentials".ToBytes(), ResponseStatus.Unauthorized);
                    return;
                }

                if (!Msf.Security.ValidatePassword(userPassword, userAccount.Password))
                {
                    // Password is not correct
                    message.Respond("Invalid Credentials".ToBytes(), ResponseStatus.Unauthorized);
                    return;
                }

                // If another session found
                if (IsUserLoggedIn(userAccount.Username))
                {
                    // And respond to requester
                    message.Respond("This account is already logged in".ToBytes(), ResponseStatus.Unauthorized);
                    return;
                }

                userAccount.Token = Msf.Helper.CreateRandomString(64);
                userAccount.Properties["expires"] = DateTime.Now.AddDays(7).ToFileTime().ToString();

                if (!useEmailConfirmation)
                {
                    userAccount.IsEmailConfirmed = true;
                }

                authDbAccessor.UpdateAccount(userAccount);
            }

            if (userAccount == null)
            {
                message.Respond("Invalid request", ResponseStatus.Unauthorized);
                return;
            }

            // Setup auth extension
            var userExtension = message.Peer.AddExtension(CreateUserPeerExtension(message.Peer));
            userExtension.Account = userAccount;

            // Listen to disconnect event
            userExtension.Peer.OnPeerDisconnectedEvent += OnUserDisconnectedEventListener;

            // Add to lookup of logged in users
            LoggedInUsers.Add(userExtension.Username.ToLower(), userExtension);

            logger.Debug($"User {message.Peer.Id} signed in as {userAccount.Username}");

            // Send response to logged in user
            message.Respond(userExtension.CreateAccountInfoPacket().ToBytes(), ResponseStatus.Success);

            // Trigger the login event
            OnUserLoggedInEvent?.Invoke(userExtension);
        }

        #endregion
    }
}