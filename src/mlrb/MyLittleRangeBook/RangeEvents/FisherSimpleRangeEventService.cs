namespace MyLittleRangeBook.RangeEvents
{
    public class FisherSimpleRangeEventService : ISimpleRangeEventService
    {
        public async Task<Result>                                DeleteAsync(SimpleRangeEvent                simpleRangeEvent,   CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public async Task<Result<SimpleRangeEvent>>              GetAsync(Guid                               simpleRangeEventId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public async Task<Result<Guid>>                          UpsertAsync(SimpleRangeEvent                simpleRangeEvent,   CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(CancellationToken cancellationToken                                = default) => throw new NotImplementedException();

        public async Task<Result>                                ExportToCsv(string                          csvFileName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}