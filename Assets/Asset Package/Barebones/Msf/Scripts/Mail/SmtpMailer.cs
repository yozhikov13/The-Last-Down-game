using Barebones.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class SmtpMailer : Mailer
    {
        private List<Exception> sendMailExceptions;
        private Logging.Logger logger;

#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
        protected SmtpClient smtpClient;
#endif

        [Header("E-mail settings")]
        public string smtpHost = "smtp.gmail.com";
        public string smtpUsername = "username@gmail.com";
        public string smtpPassword = "password";
        public int smtpPort = 587;
        public bool enableSsl = true;
        public int timeoutInSeconds = 60;
        public string mailFrom = "yourgame@gmail.com";
        public string senderDisplayName = "Awesome Game";

        [Header("E-mail template"), SerializeField]
        protected TextAsset emailBodyTemplate;

        protected virtual void Awake()
        {
            logger = Msf.Create.Logger(typeof(SmtpMailer).Name);
            sendMailExceptions = new List<Exception>();
            SetupSmtpClient();
        }

        protected virtual void Update()
        {
            // Log errors for any exceptions that might have occured
            // when sending mail
            if (sendMailExceptions.Count > 0)
            {
                lock (sendMailExceptions)
                {
                    foreach (var exception in sendMailExceptions)
                    {
                        logger.Error(exception);
                    }

                    sendMailExceptions.Clear();
                }
            }
        }

        protected virtual void SetupSmtpClient()
        {
#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR

            // Configure mail client
            smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                // set the network credentials
                Credentials = new NetworkCredential(smtpUsername, smtpPassword) as ICredentialsByHost,
                EnableSsl = enableSsl,
                Timeout = timeoutInSeconds * 1000
            };

            smtpClient.SendCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    lock (sendMailExceptions)
                    {
                        sendMailExceptions.Add(args.Error);
                    }
                }else if(args.Cancelled)
                {
                    lock (sendMailExceptions)
                    {
                        sendMailExceptions.Add(new Exception("Email sending cancelled!"));
                    }
                }
                else
                {
                    logger.Info("It is OK!");

                    //if(args.UserState != null)
                    //{
                    //    logger.Info(JsonConvert.SerializeObject(args.UserState));
                    //}
                }
            };

            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
#endif
        }

        public override bool SendMail(string to, string subject, string body)
        {
#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR

            string messageBody = body;

            if (emailBodyTemplate)
            {
                string generatedMessageBody = emailBodyTemplate.text;

                generatedMessageBody = generatedMessageBody.Replace("#{MESSAGE_SUBJECT}", subject);
                generatedMessageBody = generatedMessageBody.Replace("#{MESSAGE_BODY}", body);
                generatedMessageBody = generatedMessageBody.Replace("#{MESSAGE_YEAR}", DateTime.Now.Year.ToString());

                messageBody = generatedMessageBody;
            }

            // Create the mail message (from, to, subject, body)
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(mailFrom, senderDisplayName),
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            // send the mail
            smtpClient.SendAsync(mailMessage, "");
#endif
            return true;
        }
    }
}