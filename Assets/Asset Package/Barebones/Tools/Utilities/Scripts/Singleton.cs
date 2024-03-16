﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aevien.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;

            DontDestroyOnLoad(gameObject);
        }
    }
}