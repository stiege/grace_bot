using GraceBot.Models;
using Microsoft.Bot.Connector;
using System;

namespace GraceBot
{
    public class ActivityData
    {
        private ActivityData() { }

        internal ActivityData(Activity activity, UserRole userRole = UserRole.User, LuisResponse luisResponse = null)
        {
            Activity = activity;
            UserRole = userRole;
            LuisResponse = luisResponse;
        }
        public Activity Activity { get; internal set; }
        public UserRole UserRole { get; internal set; }
        public LuisResponse LuisResponse { get; internal set; }
    }
}