using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Exceptions.Users;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.Users
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        private IMongoCollection<User> _users;

        public long CountUsers() { return _users.EstimatedDocumentCount(); }

        public UsersRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public UsersRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _users = database.GetCollection<User>("Users");

            // var options = new CreateIndexOptions() { Unique = true };
            //var field = new StringFieldDefinition<User>("username");
            // var indexDefinition = new IndexKeysDefinitionBuilder<User>().Ascending(field);

            var notificationLogBuilder = Builders<User>.IndexKeys;
            var indexModel = new CreateIndexModel<User>(notificationLogBuilder.Text(x => x.Username));

            //Create a index
            _users.Indexes.CreateOne(indexModel);
        }

        public async Task<User> InsertUserAsync(User user)
        {
            try
            {
                user.CreatedOn = DateTime.UtcNow;
                await _users.InsertOneAsync(user);
                return user;
            }
            catch (MongoDB.Driver.MongoWriteException e)
            {
                if (e.Message.Contains("E11000 duplicate key error collection"))
                {
                    throw new ConflictingUsersException("Username " + user + " already exists.");
                }
                throw e;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public async Task<User> GetUserAsync(string username)
        {
            var user = await _users.FindAsync(u => u.Username == username);
            return user.FirstOrDefault();
        }

        public async Task<List<User>> GetUsersAsync(int size = 10, int page = 0)
        {

            var builder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();
            var userFilter = FilterDefinition<User>.Empty;
            FindOptions<User> options = new FindOptions<User>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size
            };

            var validUsers = (await _users.FindAsync(userFilter, options)).ToList();

            return validUsers;
        }

        public async Task<bool> DeleteUser(string username)
        {
            var result = await _users.DeleteOneAsync(u => u.Username == username);

            if (result.IsAcknowledged)
            {
                return true;
            }
            return false;
        }

        public async Task<User> GetUserAsync(Guid id)
        {
            var user = await _users.FindAsync(u => u.Id == id);
            return user.FirstOrDefault();
        }
    }
}
