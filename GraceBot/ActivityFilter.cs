using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class ActivityFilter : IFilter
    {
        private readonly string[] _badWords;
        private const int MESSAGE_MAX_LENGTH = 200;

        // A constructor given a string array of bad words.
        public ActivityFilter(string[] badWords)
        {
            _badWords = badWords;
        }

        // Analyse whether an activity (user message) contains bad words as an asynchronous operation.
        public async Task<string> FilterAsync(Activity activity)
        {
            if (_badWords.Any(badWord => activity.Text.ToLower().Contains(badWord.ToLower())))
            {
                return await Task.FromResult("Sorry, bad words detected. Please try again.");
            }

            if (activity.Text.Length > MESSAGE_MAX_LENGTH)
            {
                return await Task.FromResult("Sorry, your message is too long. Please try again.");
            }

            return await Task.FromResult("Passed");
        }
    }
}
