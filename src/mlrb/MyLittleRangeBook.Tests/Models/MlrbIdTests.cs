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
            MlrbId id           = new();
            string json         = JsonSerializer.Serialize(id, MlrbJsonSerializerContext.Default.MlrbId);
            MlrbId deserialized = JsonSerializer.Deserialize(json, MlrbJsonSerializerContext.Default.MlrbId);

            Assert.Equal(id,          deserialized);
            Assert.Equal($"\"{id}\"", json);
        }

        [Fact]
        public void FromFitFile_should_create_same_MlrbId_each_time()
        {
            const string fileName = "06-21-2026_13-15-45.fit";
            MlrbId       id1      = MlrbId.FromString(fileName);
            MlrbId       id2      = MlrbId.FromString(fileName);

            id1.ShouldBeEquivalentTo(id2);
            id1.DateTimeOffset.ShouldBeEquivalentTo(id2.DateTimeOffset);
        }


        [Fact]
        public void FromEntityId_creates_a_valid_MlrbId()
        {
            MlrbId mlrbid = new MlrbId();

            Assert.NotEqual(MlrbId.Empty, mlrbid);
            Assert.True(Ulid.IsValid(mlrbid.ToString()));
        }

        [Fact]
        public void EntityIds_with_same_Id_should_have_equal_MlrbId()
        {
            MlrbId   mlrbId1   = new MlrbId();

            MlrbId mlrbId2 = mlrbId1;

            Assert.Equal(mlrbId1, mlrbId2);
        }

        [Fact]
        public void Default_MlrbId_should_equal_Empty() => Assert.Equal(MlrbId.Empty, default);

        [Fact]
        public void New_MlrbId_with_no_args_should_not_be_Empty() => Assert.NotEqual(MlrbId.Empty, new MlrbId());
    }
}