using Cindi.Domain.Entities.Users;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Users
{
    public static class UsersClassMap
    {
        public static void Register(BsonClassMap<User> cm)
        {
            cm.AutoMap();
            //cm.MapIdField(a )
            //cm.MapIdMember(c => c.Username);
          
            //  cm.MapIdProperty(c => c.Username);
          //  cm.SetIdMember(cm.GetMemberMap(c => c.Username));
            cm.SetIgnoreExtraElements(true);
        }
    }
}
