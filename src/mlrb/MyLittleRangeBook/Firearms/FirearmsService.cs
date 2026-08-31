using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmsService : IFirearmsService
    {
        public Task<Result>                       DeleteAsync(FirearmTableRow    firearmTableRow) => throw new NotImplementedException();

        public Task<Result>                       DeleteAsync(MlrbId     firearmId) => throw new NotImplementedException();

        public Task<Result<FirearmTableRow>>              GetFirearmAsync(MlrbId id) => throw new NotImplementedException();

        public Task<Result<IEnumerable<FirearmTableRow>>> GetFirearmsAsync(bool activeOnly = true) => throw new NotImplementedException();

        public Task<Result<MlrbId>>               UpsertAsync(FirearmTableRow   firearmTableRow) => throw new NotImplementedException();
    }
}