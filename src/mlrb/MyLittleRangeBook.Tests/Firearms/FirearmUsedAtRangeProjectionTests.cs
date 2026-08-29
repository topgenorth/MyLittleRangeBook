using Fisher;
using Fisher.Linq;
using Fisher.Projections;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEvents;
using Shouldly;

namespace MyLittleRangeBook.Tests.Firearms
{
    public sealed class FirearmUsedAtRangeProjectionTests : IAsyncDisposable
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():n}.db");
        public DocumentStore Store { get; private set; } = null!;

        private async Task InitializeAsync()
        {
            Store = DocumentStore.For(opts =>
            {
                opts.Connection($"Data Source={_dbPath}");
                opts.Schema.For<FirearmRoundCount>().UniqueIndex(x => x.Name);
                opts.Schema.For<RangeVisitCount>().UniqueIndex(x => x.Name);
            });

            await Store.ApplyAllConfiguredChangesToDatabaseAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (Store is not null)
            {
                await Store.DisposeAsync();
            }

            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        [Fact]
        public async Task Projects_FirearmUsedAtRange_Creates_New_FirearmRoundCount_When_None_Exists()
        {
            await InitializeAsync();

            var projection = new FirearmUsedAtRangeProjection();
            string firearmName = "Glock 19";
            var event1 = new FirearmUsedAtRange(firearmName, "Local Range", 50, "9mm FMJ", DateTimeOffset.UtcNow);

            await using (IDocumentSession session = Store.IdentitySession())
            {
                await projection.Project(event1, session);
                await session.SaveChangesAsync();
            }

            await using (IQuerySession querySession = Store.QuerySession())
            {
                FirearmRoundCount? roundCount = await querySession.Query<FirearmRoundCount>()
                    .FirstOrDefaultAsync(x => x.Name == firearmName);

                roundCount.ShouldNotBeNull();
                roundCount.Name.ShouldBe(firearmName);
                roundCount.RoundCount.ShouldBe(50);
            }
        }

        [Fact]
        public async Task Projects_FirearmUsedAtRange_Accumulates_RoundCount_When_Existing()
        {
            await InitializeAsync();

            var projection = new FirearmUsedAtRangeProjection();
            string firearmName = "CZ P-10C";
            var event1 = new FirearmUsedAtRange(firearmName, "Range A", 30, "9mm FMJ", DateTimeOffset.UtcNow);
            var event2 = new FirearmUsedAtRange(firearmName, "Range B", 70, "9mm Hollow Point", DateTimeOffset.UtcNow);

            await using (IDocumentSession session = Store.IdentitySession())
            {
                await projection.Project(event1, session);
                await session.SaveChangesAsync();
            }

            await using (IDocumentSession session = Store.IdentitySession())
            {
                await projection.Project(event2, session);
                await session.SaveChangesAsync();
            }

            await using (IQuerySession querySession = Store.QuerySession())
            {
                FirearmRoundCount? roundCount = await querySession.Query<FirearmRoundCount>()
                    .FirstOrDefaultAsync(x => x.Name == firearmName);

                roundCount.ShouldNotBeNull();
                roundCount.Name.ShouldBe(firearmName);
                roundCount.RoundCount.ShouldBe(100);
            }
        }

        [Fact]
        public async Task Projects_FirearmUsedAtRange_Maintains_Separate_Counts_For_Different_Firearms()
        {
            await InitializeAsync();

            var projection = new FirearmUsedAtRangeProjection();
            var event1 = new FirearmUsedAtRange("Glock 19", "Range A", 50, "9mm", DateTimeOffset.UtcNow);
            var event2 = new FirearmUsedAtRange("Sig P365", "Range A", 25, "9mm", DateTimeOffset.UtcNow);

            await using (IDocumentSession session = Store.IdentitySession())
            {
                await projection.Project(event1, session);
                await projection.Project(event2, session);
                await session.SaveChangesAsync();
            }

            await using (IQuerySession querySession = Store.QuerySession())
            {
                FirearmRoundCount? g19 = await querySession.Query<FirearmRoundCount>()
                    .FirstOrDefaultAsync(x => x.Name == "Glock 19");
                FirearmRoundCount? p365 = await querySession.Query<FirearmRoundCount>()
                    .FirstOrDefaultAsync(x => x.Name == "Sig P365");

                g19.ShouldNotBeNull();
                g19.RoundCount.ShouldBe(50);

                p365.ShouldNotBeNull();
                p365.RoundCount.ShouldBe(25);
            }
        }

        [Fact]
        public void Create_SimpleRangeEventCreated_Returns_Expected_RangeVisitCount()
        {
            var projection = new FirearmUsedAtRangeProjection();
            var evt = new SimpleRangeEventCreated(
                new DateOnly(2026, 8, 20),
                "Glock 19",
                "Local Range",
                50,
                "9mm FMJ",
                "Great session"
            );

            RangeVisitCount result = projection.Create(evt);

            result.ShouldNotBeNull();
            result.Name.ShouldBe("Local Range");
            result.VisitCount.ShouldBe(1);
            result.MostRecentVisit.ShouldBe(new DateTimeOffset(new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc), TimeSpan.Zero));
        }
    }
}
