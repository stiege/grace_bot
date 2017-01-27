using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    interface ICommand
    {
        Task Execute(Activity activity);
    }
}
