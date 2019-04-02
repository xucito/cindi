using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.BotKeys
{
    public class BotKey
    {
        public string BotName { get; set; }
        public Guid Id { get; set; }
        public string HashedIdKey { get; set; }
        public byte[] HashedIdKeySalt { get; set; }
        public string PublicEncryptionKey { get; set; }
        public bool IsDisabled { get; set; }
    }
}
