using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Domain.Entities.Users
{
    public class User : TrackedEntity
    {
        public User() { }

        public User(
            string username,
            string hashedPassword,
            string email,
            byte[] salt,
            string createdBy,
            DateTime createdOn,
            Guid id
            ) : base(
            new Journal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                new Update()
                    {
                        FieldName = "id",
                        Value = id,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "username",
                        Value = username,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "hashedpassword",
                        Value = hashedPassword,
                        Type = UpdateType.Create
                    },
                   new Update()
                    {
                        FieldName = "email",
                        Value = email,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "salt",
                        Value = salt,
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
            Type = typeof(User).Name;
        }

        [Key]
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string Email { get; set; }
        public byte[] Salt { get; set; }
        public bool IsDisabled { get; set; }
    }
}
