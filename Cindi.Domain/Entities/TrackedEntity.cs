using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cindi.Domain.Entities
{
    public class TrackedEntity : ShardData
    {
        public TrackedEntity() { }

        public TrackedEntity(Journal journal)
        {
            Journal = journal;
            Reload();
        }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Journal Journal { get; set; }

        public void UpdateJournal(JournalEntry entry)
        {
            Journal.AddJournalEntry(entry);
            Reload();
        }

        public void Reload()
        {
            var properties = this.GetType().GetProperties(BindingFlags.Instance |
                       BindingFlags.NonPublic |
                       BindingFlags.Public);



            foreach (var property in properties.Where(p => p.Name != "Journal" && (p.GetSetMethod(true) != null || p.GetSetMethod() != null) && p.Name != "ShardType" && p.Name != "ShardId"))
            {
                var latestValue = Journal.GetLatestAction(property.Name.ToLower());

                if (latestValue != null && latestValue.Update.Type == UpdateType.Append)
                {
                    List<object> appendedList = new List<object>();
                    foreach (var entry in Journal.GetAllUpdates(property.Name.ToLower()))
                    {
                        appendedList.Add(entry.Update.Value);
                    }

                    property.SetValue(this, ConvertList(appendedList, property.PropertyType));
                }
                else
                {
                    property.SetValue(this, latestValue == null ? null : latestValue.Update.Value);
                }
            }
        }

        public static object ConvertList(List<object> value, Type type)
        {
            IList list = (IList)Activator.CreateInstance(type);
            foreach (var item in value)
            {
                list.Add(item);
            }
            return list;
        }

        public object Clone()
        {
            if (Object.ReferenceEquals(this, null))
            {
                return null;
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(this), deserializeSettings);
        }
    }
}
