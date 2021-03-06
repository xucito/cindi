﻿using ConsensusCore.Domain.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.Conditions
{
    public class ConditionSerializer : JsonCreationConverter<Condition>
    {
        protected override Condition Create(Type objectType, Newtonsoft.Json.Linq.JObject jObject)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(Condition)))
                    {
                        var name = jObject.ContainsKey("name") ? jObject.Value<string>("name") : jObject.Value<string>("Name");
                        if (!t.IsGenericTypeDefinition && name == ((Condition)Activator.CreateInstance(t)).Name)
                        {
                            return (Condition)Activator.CreateInstance(t);
                        }
                    }
                }
            }

            throw new Exception("Routed request configuration was not found");
        }

    }
}
