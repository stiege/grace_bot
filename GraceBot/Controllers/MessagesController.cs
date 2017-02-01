using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;

namespace GraceBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                var app = Factory.GetFactory().GetApp();
                await app.RunAsync(activity);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                string message;
                if (!activity.Type.Equals(ActivityTypes.Message))
                {
                    message = "[EMPTY]";
                }
                else if (activity.Text == null || activity.Text.Length == 0)
                {
                    if (activity.Attachments.Count != 0)
                        message = "[ATTACHMENTS]";
                    else
                        message = "[EMPTY]";
                }
                else
                {
                    message = activity.Text;
                }

                var slackMessage = $"Activity_Type: {activity.Type.ToString()}\n\n";
                slackMessage += $"Channel: {activity.ChannelId}\n\n";
                slackMessage += $"From_Id: {activity.From.Id}\n\n";
                slackMessage += $"Recipient_Id: {activity.Recipient.Id}\n\n";
                slackMessage += $"Timestamp: {activity.Timestamp}\n\n";
                slackMessage += $"Text: {message}\n\n";
                slackMessage += "=================================================\n\n";
                slackMessage += $"Exception_Info: {e.ToString()}";

                var innerException = e.InnerException;
                while (innerException != null)
                {
                    slackMessage += "=================================================\n\n";
                    slackMessage += $"Exception_Info: {innerException.ToString()}";
                    innerException = innerException.InnerException;
                }                

                // TODO could add a feature to save the exceptions which are failed to forward
                Factory.GetFactory().GetExceptionSlackManager()
                    .ForwardMessageAsync(slackMessage);

                Debug.WriteLine(slackMessage);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
