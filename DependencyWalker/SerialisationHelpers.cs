/* DEPENDENCY WALKER
 * Copyright (c) 2019 Gray Barn Limited. All Rights Reserved.
 *
 * This library is free software: you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.  If not, see
 * <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DependencyWalker.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NuGet;
using Serilog;
using Serilog.Events;

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
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            if (reader.TokenType == JsonToken.String)
            {
                string deserialisedString = (string)serializer.Deserialize(reader, typeof(string));
                var parts = deserialisedString.Split(' ');

                return new DataServicePackage() { Id = parts[0], Version = parts[1] };
            }

            return serializer.Deserialize(reader, objectType);
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


    public class NugetDependencyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(INugetDependency);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(NugetDependency));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class NugetDependencyTreeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(INugetDependencyTree);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(NugetDependencyTree));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class ProjectDependencyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IProjectDependency);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(ProjectDependency));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }


    public class ProjectDependencyTreeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IProjectDependencyTree);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(ProjectDependencyTree));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }



    public class ProjectInSolutionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IProjectInSolution);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(ProjectInSolution));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }


    public class SolutionDependencyTreeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ISolutionDependencyTree);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(SolutionDependencyTree));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class PackageDependencyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PackageDependency);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            if (reader.TokenType == JsonToken.String)
            {
                return serializer.Deserialize(reader, objectType);

            }

            JObject obj = JObject.Load(reader);
            if (obj["Id"] != null)
            {
                var version = new VersionSpec()
                    {IsMaxInclusive = obj["VersionSpec"]["IsMaxInclusive"].ToObject<bool>()};
                    //, IsMinInclusive = true, MaxVersion = "blah", MinVersion = "blah" };
                return new PackageDependency(obj["Id"].ToString(), version, obj["Include"].ToString(), obj["Exclude"].ToString());
            }

            if (obj["Code"] != null)
            {
                return obj["Code"].ToString();
            }

            return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class SerilogTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter
        {
            // trace all messages. nlog can handle filtering
            get { return TraceLevel.Verbose; }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            // log Json.NET message to NLog
            Log.Write(LogEventLevel.Verbose, ex, message);
        }

    }

}
