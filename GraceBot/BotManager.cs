using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using GraceBot.Dialogs;

namespace GraceBot
{
    public class BotManager : IBotManager
    {
        private IApp _app;

        internal BotManager(IApp appInstance)
        {
            _app = appInstance;
        }

        public async Task<Activity> ReplyToActivityAsync(string replyText, 
            Activity originalAcitivty = null, 
            Func<Activity, List<Attachment>> generateAttachments = null, 
            Action<Activity> invokeBeforeSend = null)
        {
            if (replyText == null && generateAttachments == null)
                throw new ArgumentNullException("replyText and attachments cannot both be null.");
            if (originalAcitivty == null)
            {
                if(_app.ActivityData.Activity == null || 
                    string.IsNullOrWhiteSpace(_app.ActivityData.Activity.ServiceUrl))
                    throw new ArgumentException("No valid original Activity can be based on");
            } else
            {
                if (string.IsNullOrWhiteSpace(_app.ActivityData.Activity.ServiceUrl))
                    throw new ArgumentException("The service url of original Activity must not be null or empty.");
            }

            Activity oa = originalAcitivty ?? _app.ActivityData.Activity;
            Activity ra = oa.CreateReply(replyText);
            if (ra.Id == null)
                ra.Id = Guid.NewGuid().ToString();

            ra.Attachments = generateAttachments?.Invoke(ra);
            invokeBeforeSend?.Invoke(ra);
            var connector = new ConnectorClient(new Uri(oa.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(ra);
            return ra;
        }

        public async Task ReplyIsTypingActivityAsync(Activity originalActivity)
        {
            Activity typingActivity = originalActivity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            typingActivity.Text = null;
            var connector = new ConnectorClient(new Uri(originalActivity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(typingActivity);
        }

        public List<Attachment> GenerateQuestionsAttachments(List<Activity> activityList)
        {
            List<Attachment> attachments = new List<Attachment>();
            foreach (var ua in activityList)
            {
                var cardButtons = new List<CardAction>();
                cardButtons.Add(new CardAction()
                {
                    Title = "Answer this question",
                    Type = "postBack",
                    Value = $"{CommandString.REPLYING_TO_QUESTION} {ua.Id}"
                });

                var card = new HeroCard()
                {
                    Subtitle = $"{ua.From.Name} asked at {ua.Timestamp}",
                    Text = $"{ua.Text}",
                    Buttons = cardButtons
                };
                attachments.Add(card.ToAttachment());
            }
            return attachments;
        }

        public async Task<T> GetUserDataPropertyAsync<T>(string property, Activity activity)
        {
            var stateClient = activity.GetStateClient();
            var userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            return userData.GetProperty<T>(property);
        }

        public async Task SetUserDataPropertyAsync<T>(string property, T data, Activity activity)
        {
            var stateClient = activity.GetStateClient();
            var userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            userData.SetProperty<T>(property, data);
            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
        }

        public T GetPrivateConversationDataProperty<T>(string property)
        {
            var activity = _app.ActivityData.Activity;
            var stateClient = activity.GetStateClient();
            var privateConversationData = stateClient.BotState.GetPrivateConversationData(
                activity.ChannelId, activity.Conversation.Id, activity.From.Id);
            var data = privateConversationData.GetProperty<T>(property);
            return data;
        }

        public void SetPrivateConversationDataProperty<T>(string property, T data)
        {
            var activity = _app.ActivityData.Activity;
            var stateClient = activity.GetStateClient();
            var privateConversationData = stateClient.BotState.GetPrivateConversationData(
                activity.ChannelId, activity.Conversation.Id, activity.From.Id);
            privateConversationData.SetProperty(property, data);
            stateClient.BotState.SetPrivateConversationData(
                activity.ChannelId,
                activity.Conversation.Id,
                activity.From.Id, 
                privateConversationData);
        }

        public async Task<string[]> DeleteStateForUserAsync(Activity activity)
        {
            var stateClient = activity.GetStateClient();
            return await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
        }

        public Attachment GenerateHeroCard(string title, string subTitle, string imgUrl, Dictionary<string, string> buttonsTitleUrlDictionary=null)
        {
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: imgUrl));
            List<CardAction> cardButtons=null;

            if (buttonsTitleUrlDictionary!=null)
            {
                cardButtons = new List<CardAction>();
                foreach (var btn in buttonsTitleUrlDictionary)
                {
                    CardAction plButton = new CardAction()
                    {
                        Value = btn.Value,
                        Type = "openUrl",
                        Title = btn.Key
                    };
                    cardButtons.Add(plButton);
                }
            }

            HeroCard plCard = new HeroCard()
            {
                Title = title,
                Subtitle = subTitle,
                Images = cardImages,
                Buttons = cardButtons
            };
            return plCard.ToAttachment();
        }
    }
}
