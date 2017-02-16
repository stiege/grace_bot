using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using GraceBot.Models;

namespace GraceBot.Dialogs
{
    /// <summary>
    /// The format of the PrivateConversationData property "Command" of RateAnswerDialog is:
    /// Command[0] = CommandString.RATE_ANSWER
    /// Command[1] = subject
    /// Command[2] = AnswerGrade
    /// Command[3] = answerActivityId
    /// </summary>
    [Serializable]
    internal class RateAnswerDialog : GraceDialog, IDialog<IDialogResult>
    {
        #region Configurations
        private const DialogTypes TYPE = DialogTypes.RateAnswer;

        private static readonly List<string> PROPERTY_USED = new List<string>
        { "SubjectOfAnswer", "AnswerRate", "AnswerActivity", "RatingActivity" };
        #endregion

        internal RateAnswerDialog(IFactory factory, IResponseManager responses)
            : base(TYPE, PROPERTY_USED, factory, responses) { }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(
            IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            string errorMessage = "";
            string[] command = null;
            if(!context.PrivateConversationData.TryGetValue("Command", out command))
                errorMessage += "Cannot get command from Bot State\n";
            if (string.IsNullOrWhiteSpace(command[1]) || string.IsNullOrWhiteSpace(command[2]) || string.IsNullOrWhiteSpace(command[3]))
                errorMessage += "Command format error.\n";
            var subject = command[1];
            AnswerGrade rate = AnswerGrade.NotRated;
            if(!Enum.TryParse(command[2], out rate))
                errorMessage += $"Cannot parse \"{command[2]}\" into AnswerGrade.";
            var answerActivity = _factory.GetDbManager().FindActivity(command[3]);
            if (answerActivity == null)
                errorMessage += $"Cannot find answerActivity (Id: {command[3]}).";

            if (!_factory.GetAnswerManager().ContainsAnswerTo(subject) && answerActivity != null)
                _factory.GetAnswerManager().AddAnswer(subject, answerActivity);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                context.PostAsync(_responses.GetResponseByKey("Error:General"));
                ReturnToParentDialog(context);
                throw new InvalidOperationException(errorMessage);
            }
            var ratingActivity = _factory.GetApp().ActivityData.Activity;
            if (_factory.GetAnswerManager().AnswerIsAlreadyRated(subject, answerActivity,
                ratingActivity))
            {
                context.PostAsync(_responses.GetResponseByKey("Error:AlreadyRated"));
                ReturnToParentDialog(context);
                return;
            }

            context.PrivateConversationData.SetValue("SubjectOfAnswer", subject);
            context.PrivateConversationData.SetValue("AnswerRate", rate);
            context.PrivateConversationData.SetValue("AnswerActivity", answerActivity);
            context.PrivateConversationData.SetValue("RatingActivity", ratingActivity);

            PromptDialog.Confirm(context,
                AfterConfirmToComment,
                _responses.GetResponseByKey("CommentOrNot"),
                _responses.GetResponseByKey("Retry:CommentOrNot"));
        }

        private async Task AfterConfirmToComment(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var wantToComment = await result;
                if (!wantToComment)
                {
                    if(RateTheAnswer(context))
                        await context.PostAsync(_responses.GetResponseByKey("RatingReceivedWithoutComment"));
                    ReturnToParentDialog(context);
                    return;
                }
                PromptDialog.Text(
                    context,
                    AfterInputComment,
                    _responses.GetResponseByKey("InputComment"),
                    _responses.GetResponseByKey("Retry:InputComment")
                    );
            }
            catch (TooManyAttemptsException)
            {
                context.PostAsync(_responses.GetResponseByKey("AbortRatingAnswer"));
                ReturnToParentDialog(context);
            }
        }

        private async Task AfterInputComment(IDialogContext context, IAwaitable<string> result)
        {
            var commentActivity = _factory.GetApp().ActivityData.Activity;
            context.PrivateConversationData.SetValue("CommentActivity",
                commentActivity);
            await _factory.GetDbManager().AddActivity(commentActivity);
            if (RateTheAnswer(context))
                await context.PostAsync(_responses.GetResponseByKey("RatingReceivedWithComment"));
            ReturnToParentDialog(context);
        }

        private bool RateTheAnswer(IDialogContext context)
        {
            string subject;
            AnswerGrade rate = AnswerGrade.NotRated;
            Activity answerActivity;
            Activity ratingActivity;
            Activity commentActivity;

            context.PrivateConversationData.TryGetValue("SubjectOfAnswer", out subject);
            context.PrivateConversationData.TryGetValue("AnswerRate", out rate);
            context.PrivateConversationData.TryGetValue("AnswerActivity", out answerActivity);
            context.PrivateConversationData.TryGetValue("RatingActivity", out ratingActivity);
            context.PrivateConversationData.TryGetValue("CommentActivity", out commentActivity);
            if (string.IsNullOrWhiteSpace(subject) || rate == AnswerGrade.NotRated 
                || answerActivity == null || ratingActivity == null)
            {
                context.PostAsync(_responses.GetResponseByKey("Error:General"));
                ReturnToParentDialog(context);
                return false;
            }
            _factory.GetAnswerManager().RateAnswer(subject, rate, answerActivity, ratingActivity, commentActivity);
            return true;
        }
    }
}