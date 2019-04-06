using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ValueObjects
{
    public class RSAEncodedKeyPair
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
