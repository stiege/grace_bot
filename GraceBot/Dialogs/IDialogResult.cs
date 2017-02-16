using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot.Dialogs
{
    internal interface IDialogResult
    {
        DialogTypes Type { get; set; }
        string CompleteInformation { get; set; }
        IList<string> PropertiesUsed { get; set; }
        IDialogResult InnerResult { get; set; }
    }
}
