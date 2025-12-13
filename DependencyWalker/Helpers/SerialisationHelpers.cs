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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

[assembly: InternalsVisibleTo("DependencyWalkerTests")]
namespace DependencyWalker
{

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
