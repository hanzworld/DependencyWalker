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
using NuGet;

[assembly: InternalsVisibleTo("DependencyWalkerTests")]

namespace DependencyWalker.Serialisation.JsonConverters
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
}
