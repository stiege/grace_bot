using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    public class BotManager:IBotManager
    {

        public async Task<Activity> ReplyToActivityAsync(string replyText, Activity originalAcitivty,
            List<Attachment> attachments=null)
        {
            var connector = new ConnectorClient(new Uri(originalAcitivty.ServiceUrl));
            var replyAcitivty = originalAcitivty.CreateReply(replyText);
            if (attachments!=null)
            {
                replyAcitivty.Attachments = attachments;
            }
            //replyAcitivty.Recipient = originalAcitivty.From;
            //replyAcitivty.Type = ActivityTypes.Message;
            await connector.Conversations.ReplyToActivityAsync(replyAcitivty);
            return replyAcitivty;
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
                    Value = $"/replyActivity {ua.Id}"
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

        public async Task<string[]> DeleteStateForUserAsync(Activity activity)
        {
            var stateClient = activity.GetStateClient();
            return await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
        }
    }
}