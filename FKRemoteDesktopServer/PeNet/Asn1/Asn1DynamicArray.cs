﻿using System.IO;
using System.Xml.Linq;

namespace PeNet.Asn1 {
    public class Asn1DynamicArray : Asn1CompositeNode {

        public const string NODE_NAME = "Dynamic Length Array";

        public static Asn1DynamicArray ReadFrom(Asn1UniversalNodeType type, Stream stream) {
            var array = new Asn1DynamicArray(type);
            Asn1Node node;
            while ((node = Asn1Node.ReadNode(stream)) != null) {
                array.Nodes.Add(node);
            }
            return array;
        }

        public Asn1DynamicArray(Asn1UniversalNodeType type) {
            NodeType = type;
        }

        public override Asn1UniversalNodeType NodeType { get; }

        protected override XElement ToXElementCore() {
            return new XElement(NODE_NAME);
        }

        public override byte[] GetBytes() {
            var payload = GetBytesCore();
            var type = NodeType;
            var tagClass = TagClass;
            var tagForm = TagForm;
            var mem = new MemoryStream();
            var identifier = ((int)tagClass << 6) | ((int)tagForm << 5) | (int)type;
            mem.WriteByte((byte)identifier);
            mem.WriteByte((byte)0x80);  // Marker that what follows is of indefinite length
            mem.Write(payload, 0, payload.Length);
            mem.WriteByte((byte)0x00);  // End marker is two zero values
            mem.WriteByte((byte)0x00);  // End marker is two zero values
            return mem.ToArray();
        }
    }
}
