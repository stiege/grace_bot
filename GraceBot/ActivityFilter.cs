using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class ActivityFilter : IFilter
    {
        private readonly string[] _badWords;

        public ActivityFilter(string[] badWords)
        {
            _badWords = badWords;
        }

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
