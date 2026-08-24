using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmsService : IFirearmsService
    {
        public Task<Result>                       DeleteAsync(Firearm    firearm) => throw new NotImplementedException();

        public Task<Result>                       DeleteAsync(MlrbId     firearmId) => throw new NotImplementedException();

        public Task<Result<Firearm>>              GetFirearmAsync(MlrbId id) => throw new NotImplementedException();

        public Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(bool activeOnly = true) => throw new NotImplementedException();

        public Task<Result<MlrbId>>               UpsertAsync(Firearm   firearm) => throw new NotImplementedException();
    }
}