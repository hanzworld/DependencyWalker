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
using System.Runtime.Serialization;
using NuGet;
using Serilog;

namespace DependencyWalker
{
    [Serializable]
    internal class UnableToRetrievePackageException : Exception
    {
        private string id;
        private SemanticVersion version;

        public UnableToRetrievePackageException(string id, SemanticVersion version)
        {
            this.id = id;
            this.version = version;
        }


        protected UnableToRetrievePackageException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }

        public override string Message
        {
            get { return $"Couldn't retrieve {id} ({version}) from server. It appears to be missing."; }
        }
    }

    [Serializable]
    internal class ShortCircuitingResolveDependencyException : Exception
    {
        private readonly PackageDependency dependency;

        public ShortCircuitingResolveDependencyException(PackageDependency dependency)
        {
            this.dependency = dependency;
        }

        protected ShortCircuitingResolveDependencyException(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }

        public override string Message
        {
            get { return $"Encountered null exception resolving {dependency}"; }
        }
    }

    [Serializable]
    internal class UnableToResolvePackageDependencyException : Exception
    {
        private PackageDependency dependency;


        public UnableToResolvePackageDependencyException(PackageDependency dependency)
        {
            this.dependency = dependency;
        }

        protected UnableToResolvePackageDependencyException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        public override string Message
        {
            get { return $"Couldn't resolve dependency {dependency}"; }
        }
    }
}