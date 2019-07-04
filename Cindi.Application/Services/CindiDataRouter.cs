using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.Steps;
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
        IStepsRepository _stepsRepository;
        ISequencesRepository _sequenceRepository;

        public CindiDataRouter(IUsersRepository users,
            IBotKeysRepository botKeys,
            IGlobalValuesRepository globalValues,
            IStepTemplatesRepository stepTemplatesRepository,
            ISequenceTemplatesRepository sequenceTemplateRepository,
                    IStepsRepository stepsRepository,
        ISequencesRepository sequenceRepository)
        {
            _users = users;
            _botKeys = botKeys;
            _globalValues = globalValues;
            _stepTemplatesRepository = stepTemplatesRepository;
            _sequenceTemplateRepository = sequenceTemplateRepository;
            _stepsRepository = stepsRepository;
            _sequenceRepository = sequenceRepository;
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
                case nameof(GlobalValue):
                    return await _globalValues.GetGlobalValueAsync(objectId);
                case nameof(StepTemplate):
                    return await _stepTemplatesRepository.GetStepTemplateAsync(objectId);
                case nameof(SequenceTemplate):
                    return await _sequenceTemplateRepository.GetSequenceTemplateAsync(objectId);
                case nameof(Step):
                    return await _stepsRepository.GetStepAsync(objectId);
                case nameof(Sequence):
                    return await _sequenceRepository.GetSequenceAsync(objectId);
                default:
                    return null;
            }
        }

        public async Task<ShardData> InsertDataAsync(ShardData data)
        {
            data.ShardType = data.GetType().Name;
            try
            {
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
                    case Sequence t1:
                        return await _sequenceRepository.InsertSequenceAsync(t1);
                    case Step t1:
                        return await _stepsRepository.InsertStepAsync(t1);
                }
                return null;
            }
            catch(Exception e)
            {
                if(e.Message.Contains("E11000 duplicate key error collection"))
                {
                    Console.WriteLine("Detected that there was a duplicate record in the database... Updating existing record and continueing");
                    return await UpdateDataAsync(data);
                }
                throw e;
            }
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
                case Sequence t1:
                    return await _sequenceRepository.UpdateSequence(t1);
                case Step t1:
                    return await _stepsRepository.UpdateStep(t1);
            }
            return null;
        }
    }
}
