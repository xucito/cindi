﻿using ConsensusCore.Domain.BaseClasses;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class ShardOperationClassMap
    {
        public static void Register(BsonClassMap<ShardOperation> cm)
        {
            cm.AutoMap();
            cm.MapIdMember(lsm => lsm.Id);
        }
    }
}
