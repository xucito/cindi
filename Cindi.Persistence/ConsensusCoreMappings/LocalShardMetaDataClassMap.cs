using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
     public static class LocalShardMetaDataClassMap
    {
        public static void Register(BsonClassMap<ShardMetadata> cm)
        {
            cm.AutoMap();
            cm.MapIdMember(lsm => lsm.ShardId);
        }
    }
}
