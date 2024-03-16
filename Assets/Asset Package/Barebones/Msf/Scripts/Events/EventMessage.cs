using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class EventMessage
    {
        private object _data;

        public EventMessage() { }

        public EventMessage(object data)
        {
            _data = data;
        }

        public void SetData(object data)
        {
            _data = data;
        }

        public T GetData<T>() where T : class
        {
            return (T)_data as T;
        }
    }
}
