using Cindi.Application.Services.ClusterState;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Utilities;
using System;
using System.Collections.Concurrent;

namespace Cindi.Domain.Entities.States
{
    public class CindiClusterState
    {
        public Guid Id { get; set; }
        public string Version { get; set; } = "1.0";
        public string EncryptionKeyHash { get; set; }
        public byte[] EncryptionKeySalt { get; set; }
        public ClusterSettings Settings { get; set; }
        public bool Initialized { get; set; } = false;
        public ConcurrentDictionary<string, Lock> Locks = new ConcurrentDictionary<string, Lock>();
    }
}
