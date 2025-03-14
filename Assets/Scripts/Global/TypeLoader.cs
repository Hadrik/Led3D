using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Global
{
    public static class TypeLoader
    {
        public static IEnumerable<Type> GetDerivedType<T>()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(type => 
                type != typeof(T) &&
                typeof(T).IsAssignableFrom(type) &&
                !type.IsAbstract);
        }
    }
}