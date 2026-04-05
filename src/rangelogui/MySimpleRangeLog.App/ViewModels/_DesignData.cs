using System;
using System.Linq;
using MyLittleRangeBook.Gui.Models;

namespace MyLittleRangeBook.Gui.ViewModels
{
    /// <summary>
    ///     Provides design-time data for the Avalonia previewer and designer.
    ///     This class is only used during design time to display meaningful sample data
    ///     in visual designers and previews, making UI development easier.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class _DesignData
    {
        internal static readonly Firearm[] TestFirearms =
        [
            new()
            {
                RowId = 1,
                Id = "NANOID-1",
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                Name = "STAG-10",
                Notes = null
            },
            new()
            {
                Id = "NANOID-2",
                RowId = 2,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                Name = "Ruger 10/22",
                Notes = "Mapleseed rifle."
            }
        ];

        internal static readonly SimpleRangeEvent[] TestRangeEvents =
        [
            new()
            {
                Id = "NANOID-3",
                RowId = 1,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                EventDate = new DateTime(2024, 03, 12),
                FirearmName = "Ruger 10/122",
                RangeName = "CHAS",
                RoundsFired = 350,
                AmmoDescription = "CCI SV",
                Notes = "Sample Event #1"
            },
            new()
            {
                Id = "NANOID-4",
                RowId = 2,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow,
                EventDate = new DateTime(2025, 03, 12),
                FirearmName = "Tikka T3",
                RangeName = "SPFGA",
                RoundsFired = 10,
                AmmoDescription = "178gr Hornady BTHP;39.1gr IMR-3031;CCI #200; Federal Brass;2.902 COAL",
                Notes = "Sample event #2"
            }
        ];

        /// <summary>
        ///     Static constructor that initializes design-time data.
        ///     Loads real data from the database to provide realistic examples
        ///     for the visual designer to display.
        /// </summary>
        static _DesignData()
        {
            EditSimpleRangeEventViewModel =
                new EditSimpleRangeEventViewModel(new SimpleRangeEventViewModel(TestRangeEvents.First()));
        }

        /// <summary>
        ///     Gets the design-time data instance for the EditCategoryView.
        ///     Used by the visual designer to display a realistic category editing interface.
        /// </summary>
        public static EditSimpleRangeEventViewModel EditSimpleRangeEventViewModel { get; }
    }
}
