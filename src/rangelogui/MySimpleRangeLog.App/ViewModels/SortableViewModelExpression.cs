using System;

namespace MyLittleRangeBook.Gui.ViewModels
{
    public abstract class SortableViewModelExpression<T> : ISortableExpression<T> where T : ViewModelBase
    {
        protected SortableViewModelExpression(Func<T, IComparable> sortExpression, string name)
        {
            DisplayName = name;
            SortExpression = sortExpression;
        }

        public string DisplayName { get; }
        public Func<T, IComparable> SortExpression { get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
