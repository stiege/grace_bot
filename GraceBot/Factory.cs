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
        private static ILocalJsonManager _definitionManager;
        private static ILocalJsonManager _autoReplyHomeManager;
        private static Dictionary<DialogTypes, Func<object>> _dialogs;

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
            _commandManagerInstance = _commandManagerInstance ?? new CommandManager(GetFactory());
            return _commandManagerInstance;
        }

        public IFilter GetActivityFilter()
        {
            var sep = Path.DirectorySeparatorChar;
            return new ActivityFilter(File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + $"{sep}BadWords{sep}en"));
        }

        public ILocalJsonManager GetDefinitionManager()
        {
            _definitionManager = _definitionManager ?? new AutoReplyDefinitionManager();
            return _definitionManager;
        }

        public GraceDialog<R> MakeGraceDialog<R>(DialogTypes dialogType)
        {
            if (_dialogs == null)
                InitialDialog();
            Func<object> func = null;
            if (_dialogs.TryGetValue(dialogType, out func))
            {
                var dialog = func.Invoke();
                if (dialog is GraceDialog<R>)
                    return (GraceDialog<R>)dialog;
            }
            return null;
        }

        public Dictionary<DialogTypes, List<string>> GetResponseData(DialogTypes dialogType)
        {
            return new Dictionary<DialogTypes, List<string>>();
        }

        private void InitialDialog()
        {
            _dialogs = new Dictionary<DialogTypes, Func<object>>();
            _dialogs.Add(DialogTypes.Home, () => new HomeDialog(this));
            _dialogs.Add(DialogTypes.GetDefinition, () => new GetDefinitionDialog(this));
        }

        public ILocalJsonManager GetAutoReplyHomeManager()
        {
            _autoReplyHomeManager = _autoReplyHomeManager ?? new AutoReplyHomeManager();
            return _autoReplyHomeManager;
        }
    }
}
