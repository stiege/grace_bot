using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;

namespace GraceBot
{
    [Serializable]
    internal class Factory : IFactory
    {
        private static IFactory _factoryInstance;
        private static IApp _appInstance;
        private static ILuisManager _luisManagerInstance;
        private static ISlackManager _slackManagerInstance;
        private static IDbManager _dbManagerInstance;
        private static IBotManager _botManagerInstance;
        private static ICommandManager _commandManagerInstance;
        private static Dictionary<DialogTypes, Func<GraceDialog>> _dialogs;

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
            _appInstance = _appInstance ?? new App(this);
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
            if (_slackManagerInstance != null)
                return _slackManagerInstance;

            var uri = Environment.GetEnvironmentVariable("WEBHOOK_URL");
            _slackManagerInstance = new SlackManager(uri, "#5-grace-questions", "GraceBot_UserEnquiry");
            return _slackManagerInstance;
        }

        public IDbManager GetDbManager()
        {
            _dbManagerInstance= _dbManagerInstance??new DbManager(new Models.GraceBotContext());
            return _dbManagerInstance;
        }

        public IBotManager GetBotManager()
        {
            _botManagerInstance= _botManagerInstance??new BotManager(_appInstance);
            return _botManagerInstance;
        }

        public ICommandManager GetCommandManager()
        {
            _commandManagerInstance = _commandManagerInstance ?? new CommandManager(GetFactory());
            return _commandManagerInstance;
        }

        public IFilter GetActivityFilter()
        {
            var sep = Path.DirectorySeparatorChar;
            return new ActivityFilter(File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + $"{sep}BadWords{sep}en"));
        }


        public IDialog<R> MakeIDialog<R>(DialogTypes dialogType)
        {
            var graceDialog = MakeGraceDialog(dialogType);
            return (IDialog<R>)graceDialog;
        }

        public Dictionary<string, List<string>> GetResponseData(DialogTypes dialogType)
        {
            var sep = Path.DirectorySeparatorChar;
            using (var reader =
                new JsonTextReader(
                new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"{sep}Responses{sep}{dialogType.ToString()}.json"))
            )
            {
                return new JsonSerializer().Deserialize<Dictionary<string, List<string>>>(reader);
            }
            return null;
        }


        #region Private Methods
        private GraceDialog MakeGraceDialog(DialogTypes dialogType)
        {
            if (_dialogs == null)
                InitialDialog();
            Func<GraceDialog> func = null;
            if (_dialogs.TryGetValue(dialogType, out func))
            {
                return func.Invoke();
            }
            return null;
        }


        private void InitialDialog()
        {
            _dialogs = new Dictionary<DialogTypes, Func<GraceDialog>>();
            _dialogs.Add(DialogTypes.Home, () => new HomeDialog(this));
            _dialogs.Add(DialogTypes.Ranger, () => new RangerDialog(this));
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

        public IResponseManager GetResponseManager(string fileName)
        {
            Dictionary<string, string[]> dictionary;
            var sep = Path.DirectorySeparatorChar;
            using (var reader =
                new JsonTextReader(
                new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"{sep}Responses{sep}{fileName}.json")))
            {
                dictionary = new JsonSerializer().Deserialize<Dictionary<string, string[]>>(reader);
                dictionary = new Dictionary<string, string[]>(dictionary, StringComparer.OrdinalIgnoreCase);
            }
            return new ResponseManager(dictionary);
        }


        #endregion
    }
}
