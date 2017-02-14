using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceBot.Models
{
    public class Answer : IEFModel
    {
        private Answer() { }
        public Answer(string subject, string text, DateTime timeStamp, 
            UserAccount author = null, string remarks = null)
        {
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentException("An Answer must refer to a subject.");
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("An Answer must have a text content.");
            if (timeStamp == null)
                throw new ArgumentNullException("The timeStamp cannot be null");

            Id = Guid.NewGuid().ToString();
            Subject = subject;
            Text = text;
            Timestamp = timeStamp;
            if(author != null && !string.IsNullOrWhiteSpace(author.Id))
            {
                Author = author;
                AuthorId = author.Id;
            }
            Ratings = new List<AnswerRating>();
        }

        public Answer(string subject, ActivityModel answerActivity, UserAccount author = null) 
            : this(subject, answerActivity?.Text, (DateTime)answerActivity?.Timestamp, author)
        {

        }

        #region Entity Framework Map Zone
        [Key]
        public string Id { get; private set; }

        [Required]
        public string Subject { get; private set; }

        [Required]
        public string Text { get; private set; }

        [Required]
        public DateTime Timestamp { get; private set; }

        public string Remarks { get; set; }

        public string AuthorId { get; private set; }
        #endregion
        public virtual UserAccount Author { get; private set; }
        public virtual List<AnswerRating> Ratings { get; private set; }
    }
}