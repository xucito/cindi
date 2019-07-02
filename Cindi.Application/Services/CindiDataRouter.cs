using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.StepTemplates;
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
        IBotKeysRepository _botKeys;
        IGlobalValuesRepository _globalValues;
        IStepTemplatesRepository _stepTemplatesRepository;
        ISequenceTemplatesRepository _sequenceTemplateRepository;

        public CindiDataRouter(IUsersRepository users, IBotKeysRepository botKeys, IGlobalValuesRepository globalValues, IStepTemplatesRepository stepTemplatesRepository, ISequenceTemplatesRepository sequenceTemplateRepository)
        {
            _users = users;
            _botKeys = botKeys;
            _globalValues = globalValues;
            _stepTemplatesRepository = stepTemplatesRepository;
            _sequenceTemplateRepository = _sequenceTemplateRepository;
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
                case nameof(BotKey):
                    return await _botKeys.GetBotKeyAsync(objectId);
                default:
                    return null;
            }
        }

        public async Task<ShardData> InsertDataAsync(ShardData data)
        {
            data.ShardType = data.GetType().Name;
            switch (data)
            {
                case User t1:
                    return await _users.InsertUserAsync(t1);
                case BotKey t1:
                    return await _botKeys.InsertBotKeyAsync(t1);
                case GlobalValue t1:
                    return await _globalValues.InsertGlobalValue(t1);
                case StepTemplate t1:
                    return await _stepTemplatesRepository.InsertAsync(t1);
                case SequenceTemplate t1:
                    return await _sequenceTemplateRepository.InsertSequenceTemplateAsync(t1);
            }
            return null;
        }

        public async Task<ShardData> UpdateDataAsync(ShardData data)
        {
            data.ShardType = data.GetType().Name;
            switch (data)
            {
                case User t1:
                    throw new Exception();
                //return await _users.Upda(t1);
                case BotKey t1:
                    return await _botKeys.UpdateBotKey(t1);
            }
            return null;
        }
    }
}
