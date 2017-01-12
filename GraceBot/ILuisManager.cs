using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraceBot.Models;

namespace GraceBot
{
    interface ILuisManager
    {
        Task<LuisResponse> GetResponse(string activityText);
    }
}
