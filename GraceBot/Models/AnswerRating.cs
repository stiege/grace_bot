using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    public class AnswerRating : IEFModel
    {
        private AnswerRating() { }
        public AnswerRating(Answer answer, AnswerGrade rate, 
            ActivityModel answerActivity, ChannelAccountModel raterChannelAccount, ActivityModel commentActivity = null)
        {
            if (answer == null || answerActivity == null || raterChannelAccount == null)
                throw new ArgumentException("None of these arguments can be null: answer, answerActivity, raterChannelAccount.");

            Id = Guid.NewGuid().ToString();
            Rate = rate;
            Answer = answer;
            AnswerId = answer.Id;
            AnswerActivity = answerActivity;
            AnswerActivityId = answerActivity.Id;
            RaterChannelAccount = raterChannelAccount;
            RaterChannelAccountId = raterChannelAccount.Id;
            CommentActivity = commentActivity;
            CommentActivityId = (commentActivity == null ? null : commentActivity.Id);
        }

        #region Entity Framework Map Zone
        [Key]
        public string Id { get; private set; }

        [Required]
        public AnswerGrade Rate { get; set;}

        public string AnswerId { get; set; }

        public string AnswerActivityId { get; set; }

        public string RaterChannelAccountId { get; set; }

        public string CommentActivityId { get; set; }
        #endregion

        [Required]
        public virtual Answer Answer { get; set; }

        [Required]
        public virtual ActivityModel AnswerActivity { get; set; }

        [Required]
        public virtual ChannelAccountModel RaterChannelAccount { get; set; }

        public virtual ActivityModel CommentActivity { get; set; }
    }


    public enum AnswerGrade
    {
        NotRated = -1,

        Helpful = 10,
        Partially_helpful = 5,
        Unhelpful = 0
    }
}