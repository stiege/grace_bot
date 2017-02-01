using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;
using System.Text.RegularExpressions;
using GraceBot.Models;
using System.Data;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class RangerDialog : GraceDialog, IDialog<object>
    {
        #region Fields
        private int _amount;
        private List<string> _keywords;
        #endregion

        internal RangerDialog(IFactory factory, IResponseManager responses)
            : base(factory, responses) { }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(
            IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.Ranger);
            var cmd = _factory.GetApp().ActivityData.Activity.Text.Trim(' ').Split(' ');
            switch (cmd[0])
            {
                case CommandString.GET_UNPROCESSED_QUESTIONS:
                    {
                        PromptDialog.Choice(context,
                            AfterAmount,
                            (new long[] { 3, 5, 10 }).AsEnumerable(),
                            _responses.GetResponseByKey("SelectAnAmount"),
                            _responses.GetResponseByKey("Retry:InputAmount")
                            );
                        break;
                    }
                case CommandString.REPLYING_TO_QUESTION:
                    {
                        var question = _factory.GetDbManager().FindActivity(cmd[1]);
                        if (question == null) goto default;
                        context.PrivateConversationData.SetValue("QuestionActivity", question);
                        await ReplyToQuestion(context);
                        break;
                    }
                default:
                    {
                        await context.PostAsync(_responses.GetResponseByKey("Error:General"));
                        ResetDialog(context);
                        break;
                    }
            }
        }

        #region Reset Dialog
        private void ResetDialog(IDialogContext context)
        {
            _amount = -1;
            _keywords = null;

            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
            context.PrivateConversationData.RemoveValue("QuestionActivity");
            context.PrivateConversationData.RemoveValue("AnswerActivity");
            context.Done<object>(null);
        }
        #endregion

        #region Search Unprocessed Questions
        private async Task AfterAmount(IDialogContext context, IAwaitable<long> result)
        {
            try
            {
                _amount = (int)await result;
                PromptDialog.Text(context,
                    AfterKeywords,
                    _responses.GetResponseByKey("InputKeywords"),
                    _responses.GetResponseByKey("Retry:General"));
            } catch (TooManyAttemptsException)
            {
                await context.PostAsync(_responses.GetResponseByKey("AbortSearchingUnprocessedQuestions"));
                ResetDialog(context);
            }
        }

        private async Task AfterKeywords(IDialogContext context, IAwaitable<string> result)
        {
            _keywords = (await result).Split(',').ToList();
            for(int i = 0; i < _keywords.Count; i++)
            {
                _keywords[i] = _keywords[i].Trim(' ');
            }
            _keywords.RemoveAll(w => w == "");
            await PostQuestionsToRanger(context);
            ResetDialog(context);
        }

        private async Task PostQuestionsToRanger(IDialogContext context)
        {
            var upq = _factory.GetDbManager().FindUnprocessedQuestions(_amount, _keywords);
            if (upq.Any())
            {
                var attachments = _factory.GetBotManager()
                    .GenerateQuestionsAttachments(upq);
                var reply = context.MakeMessage();
                reply.Attachments = attachments;
                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync(_responses.GetResponseByKey("NoUnprocessQuestionsFound"));
            }
        }
        #endregion

        #region Reply to Qustions
        private async Task ReplyToQuestion(IDialogContext context)
        {
            Activity question;
            if (!context.PrivateConversationData.TryGetValue("QuestionActivity", out question))
            {
                await context.PostAsync(_responses.GetResponseByKey("Error:FailToRetrieveQuestion"));
                ResetDialog(context);
                return;
            }

            try
            {
                if (_factory.GetDbManager().GetProcessStatus(question.Id) != ProcessStatus.Unprocessed)
                    throw new InvalidOperationException(_responses.GetResponseByKey("Error:QuestionHasBeenAnswered"));
            }
            catch (Exception ex)
            {
                context.PostAsync(_responses.GetResponseByKey("Error:General"));
                ResetDialog(context);
                throw ex;
            }

            // ***********************************************
            // TODO: UserName should be changed to UserAccount's Name, instead of 
            // ChannelAccount's Name
            // ***********************************************
            var promptMsg = _responses.GetResponseByKey("AnsweringQuestionPrompt_{UserName}{QuestionText}");
            promptMsg = Regex.Replace(promptMsg, "{UserName}", question.From.Name);
            promptMsg = Regex.Replace(promptMsg, "{QuestionText}", question.Text);
            PromptDialog.Text(context,
                AfterInputAnswer,
                promptMsg,
                _responses.GetResponseByKey("Error:General"));
        }

        private async Task AfterInputAnswer(IDialogContext context, IAwaitable<string> result)
        {
            var answerText = await result;
            var answerActivity = _factory.GetApp().ActivityData.Activity;
            context.PrivateConversationData.SetValue("AnswerActivity", answerActivity);

            var confirmMsg = _responses.GetResponseByKey("ConfirmAnswer_{Answer}");
            confirmMsg = Regex.Replace(confirmMsg, "{Answer}", answerText);
            await context.PostAsync(confirmMsg);
            PromptDialog.Confirm(context,
                AfterConfirmAnswer,
                "Please confirm.",
                _responses.GetResponseByKey("Retry:Confirm"));
        }

        private async Task AfterConfirmAnswer(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                Activity question;
                if (!context.PrivateConversationData.TryGetValue("QuestionActivity", out question))
                    throw new InvalidOperationException("Cannot get the question data");

                var confirm = await result;
                Activity answer;
                if (!context.PrivateConversationData.TryGetValue("AnswerActivity", out answer))
                    throw new InvalidOperationException();
                if (confirm)
                {
                    answer.ReplyToId = question.Id;
                    await _factory.GetDbManager().AddActivity(answer,
                        ProcessStatus.BotReplied);
                    await _factory.GetDbManager().UpdateActivityProcessStatus(
                        question.Id, ProcessStatus.Processed);
                    await context.PostAsync(_responses.GetResponseByKey("AnswerReceived"));
                    PostBackToUser(question, answer);
                }
                // throw an exception to abort
                else throw new TooManyAttemptsException("");
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(_responses.GetResponseByKey("AbortReplyingQuestion"));
            }
            catch (DataException)
            {
                // TODO Handle DataException
            }
            catch(InvalidOperationException ex)
            {
                await context.PostAsync(_responses.GetResponseByKey("Error:General") + "\n\n" + ex.Message);
            }
            ResetDialog(context);
        }

        private void PostBackToUser(Activity question, Activity answer)
        {
            var reply = _responses.GetResponseByKey("PostAnswerBackToUser_{UserName}{Question}{RangerName}{Answer}");
            // TODO change the names to UserAccount's Name
            reply = Regex.Replace(reply, "{UserName}", question.From.Name);
            reply = Regex.Replace(reply, "{Question}", question.Text);
            reply = Regex.Replace(reply, "{RangerName}", answer.From.Name);
            reply = Regex.Replace(reply, "{Answer}", answer.Text);
            _factory.GetBotManager().ReplyToActivityAsync(reply, question);
        }
        #endregion

    }
}