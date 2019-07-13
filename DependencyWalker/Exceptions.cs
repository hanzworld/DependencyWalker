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