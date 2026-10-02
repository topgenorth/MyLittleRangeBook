namespace MyLittleRangeBook.GUI.ViewModels
{
    public class SimpleRangeSessionsSortExpression : SortableViewModelExpression<GUI.SimpleRangeSessionViewModel>
    {
        public SimpleRangeSessionsSortExpression(
            Func<GUI.SimpleRangeSessionViewModel, IComparable> sortExpression,
            string name,
            bool isDescending = false)
            : base(sortExpression, name, isDescending)
        {
        }


        public static SimpleRangeSessionsSortExpression SortBySessionDateExpression { get; } =
            new(x => x.EventDate, "Event Date", true);

        public static SimpleRangeSessionsSortExpression SortByFirearmNameExpression { get; } =
            new(x => x.FirearmName, "Firearm Name");

        public static SimpleRangeSessionsSortExpression SortByRangeNameExpression { get; } =
            new(x => x.RangeName, "Range Name");

        public static SimpleRangeSessionsSortExpression[] AvailableSortExpressions { get; } =
        [
            SortBySessionDateExpression,
            SortByFirearmNameExpression,
            SortByRangeNameExpression
        ];
    }
}
