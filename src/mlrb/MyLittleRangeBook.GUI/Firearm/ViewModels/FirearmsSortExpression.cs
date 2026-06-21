namespace MyLittleRangeBook.GUI.ViewModels
{
    public class FirearmsSortExpression : SortableViewModelExpression<FirearmViewModel>
    {
        public FirearmsSortExpression(
            Func<FirearmViewModel, IComparable> sortExpression,
            string name,
            bool isDescending = false) : base(
            sortExpression, name, isDescending)
        {
        }

        public static FirearmsSortExpression SortByName => new(x => x.Name, "Name");
    }
}
