using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Builder
{
    public class Renamer
    {
        public AssemblyDefinition AsmDef { get; set; }

        private int Length { get; set; }
        private MemberOverloader _typeOverloader;
        private Dictionary<TypeDefinition, MemberOverloader> _methodOverloaders;
        private Dictionary<TypeDefinition, MemberOverloader> _fieldOverloaders;
        private Dictionary<TypeDefinition, MemberOverloader> _eventOverloaders;

        public Renamer(AssemblyDefinition asmDef)
            : this(asmDef, 20)
        {
        }

        public Renamer(AssemblyDefinition asmDef, int length)
        {
            this.AsmDef = asmDef;
            this.Length = length;
            _typeOverloader = new MemberOverloader(this.Length);
            _methodOverloaders = new Dictionary<TypeDefinition, MemberOverloader>();
            _fieldOverloaders = new Dictionary<TypeDefinition, MemberOverloader>();
            _eventOverloaders = new Dictionary<TypeDefinition, MemberOverloader>();
        }

        // 尝试修改程序集定义数据
        public bool Perform()
        {
            try
            {
                foreach (TypeDefinition typeDef in AsmDef.Modules.SelectMany(module => module.Types))
                {
                    RenameInType(typeDef);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RenameInType(TypeDefinition typeDef)
        {
            if (!typeDef.Namespace.StartsWith("Quasar") || typeDef.Namespace.StartsWith("Quasar.Common.Messages") || typeDef.IsEnum /* || typeDef.HasInterfaces */)
                return;

            _typeOverloader.GiveName(typeDef);

            typeDef.Namespace = string.Empty;

            MemberOverloader methodOverloader = GetMethodOverloader(typeDef);
            MemberOverloader fieldOverloader = GetFieldOverloader(typeDef);
            MemberOverloader eventOverloader = GetEventOverloader(typeDef);

            if (typeDef.HasNestedTypes)
                foreach (TypeDefinition nestedType in typeDef.NestedTypes)
                    RenameInType(nestedType);

            if (typeDef.HasMethods)
                foreach (MethodDefinition methodDef in
                        typeDef.Methods.Where(methodDef =>
                                !methodDef.IsConstructor && !methodDef.HasCustomAttributes &&
                                !methodDef.IsAbstract && !methodDef.IsVirtual))
                    methodOverloader.GiveName(methodDef);

            if (typeDef.HasFields)
                foreach (FieldDefinition fieldDef in typeDef.Fields)
                    fieldOverloader.GiveName(fieldDef);

            if (typeDef.HasEvents)
                foreach (EventDefinition eventDef in typeDef.Events)
                    eventOverloader.GiveName(eventDef);
        }

        private MemberOverloader GetMethodOverloader(TypeDefinition typeDef)
        {
            return GetOverloader(this._methodOverloaders, typeDef);
        }

        private MemberOverloader GetFieldOverloader(TypeDefinition typeDef)
        {
            return GetOverloader(this._fieldOverloaders, typeDef);
        }

        private MemberOverloader GetEventOverloader(TypeDefinition typeDef)
        {
            return GetOverloader(this._eventOverloaders, typeDef);
        }

        private MemberOverloader GetOverloader(Dictionary<TypeDefinition, MemberOverloader> overloaderDictionary,
            TypeDefinition targetTypeDef)
        {
            MemberOverloader overloader;
            if (!overloaderDictionary.TryGetValue(targetTypeDef, out overloader))
            {
                overloader = new MemberOverloader(this.Length);
                overloaderDictionary.Add(targetTypeDef, overloader);
            }
            return overloader;
        }
    }
}
