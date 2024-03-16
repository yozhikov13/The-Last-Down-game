using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    public class PlayerCharacterBehaviour : NetworkBehaviour
    {
        /// <summary>
        /// Check if this behaviour is ready
        /// </summary>
        public virtual bool IsReady { get; protected set; } = true;
    }
}