﻿using FKRemoteDesktop.Enums;
using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class DoMouseEvent : IMessage
    {
        [ProtoMember(1)]
        public EMouseAction Action { get; set; }

        [ProtoMember(2)]
        public bool IsMouseDown { get; set; }

        [ProtoMember(3)]
        public int X { get; set; }

        [ProtoMember(4)]
        public int Y { get; set; }

        [ProtoMember(5)]
        public int MonitorIndex { get; set; }
    }
}