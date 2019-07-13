using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet;

[assembly: InternalsVisibleTo("DependencyWalkerTests")]
namespace DependencyWalker
{

    public class IPackageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPackage);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var package = value as IPackage;
            writer.WriteValue(package.ToString());
        }
    }
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ConcurrentBag<>))
            {
                property.ShouldSerialize =
                    instance =>
                    {                        
                        var list = property.ValueProvider.GetValue(instance) as IEnumerable;

                        //check to see if there is at least one item in the Enumeration
                        return list.GetEnumerator().MoveNext();
                    };
            }

            return property;
        }
    }

}
