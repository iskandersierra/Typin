﻿namespace Typin.Internal.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensions
    {
        public static bool Implements(this Type type, Type interfaceType)
        {
            return type.GetInterfaces().Contains(interfaceType);
        }

        public static Type? TryGetEnumerableArgumentUnderlyingType(this PropertyInfo? property)
        {
            return property is not null && property.PropertyType != typeof(string)
                       ? property.PropertyType.TryGetEnumerableUnderlyingType()
                       : null;
        }

        public static Type? TryGetNullableUnderlyingType(this Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        public static Type? TryGetEnumerableUnderlyingType(this Type type)
        {
            if (type.IsPrimitive)
                return null;

            if (type == typeof(IEnumerable))
                return typeof(object);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments().FirstOrDefault();

            return type.GetInterfaces()
                       .Select(TryGetEnumerableUnderlyingType)
                       .Where(t => t is not null)
                       .OrderByDescending(t => t != typeof(object)) // prioritize more specific types
                       .FirstOrDefault();
        }
    }
}