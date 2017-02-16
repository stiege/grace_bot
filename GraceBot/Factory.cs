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
        private static ISlackManager _questionSlackMangerInstance;
        private static ISlackManager _exceptionSlackMangerInstance;
        private static IDbManager _dbManagerInstance;
        private static IBotManager _botManagerInstance;
        private static ICommandManager _commandManagerInstance;
        private static IAnswerManager _answerManagerInstance;
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

        public ILuisManager GetLuisManager()
        {
            _luisManagerInstance= _luisManagerInstance??new LuisManager();
            return _luisManagerInstance;
        }

        public ISlackManager GetQuestionSlackManager()
        {
            if (_questionSlackMangerInstance != null)
                return _questionSlackMangerInstance;

            var uri = Environment.GetEnvironmentVariable("WEBHOOK_URL");
            _questionSlackMangerInstance = new SlackManager(uri, "#5-grace-questions", "GraceBot_UserEnquiry");
            return _questionSlackMangerInstance;
        }

        public ISlackManager GetExceptionSlackManager()
        {
            if (_exceptionSlackMangerInstance != null)
                return _exceptionSlackMangerInstance;

            var uri = Environment.GetEnvironmentVariable("WEBHOOK_URL");
            _exceptionSlackMangerInstance = new SlackManager(uri, "#5-grace-questions", "GraceBot_Exceptions");
            return _exceptionSlackMangerInstance;
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
            if (_dialogs == null)
                InitialDialog();
            Func<GraceDialog> func = null;
            if (_dialogs.TryGetValue(dialogType, out func))
            {
                return (IDialog<R>) func.Invoke();
            }
            return null;
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

        public IAnswerManager GetAnswerManager()
        {
            if (_answerManagerInstance != null)
                return _answerManagerInstance;

            _answerManagerInstance = new DefinitionAnswerManager(GetDbManager());
            return _answerManagerInstance;
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
            //return null;
        }


        #region Private Methods
        private void InitialDialog()
        {
            _dialogs = new Dictionary<DialogTypes, Func<GraceDialog>>();

            var rootResponses = GetResponseManager(DialogTypes.Root.ToString());
            _dialogs.Add(DialogTypes.Root,
                () => new RootDialog(this, rootResponses));

            var rangerResponses = GetResponseManager(DialogTypes.Ranger.ToString());
            _dialogs.Add(DialogTypes.Ranger, 
                () => new RangerDialog(this, rangerResponses));

            var helpResponses = GetResponseManager(DialogTypes.Help.ToString());
            _dialogs.Add(DialogTypes.Help,
                () => new HelpDialog(this, helpResponses));

            var rateAnswerResponses = GetResponseManager(DialogTypes.RateAnswer.ToString());
            _dialogs.Add(DialogTypes.RateAnswer,
                () => new RateAnswerDialog(this, rateAnswerResponses));

            var answerResponses = GetResponseManager(DialogTypes.Answer.ToString());
            _dialogs.Add(DialogTypes.Answer,
                () => new AnswerDialog(this, answerResponses));
        }
        #endregion
    }
}
