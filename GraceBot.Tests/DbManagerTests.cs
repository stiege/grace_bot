using Microsoft.Bot.Connector;
using NUnit.Framework;
using Moq;
using GraceBot.Models;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace GraceBot.Tests
{
    [TestFixture]
    class DbManagerTests
    {
        private Mock<DbSet<ActivityModel>> _mockActivities;
        private Mock<DbSet<ChannelAccount>> _mockChannelAccounts;
        private Mock<DbSet<ConversationAccount>> _mockConversationAccounts;
        private Mock<GraceBotContext> _mockContext;
        private DbManager _dbManager;

        [SetUp]
        public void Setup()
        {
            _mockActivities = new Mock<DbSet<ActivityModel>>();
            _mockChannelAccounts = new Mock<DbSet<ChannelAccount>>();
            _mockConversationAccounts = new Mock<DbSet<ConversationAccount>>();
            _mockContext = new Mock<GraceBotContext>();

            _mockContext.Setup(m => m.Activities).Returns(_mockActivities.Object);
            _mockContext.Setup(m => m.ChannelAccounts).Returns(_mockChannelAccounts.Object);
            _mockContext.Setup(m => m.ConversationAccounts).Returns(_mockConversationAccounts.Object);

            _dbManager = new DbManager(_mockContext.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockActivities = null;
            _mockChannelAccounts = null;
            _mockConversationAccounts = null;
            _mockContext = null;
            _dbManager = null;
        }

        /// <summary>
        /// Tests the data integrity within conversion process between <see cref="Activity>"/> and <see cref="ActivityModel>"/>. 
        /// 
        /// The latter is used as an Entity Framework Model. The <see cref="ActivityModel>"/> instance should 
        /// hold identical data of the necessary properties of the <see cref="Activity>"/> instance. Moreover,
        /// data integrity should maintain after an <see cref="ActivityModel>"/> is converted back to a <see cref="Activity>"/>
        /// object.
        /// </summary>
        [Test]
        public void ConversionDataIntegrityTest()
        {
            var expected = MakeActivity();
            var model = DbManager.ConvertToModel(expected);
            var actual = DbManager.ConvertToActivity(model);

            AssertActivityEquality(expected, actual);
        }

        /// <summary>
        /// Tests whether the Entity Framework funcitons for adding an <see cref="ActivityModel"/> to the 
        /// database have been invoked correctly when <see cref="IDbManager.AddActivity(Activity, ProcessStatus)"/> 
        /// is invoked. 
        /// </summary>
        [Test]
        public async Task AddActivityTest()
        {
            var activity = MakeActivity();
            var data = new List<ActivityModel> { };
            SetupMockDb_Activities(data);
            _mockActivities.Setup(m => m.Add(It.IsAny<ActivityModel>())).Callback<ActivityModel>(a => data.Add(a));
            
            await _dbManager.AddActivity(activity);
            Assert.AreEqual(activity.Id, data[0].ActivityId);
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Once(), "Context failed to save changes");
        }

        /// <summary>
        /// Tests if ArugmentNullException will be thrown when activity is null.
        /// </summary>
        /// <returns></returns>
        [Test]
        public void AddActivity_ArgumentsNotNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _dbManager.AddActivity(null),
                "ArgumentNullException is not thrown properly");
            _mockActivities.Verify(m => m.Add(It.IsAny<ActivityModel>()), Times.Never(),
                "Null should not be added to the database.");
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Never(),
                "Null should not be added to the database.");
        }

        /// <summary>
        /// <see cref="Activity.Id"/> Could be null as long as <see cref="Activity.ReplyToId"/> is not null and
        /// <see cref="ActivityModel.ProcessStatus"/> equals <see cref="ProcessStatus.BotReplied"/>. Otherwise,
        /// it is supposed to throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AddActivity_IdIsNullable_Test()
        {
            var activity = MakeActivity();
            activity.Id = null;
            activity.ReplyToId = null;

            //await _dbManager.AddActivity(activity);
            Assert.ThrowsAsync<InvalidOperationException>(() => _dbManager.AddActivity(activity), 
                "InvalidOperation is not thrown properly");
            _mockActivities.Verify(m => m.Add(It.IsAny<ActivityModel>()), Times.Never(), 
                "Activity with both Id and ReplyToId are null should not be added to the database.");
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Never(), 
                "Activity with both Id and ReplyToId are null should not be added to the database.");

            activity.ReplyToId = MakeActivity().Id;
            Assert.ThrowsAsync<InvalidOperationException>(() => _dbManager.AddActivity(activity, ProcessStatus.BotReplied),
                "InvalidOperation is not thrown properly");
            _mockActivities.Verify(m => m.Add(It.IsAny<ActivityModel>()), Times.Never(),
                "Activity whose Id is null and the process status is not BotMessage should not be added to the database.");
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Never(),
                "Activity whose Id is null and the process status is not BotMessage should not be added to the database.");

            await _dbManager.AddActivity(activity);
            // processStatus argument is default as ProcessStatus.BotMessage
            _mockActivities.Verify(m => m.Add(It.IsAny<ActivityModel>()), Times.Once(), "Activity failed to add.");
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Once(), "Failed to save changes");
        }

        /// <summary>
        /// Tests if <see cref="DataException"/> will be thrown when Entity Framework throws 
        /// exceptions.
        /// </summary>
        /// <returns></returns>
        [Test]
        public void AddActivity_DataSourceFailed_Test()
        { 
            var activity = MakeActivity();
            _mockContext.Setup(m => m.SaveChangesAsync()).ThrowsAsync(new System.Data.Entity.Infrastructure.DbUpdateException());
            Assert.ThrowsAsync(Is.InstanceOf<DataException>(), () => _dbManager.AddActivity(activity),
                "DataException is not thrown properly");

            _mockContext.Setup(m => m.SaveChangesAsync()).ThrowsAsync(new System.Data.Entity.Infrastructure.DbUpdateConcurrencyException());
            Assert.ThrowsAsync(Is.InstanceOf<DataException>(), () => _dbManager.AddActivity(activity),
                "DataException is not thrown properly");

            _mockContext.Setup(m => m.SaveChangesAsync()).ThrowsAsync(new System.Data.Entity.Validation.DbEntityValidationException());
            Assert.ThrowsAsync(Is.InstanceOf<DataException>(), () => _dbManager.AddActivity(activity),
                "DataException is not thrown properly");
        }

        /// <summary>
        /// Tests if the Activity can be updated correctly.
        /// </summary>
        [Test]
        public async Task UpdateActivityTest()
        {
            // Setup the old record in the database
            var oldActivity = MakeActivity();
            var data = new List<ActivityModel> { DbManager.ConvertToModel(oldActivity, ProcessStatus.Unprocessed), };
            SetupMockDb_Activities(data);

            // change the property values of newRecord except Id and ActivityId
            var newActivity = MakeActivity();
            newActivity.Id = oldActivity.Id;
            newActivity.ReplyToId = Guid.NewGuid().ToString();
            newActivity.Type = (oldActivity.Type == ActivityTypes.Message ? ActivityTypes.Ping : ActivityTypes.Message);

            // Testing
            await _dbManager.UpdateActivity(newActivity, ProcessStatus.Processed);
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Once(), "Failed to save updated changes");

            Assert.AreEqual(ProcessStatus.Processed, data[0].ProcessStatus);
            AssertActivityEquality(newActivity, DbManager.ConvertToActivity(data[0]));
        }

        /// <summary>
        /// Tests if ArugmentNullException will be thrown when activity is null.
        /// </summary>
        [Test]
        public void UpdateActivity_ArgumentsNotNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _dbManager.UpdateActivity(null),
                "ArgumentNullException is not thrown properly");
            _mockContext.Verify(m => m.SaveChangesAsync(), Times.Never(),
                "Null should not be added to the database.");
        }

        /// <summary>
        /// Tests if DataException will be thrown when there is no matching data in the database.
        /// </summary>
        [Test]
        public void UpdateActivity_NoMatchingActivity_Test()
        {
            // Setup the old record in the database
            var oldActivity = MakeActivity();
            var data = new List<ActivityModel> { DbManager.ConvertToModel(oldActivity, ProcessStatus.Unprocessed), };
            SetupMockDb_Activities(data);

            var newActivity = MakeActivity();

            // Testing
            Assert.ThrowsAsync<DataException>(() => _dbManager.UpdateActivity(newActivity, ProcessStatus.Processed));
        }

        /// <summary>
        /// Tests if <see cref="DataException"/> will be thrown when Entity Framework throws exceptions.
        /// </summary>
        [Test]
        public void UpdateActivity_DataSourceFailed_Test()
        {
            var activity = MakeActivity();
            var data = new List<ActivityModel> { DbManager.ConvertToModel(activity, ProcessStatus.Unprocessed), };
            SetupMockDb_Activities(data);

            _mockContext.Setup(m => m.SaveChangesAsync()).ThrowsAsync(new System.Data.Entity.Infrastructure.DbUpdateException());
            Assert.ThrowsAsync(Is.InstanceOf<DataException>(), () => _dbManager.UpdateActivity(activity),
                "DataException is not thrown properly");

            _mockContext.Setup(m => m.SaveChangesAsync()).ThrowsAsync(new System.Data.Entity.Infrastructure.DbUpdateConcurrencyException());
            Assert.ThrowsAsync(Is.InstanceOf<DataException>(), () => _dbManager.UpdateActivity(activity),
                "DataException is not thrown properly");

            _mockContext.Setup(m => m.SaveChangesAsync()).ThrowsAsync(new System.Data.Entity.Validation.DbEntityValidationException());
            Assert.ThrowsAsync(Is.InstanceOf<DataException>(), () => _dbManager.UpdateActivity(activity),
                "DataException is not thrown properly");
        }

        /// <summary>
        /// Tests if <see cref="DbManager.FindActivity(string)"/> returns an Activity which is equal to the recorded
        /// one when they have the same <see cref="Activity.Id"/>.
        /// </summary>
        [Test]
        public void FindActivityTest()
        {
            var expected = MakeActivity();
            SetupMockDb_Activities(new List<ActivityModel>
            {
                DbManager.ConvertToModel(expected),
            });

            var actual = _dbManager.FindActivity(expected.Id);
            AssertActivityEquality(expected, actual);
        }

        /// <summary>
        /// Tests if <see cref="DbManager.FindActivity(string)"/> returns null if there is no Activity in database 
        /// matches the given id.
        /// </summary>
        [Test]
        public void FindActivity_NotInDb_Test()
        {
            SetupMockDb_Activities(new List<ActivityModel>
            {
                DbManager.ConvertToModel(MakeActivity()),
            });

            Assert.IsNull(_dbManager.FindActivity(MakeActivity().Id));
        }

        /// <summary>
        /// Tests if unprocessed questions are found and returned as expected.
        /// </summary>
        [Test]
        public void FindUnprocessedQuestionsTest()
        {
            var data = new List<ActivityModel>();
            SetupMockDb_Activities(data);
            var questions = _dbManager.FindUnprocessedQuestions(3);
            Assert.AreEqual(0, questions.Count());

            var activity = MakeActivity();
            data.Add(DbManager.ConvertToModel(activity, ProcessStatus.Unprocessed));
            questions = _dbManager.FindUnprocessedQuestions(5);
            Assert.AreEqual(1, questions.Count());
            Assert.AreEqual(activity.Id, questions[0].Id);
            data.RemoveAt(0);

            for (int i = 0; i < 5; i++)
            {
                data.Add(DbManager.ConvertToModel(MakeActivity(), ProcessStatus.Unprocessed));
                data.Add(DbManager.ConvertToModel(MakeActivity(), ProcessStatus.Processed));
            }
            questions = _dbManager.FindUnprocessedQuestions(10);
            Assert.AreEqual(5, questions.Count());
        }

        /// <summary>
        /// ArugmentOutOfRangeException should be thrown when amount is less than 1.
        /// </summary>
        [Test]
        public void FindUnprocessedQuestions_ArgumentOutOfRange_Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _dbManager.FindUnprocessedQuestions(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _dbManager.FindUnprocessedQuestions(-1));
        }


        // This method setups a mock Activities table as an IQueryable for testing.
        private void SetupMockDb_Activities(IList<ActivityModel> activityModels = null)
        {
            var ma = activityModels.AsQueryable();
            _mockActivities.As<IQueryable<ActivityModel>>().Setup(m => m.Provider).Returns(ma.Provider);
            _mockActivities.As<IQueryable<ActivityModel>>().Setup(m => m.Expression).Returns(ma.Expression);
            _mockActivities.As<IQueryable<ActivityModel>>().Setup(m => m.ElementType).Returns(ma.ElementType);
            _mockActivities.As<IQueryable<ActivityModel>>().Setup(m => m.GetEnumerator()).Returns(ma.GetEnumerator());
            _mockActivities.Setup(m => m.Include(It.IsAny<string>())).Returns(_mockActivities.Object);
        }

        // This method asserts the equality of two Activities refering to the equalities of 
        // all their recorded properties.
        private void AssertActivityEquality(Activity expected, Activity actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Text, actual.Text);
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.ServiceUrl, actual.ServiceUrl);
            Assert.AreEqual(expected.Timestamp, actual.Timestamp);
            Assert.AreEqual(expected.ChannelId, actual.ChannelId);
            Assert.AreEqual(expected.From, actual.From);
            Assert.AreEqual(expected.Conversation, actual.Conversation);
            Assert.AreEqual(expected.Recipient, actual.Recipient);
            Assert.AreEqual(expected.ReplyToId, actual.ReplyToId);
        }

        // This method initialises all the necessary properties (which should be recorded into the database)
        // of an Activity.
        private Activity MakeActivity(string replyToActivityId = null)
        {
            var guid = Guid.NewGuid().ToString();

            var from = new ChannelAccount()
            {
                Id = "FromId" + guid,
                Name = "FromName" + guid
            };

            var conversation = new ConversationAccount()
            {
                Id = "ConversationAccountId" + guid,
                IsGroup = false,
                Name = "ConversationAccountName" + guid
            };

            var recipient = new ChannelAccount()
            {
                Id = "RecipientId" + guid,
                Name = "RecipientName" + guid
            };

            return new Activity()
            {
                Text = "Text" + guid,
                Type = ActivityTypes.Message,
                Id = $"{guid}",
                ServiceUrl = "ServiceUrl" + guid,
                Timestamp = DateTime.Now,
                ChannelId = "ChannelId" + guid,
                From = from,
                Conversation = conversation,
                Recipient = recipient,
                ReplyToId = replyToActivityId,
            };
        }
    }
}
