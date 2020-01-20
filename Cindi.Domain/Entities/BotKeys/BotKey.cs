using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.BotKeys
{
    public class BotKey : ShardData
    {
        public BotKey()
        {
            ShardType = typeof(BotKey).Name;
        }
        public string BotName { get; set; }
        public string HashedIdKey { get; set; }
        public byte[] HashedIdKeySalt { get; set; }
        public string PublicEncryptionKey { get; set; }
        public bool IsDisabled { get; set; }
        public double Nonce { get; set; } = 0;
        public DateTime RegisteredOn { get; set; }
    }
}
