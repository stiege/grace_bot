using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GraceBot.Models;

namespace GraceBot
{
    internal class CommandManager : ICommandManager
    {
        private IFactory _factory;
        private Dictionary<string, ICommand> _commandsList;

        internal Dictionary<string, ICommand> Commands
        {
            get
            {
                return _commandsList;
            }
        }

        public CommandManager(IFactory factory)
        {
            _factory = factory;
            _commandsList = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
                {
                    { CommandString.GET_UNPROCESSED_QUESTIONS, new CommandGetQuestion(_factory) },
                    { CommandString.REPLYING_TO_QUESTION, new CommandReplyQuestion(_factory) }
                };
        }

        public ICommand GetCommand(string cmd, UserRole userRole)
        {
            if (userRole.Equals(UserRole.Ranger))
            {
                return Commands[cmd];

            }
            else
            {
                // TO DO ....
                return null;
            }
        }


    }
}