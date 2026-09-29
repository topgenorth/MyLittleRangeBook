using System.Diagnostics;
using Fisher;
using JasperFx.Events.Projections;
using MyLittleRangeBook.Cartridges;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook.Sqlite
{
    public sealed class store_fixture : IAsyncDisposable
    {
        readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():n}.db");

        public DocumentStore Store { get; private set; } = null!;

        public async ValueTask DisposeAsync()
        {
            if (Store != null)
            {
                await Store.DisposeAsync(); // releases this store's pooled connections
            }
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        public async Task InitializeAsync()
        {
            Store = DocumentStore.For(opts =>
                                      {
                                          opts.Schema.For<Cartridge>()
                                              .Index(x => x.CommonName)
                                              .UniqueIndex(x => x.Name)
                                              .UseNumericRevisions()
                                              .SoftDeleted()
                                              ;
                                          opts.Projections.Add<MyLittleRangeBook.Recipes.AmmoDescriptionProjection2>(ProjectionLifecycle.Inline);
                                          opts.Connection($"Data Source={_path}");
                                      });

            Debug.WriteLine($"Initializing store at {_path}");
            await Store.ApplyAllConfiguredChangesToDatabaseAsync();
        }

        [Fact]
        public async Task Test1()
        {
            await InitializeAsync();
            Cartridge cartridge = new()
                                  {
                                      CommonName = "Test", ProjectileDiameterMetric = 11.43, Name = "Testing cartridge",
                                  };
            await using IDocumentSession session = Store.IdentitySession();
            session.Store(cartridge);
            await session.SaveChangesAsync();

            Cartridge? loaded = await session.LoadAsync<Cartridge>((Guid)cartridge.Id);
            Debug.WriteLine($"Loaded cartridge: {loaded?.CommonName}");
        }
    }
}