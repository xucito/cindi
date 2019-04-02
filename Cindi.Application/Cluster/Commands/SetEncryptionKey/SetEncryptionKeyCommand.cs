using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Commands.SetEncryptionKey
{
    public class SetEncryptionKeyCommand: IRequest<CommandResult>
    {
        public string EncryptionKey { get; set; }
    }
}
