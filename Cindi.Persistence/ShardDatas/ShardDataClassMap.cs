using Cindi.Domain.Entities.GlobalValues;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ShardDatas
{
    public static class ShardData
    {
        public static void Register(BsonClassMap<ConsensusCore.Domain.BaseClasses.ShardData> sm)
        {
            sm.AutoMap();
        }
    }
}

