using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTwitter;
using GraceBot;
using GraceBot.Models;
using Newtonsoft.Json;

namespace TwitterWebJob
{
    public class TwitterManager
    {
        private TwitterContext _twitterContext;
        private readonly string _graceBotScreenName;
        private static TwitterManager _instance;

        private ILuisManager _luisManager;
        private IAnswerManager _definitionAnswerManager;

        private TwitterManager()
        {
            _luisManager = new LuisManager();
            GraceBotContext db = new GraceBotContext();
            DbManager dbManager = new DbManager(db);
            _definitionAnswerManager = new DefinitionAnswerManager(dbManager);

            _graceBotScreenName = Environment.GetEnvironmentVariable("Twitter_ScreenName");

            InitApiConnection();

        }

        public static TwitterManager GetInstance()
        {
            return _instance ?? new TwitterManager();
        }

        public void InitApiConnection()
        {
            _twitterContext = new TwitterContext(new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = Environment.GetEnvironmentVariable("Twitter_ConsumerKey"),
                    ConsumerSecret = Environment.GetEnvironmentVariable("Twitter_ConsumerSecret"),
                    OAuthToken = Environment.GetEnvironmentVariable("Twitter_AccessToken"),
                    OAuthTokenSecret = Environment.GetEnvironmentVariable("Twitter_AccessTokenSecret")
                }
            });
        }


        public async Task StartStreamingMentionsAsync()
        {

            await _twitterContext.Streaming.Where(o => o.Type == StreamingType.Filter && o.Track == _graceBotScreenName)
                .StartAsync(StreamMentionsCallBack);
        }

        private async Task StreamMentionsCallBack(StreamContent strm)
        {
            if (strm.Entity==null)
            {
                return;
            }
            if (strm.EntityType == StreamEntityType.Status)
            {
                var streamContent = (Status)strm.Entity;
                var isMentioned = streamContent?.Entities?.UserMentionEntities?.FirstOrDefault()?.ScreenName?.Equals(_graceBotScreenName);
                if (isMentioned ?? false)
                {
                    var userName = streamContent.User.ScreenNameResponse;
                    var tweetId = streamContent.StatusID.ToString();
                    var replyText = await Process(TrimStatusText(streamContent.Text));

                    await ReplyToTweet(tweetId, userName, replyText);
                }

            }
        }

        private async Task<string> Process(string statusText)
        {
            // 
            string reply = null;
            // get response from Luis
            var response = await _luisManager
                .GetResponse(statusText);

            // Check Luis response
            if (response == null)
                return null;

            var intent = response.TopScoringIntent.Name;

            switch (intent)
            {
                case "GetDefinition":
                    {
                        var subjectEntities = response.Entities.Where(e => e.Type == "subject").ToList();

                        if (subjectEntities.Count == 0)
                        {
                            reply = "Sorry, I didn't figure out the topic.";
                            break;
                        }

                        if (subjectEntities.Count > 1)
                        {
                            reply = "Please ask only one question at a time";
                            break;
                        }

                        // Get definition
                            reply = _definitionAnswerManager.GetAnswerTo(subjectEntities.FirstOrDefault()?.Name);


                        if (reply == null)
                        {
                            reply = "Sorry, we currently don't have an answer to your question.";

                            reply += " Your question has been forwarded to OMGTech! team. We will get back to you ASAP.";

                            break;
                        }

                        break;
                    }
            }

            return reply;
        }

        private async Task ReplyToTweet(string tweetId, string userScreenName, string text)
        {

            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>()
                {
                    {"status", $"@{userScreenName} {text}"},
                    {"in_reply_to_status_id", tweetId}
                };
            await _twitterContext.ExecuteRawAsync("/statuses/update.json", parametersDictionary);
        }

        private string TrimStatusText(string status)
        {
            string[] split = status.Split();
            string result = "";
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].StartsWith("@"))
                {
                    split[i] = "";
                }
                result += split[i];
                if (!result.Equals(""))
                {
                    result += " ";
                }
            }
            return result;
        }

    }
}
