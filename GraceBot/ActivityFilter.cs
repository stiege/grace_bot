using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal class ActivityFilter : IFilter
    {
        private IFactory _factory;

        public ActivityFilter(IFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> FilterAsync(IExtendedActivity activity)
        {
            if (
                !File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\BadWords\\en")
                    .Any(badWord => activity.Text.ToLower().Contains(badWord.ToLower())))
            {
                return await Task.FromResult(true);
            }
            await _factory.RespondAsync("...", activity);
            return await Task.FromResult(false);
        }
    }
}