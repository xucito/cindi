using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    property.SetValue(this, latestValue == null ? null : ConvertObject(latestValue.Update.Value, property.PropertyType));
                }
            }
        }

        public static object ConvertList(List<object> value, Type type)
        {
            IList list = (IList)Activator.CreateInstance(type);
            foreach (var item in value)
            {
                var checkType = item.GetType();
                //                list.Add(Convert.ChangeType(item, type));
                if(item is JObject && type != typeof(JObject))
                {
                    list.Add(((JObject)item).ToObject(type.GetGenericArguments()[0]));
                }
                else
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static object ConvertObject(object value, Type type)
        {
            if (value is System.String && (type == typeof(Guid) || type == typeof(Guid?)))
            {
                return new Guid((string)value);
            }

            if (value is System.Int64 && type == typeof(int?))
            {
                return Convert.ToInt32((System.Int64)value);
            }

            if (value is JObject)
            {
                return ((JObject)value).ToObject(type);
            }

            return value;
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
