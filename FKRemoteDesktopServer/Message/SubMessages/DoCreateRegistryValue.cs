﻿using Microsoft.Win32;
using ProtoBuf;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class DoCreateRegistryValue : IMessage
    {
        [ProtoMember(1)]
        public string KeyPath { get; set; }

        [ProtoMember(2)]
        public RegistryValueKind Kind { get; set; }
    }
}
