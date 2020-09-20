
using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.State
{
    public partial class NodeStorageRepository
    {
        public bool DoesStateExist = false;
        private string _databaseLocation;
        private EntitiesRepository entitiesRepository;

        public NodeStorageRepository(EntitiesRepository entityRepository)
        {
            entitiesRepository = entityRepository;
        }
    }
}
