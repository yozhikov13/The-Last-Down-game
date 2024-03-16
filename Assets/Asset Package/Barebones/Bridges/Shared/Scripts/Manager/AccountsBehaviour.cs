using Aevien.UI;
using Barebones.MasterServer;
using Barebones.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.Games
{
    [AddComponentMenu("MSF/Shared/AccountsBehaviour")]
    public class AccountsBehaviour : BaseClientBehaviour
    {
        #region INSPECTOR

        [Header("Components"), SerializeField]
        protected ClientToMasterConnector clientToMasterConnector;

        [Header("Settings"), SerializeField]
        protected bool rememberUser = true;

        [Header("Editor Settings"), SerializeField]
        protected string defaultUsername = "qwerty";
        [SerializeField]
        protected string defaultEmail = "qwerty@mail.com";
        [SerializeField]
        protected string defaultPassword = "qwerty123!@#";
        [SerializeField]
        protected bool useDefaultCredentials = false;

        public UnityEvent OnSignedInEvent;
        public UnityEvent OnSignedOutEvent;
        public UnityEvent OnEmailConfirmedEvent;
        public UnityEvent OnPasswordChangedEvent;

        #endregion

        protected string outputMessage = string.Empty;

        protected SignInView signinView;
        protected SignUpView signupView;
        protected PasswordResetView passwordResetView;
        protected PasswordResetCodeView passwordResetCodeView;
        protected EmailConfirmationView emailConfirmationView;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Connection?.RemoveConnectionListener(OnClientConnectedToServer);
            Connection?.RemoveDisconnectionListener(OnClientDisconnectedFromServer);
        }

        protected override void OnInitialize()
        {
            FindOrCreateMasterConnector();

            signinView = ViewsManager.GetView<SignInView>("SigninView");
            signupView = ViewsManager.GetView<SignUpView>("SignupView");
            passwordResetView = ViewsManager.GetView<PasswordResetView>("PasswordResetView");
            passwordResetCodeView = ViewsManager.GetView<PasswordResetCodeView>("PasswordResetCodeView");
            emailConfirmationView = ViewsManager.GetView<EmailConfirmationView>("EmailConfirmationView");

            Msf.Client.Auth.RememberMe = rememberUser;

            MsfTimer.WaitForEndOfFrame(() =>
            {
                if (useDefaultCredentials && Application.isEditor)
                {
                    if (signinView)
                    {
                        signinView.SetInputFieldsValues(defaultUsername, defaultPassword);
                    }

                    if (signupView)
                    {
                        signupView.SetInputFieldsValues(defaultUsername, defaultEmail, defaultPassword);
                    }
                }

                Connection.AddConnectionListener(OnClientConnectedToServer);
                Connection.AddDisconnectionListener(OnClientDisconnectedFromServer, false);

                if (!Connection.IsConnected && !Connection.IsConnecting)
                {
                    Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Connecting to master... Please wait!");
                    clientToMasterConnector.StartConnection();
                }
            });
        }

        protected virtual void OnClientConnectedToServer()
        {
            Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

            if (Msf.Client.Auth.IsSignedIn)
            {
                OnSignedInEvent?.Invoke();
            }
            else
            {
                if (Msf.Client.Auth.HasAuthToken())
                {
                    MsfTimer.WaitForSeconds(0.2f, () => {
                        SignInWithToken();
                    });
                }
                else
                {
                    if (!signinView)
                    {
                        ViewsManager.NotifyNoViewFound("SigninView");
                        return;
                    }

                    signinView.Show();
                }
            }
        }

        protected virtual void OnClientDisconnectedFromServer()
        {
            Msf.Events.Invoke(MsfEventKeys.showOkDialogBox,
                new OkDialogBoxViewEventMessage("The connection to the server has been lost. "
                + "Please try again or contact the developers of the game or your internet provider.",
                () =>
                {
                    SignOut();
                }));
        }

        protected virtual void FindOrCreateMasterConnector()
        {
            if (!clientToMasterConnector)
                clientToMasterConnector = FindObjectOfType<ClientToMasterConnector>();

            if (!clientToMasterConnector)
            {
                var connectorObject = new GameObject("--CLIENT_TO_MASTER_CONNECTOR");
                clientToMasterConnector = connectorObject.AddComponent<ClientToMasterConnector>();
            }
        }

        public void SignIn()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Signing in... Please wait!");

            logger.Debug("Signing in... Please wait!");

            if (!signinView)
            {
                ViewsManager.NotifyNoViewFound("SigninView");
                return;
            }

            if (!emailConfirmationView)
            {
                ViewsManager.NotifyNoViewFound("EmailConfirmationView");
                return;
            }

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Auth.SignIn(signinView.Username, signinView.Password, (accountInfo, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (accountInfo != null)
                    {
                        signinView.Hide();

                        if (accountInfo.IsEmailConfirmed)
                        {
                            OnSignedInEvent?.Invoke();
                            logger.Debug($"You are successfully logged in as {Msf.Client.Auth.AccountInfo}");
                        }
                        else
                        {
                            emailConfirmationView.Show();
                        }
                    }
                    else
                    {
                        outputMessage = $"An error occurred while signing in: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void SignInWithToken()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Signing in... Please wait!");

            logger.Debug("Signing in... Please wait!");

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Auth.SignInWithToken((accountInfo, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (accountInfo != null)
                    {
                        if (accountInfo.IsEmailConfirmed)
                        {
                            OnSignedInEvent?.Invoke();
                            logger.Debug($"You are successfully logged in. {Msf.Client.Auth.AccountInfo}");
                        }
                        else
                        {
                            if (!emailConfirmationView)
                            {
                                ViewsManager.NotifyNoViewFound("EmailConfirmationView");
                                return;
                            }

                            emailConfirmationView.Show();
                        }
                    }
                    else
                    {
                        outputMessage = $"An error occurred while signing in: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void SignUp()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Signing up... Please wait!");

            if (!signinView)
            {
                ViewsManager.NotifyNoViewFound("SigninView");
                return;
            }

            if (!signupView)
            {
                ViewsManager.NotifyNoViewFound("SignUpView");
                return;
            }

            MsfTimer.WaitForSeconds(1f, () =>
            {
                string username = signupView.Username;
                string email = signupView.Email;
                string password = signupView.Password;

                var credentials = new Dictionary<string, string>
                {
                    { "username", username },
                    { "email", email },
                    { "password", password }
                };

                Msf.Client.Auth.SignUp(credentials, (isSuccessful, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (isSuccessful)
                    {
                        signupView.Hide();
                        signinView.SetInputFieldsValues(username, password);
                        signinView.Show();

                        logger.Debug($"You have successfuly signed up. Now you may sign in");
                    }
                    else
                    {
                        outputMessage = $"An error occurred while signing up: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void SignInAsGuest()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Signing in... Please wait!");

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Auth.SignInAsGuest((accountInfo, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (accountInfo != null)
                    {
                        signinView.Hide();

                        OnSignedInEvent?.Invoke();
                        logger.Debug($"You are successfully logged in as {Msf.Client.Auth.AccountInfo.Username}");
                    }
                    else
                    {
                        outputMessage = $"An error occurred while signing in: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void SignOut()
        {
            logger.Debug("Sign out");

            OnSignedOutEvent?.Invoke();
            Msf.Client.Auth.SignOut(true);
            ViewsManager.HideAllViews();
            OnInitialize();
        }

        public void Quit()
        {
            Msf.Runtime.Quit();
        }

        public void RequestResetPasswordCode()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Sending reset password code... Please wait!");

            if (!passwordResetCodeView)
            {
                ViewsManager.NotifyNoViewFound("PasswordResetCodeView");
                return;
            }

            if (!passwordResetView)
            {
                ViewsManager.NotifyNoViewFound("PasswordResetView");
                return;
            }

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Auth.RequestPasswordReset(passwordResetCodeView.Email, (isSuccessful, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (isSuccessful)
                    {
                        passwordResetCodeView.Hide();
                        passwordResetView.Show();

                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage($"We have sent an email with reset code to your address '{passwordResetCodeView.Email}'", null));
                    }
                    else
                    {
                        outputMessage = $"An error occurred while password reset code: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void ResetPassword()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Changing password... Please wait!");

            if (!passwordResetCodeView)
            {
                ViewsManager.NotifyNoViewFound("PasswordResetCodeView");
                return;
            }

            if (!passwordResetView)
            {
                ViewsManager.NotifyNoViewFound("PasswordResetView");
                return;
            }

            if (!signinView)
            {
                ViewsManager.NotifyNoViewFound("SigninView");
                return;
            }

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Auth.ChangePassword(new PasswordChangeData()
                {
                    Email = passwordResetCodeView.Email,
                    Code = passwordResetView.ResetCode,
                    NewPassword = passwordResetView.NewPassword
                },
                (isSuccessful, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (isSuccessful)
                    {
                        passwordResetView.Hide();
                        signinView.Show();

                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage("You have successfuly changed your password. Now you can sign in.", null));
                        OnPasswordChangedEvent?.Invoke();
                    }
                    else
                    {
                        outputMessage = $"An error occurred while changing password: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void RequestConfirmationCode()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Sending confirmation code... Please wait!");

            if (!emailConfirmationView)
            {
                ViewsManager.NotifyNoViewFound("EmailConfirmationView");
                return;
            }

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Auth.RequestEmailConfirmationCode((isSuccessful, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (isSuccessful)
                    {
                        emailConfirmationView.Show();
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage($"We have sent an email with confirmation code to your address '{Msf.Client.Auth.AccountInfo.Email}'", null));
                    }
                    else
                    {
                        outputMessage = $"An error occurred while requesting confirmation code: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }

        public void ConfirmAccount()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Confirming your account... Please wait!");

            if (!emailConfirmationView)
            {
                ViewsManager.NotifyNoViewFound("EmailConfirmationView");
                return;
            }

            MsfTimer.WaitForSeconds(1f, () =>
            {
                string confirmationCode = emailConfirmationView.ConfirmationCode;

                Msf.Client.Auth.ConfirmEmail(confirmationCode, (isSuccessful, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (isSuccessful)
                    {
                        emailConfirmationView.Hide();
                        OnEmailConfirmedEvent?.Invoke();
                    }
                    else
                    {
                        outputMessage = $"An error occurred while confirming yor account: {error}";
                        Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(outputMessage, null));
                        logger.Error(outputMessage);
                    }
                });
            });
        }
    }
}