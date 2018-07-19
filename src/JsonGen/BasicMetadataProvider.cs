using System;

namespace JsonGen
{
    public class BasicMetadataProvider : IMetadataProvider
    {
        private readonly Func<string, Metadata> metadata;

        public BasicMetadataProvider(Func<string, Metadata> metadata)
        {
            this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public Metadata GetMetadata(string name)
        {
            return metadata(name);
        }
    }
}
