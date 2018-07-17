using Newtonsoft.Json;

namespace JsonGen
{
    public class Metadata
    {
        public Layout Layout { get; set; }

        public Labels Labels { get; set; }

        public DataSource[] DataSources { get; set; }

        public static Metadata Load(string json) => JsonConvert.DeserializeObject<Metadata>(json);
    }
}
