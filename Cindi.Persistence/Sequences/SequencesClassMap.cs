﻿using Cindi.Domain.Entities.Sequences;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Sequences
{
    public static class SequencesClassMap
    {
        public static void Register(BsonClassMap<SequenceMetadata> sm)
        {
            sm.AutoMap();
            sm.MapIdMember(c => c.SequenceId);
            //cm.SetIdMember(cm.GetMemberMap(c => c.Id)).SetIdGenerator(CombGuidGenerator.Instance); 
        }
    }
}