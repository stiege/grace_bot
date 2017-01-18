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
        /// Updates the process status of an Activity. 
        /// </summary>
        /// 
        /// <param name="activityId">The <see cref="Activity.Id"/> of the Activity to be updated 
        /// in database.</param>
        /// <param name="processStatus">The process status of the activity.</param>
        /// 
        /// <exception cref="System.ArgumentNullException">Thrown when activityId is null. </exception>
        /// <exception cref="System.Data.DataException">Thrown when no matching
        /// Activity can be updated. </exception>
        /// <exception cref="System.Data.DataException">Thrown when updating activity
        /// failed due to data source.</exception> 
        /// 
        /// <returns></returns>
        Task UpdateActivityProcessStatus(string activityId, ProcessStatus processStatus);

        /// <summary>
        /// Locates the Activity instance with the given id.
        /// </summary>
        /// 
        /// <param name="id">the ID of an activity</param>
        /// <returns>The Activity instance which has the given id as its <see cref="Activity.Id"/>; 
        /// Or null if there is no Activity in database matches the given id.</returns>
        Activity FindActivity(string id);

        /// <summary>
        /// Returns a list of activities whose process status are <see cref="ProcessStatus.Unprocessed"/>.
        /// </summary>
        /// 
        /// <param name="amount">The max number of unprocessed activities wished to be included.</param>
        /// 
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when amount is less than 1.</exception>
        /// <returns></returns>
        List<Activity> FindUnprocessedQuestions(int amount);

        /// <summary>
        /// Gets the Role of a UserAccount given the id of one of the user's ChannelAccount.
        /// </summary>
        /// 
        /// <param name="channelAccountId">The id of the user's ChannelAccount</param>
        /// 
        /// <exception cref="System.ArgumentNullException">Thrown when channelAccountId is null.</exception>
        /// <exception cref="System.Data.DataException">Thrown when the given id is not in the database.</exception>
        /// 
        /// <returns></returns>
        UserRole GetUserRole(string channelAccountId);
    }
}
