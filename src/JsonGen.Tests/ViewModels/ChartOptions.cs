namespace Report.ViewModels
{
    public class ChartOptions
    {
        public string XAxisTitle;
        public string[] XAxisCategories;
        public bool XAxisDate;
        public string XAxisDateFormat;
        public string XColumn;
        public string YAxisTitle;
        public string Y2AxisTitle;
        public string YColumn;
        public string ScatterPlotTooltipFormat;
        public ChartSeriesLine[] Series;
        public bool? enablePieChartDataLabels;
        public double? YAxisPlotLine;
    }
}