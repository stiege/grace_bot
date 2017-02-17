using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private GraceBotContext _db;

        private ILuisManager _luisManager;
        private IAnswerManager _definitionAnswerManager;

        private TwitterManager()
        {
            _luisManager = new LuisManager();
            _db = new GraceBotContext();
            _definitionAnswerManager = new DefinitionAnswerManager(new DbManager(_db));
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
            if (strm.Entity == null)
            {
                return;
            }
            if (strm.EntityType == StreamEntityType.Status)
            {
                var streamContent = (Status)strm.Entity;
                var trimmedContentText = TrimStatusText(streamContent.Text);
                if (string.IsNullOrEmpty(trimmedContentText))
                {
                    return;
                }
                var isMentioned = streamContent?.Entities?.UserMentionEntities?.FirstOrDefault()?.ScreenName?.Equals(_graceBotScreenName);
                if (isMentioned ?? false)
                {
                    var userName = streamContent.User.ScreenNameResponse;
                    var tweetId = streamContent.StatusID.ToString();
                    var replyText = await Process(trimmedContentText);

                    if (replyText == null)
                    {
                        replyText = "Hi I'm GraceBot,your question has been forwarded to OMGTech! team. We will get back to you ASAP.";

                        // Save to DB
                        await AddOrUpdateInDb(trimmedContentText, tweetId, userName);
                    }

                    await ReplyToTweet(tweetId, userName, replyText);
                }

            }
        }

        private async Task<string> Process(string statusText)
        {
            string reply = null;

            // Luis
            var response = await _luisManager.GetResponse(statusText);
            var intent = response?.TopScoringIntent?.Name;
            Console.WriteLine("Luis Response:" + intent);

            switch (intent)
            {
                case "GetDefinition":
                    {
                        var subjectEntities = response.Entities.Where(e => e.Type == "subject").ToList();

                        // Get definition
                        var subject = subjectEntities.FirstOrDefault()?.Name;
                        Console.WriteLine("Subject:" + subject);
                        try
                        {
                            reply = _definitionAnswerManager.GetAnswerTo(subject);
                        }
                        catch (Exception e)
                        {

                        }
                        Console.WriteLine("Get Definition From Local Json:" + reply);

                        break;
                    }
            }

            Console.WriteLine("Final Reply:" + reply);

            return reply;
        }

        private async Task ReplyToTweet(string tweetId, string userScreenName, string text)
        {
            // Limit characters
            text = $"@{userScreenName} {text}";
            if (text.Length > 140)
            {
                text = text.Substring(0, 140);
            }

            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>()
                {
                    {"status", text},
                    {"in_reply_to_status_id", tweetId}
                };
            await _twitterContext.ExecuteRawAsync("/statuses/update.json", parametersDictionary);
        }

        private async Task AddOrUpdateInDb(string question, string statusId, string userScreenName)
        {
            TwitterQuestion twitterQuestion = new TwitterQuestion()
            {
                Text = question,
                StatusId = statusId,
                UserScreenName = userScreenName
            };

            await AddOrUpdateInDb(twitterQuestion);
        }

        private async Task AddOrUpdateInDb(TwitterQuestion twitterQuestion)
        {

            if (!twitterQuestion.IsRequiredPropertyNotNull())
            {
                throw new ArgumentNullException();
            }

            _db.TwitterQuestions.AddOrUpdate(twitterQuestion);

            await _db.SaveChangesAsync();
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
            return result.Trim();
        }

    }
}
