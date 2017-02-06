using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GraceBotAdmin.Models
{
    public class ActivityViewModels
    {
        public class ActivityAnswerViewModel
        {
        public string Question { get; set; }
        public string Channel { get; set; }
        public string UserName { get; set; }
        public DateTime? Date { get; set; }
        public string Answer { get; set; }
        public string AnsweredBy { get; set; }
        public DateTime? DateOfQuestionAnswered { get; set; }
        }

    }
}