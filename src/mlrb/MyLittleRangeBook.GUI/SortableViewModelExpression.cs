using MyLittleRangeBook.GUI.ViewModels;

namespace MyLittleRangeBook.GUI
{
    public abstract class SortableViewModelExpression<T>(
        Func<T, IComparable> sortExpression,
        string name,
        bool isDescending = false)
        : ISortableExpression<T>
        where T : ViewModelBase
    {
        public string DisplayName { get; } = name;
        public Func<T, IComparable> SortExpression { get; } = sortExpression;
        public bool IsDescending { get; } = isDescending;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
