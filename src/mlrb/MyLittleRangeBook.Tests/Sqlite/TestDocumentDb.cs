using System.Data;
using Fisher;
using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Projections;
using MyLittleRangeBook.Cartridges;

namespace MyLittleRangeBook.Sqlite
{
    public class TestDocumentDb : IAsyncDisposable
    {
        readonly string        _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():n}.db");
        public   DocumentStore Store { get; private set; } = null!;

        public async ValueTask DisposeAsync() => await Store.DisposeAsync(); // releases this store's pooled connections

        public async Task InitializeAsync()
        {
            // [TO20260820] https://fisher.jasperfx.net/configuration/storeoptions.html
            Store = DocumentStore.For(opts =>
                                      {
                                          opts.Connection("Data Source=test;Mode=Memory;Cache=Shared");


                                          opts.Policies.AllDocumentsSoftDeleted();

                                          opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;


                                          opts.Events.StreamIdentity      = StreamIdentity.AsString;
                                          opts.Events.EnableCausationId   = true;
                                          opts.Events.EnableCorrelationId = true;
                                          opts.Events.EnableUserName      = false;

                                          opts.Schema.For<Cartridge>()
                                              ;

                                          // opts.Projections.Snapshot<Cartridge>(SnapshotLifecycle.Inline);
                                      });

            await Store.ApplyAllConfiguredChangesToDatabaseAsync();
        }

        [Fact]
        public async Task Test1()
        {
            await InitializeAsync();
            SessionOptions               options = new()
                                                   {
                                                       IsolationLevel = IsolationLevel.Serializable

                                                   };
            await using IDocumentSession session = Store.IdentitySession();
            Cartridge cartridge = new()
                                  {
                                      CommonName = "Test", ProjectileDiameterMetric = 11.43, Name = "Testing cartridge",
                                  };
            session.CurrentUserName = "tom";
            session.Store(cartridge);

            await session.SaveChangesAsync();

            Cartridge? loaded = await session.LoadAsync<Cartridge>("FakeId");
        }
    }
}