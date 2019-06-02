using System;
using System.Collections.Generic;

namespace CliFx.Services
{
    public interface ITypeProvider
    {
        IReadOnlyList<Type> GetTypes();
    }
}