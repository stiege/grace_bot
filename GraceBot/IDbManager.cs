using GraceBot.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IDbManager
    {
        /// <summary>
        /// Adds into the database an Activity whose process status is as given.
        /// 
        /// The <see cref="Activity.Id"/> can only be null when processStatus equals to 
        /// <see cref="ProcessStatus.BotMessage"/> and <see cref="Activity.ReplyToId"/> 
        /// is not null. <see cref="Activity.Id"/> cannot be duplicate.
        /// </summary>
        /// 
        /// <param name="activity">The Activity to be added into the database.</param>
        /// <param name="processStatus">The process status of the Activity.</param>
        /// 
        /// <exception cref="System.ArgumentNullException">Thrown when activity is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when <see cref="Activity.Id"/> is 
        /// null, and meanwhile processStatus is not <see cref="ProcessStatus.BotMessage"/> or 
        /// <see cref="Activity.ReplyToId"/> is null.</exception>
        /// <exception cref="System.Data.DataException">Thrown when adding activity with duplicate 
        /// <see cref="Activity.Id"/>. </exception>
        /// <exception cref="System.Data.DataException">Thrown when adding activity to database
        /// failed due to data source.</exception> 
        /// 
        /// <returns></returns>
        Task AddActivity(Activity activity, ProcessStatus processStatus = ProcessStatus.BotMessage);

        /// <summary> 
        /// Updates an activity whose process status is as given. The Activity record with the 
        /// same <see cref="Activity.Id"/> will be updated. 
        /// </summary>
        /// 
        /// <param name="activity">The activity to be updated in database.</param>
        /// <param name="processStatus">The process status of the activity.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when activity is null or 
        /// <see cref="Activity.Id"/> is null. </exception>
        /// <exception cref="System.Data.DataException">Thrown when no matching
        /// Activity can be updated. </exception>
        /// <exception cref="System.Data.DataException">Thrown when updating activity
        /// failed due to data source.</exception> 
        /// 
        /// <returns></returns>
        Task UpdateActivity(Activity activity, ProcessStatus? processStatus = null);

        /// <summary>
        /// Locates the Activity instance with the given id.
        /// </summary>
        /// 
        /// <param name="id">the ID of an activity</param>
        /// <returns>The Activity instance which has the given id as its <see cref="Activity.Id"/>; 
        /// Or null if there is no Activity in database matches the given id.</returns>
        Activity FindActivity(string id);

        /// <summary>
        /// Return a list of activities whose process status are <see cref="ProcessStatus.Unprocessed"/>.
        /// </summary>
        /// 
        /// <param name="amount">The max number of unprocessed activities wished to be included.</param>
        /// 
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when amount is less than 1.</exception>
        /// <returns></returns>
        List<Activity> FindUnprocessedQuestions(int amount);
    }
}
