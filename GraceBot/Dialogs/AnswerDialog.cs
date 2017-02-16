using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using GraceBot.Models;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal class AnswerDialog : GraceDialog, IDialog<object>
    {
        // if set to false, then the buttons of ratings will lie in separate cards
        private const bool  BUTTONS_IN_ONE_CARD = true;

        internal AnswerDialog(IFactory factory, IResponseManager responses)
            : base(factory, responses) { }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(
            IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = _factory.GetApp().ActivityData.Activity;
            var response = _factory.GetApp().ActivityData.LuisResponse;

            await _factory.GetDbManager().AddActivity(activity, ProcessStatus.Unprocessed);
            var subjectEntities = response?.Entities.Where(e => e.Type == "subject").ToList();

            if(subjectEntities == null || subjectEntities.Count < 1)
            {
                await context.PostAsync(_responses.GetResponseByKey("Error:FailToAnalyseQuestion"));
                context.Done(new object());
                return;
            }
            if (subjectEntities.Count > 1)
            {
                await context.PostAsync(_responses.GetResponseByKey("AskOnlyOneQuestion"));
                context.Done(new object());
                return;
            }

            var subject = subjectEntities.FirstOrDefault().Name;
            if (string.IsNullOrWhiteSpace(subject))
            {
                await context.PostAsync(_responses.GetResponseByKey("Error:FailToAnalyseQuestion"));
                context.Done(new object());
                return;
            }

            var answer = _factory.GetAnswerManager().GetAnswerTo(subject);
            if (string.IsNullOrWhiteSpace(answer))
            {
                await context.PostAsync(await ForwardToRangerChannelAsync(activity.Text));
                context.Done(new object());
                return;
            }

            var answerReply = activity.CreateReply(answer);
            if (string.IsNullOrWhiteSpace(answerReply.Id))
                answerReply.Id = Guid.NewGuid().ToString();
            await _factory.GetDbManager().AddActivity(answerReply);

            // since not all the answers are transferred to database yet
            // the answer needs to be added to the database
            _factory.GetDbManager().AddAnswer(subject, answerReply.Id);

            await _factory.GetDbManager().UpdateActivityProcessStatus(activity.Id, ProcessStatus.BotReplied);
            await context.PostAsync(answerReply);

            var rateOptions = activity.CreateReply(_responses.GetResponseByKey("PleaseRate"));
            if (string.IsNullOrWhiteSpace(rateOptions.Id))
                rateOptions.Id = Guid.NewGuid().ToString();
            rateOptions.AttachmentLayout = AttachmentLayoutTypes.List;
            rateOptions.Attachments = GenerateRateAnswerCard(subject, answerReply.Id);
            await context.PostAsync(rateOptions);

            context.Done(new object());
        }

        private async Task<string> ForwardToRangerChannelAsync(string msg)
        {
            var forwardResult = await _factory.GetQuestionSlackManager().ForwardMessageAsync(msg);

            var reply = _responses.GetResponseByKey("DoNotHaveTheAnswer");
            if (forwardResult)
            {
                reply += " " + _responses.GetResponseByKey("QuestionBeenForwarded");
            }
            return reply;
        }

        private List<Attachment> GenerateRateAnswerCard(string subject, string answerActivityId)
        {
            var cards = new List<Attachment>();
            var theOneCard = new HeroCard()
            {
                Buttons = new List<CardAction>(),
            };

            foreach (AnswerGrade grade in Enum.GetValues(typeof(AnswerGrade)))
            {
                if (grade == AnswerGrade.NotRated)
                    continue;
                var cardBtn = new CardAction()
                {
                    Title = grade.ToString().Replace('_', ' '),
                    Type = ActionTypes.PostBack,
                    Value = CommandStringFactory.GenerateRateAnswerCmd(subject, grade, answerActivityId)
                };

                if (BUTTONS_IN_ONE_CARD)
                {
                    theOneCard.Buttons.Add(cardBtn);
                }
                else
                {
                    var card = new HeroCard()
                    {
                        Buttons = new List<CardAction> { cardBtn }
                    };
                    cards.Add(card.ToAttachment());
                }
            }
            if (BUTTONS_IN_ONE_CARD)
                return new List<Attachment> { theOneCard.ToAttachment() };
            else
                return cards;
        }
    }
}