using System.Collections.Generic;

namespace Report.ViewModels
{
    public class AnalyticsPage
    {
        public IReadOnlyCollection<FilterGroup> FilterGroups;
        public IReadOnlyCollection<FilterItem> Filters;
        public IReadOnlyDictionary<string, string> Res;
    };
}