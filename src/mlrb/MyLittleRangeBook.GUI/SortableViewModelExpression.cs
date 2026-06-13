using MyLittleRangeBook.GUI.ViewModels;

namespace MyLittleRangeBook.GUI
{
    public abstract class SortableViewModelExpression<T>(Func<T, IComparable> sortExpression, string name)
        : ISortableExpression<T>
        where T : ViewModelBase
    {
        public string DisplayName { get; } = name;
        public Func<T, IComparable> SortExpression { get; } = sortExpression;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
