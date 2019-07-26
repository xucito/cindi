using Cindi.Domain.Entities.Workflows;
using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.NodeStorages
{
    public static class NodeStorageClassMap
    {
        public static void Register(BsonClassMap<NodeStorage> sm)
        {
            sm.AutoMap();
        }
    }
}
