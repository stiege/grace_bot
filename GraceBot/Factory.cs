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
        private static ILuisManager _luisManagerInstance;
        private static ISlackManager _slackManagerInstance;
        private static IDbManager _dbManagerInstance;
        private static IBotManager _botManagerInstance;
        private static ICommandManager _commandManagerInstance;

        // disable default constructor
        private Factory()
        { }

        // a static constructor
        internal static IFactory GetFactory()
        {
            _factoryInstance = _factoryInstance ?? new Factory();
            return _factoryInstance;
        }
        public IApp GetApp()
        {
            _appInstance = _appInstance ?? new App(GetFactory());
            return _appInstance;
        }

        // Return a new 
        public ILuisManager GetLuisManager()
        {
            _luisManagerInstance= _luisManagerInstance??new LuisManager();
            return _luisManagerInstance;
        }

        // Return a new 
        public ISlackManager GetSlackManager()
        {
            _slackManagerInstance= _slackManagerInstance?? new SlackManager();
            return _slackManagerInstance;
        }

        public IDbManager GetDbManager()
        {
            _dbManagerInstance= _dbManagerInstance??new DbManager(new Models.GraceBotContext());
            return _dbManagerInstance;
        }

        public IBotManager GetBotManager()
        {
            _botManagerInstance= _botManagerInstance??new BotManager();
            return _botManagerInstance;
        }

        public ICommandManager GetCommandManager()
        {
            _commandManagerInstance = _commandManagerInstance ?? new CommandManager();
            return _commandManagerInstance;
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

    }
}
