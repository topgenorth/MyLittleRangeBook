using ByteAether.Ulid;
using NanoidDotNet;

namespace MyLittleRangeBook.Models
{
    public class MlrbIdTests
    {
        [Fact]
        public void CreateMlrbFromEntityIdTests()
        {
            string? nanoid = Nanoid.Generate();
            var entityId = new EntityId(nanoid, null);
            var id = new MlrbId(entityId);

            Assert.NotEqual(MlrbId.Empty, id);
            Assert.True(Ulid.IsValid(id.ToString()));
        }

        [Fact]
        public void ConsistentlyConvertEntityIdToMlrbId()
        {
            string? nanoid = Nanoid.Generate();
            var entityId1 = new EntityId(nanoid, null);
            var mlrbId1 = MlrbId.From(entityId1);

            var entityId2 = new EntityId(nanoid, 1111);
            var mlrbId2 = MlrbId.From(entityId2);

            Assert.Equal(mlrbId1, mlrbId2);
        }
    }
}
