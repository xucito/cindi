using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Users;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Node.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Application.Services
{
    public class CindiDataRouter : IDataRouter
    {
        IUsersRepository _users;
        public CindiDataRouter(IUsersRepository users)
        {
            _users = users;
        }


        public Task<bool> DeleteDataAsync(ShardData data)
        {
            throw new NotImplementedException();
        }

        public async Task<ShardData> GetDataAsync(string type, Guid objectId)
        {
            switch (type)
            {
                case nameof(User):
                    return await _users.GetUserAsync(objectId);
                default:
                    return null;
            }
        }

        public async Task<ShardData> InsertDataAsync(ShardData data)
        {
            data.Type = data.GetType().Name;
            switch (data)
            {
                case User t1:
                    return await _users.InsertUserAsync(t1);
                    break;
            }
            return null;
        }

        public Task<ShardData> UpdateDataAsync(ShardData data)
        {
            throw new NotImplementedException();
        }
    }
}
