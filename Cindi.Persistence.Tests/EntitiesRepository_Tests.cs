using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Events;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            repository = new EntitiesRepository("C:/Users/TNguy/Repositories/xucito/cindi/Cindi.Presentation/db/cindidb_test" + Guid.NewGuid() + ".db");
            repository.Setup();
        }

        [Fact]
        public async void SaveAndLoadState()
        {
            repository.StateChanged(null, new StateChangedEventArgs()
            {
                NewState = new CindiClusterState()
                {
                }
            });

            var test = await repository.GetFirstOrDefaultAsync<CindiClusterState>(u => true);
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

            var user = await repository.GetAsync<User>(u => u.Id == id, null, "id:-1");
            Assert.NotNull(user);

            user = await repository.GetAsync<User>(_ => true, null, "id:-1");
            Assert.NotNull(user);
        }

        /*[Fact]
        public async void GetAsyncSort_Test()
        {
            await repository.Insert(new ShardWriteOperation()
            {
                Id = Guid.NewGuid().ToString(),
                Pos = 1
            });

            await repository.Insert(new ShardWriteOperation()
            {
                Id = Guid.NewGuid().ToString(),
                Pos = 2
            });

            var swo = await repository.GetAsync<ShardWriteOperation>(_ => true, null, "pos:1");
            Assert.Equal(1, swo.First().Pos);
            swo = await repository.GetAsync<ShardWriteOperation>(_ => true, null, "pos:-1");
            Assert.Equal(2, swo.First().Pos);
            //Test case insensitivity
            swo = await repository.GetAsync<ShardWriteOperation>(_ => true, null, "pOs:-1");
            Assert.Equal(2, swo.First().Pos);
        }*/

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


        [Fact]
        public async void SaveAndLoadState_Test()
        {
            var id = Guid.NewGuid();
            repository.StateChanged(null, new StateChangedEventArgs()
            {
                NewState = new CindiClusterState()
                {
                    Id = Guid.NewGuid()
                }
            }); 

            Assert.NotNull(await repository.GetFirstOrDefaultAsync<CindiClusterState>(_ => true));

            repository.StateChanged(null, new StateChangedEventArgs()
            {
                NewState = new CindiClusterState() { 
                    Id = Guid.NewGuid()
                }
            });
            Assert.NotNull(await repository.GetFirstOrDefaultAsync<CindiClusterState>(_ => true));
        }
    }
}
