using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.GlobalValues
{
    public class GlobalValue: TrackedEntity
    {
        public GlobalValue(Journal journal) : base(journal) { }
        
        public GlobalValue(string name,
            string type, 
            string description, 
            object newValue, 
            string status, 
            Guid id,
            string createdBy,
            DateTime createdOn
            ): base(
            new Journal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "name",
                        Value = name,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "type",
                        Value = type,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "description",
                        Value = description,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "value",
                        Value = newValue,
                        Type = UpdateType.Create
                    },
                   new Update()
                    {
                        FieldName = "status",
                        Value = status,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "id",
                        Value = Guid.NewGuid(),
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdon",
                        Value = createdOn,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdby",
                        Value = createdBy,
                        Type = UpdateType.Create
                    }
                }
            })
            )
        {

        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public string Description { get; private set; }

        public object Value { get; private set; }

        public string Status { get; private set; }

        public Guid Id { get; private set; }
    }
}
