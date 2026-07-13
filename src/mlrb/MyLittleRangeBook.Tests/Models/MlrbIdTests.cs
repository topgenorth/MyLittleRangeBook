using System.Text.Json;
using ByteAether.Ulid;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Models
{
    public class MlrbIdTests
    {
        [Fact]
        public void Can_serialize_and_deserialize_MlrbId_via_JSON()
        {
            var id = new MlrbId();
            string json = JsonSerializer.Serialize(id, MlrbJsonSerializerContext.Default.MlrbId);
            MlrbId deserialized = JsonSerializer.Deserialize(json, MlrbJsonSerializerContext.Default.MlrbId);

            Assert.Equal(id, deserialized);
            Assert.Equal($"\"{id}\"", json);
        }

        [Fact]
        public void FromFitFile_should_create_same_MlrbId_each_time()
        {
            const string fileName = "06-21-2026_13-15-45.fit";
            var id1 = MlrbId.FromString(fileName);
            var id2 = MlrbId.FromString(fileName);

            id1.ShouldBeEquivalentTo(id2);
            id1.DateTimeOffset.ShouldBeEquivalentTo(id2.DateTimeOffset);
        }


        [Fact]
        public void FromEntityId_creates_a_valid_MlrbId()
        {
            var id = new MlrbId().ToString();
            var entityId = new EntityId(id, null);
            var mlrbid = MlrbId.From(entityId);

            Assert.NotEqual(MlrbId.Empty, mlrbid);
            Assert.True(Ulid.IsValid(mlrbid.ToString()));
        }

        [Fact]
        public void EntityIds_with_same_Id_should_have_equal_MlrbId()
        {
            var id = new MlrbId().ToString();
            var entityId1 = new EntityId(id, null);
            var mlrbId1 = MlrbId.From(entityId1);

            var entityId2 = new EntityId(id, 1111);
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
