namespace JsonGen
{
    public interface IMetadataProvider
    {
        Metadata GetMetadata(string reportName);
    }
}