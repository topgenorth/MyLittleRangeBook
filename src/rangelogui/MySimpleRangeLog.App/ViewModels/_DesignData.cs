using System.Linq;
using MySimpleRangeLog.Helper;

namespace MySimpleRangeLog.ViewModels
{
    /// <summary>
    ///     Provides design-time data for the Avalonia previewer and designer.
    ///     This class is only used during design time to display meaningful sample data
    ///     in visual designers and previews, making UI development easier.
    /// </summary>
    public static class _DesignData
    {
        /// <summary>
        ///     Static constructor that initializes design-time data.
        ///     Loads real data from the database to provide realistic examples
        ///     for the visual designer to display.
        /// </summary>
        static _DesignData()
        {
            var rangeEvents = DatabaseHelper.GetSimpleRangeEventsAsync().Result;
            EditSimpleRangeEventViewModel =
                new EditSimpleRangeEventViewModel(new SimpleRangeEventViewModel(rangeEvents.First()));
        }

        /// <summary>
        ///     Gets the design-time data instance for the EditCategoryView.
        ///     Used by the visual designer to display a realistic category editing interface.
        /// </summary>
        public static EditSimpleRangeEventViewModel EditSimpleRangeEventViewModel { get; }
    }
}
