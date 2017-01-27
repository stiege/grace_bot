using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GraceBot
{
    internal class AutoReplyDefinitionManager : ILocalJsonManager
    {
        private readonly Dictionary<string, string> _definitions;

        // constructor
        public AutoReplyDefinitionManager()
        {
            var sep = Path.DirectorySeparatorChar;
            using (var reader =
                new JsonTextReader(
                new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"{sep}Words{sep}dictionary.json"))
            )
            {
                var definitions = new JsonSerializer().Deserialize<Dictionary<string, string>>(reader);
                _definitions = new Dictionary<string, string>(definitions, StringComparer.OrdinalIgnoreCase);
            }
        }

        // Implement the method for ILocalJsonManager interface. 
        public string GetValueByKey(string key)
        {
            if (key == null)
            {
                return null;
            }
            string result;
            _definitions.TryGetValue(key.ToUpper(), out result);
            return result;
        }
    }
}
