using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace GraceBot
{
    internal class Factory : IFactory
    {
        private static IFactory _factoryInstance;
        private static IApp _appInstance;

        // disable default constructor
        private Factory()
        { }

        // a static constructor
        internal static IFactory GetFactory()
        {
            _factoryInstance = _factoryInstance ?? new Factory();
            return _factoryInstance;
        }

        // Return a new 
        public ILuisManager GetLuisManager()
        {
            return new LuisManager();
        }

        // Return a new 
        public ISlackManager GetSlackManager()
        {
            return new SlackManager();
        }

        public IApp GetApp()
        {
            _appInstance = _appInstance ?? new App(GetFactory());
            return _appInstance;
        }

        public IFilter GetActivityFilter()
        {
            var sep = Path.DirectorySeparatorChar;
            return new ActivityFilter(File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + $"{sep}BadWords{sep}en"));
        }

        public IDefinition GetActivityDefinition()
        {
            var sep = Path.DirectorySeparatorChar;
            using (var reader =
                new JsonTextReader(
                new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"{sep}Words{sep}dictionary.json"))

            )
            {
                var definitions = new JsonSerializer().Deserialize<Dictionary<string, string>>(reader);
                return new ActivityDefinition(definitions);
            }
        }

        public IDbManager GetDbManager()
        {
            return new DbManager(new Models.GraceBotContext());
        }

        public IBotManager GetBotManager()
        {
            return new BotManager();
        }

    }
}
