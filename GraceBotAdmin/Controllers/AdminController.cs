using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GraceBot.Models;
using GraceBotAdmin.Models;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.EnterpriseServices;
using System.Threading.Tasks;
using WebGrease.Css.Extensions;
using ProcessStatus = GraceBot.Models.ProcessStatus;

namespace GraceBotAdmin.Controllers
{
    public class AdminController : Controller
    {
        private readonly GraceBotContext _db = new GraceBotContext();
        // GET: Admin
        public async Task<ActionResult> Index(string questionKeyWord, string processStatus)
        {
            var activityModels = await _db.Activities.
                Include(a => a.From).
                Include(a => a.From).
                Include(a => a.Recipient).
                Include(a => a.Conversation).
                ToListAsync();

            var activitiesFiltered = activityModels;

            if (!string.IsNullOrEmpty(questionKeyWord))
            {
                activitiesFiltered = activitiesFiltered.Where(o => o.Text.Contains(questionKeyWord)).ToList();
            }
            if (!string.IsNullOrEmpty(processStatus))
            {
                try
                {
                    var processStatusInt = (ProcessStatus)Convert.ToInt32(processStatus);
                    activitiesFiltered = activitiesFiltered.Where(o => o.ProcessStatus.Equals(processStatusInt)).ToList();
                }
                catch (Exception e)
                {

                }
            }

            var activityViewModels = new List<ActivityViewModels.ActivityAnswerViewModel>();
            activitiesFiltered = activitiesFiltered.Where(o => o.ProcessStatus != ProcessStatus.BotMessage).ToList();

            activitiesFiltered?.ForEach(questionActivity =>
                {
                    var answerActivity = activityModels.FirstOrDefault(o => object.Equals(o.ReplyToId, questionActivity.Id));
                    activityViewModels.Add(GenerateActivityViewModel(questionActivity, answerActivity));
                }
                );

            return View(activityViewModels.OrderByDescending(o => o.Date).ToList());
        }

        // GET: EnglishTests/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var questionActivity = await GetActivityById(id);

            var answerActivity = await GetActivityByReplyToId(id);

            return View(GenerateEditAnswerViewModel(questionActivity, answerActivity));
        }

        // POST: 
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "QuestionId,Question,AnswerId,Answer")] ActivityViewModels.EditAnswerViewModel model)
        {

            // Find answer activity if any
            var answerActivity = await GetActivityById(model.AnswerId);

            if (answerActivity == null)
            {
                // New answer activity
                answerActivity = new ActivityModel()
                {
                    Id = model.AnswerId,
                    Text = model.Answer,
                    ProcessStatus=ProcessStatus.BotMessage,
                    ReplyToId = model.QuestionId,
                    Timestamp = DateTime.UtcNow,
                };
                _db.Activities.Add(answerActivity);
            }
            else
            {
                answerActivity.Text = model.Answer;
                _db.Entry(answerActivity).State = EntityState.Modified;
               // _db.Entry(answerActivity.Text).CurrentValues.SetValues(model.Answer ?? string.Empty);
            }


            await _db.SaveChangesAsync();
            return RedirectToAction("Index");


        }

        private ActivityViewModels.ActivityAnswerViewModel GenerateActivityViewModel(ActivityModel questionActivity, ActivityModel answerActivity)
        {
            if (questionActivity == null && answerActivity == null)
            {
                return null;
            }
            return new ActivityViewModels.ActivityAnswerViewModel()
            {
                ActivityId = questionActivity?.Id,
                Question = questionActivity?.Text,
                Channel = questionActivity?.ChannelId,
                UserName = questionActivity?.From?.Name,
                Date = questionActivity?.Timestamp,
                Answer = answerActivity?.Text,
                AnsweredBy = answerActivity?.From?.Name?? answerActivity?.From?.Id,
                DateOfQuestionAnswered = answerActivity?.Timestamp,
            };

        }

        private ActivityViewModels.EditAnswerViewModel GenerateEditAnswerViewModel(ActivityModel questionActivity, ActivityModel answerActivity)
        {
            if (questionActivity == null && answerActivity == null)
            {
                return null;
            }
            return new ActivityViewModels.EditAnswerViewModel()
            {
                QuestionId = questionActivity?.Id,
                Question = questionActivity?.Text,
                AnswerId = answerActivity?.Id ?? Guid.NewGuid().ToString(),
                Answer = answerActivity?.Text ?? string.Empty,
            };

        }

        private async Task<ActivityModel> GetActivityById(string id)
        {
            return await _db.Activities.
    Include(a => a.From).
    Include(a => a.From).
    Include(a => a.Recipient).
    Include(a => a.Conversation).
    FirstOrDefaultAsync(o => o.Id == id);
        }
        private async Task<ActivityModel> GetActivityByReplyToId(string id)
        {
            return await _db.Activities.
    Include(a => a.From).
    Include(a => a.From).
    Include(a => a.Recipient).
    Include(a => a.Conversation).
    FirstOrDefaultAsync(o => o.ReplyToId == id);
        }
    }
}