using System;

namespace MySimpleRangeLog.ViewModels
{
    public class SimpleRangeEventsSortExpression : SortableViewModelExpression<SimpleRangeEventViewModel>
    {
        public SimpleRangeEventsSortExpression(Func<SimpleRangeEventViewModel, IComparable> sortExpression, string name)
            : base(sortExpression, name)
        {
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
    }
}
