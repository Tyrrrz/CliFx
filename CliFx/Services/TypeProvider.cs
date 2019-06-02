using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CliFx.Services
{
    public partial class TypeProvider : ITypeProvider
    {
        private readonly IReadOnlyList<Type> _types;

        public TypeProvider(IReadOnlyList<Type> types)
        {
            _types = types;
        }

        public TypeProvider(params Type[] types)
            : this((IReadOnlyList<Type>) types)
        {
        }

        public IReadOnlyList<Type> GetTypes() => _types;
    }

    public partial class TypeProvider
    {
        public static TypeProvider FromAssembly(Assembly assembly) => new TypeProvider(assembly.GetExportedTypes());

        public static TypeProvider FromAssemblies(IReadOnlyList<Assembly> assemblies) =>
            new TypeProvider(assemblies.SelectMany(a => a.ExportedTypes).ToArray());

        public static TypeProvider FromAssemblies(params Assembly[] assemblies) => FromAssemblies((IReadOnlyList<Assembly>) assemblies);
    }
}