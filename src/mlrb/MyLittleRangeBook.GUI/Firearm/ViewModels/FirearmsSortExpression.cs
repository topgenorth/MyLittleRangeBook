namespace MyLittleRangeBook.GUI.ViewModels
{
    public class FirearmsSortExpression : SortableViewModelExpression<FirearmViewModel>
    {
        public FirearmsSortExpression(Func<FirearmViewModel, IComparable> sortExpression, string name) : base(
            sortExpression, name)
        {
        }

        public static FirearmsSortExpression SortByName => new(x => x.Name, "Name");
    }
}
