namespace JsonGen
{
    public class DatasourceOptions
    {
        public bool ApplyFilter { get; set; } = true;

        public string[] IgnoreFilteringOn { get; set; }

        public bool ReplaceMacrosOnly { get; set; } = false;
    }
}
