using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Users;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Cindi.Persistence.Tests
{
    public class EntitiesRepository_Tests
    {
        EntitiesRepository repository;
        public EntitiesRepository_Tests()
        {
            Directory.CreateDirectory("db");
            repository = new EntitiesRepository("db/cindidb_test" + Guid.NewGuid() + ".db");
            repository.Setup();
        }

        [Fact]
        public async void SaveAndLoadState()
        {
            await repository.Insert(new NodeStorage<CindiClusterState>()
            {
                LastSnapshot = new CindiClusterState()
                {
                    Nodes = new System.Collections.Concurrent.ConcurrentDictionary<Guid, ConsensusCore.Domain.Models.NodeInformation>(new Dictionary<Guid, NodeInformation>() {
                         {
                            Guid.NewGuid(),
                            new NodeInformation()
                        }
                    })
                }
            });

            var test = await repository.GetFirstOrDefaultAsync<NodeStorage<CindiClusterState>>(u => u != null);
            Assert.NotNull(test);
        }

        [Fact]
        public async void SaveAndLoadShardWriteOperation()
        {
            var id = Guid.NewGuid();
            await repository.Insert(new ShardWriteOperation()
            {
                Id = Guid.NewGuid().ToString(),
                Data = new User()
                {
                    ShardId = id
                }
            });;

            var test = await repository.GetAsync<ShardWriteOperation>(swo => swo.Data.ShardId == id);
            Assert.NotNull(test);
        }

        [Fact]
        public async void Insert_Test()
        {
            var id = Guid.NewGuid();
            await repository.Insert(new User()
            {
                Id = id
            });

            var user = await repository.GetAsync<User>(u => u.Id == id);
            Assert.NotNull(user);
        }

        [Fact]
        public async void GetAsync_Test()
        {
            var id = Guid.NewGuid();
            await repository.Insert(new User()
            {
                Id = id
            });

            var user = await repository.GetAsync<User>(u => u.Id == id);
            Assert.NotNull(user);
        }

        [Fact]
        public async void GetFirstOrDefaultAsync()
        {
            var id = Guid.NewGuid();
            await repository.Insert(new User()
            {
                Id = id
            });

            var user = await repository.GetFirstOrDefaultAsync<User>(u => u.Id == new Guid(id.ToString()));
            Assert.NotNull(user);

            user = await repository.GetFirstOrDefaultAsync<User>(_ => true);
            Assert.NotNull(user);
        }

        [Fact]
        public async void Update_Test()
        {
            var id = Guid.NewGuid();
            await repository.Insert(new User()
            {
                Id = id
            });

            await repository.Update(new User()
            {
                Id = id,
                Email = "test@email.com"
            });

            var user = await repository.GetFirstOrDefaultAsync<User>(u => u.Id == id);
            Assert.NotNull(user);
            Assert.Equal("test@email.com", user.Email);
        }

        [Fact]
        public async void Delete_Test()
        {
            var id = Guid.NewGuid();
            await repository.Insert(new User()
            {
                Id = id
            });

            await repository.Delete<User>(u => u.Id == id);

            var user = await repository.GetFirstOrDefaultAsync<User>(u => u.Id == id);
            Assert.Null(user);
        }
    }
}
