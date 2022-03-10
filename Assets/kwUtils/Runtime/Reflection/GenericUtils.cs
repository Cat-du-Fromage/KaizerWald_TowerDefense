using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KWUtils.Reflection
{
    public static class GenericUtils
    {
        public static IEnumerable<Type> GetAllTypes(Type genericType)
        {
            if (!genericType.IsGenericTypeDefinition)
                throw new ArgumentException("Specified type must be a generic type definition.", nameof(genericType));

            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition().Equals(genericType)));
        }
        
        public static IEnumerable<Type> GetAllTypes(Type genericType, params Type[] genericParameterTypes)
        {
            if (!genericType.IsGenericTypeDefinition)
                throw new ArgumentException("Specified type must be a generic type definition.", nameof(genericType));

            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition().Equals(genericType) &&
                              i.GetGenericArguments().Count() == genericParameterTypes.Length &&
                              i.GetGenericArguments().Zip(genericParameterTypes, 
                                      (f, s) => s.IsAssignableFrom(f))
                                  .All(z => z)));
        }
        
        private static IEnumerable<Type> GetAllTypesThatImplementInterface<T>()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface);
        }
    }
}
