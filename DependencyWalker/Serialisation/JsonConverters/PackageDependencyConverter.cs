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
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet;

[assembly: InternalsVisibleTo("DependencyWalkerTests")]

namespace DependencyWalker.Serialisation.JsonConverters
{
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
                { IsMaxInclusive = obj["VersionSpec"]["IsMaxInclusive"].ToObject<bool>() };
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

}
