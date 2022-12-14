using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityCodeGen.Ast;
using UnityCodeGen.Extensions;

namespace UnityCodeGen.Builder
{
    public class NamespaceBuilder
    {
        private string _name;
        private readonly List<ClassBuilder> _classBuilder = new List<ClassBuilder>();
        private readonly List<StructBuilder> _structBuilder = new List<StructBuilder>();
        private readonly List<EnumBuilder> _enumBuilder = new List<EnumBuilder>();

        public ClassBuilder WithClass()
        {
            var classBuilder = new ClassBuilder();
            _classBuilder.Add(classBuilder);
            return classBuilder;
        }

        public StructBuilder WithStruct()
        {
            var structBuilder = new StructBuilder();
            _structBuilder.Add(structBuilder);
            return structBuilder;
        }

        public NamespaceBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public EnumBuilder WithEnum()
        {
            var enumBuilder = new EnumBuilder();
            _enumBuilder.Add(enumBuilder);
            return enumBuilder;
        }

        public NamespaceNode Build()
        {
            return new NamespaceNode
            {
                Name = _name,
                Classes = _classBuilder.Map(c => c.Build()).ToArray(),
                Structs = _structBuilder.Map(s => s.Build()).ToArray(),
                Enums = _enumBuilder.Map(e => e.Build()).ToArray(),
            };
        }
    }
}
