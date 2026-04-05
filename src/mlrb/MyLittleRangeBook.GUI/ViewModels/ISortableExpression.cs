using System;

namespace MyLittleRangeBook.GUI.ViewModels
{
    public interface ISortableExpression<in T> where T : ViewModelBase
    {
        string DisplayName { get; }
        Func<T, IComparable> SortExpression { get; }
        string ToString();
    }
}
