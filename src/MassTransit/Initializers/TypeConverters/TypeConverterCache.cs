namespace MassTransit.Initializers.TypeConverters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Internals.Extensions;
    using Util;


    public class TypeConverterCache :
        ITypeConverterCache
    {
        readonly ConcurrentDictionary<Type, object> _typeConverters;
        readonly IList<object> _converters;

        TypeConverterCache()
        {
            _typeConverters = new ConcurrentDictionary<Type, object>();
            _converters = new List<object>();

            var assembly = typeof(BooleanTypeConverter).Assembly;
            var ns = typeof(BooleanTypeConverter).Namespace;

            var converterTypes = AssemblyTypeCache.FindTypes(assembly, TypeClassification.Concrete | TypeClassification.Closed,
                x => x.Namespace.StartsWith(ns) && x.HasInterface(typeof(ITypeConverter<,>))).GetAwaiter().GetResult().ToArray();

            foreach (var converterType in converterTypes)
                AddSupportedTypes(converterType);
        }

        bool ITypeConverterCache.TryGetTypeConverter<TProperty, TInput>(out ITypeConverter<TProperty, TInput> typeConverter)
        {
            var neededType = typeof(ITypeConverter<TProperty, TInput>);

            if (_typeConverters.TryGetValue(neededType, out object converter))
            {
                typeConverter = converter as ITypeConverter<TProperty, TInput>;
                return typeConverter != null;
            }

            var matched = _converters.FirstOrDefault(x => x.GetType().HasInterface(neededType));
            if (matched != default)
            {
                _typeConverters.GetOrAdd(neededType, matched);

                typeConverter = matched as ITypeConverter<TProperty, TInput>;
                return typeConverter != null;
            }

            Type propertyType = typeof(TProperty);
            if (propertyType.IsEnum)
            {
                var enumConverterType = typeof(EnumTypeConverter<>).MakeGenericType(propertyType);
                if (enumConverterType.HasInterface(neededType))
                {
                    AddSupportedTypes(enumConverterType);
                }
            }
            else if (propertyType.IsNullable(out var underlyingType))
            {
                if (underlyingType == typeof(TInput))
                {
                    var nullableType = typeof(ToNullableTypeConverter<>).MakeGenericType(underlyingType);
                    AddSupportedTypes(nullableType);
                }
                else
                {
                    var converterType = typeof(ITypeConverter<,>).MakeGenericType(underlyingType, typeof(TInput));
                    if (_typeConverters.TryGetValue(converterType, out converter))
                    {
                        var nullableType = typeof(ToNullableTypeConverter<,>).MakeGenericType(underlyingType, typeof(TInput));
                        AddSupportedTypes(nullableType, converter);
                    }
                }
            }
            else if (typeof(TInput).IsNullable(out underlyingType))
            {
                if (underlyingType == propertyType)
                {
                    var nullableType = typeof(FromNullableTypeConverter<>).MakeGenericType(underlyingType);
                    AddSupportedTypes(nullableType);
                }
                else
                {
                    var converterType = typeof(ITypeConverter<,>).MakeGenericType(propertyType, underlyingType);
                    if (_typeConverters.TryGetValue(converterType, out converter))
                    {
                        var nullableType = typeof(FromNullableTypeConverter<,>).MakeGenericType(propertyType, underlyingType);
                        AddSupportedTypes(nullableType, converter);
                    }
                }
            }

            if (_typeConverters.TryGetValue(neededType, out converter))
            {
                typeConverter = converter as ITypeConverter<TProperty, TInput>;
                return typeConverter != null;
            }

            typeConverter = null;
            return false;
        }

        void AddSupportedTypes(Type converterType, params object[] args)
        {
            Type[] interfaceTypes = converterType.GetInterfaces();

            Type[] types = interfaceTypes.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITypeConverter<,>)).ToArray();
            if (types.Length > 0)
            {
                try
                {
                    var converter = Activator.CreateInstance(converterType, args);
                    _converters.Add(converter);

                    foreach (var type in types)
                        _typeConverters[type] = converter;
                }
                catch (Exception)
                {
                    // we don't care if the type can't be created, we'll just skip adding it
                }
            }
        }

        public static bool TryGetTypeConverter<TProperty, TInputProperty>(out ITypeConverter<TProperty, TInputProperty> typeConverter)
        {
            return Cached.Cache.Value.TryGetTypeConverter(out typeConverter);
        }


        static class Cached
        {
            internal static readonly Lazy<ITypeConverterCache> Cache = new Lazy<ITypeConverterCache>(() => new TypeConverterCache());
        }
    }
}
