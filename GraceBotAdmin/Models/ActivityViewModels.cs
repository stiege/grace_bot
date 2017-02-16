using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GraceBotAdmin.Models
{
    public class ActivityViewModels
    {
        public class ActivityAnswerViewModel
        {
            public string ActivityId { get; set; }
            [Display(Name = "Question")]
            public string Question { get; set; }
            [Display(Name = "Channel")]
            public string Channel { get; set; }
            [Display(Name = "User Name")]
            public string UserName { get; set; }
            [Display(Name = "Date")]
            public DateTime? Date { get; set; }
            [Display(Name = "Answer")]
            public string Answer { get; set; }
            [Display(Name = "Answered By")]
            public string AnsweredBy { get; set; }
            [Display(Name = "Date")]
            public DateTime? DateOfQuestionAnswered { get; set; }
        }

        public class EditAnswerViewModel
        {
            public string QuestionId { get; set; }
            [Display(Name = "Question")]
            public string Question { get; set; }
            public string AnswerId { get; set; }
            [Required]
            [Display(Name = "Answer")]
            public string Answer { get; set; }
        }
    }
}