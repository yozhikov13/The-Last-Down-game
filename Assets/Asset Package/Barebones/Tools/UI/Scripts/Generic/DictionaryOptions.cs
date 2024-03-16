using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class DictionaryOptions
    {
        private Dictionary<string, string> options;

        public DictionaryOptions()
        {
            options = new Dictionary<string, string>();
        }

        public DictionaryOptions(Dictionary<string, string> options)
        {
            this.options = new Dictionary<string, string>();
            Append(options);
        }

        public DictionaryOptions(DictionaryOptions options)
        {
            this.options = new Dictionary<string, string>();
            Append(options);
        }

        public bool Remove(string key)
        {
            return options.Remove(key);
        }

        public void Clear()
        {
            options.Clear();
        }

        private void AddToOptions(string key, object value)
        {
            if (Has(key))
            {
                throw new Exception($"You have already added value with key {key}");
            }

            SetToOptions(key, value);
        }

        private void SetToOptions(string key, object value)
        {
            options[key] = value.ToString();
        }

        public void Append(DictionaryOptions options)
        {
            if (options != null)
                Append(options.ToDictionary());
        }

        public void Append(Dictionary<string, string> options)
        {
            if (options != null)
                foreach (var kvp in options)
                {
                    AddToOptions(kvp.Key, kvp.Value);
                }
        }

        public bool Has(string key)
        {
            return options.ContainsKey(key);
        }

        public bool IsValueEmpty(string key)
        {
            if (!Has(key))
            {
                return true;
            }
            else
            {
                return string.IsNullOrEmpty(AsString(key).Trim());
            }
        }

        public void Add(string key, int value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, int value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, float value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, float value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, double value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, double value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, decimal value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, decimal value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, bool value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, bool value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, short value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, short value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, byte value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, byte value)
        {
            SetToOptions(key, value);
        }

        public void Add(string key, string value)
        {
            AddToOptions(key, value);
        }

        public void Set(string key, string value)
        {
            SetToOptions(key, value);
        }

        public string AsString(string key, string defValue = "")
        {
            if (!Has(key))
            {
                return defValue;
            }

            return options[key];
        }

        public int AsInt(string key, int defValue = 0)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToInt32(options[key]);
        }

        public float AsFloat(string key, float defValue = 0f)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToSingle(options[key]);
        }

        public double AsDouble(string key, double defValue = 0d)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToDouble(options[key]);
        }

        public decimal AsDecimal(string key, decimal defValue = 0)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToDecimal(options[key]);
        }

        public bool AsBool(string key, bool defValue = false)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToBoolean(options[key]);
        }

        public short AsShort(string key, short defValue = 0)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToInt16(options[key]);
        }

        public byte AsByte(string key, byte defValue = 0)
        {
            if (!Has(key))
            {
                return defValue;
            }

            return Convert.ToByte(options[key]);
        }

        public Dictionary<string, string> ToDictionary()
        {
            return options;
        }

        public byte[] ToBytes()
        {
            return options.ToBytes();
        }

        public string ToReadableString(string itemsSeparator = "; ", string kvpSeparator = " : ")
        {
            return ToDictionary().ToReadableString(itemsSeparator, kvpSeparator);
        }

        public override string ToString()
        {
            return ToReadableString();
        }
    }
}