using System.Collections.Generic;

namespace Report.ViewModels
{
    public enum AnalyticsType
    {
        Widget = 0,
        Page = 1
    }
    public class AnalyticsModel
    {
        public string Version;
        public AnalyticsCriteria Criteria;
        public AnalyticsPage Page;
        public IReadOnlyCollection<VisualItem> Visuals;
    }

    public enum FilterType
    {
        Date = 1,
        Lookup = 2
    }

    public enum VisualType
    {
        SimpleChart = 1,
        ScatterPlotChart = 2,
        PieChart = 3,
        Column = 4,
        Bar = 5
    }
}