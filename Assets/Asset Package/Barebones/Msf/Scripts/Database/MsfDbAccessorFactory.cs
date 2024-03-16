﻿using Barebones.Logging;
using System;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class MsfDbAccessorFactory
    {
        private Dictionary<Type, object> _accessors = new Dictionary<Type, object>();

        /// <summary>
        /// Adds a database accessor to the list of available accessors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="access"></param>
        public void SetAccessor<T>(object access)
        {
            if (_accessors.ContainsKey(typeof(T)))
            {
                Logs.Warn(string.Format("Database accessor of type {0} was overwriten", typeof(T)));
            }

            _accessors[typeof(T)] = access;
        }

        /// <summary>
        /// Retrieves a database accessor from a list of available accessors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAccessor<T>() where T : class
        {
            _accessors.TryGetValue(typeof(T), out object result);
            return result as T;
        }
    }
}