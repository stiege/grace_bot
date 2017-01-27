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
        private string _questionActivityId;
        #endregion

        internal RangerDialog(IFactory factory) : base(factory)
        {
            _responses = _factory.GetResponseData(
                DialogTypes.Ranger);
            if (_responses == null)
                throw new InvalidOperationException(
                    $"Get {DialogTypes.Ranger.ToString()} Dialog responses data failed.");
        }

        public async Task StartAsync(IDialogContext context)
        {
            _amount = -1;
            _keywords = null;
            _questionActivityId = null;

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
                            _responses["SelectAnAmount"][0],
                            _responses["RetryInputAmount"][0]
                            );
                        break;
                    }
                case CommandString.REPLYING_TO_QUESTION:
                    {
                        _questionActivityId = cmd[1];
                        await ReplyToQuestion(context);
                        break;
                    }
                default:
                    {
                        await context.PostAsync(_responses["ErrorMessage"][0]);
                        context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
                        context.Done<object>(null);
                        break;
                    }
            }
        }
        
        #region Search Unprocessed Questions
        private async Task AfterAmount(IDialogContext context, IAwaitable<long> result)
        {
            try
            {
                _amount = (int)await result;
                PromptDialog.Text(context,
                    AfterKeywords,
                    _responses["InputKeywords"][0],
                    _responses["RetryInputKeywords"][0]);
            } catch (TooManyAttemptsException)
            {
                context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
                context.PostAsync(_responses["AbortSearchingUnprocessedQuestions"][0]);
                context.Done<object>(null);
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
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
            context.Done<object>(null);
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
                await context.PostAsync(_responses["NoUnprocessQuestionsFound"][0]);
            }
        }
        #endregion

        #region Reply to Qustions
        private async Task ReplyToQuestion(IDialogContext context)
        {
            var question = _factory.GetDbManager().FindActivity(_questionActivityId);
            if (question == null)
            {
                await context.PostAsync(_responses["ErrorMessage"][0]);
                context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
                context.Done<object>(null);
                return;
            }

            // ***********************************************
            // TODO: UserName should be changed to UserAccount's Name, instead of 
            // ChannelAccount's Name
            // ***********************************************
            var promptMsg = _responses["AnsweringQuestionPrompt_{UserName}_{QuestionText}"][0];
            promptMsg = Regex.Replace(promptMsg, "{UserName}", question.From.Name);
            promptMsg = Regex.Replace(promptMsg, "{QuestionText}", question.Text);
            PromptDialog.Text(context,
                AfterInputAnswer,
                promptMsg,
                _responses["ErrorMessage"][0]);
        }

        private async Task AfterInputAnswer(IDialogContext context, IAwaitable<string> result)
        {
            var answerText = await result;
            var answerActivity = _factory.GetApp().ActivityData.Activity;
            context.PrivateConversationData.SetValue("AnswerActivity", answerActivity);

            var confirmMsg = _responses["ConfirmAnswer_{Answer}"][0];
            confirmMsg = Regex.Replace(confirmMsg, "{Answer}", answerText);
            PromptDialog.Confirm(context,
                AfterConfirmAnswer,
                confirmMsg);
        }

        private async Task AfterConfirmAnswer(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var confirm = await result;
                Activity answerActivity;
                if (!context.PrivateConversationData.TryGetValue("AnswerActivity", out answerActivity))
                    throw new InvalidOperationException();
                if (confirm)
                {
                    answerActivity.ReplyToId = _questionActivityId;
                    await _factory.GetDbManager().AddActivity(answerActivity,
                        ProcessStatus.BotReplied);
                    await _factory.GetDbManager().UpdateActivityProcessStatus(
                        _questionActivityId, ProcessStatus.Processed);
                    await context.PostAsync(_responses["AnswerReceived"][0]);
                }
                // throw an exception to abort
                else throw new TooManyAttemptsException("");
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(_responses["AbortReplyingQuestion"][0]);
            }
            catch (DataException)
            {
                // TODO Handle DataException
            }
            catch(InvalidOperationException)
            {
                await context.PostAsync(_responses["ErrorMessage"][0]);
            }
            context.PrivateConversationData.SetValue("InDialog", DialogTypes.NonDialog);
            context.Done<object>(null);
        }
        #endregion
    }
}