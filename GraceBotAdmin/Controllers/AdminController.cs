using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GraceBot.Models;
using GraceBotAdmin.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using ProcessStatus = GraceBot.Models.ProcessStatus;

namespace GraceBotAdmin.Controllers
{
    public class AdminController : Controller
    {
        private readonly GraceBotContext _db = new GraceBotContext();
        // GET: Admin
        public async Task<ActionResult> Index(string questionKeyWord,string processStatus)
        {
            var activityModels =await _db.Activities.
                Include(a => a.From).
                Include(a => a.From).
                Include(a => a.Recipient).
                Include(a => a.Conversation).
                ToListAsync();

            var activitiesFiltered = activityModels;

            if (!string.IsNullOrEmpty(questionKeyWord))
            {
                activitiesFiltered = activityModels.Where(o => o.Text.Contains(questionKeyWord)).ToList();
            }
            if (!string.IsNullOrEmpty(processStatus))
            {
                try
                {
                    var processStatusInt =(ProcessStatus) Convert.ToInt32(processStatus);
                    activitiesFiltered = activityModels.Where(o => o.ProcessStatus.Equals(processStatusInt)).ToList();
                }
                catch (Exception e)
                {

                }
            }

            var activityViewModels = new List<ActivityViewModels.ActivityAnswerViewModel>();

            foreach (var activity in activitiesFiltered)
            {
                if (!string.IsNullOrEmpty(activity.ReplyToId))
                {
                    var questionActivity = activityModels.FirstOrDefault(o => o.Id != null && o.Id.Equals(activity.ReplyToId));
                    activityViewModels.Add(new ActivityViewModels.ActivityAnswerViewModel()
                    {
                        Question = questionActivity.Text,
                        Channel = questionActivity.ChannelId,
                        UserName = questionActivity.From.Name,
                        Date = questionActivity.Timestamp,
                        Answer = activity?.Text,
                        AnsweredBy = activity.From.Name,
                        DateOfQuestionAnswered = activity?.Timestamp,
                    });
                }
                else if (activity.ProcessStatus.Equals(ProcessStatus.Unprocessed))
                {
                    activityViewModels.Add(new ActivityViewModels.ActivityAnswerViewModel()
                    {
                        Question = activity.Text,
                        Channel = activity.ChannelId,
                        UserName = activity.From.Name,
                        Date = activity.Timestamp,
                        Answer = String.Empty,
                        AnsweredBy = String.Empty,
                        DateOfQuestionAnswered = null,
                    });
                }
            }



            return View(activityViewModels.OrderByDescending(o => o.Date).ToList());
        }
    }
}