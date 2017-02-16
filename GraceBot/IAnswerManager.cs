using GraceBot;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    public interface IAnswerManager
    {
        string GetAnswerTo(string subject);
        bool ContainsAnswerTo(string subject);
        void AddAnswer(string subject, Activity answerRecord);
        void RateAnswer(string subject, AnswerGrade rate, Activity answerActivity, Activity ratingActivity, Activity commentActivity = null);
        bool AnswerIsAlreadyRated(string subject, Activity answerActivity, Activity ratingActivity);
    }
}
