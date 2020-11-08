using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Node.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Entities.ExecutionSchedule;

namespace Cindi.Application.Services
{
    public class CindiDataRouter : IDataRouter
    {
        IEntitiesRepository _entitiesRepository;

        public CindiDataRouter(
            IEntitiesRepository entitiesRepository)
        {
            _entitiesRepository = entitiesRepository;
        }


        public async Task<bool> DeleteDataAsync(ShardData data)
        {
            switch (data.ShardType)
            {
                case nameof(BotKey):
                    await _entitiesRepository.DeleteById<BotKey>(data.Id);
                    break;
                case nameof(ExecutionSchedule):
                    await _entitiesRepository.DeleteById<ExecutionSchedule>(data.Id);
                    break;
                case nameof(MetricTick):
                    await _entitiesRepository.DeleteById<MetricTick>(data.Id);
                    break;
                case nameof(Step):
                    await _entitiesRepository.DeleteById<Step>(data.Id);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        public async Task<ShardData> GetDataAsync(string type, Guid objectId)
        {
            switch (type)
            {
                case nameof(User):
                    return await _entitiesRepository.GetByIdAsync<User>(objectId);
                case nameof(BotKey):
                    return await _entitiesRepository.GetByIdAsync<BotKey>(objectId);
                case nameof(GlobalValue):
                    return await _entitiesRepository.GetByIdAsync<GlobalValue>(objectId);
                case nameof(StepTemplate):
                    return await _entitiesRepository.GetByIdAsync<StepTemplate>(objectId);
                case nameof(WorkflowTemplate):
                    return await _entitiesRepository.GetByIdAsync<WorkflowTemplate>(objectId);
                case nameof(Step):
                    return await _entitiesRepository.GetByIdAsync<Step>(objectId);
                case nameof(Workflow):
                    return await _entitiesRepository.GetByIdAsync<Workflow>(objectId);
                case nameof(ExecutionTemplate):
                    return await _entitiesRepository.GetByIdAsync<ExecutionTemplate>(objectId);
                case nameof(ExecutionSchedule):
                    return await _entitiesRepository.GetByIdAsync<ExecutionSchedule>(objectId);
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
                        return await _entitiesRepository.Insert(t1);
                    case BotKey t1:
                        return await _entitiesRepository.Insert(t1);
                    case GlobalValue t1:
                        return await _entitiesRepository.Insert(t1);
                    case StepTemplate t1:
                        return await _entitiesRepository.Insert(t1);
                    case WorkflowTemplate t1:
                        return await _entitiesRepository.Insert(t1);
                    case Metric t1:
                        return await _entitiesRepository.Insert(t1);
                    case MetricTick t1:
                        return await _entitiesRepository.Insert(t1);
                    case Step t1:
                        return await _entitiesRepository.Insert(t1);
                    case Workflow t1:
                        return await _entitiesRepository.Insert(t1);
                    case ExecutionTemplate t1:
                        return await _entitiesRepository.Insert(t1);
                    case ExecutionSchedule t1:
                        return await _entitiesRepository.Insert(t1);
                }
                throw new Exception("Object type " + data.ShardType + "has no supported operations");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("E11000 duplicate key error collection"))
                {
                    Console.WriteLine("Detected that there was a duplicate record in the database... Updating existing record and continueing");
                    try
                    {
                        return await UpdateDataAsync(data);
                    }
                    catch (Exception updateError)
                    {
                        if (updateError.Message.Contains("has no supported update operations"))
                        {
                            return data;
                        }
                        else
                        {
                            throw updateError;
                        }
                    }
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
                    return await _entitiesRepository.Update(t1);
                case GlobalValue t1:
                    return await _entitiesRepository.Update(t1);
                case Workflow t1:
                    return await _entitiesRepository.Update(t1);
                case Step t1:
                    return await _entitiesRepository.Update(t1);
                case ExecutionTemplate t1:
                    return await _entitiesRepository.Update(t1);
                case ExecutionSchedule t1:
                    return await _entitiesRepository.Update(t1);
            }
            throw new Exception("Object type " + data.ShardType + " has no supported update operations");
        }
    }
}
