using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Fisher;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.GUI.ViewModels;
using MyLittleRangeBook.RangeEvents;
using NSubstitute;
using Serilog;
using SharedControls.Services;
using Shouldly;
using Xunit;

namespace MyLittleRangeBook.GUI.Tests
{
    public class EditSimpleRangeEventViewModelTests
    {
        [Fact]
        public async Task SettingFirearmNameSearchText_UpdatesAmmoDescriptions_ForSelectedFirearm()
        {
            // Arrange
            var dialogService = Substitute.For<IDialogService>();
            Func<IDialogParticipant, IDialogService> dialogServiceFactory = _ => dialogService;
            var logger = Substitute.For<ILogger>();
            var session = Substitute.For<IDocumentSession>();
            var advancedSql = Substitute.For<IAdvancedSql>();
            session.AdvancedSql.Returns(advancedSql);

            var rangeEventService = Substitute.For<ISimpleRangeEventService>();
            var rangeEventVm = new SimpleRangeEventViewModel();

            var expectedAmmo = new List<string> { "9mm Luger 124gr FMJ", "9mm Luger 115gr JHP" };
            advancedSql.QueryAsync<string>(
                Arg.Is<string>(sql => sql.Contains("firearm_ammo_descriptions") && sql.Contains("firearm_name")),
                Arg.Any<CancellationToken>(),
                Arg.Is<object[]>(args => args.Length == 1 && (string)args[0] == "Glock 19"))
                .Returns(Task.FromResult<IReadOnlyList<string>>(expectedAmmo));

            var vm = new EditSimpleRangeEventViewModel(
                dialogServiceFactory,
                logger,
                session,
                rangeEventVm,
                rangeEventService);

            var propertyChangedEvents = new List<string>();
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    propertyChangedEvents.Add(args.PropertyName);
                }
            };

            // Act - user types or selects firearm name and the control loses focus
            vm.FirearmNameSearchText = "Glock 19";

            // Wait a moment for async task to complete
            await Task.Delay(50);

            // Assert
            vm.FirearmNameSearchText.ShouldBe("Glock 19");
            vm.Item.FirearmName.ShouldBe("Glock 19");
            vm.AmmoDescription.ShouldBe(expectedAmmo);
            propertyChangedEvents.ShouldContain(nameof(EditSimpleRangeEventViewModel.FirearmNameSearchText));
            propertyChangedEvents.ShouldContain(nameof(EditSimpleRangeEventViewModel.AmmoDescription));
        }

        [Fact]
        public void Constructor_InitializesFirearmNameSearchText_FromExistingRangeEvent()
        {
            // Arrange
            var dialogService = Substitute.For<IDialogService>();
            Func<IDialogParticipant, IDialogService> dialogServiceFactory = _ => dialogService;
            var logger = Substitute.For<ILogger>();
            var session = Substitute.For<IDocumentSession>();
            var advancedSql = Substitute.For<IAdvancedSql>();
            session.AdvancedSql.Returns(advancedSql);

            var rangeEventService = Substitute.For<ISimpleRangeEventService>();
            var rangeEvent = new SimpleRangeEvent
            {
                Id = Guid.NewGuid(),
                FirearmName = "Sig Sauer P320",
                EventDate = DateTime.Now,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow
            };
            var rangeEventVm = new SimpleRangeEventViewModel(rangeEvent);

            // Act
            var vm = new EditSimpleRangeEventViewModel(
                dialogServiceFactory,
                logger,
                session,
                rangeEventVm,
                rangeEventService);

            // Assert
            vm.FirearmNameSearchText.ShouldBe("Sig Sauer P320");
            vm.Item.FirearmName.ShouldBe("Sig Sauer P320");
        }
    }
}
