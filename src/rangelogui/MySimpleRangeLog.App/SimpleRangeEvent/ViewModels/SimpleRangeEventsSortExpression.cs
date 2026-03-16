using System;

namespace MySimpleRangeLog.ViewModels
{
    public class SimpleRangeEventsSortExpression
    {
        public SimpleRangeEventsSortExpression(Func<SimpleRangeEventViewModel, IComparable> sortExpression, string name)
        {
            DisplayName = name;
            SortExpression = sortExpression;
        }


        public static SimpleRangeEventsSortExpression SortByEventDateExpression { get; } =
            new(x => x.EventDate, "Event Date");

        public static SimpleRangeEventsSortExpression SortByFirearmNameExpression { get; } =
            new(x => x.FirearmName, "Firearm Name");

        public static SimpleRangeEventsSortExpression SortByRangeNameExpression { get; } =
            new(x => x.RangeName, "Range Name");

        public static SimpleRangeEventsSortExpression[] AvailableSortExpressions { get; } =
        [
            SortByEventDateExpression,
            SortByFirearmNameExpression,
            SortByRangeNameExpression
        ];

        public string DisplayName { get; }

        public Func<SimpleRangeEventViewModel, IComparable> SortExpression { get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
