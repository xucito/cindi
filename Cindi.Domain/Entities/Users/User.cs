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
        [Key]
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string Email { get; set; }
        public byte[] Salt { get; set; }
        public bool IsDisabled { get; set; }
    }
}
