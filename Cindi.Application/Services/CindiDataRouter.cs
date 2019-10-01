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

namespace Cindi.Application.Services
{
    public class CindiDataRouter : IDataRouter
    {
        IUsersRepository _users;
        IBotKeysRepository _botKeys;
        IGlobalValuesRepository _globalValues;
        IStepTemplatesRepository _stepTemplatesRepository;
        IWorkflowTemplatesRepository _workflowTemplateRepository;
        IStepsRepository _stepsRepository;
        IWorkflowsRepository _workflowRepository;

        public CindiDataRouter(IUsersRepository users,
            IBotKeysRepository botKeys,
            IGlobalValuesRepository globalValues,
            IStepTemplatesRepository stepTemplatesRepository,
            IWorkflowTemplatesRepository workflowTemplateRepository,
                    IStepsRepository stepsRepository,
        IWorkflowsRepository workflowRepository)
        {
            _users = users;
            _botKeys = botKeys;
            _globalValues = globalValues;
            _stepTemplatesRepository = stepTemplatesRepository;
            _workflowTemplateRepository = workflowTemplateRepository;
            _stepsRepository = stepsRepository;
            _workflowRepository = workflowRepository;
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
                case nameof(WorkflowTemplate):
                    return await _workflowTemplateRepository.GetWorkflowTemplateAsync(objectId);
                case nameof(Step):
                    return await _stepsRepository.GetStepAsync(objectId);
                case nameof(Workflow):
                    return await _workflowRepository.GetWorkflowAsync(objectId);
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
                    case WorkflowTemplate t1:
                        return await _workflowTemplateRepository.InsertWorkflowTemplateAsync(t1);
                    case Workflow t1:
                        return await _workflowRepository.InsertWorkflowAsync(t1);
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
                case Workflow t1:
                    return await _workflowRepository.UpdateWorkflow(t1);
                case Step t1:
                    return await _stepsRepository.UpdateStep(t1);
                case GlobalValue t1:
                    return await _globalValues.UpdateGlobalValue(t1);
            }
            throw new Exception("Object type " + data.ShardType + "has no supported operations");
        }
    }
}
