using GraceBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Data;

namespace GraceBot
{
    internal class DefinitionAnswerManager : IAnswerManager
    {
        private readonly Dictionary<string, string> _definitions;
        private readonly IDbManager _dbManager;

        // constructor
        internal DefinitionAnswerManager(IDbManager dbManager)
        {
            _definitions = dbManager.GetDefinition();
            _dbManager = dbManager;
        }

        public string GetAnswerTo(string subject)
        {
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentException("The subject cannot be null or empty.");
            string definition;
            if (_definitions.TryGetValue(subject.ToUpper(), out definition))
            {
                return definition + "\n\n";
            }
            else return null;
        }

        public bool ContainsAnswerTo(string subject)
        {
            return _definitions.ContainsKey(subject.ToUpper());
        }

        public void AddAnswer(string subject, Activity answerRecord)
        {
            subject = subject.ToUpper();
            try
            {
                _dbManager.AddAnswer(subject, answerRecord.Id);
            }
            catch (RowNotInTableException)
            {
                _dbManager.AddActivity(answerRecord);
                AddAnswer(subject, answerRecord);
            }
            _definitions.Add(subject, answerRecord.Text);
        }

        public void RateAnswer(string subject, AnswerGrade rate, Activity answerActivity, Activity ratingActivity, Activity commentActivity = null)
        {
            subject = subject.ToUpper();
            try
            {
                _dbManager.AddAnswerRating(subject, rate, answerActivity.Id, ratingActivity.From.Id, commentActivity?.Id);
            }
            catch (RowNotInTableException)
            {
                AddAnswer(subject, answerActivity);
                _dbManager.AddActivity(answerActivity);
                _dbManager.AddActivity(commentActivity);
                _dbManager.AddActivity(ratingActivity);
                RateAnswer(subject, rate, answerActivity, ratingActivity, commentActivity);
            }
        }

        public bool AnswerIsAlreadyRated(string subject, Activity answerActivity, Activity ratingActivity)
        {
            return _dbManager.ContainsAnswerRating(subject.ToUpper(), answerActivity.Id, ratingActivity.From.Id);
        }
    }
}