namespace JsonGen.Db
{
    public class Generator : JsonGen.Generator
    {
        public Generator(IMetadataProvider metadataProvider, IFilterableDataProvider filterableDataProvider) : base(metadataProvider)
        {
            base.FilterableDataProvider = filterableDataProvider;
        }
    }
}
