using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message
{
    public static class TypeRegistry
    {
        private static int _typeIndex;      // 消息类型的内部索引

        /// <summary>
        ///  向序列化器中添加一个类型，以便消息可以正确的进行序列化
        /// </summary>
        /// <param name="parent">父类型，例如IMessage</param>
        /// <param name="type">实际需要序列化的类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddTypeToSerializer(Type parent, Type type)
        {
            if (type == null || parent == null)
                throw new ArgumentNullException();

            bool isAlreadyAdded = RuntimeTypeModel.Default[parent].GetSubtypes().Any(subType => subType.DerivedType.Type == type);

            if (!isAlreadyAdded)
                RuntimeTypeModel.Default[parent].AddSubType(++_typeIndex, type);
        }

        public static void AddTypesToSerializer(Type parent, params Type[] types)
        {
            foreach (Type type in types)
                AddTypeToSerializer(parent, type);
        }

        public static IEnumerable<Type> GetPacketTypes(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
        }
    }
}