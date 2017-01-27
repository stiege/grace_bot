using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace GraceBot
{
    public class AutoReplyHomeManager : ILocalJsonManager
    {
        private readonly Dictionary<string, string[]> _dictionary;

        public AutoReplyHomeManager()
        {
            var sep = Path.DirectorySeparatorChar;
            using (var reader =
                new JsonTextReader(
                new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"Home.json"))
            )
            {
                var dictionary = new JsonSerializer().Deserialize<Dictionary<string, string[]>>(reader);
                _dictionary = new Dictionary<string, string[]>(dictionary, StringComparer.OrdinalIgnoreCase);
            }
        }
        public string GetValueByKey(string key)
        {
            if (key == null)
            {
                return null;
            }
            string[] result;
            _dictionary.TryGetValue(key.ToUpper(), out result);

            if (result == null || result.Length <= 1) return result?[0];

            int index = new Random().Next(result.Length);
            return result[index];
        }
    }
}