using UnityEngine;

namespace Barebones.MasterServer
{
    public abstract class Mailer : MonoBehaviour
    {
        public abstract bool SendMail(string to, string subject, string body);
    }
}