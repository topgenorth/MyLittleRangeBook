using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Helper interface for getting data about firearms and ranges when trying to create a range event.
    /// </summary>
    public interface ISimpleRangeEventHelper
    {
        /// <summary>
        ///     Gets a list of firearm names and range names that can be used when creating a range event.
        /// </summary>
        /// <returns></returns>
        Task<Result<(List<string>, List<string>)>> GetFirearmsAndRangesAsync(DapperCommandContext context);

        /// <summary>
        ///     Gets a list of ammo descriptions that have been used with the specified firearm.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <returns></returns>
        Task<Result<List<string>>> GetAmmoDescriptionsForFirearmAsync(DapperCommandContext context, string firearmName);
    }
}
