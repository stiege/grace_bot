using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class ActivityFilter : IFilter
    {
        private readonly string[] _badWords;
        private readonly IFactory _factory;

        public ActivityFilter(IFactory factory, string[] badWords)
        {
            _factory = factory;
            _badWords = badWords;
        }

        public async Task<bool> FilterAsync(IExtendedActivity activity)
        {
            if (!_badWords.Any(badWord => activity.Text.ToLower().Contains(badWord.ToLower())))
            {
                return await Task.FromResult(true);
            }
            await _factory.RespondAsync("...", activity);
            return await Task.FromResult(false);
        }
    }
}