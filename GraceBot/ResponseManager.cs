using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using GraceBot.Dialogs;
using Newtonsoft.Json;

namespace GraceBot
{
    [Serializable]
    public class ResponseManager : IResponseManager
    {
        private readonly Dictionary<string, string[]> _dictionary;
        private const string ERROR_MSG = "Sorry, error occured. Please try again later, or contact OMG! Tech.";

        public ResponseManager(Dictionary<string, string[]> dictionary)
        {
            if (dictionary == null)
                _dictionary = new Dictionary<string, string[]>();
            else
                _dictionary = dictionary;
        }

        public string GetResponseByKey(string key)
        {
            if (key == null||!ContainsKey(key))
            {
                return ERROR_MSG;
            }

            string[] result;
            _dictionary.TryGetValue(key.ToUpper(), out result);

            int index = new Random().Next(result.Length);
            return result[index];
        }

        public bool ContainsKey(string key)
        {
            return key != null && _dictionary.ContainsKey(key);
        }
    }
}