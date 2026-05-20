using ByteAether.Ulid;

namespace MyLittleRangeBook.Models
{
    public class MlrbIdTests
    {
        [Fact]
        public void FromEntityId_creates_a_valid_MlrbId()
        {
            var nanoid = new MlrbId().ToString();
            var entityId = new EntityId(nanoid, null);
            var id = MlrbId.From(entityId);

            Assert.NotEqual(MlrbId.Empty, id);
            Assert.True(Ulid.IsValid(id.ToString()));
        }

        [Fact]
        public void EntityIds_with_same_Nanoid_should_have_equal_MlrbId()
        {
            var nanoid = new MlrbId().ToString();
            var entityId1 = new EntityId(nanoid, null);
            var mlrbId1 = MlrbId.From(entityId1);

            var entityId2 = new EntityId(nanoid, 1111);
            var mlrbId2 = MlrbId.From(entityId2);

            Assert.Equal(mlrbId1, mlrbId2);
        }

        [Fact]
        public void Default_MlrbId_should_equal_Empty()
        {
            Assert.Equal(MlrbId.Empty, default);
        }

        [Fact]
        public void New_MlrbId_with_no_args_should_not_be_Empty()
        {
            Assert.NotEqual(MlrbId.Empty, new MlrbId());
        }
    }
}
