using System;

namespace JsonGen
{
    public abstract class MetadataProvider : IMetadataProvider
    {
        public abstract Metadata GetMetadata(string name);
    }
}
