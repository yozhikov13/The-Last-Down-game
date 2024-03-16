using Barebones.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class BaseClientModule : MonoBehaviour, IBaseClientModule
    {
        /// <summary>
        /// Log levelof this module
        /// </summary>
        [Header("Base Module Settings"), SerializeField]
        protected LogLevel logLevel = LogLevel.Info;

        /// <summary>
        /// Logger assigned to this module
        /// </summary>
        protected Logging.Logger logger;

        public IBaseClientBehaviour ClientBehaviour { get; set; }

        protected virtual void Awake()
        {
            logger = Msf.Create.Logger(GetType().Name);
            logger.LogLevel = logLevel;
        }

        public virtual void OnInitialize(IBaseClientBehaviour clientBehaviour) { }

        protected virtual void OnDestroy() { }
    }
}
