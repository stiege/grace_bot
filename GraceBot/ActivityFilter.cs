using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class ActivityFilter : IFilter
    {
        private readonly string[] _badWords;

        // A constructor given a string array of bad words.
        public ActivityFilter(string[] badWords)
        {
            _badWords = badWords;
        }

        // Analyse whether an activity (user message) contains bad words as an asynchronous operation.
        public async Task<bool> FilterAsync(Activity activity)
        {
            if (!_badWords.Any(badWord => activity.Text.ToLower().Contains(badWord.ToLower())))
            {
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }
    }
}
