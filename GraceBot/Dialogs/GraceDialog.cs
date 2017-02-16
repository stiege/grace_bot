using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot.Dialogs
{
    [Serializable]
    internal abstract class GraceDialog
    {
        protected IFactory _factory;
        protected IResponseManager _responses;

        protected DialogTypes _type;
        protected IList<string> _propertyUsed;

        private GraceDialog() { }
        protected GraceDialog(DialogTypes type, 
            IList<string> propertyUsed,
            IFactory factory, IResponseManager responses)
        {
            _factory = factory;
            _responses = responses;

            _propertyUsed = propertyUsed;
            _type = type;
        }

        protected virtual void ReturnToParentDialog(IDialogContext context,
            string completeInformation = null, 
            IDialogResult innerResult = null)
        {
            var result = new GraceDialogResult()
            {
                Type = _type,
                PropertiesUsed = _propertyUsed ?? new List<string>(),
                CompleteInformation = completeInformation,
                InnerResult = innerResult
            };
            context.Done(result);
        }
    }

    [Serializable]
    internal class GraceDialogResult : IDialogResult
    {
        public DialogTypes Type { get; set; }
        public string CompleteInformation { get; set; }
        public IList<string> PropertiesUsed { get; set; }
        public IDialogResult InnerResult { get; set; }
    }

    public enum DialogTypes
    {
        NoneDialog = 0,

        Root,
        Help,
        Home,
        GetDefinition,
        Ranger,
        RateAnswer,
        Answer,
    }
}
