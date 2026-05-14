using FluentResults;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook
{
    public class UniqueAssetNamingStrategyTests
    {
        const string Asset1 = @"C:\temp\Picture1.jpg";
        const string Asset2 = @"C:\temp\Picture2.jpg";
        const string Asset3 = @"C:\temp\Picture3.jpg";
        static readonly string TempPath = Path.GetTempPath();
        readonly IRangeEventAssetNamingStrategy _sut = new UniqueAssetNameStrategy().In(TempPath);

        [Fact]
        public void TestUniqueNameCreated()
        {
            Result<(string assetId, string assetPath)> name = _sut.GenerateAssetFileName("rangeEventId", Asset1);
            name.IsSuccess.ShouldBeTrue();

            Path.GetExtension(name.Value.assetPath).ShouldBe(".jpg");
            Path.GetFileNameWithoutExtension(name.Value.assetPath).ShouldBe(name.Value.assetId);
            name.Value.assetPath.ShouldStartWith(TempPath + "rangeEventId\\");
        }

        [Fact]
        public void TestIdsAreSortable()
        {
            Result<(string assetId, string assetPath)> name1 = _sut.GenerateAssetFileName("rangeEventId", Asset1);
            name1.IsSuccess.ShouldBeTrue();
            Result<(string assetId, string assetPath)> name2 = _sut.GenerateAssetFileName("rangeEventId", Asset2);
            name2.IsSuccess.ShouldBeTrue();
            Result<(string assetId, string assetPath)> name3 = _sut.GenerateAssetFileName("rangeEventId", Asset3);
            name3.IsSuccess.ShouldBeTrue();

            var l = new List<string> { name3.Value.assetId, name1.Value.assetId, name2.Value.assetId };
            l.Sort();

            l.ShouldBeInOrder(SortDirection.Ascending);
        }
    }
}
