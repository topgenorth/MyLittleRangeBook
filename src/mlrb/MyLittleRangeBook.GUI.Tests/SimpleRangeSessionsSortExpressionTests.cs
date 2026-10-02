using MyLittleRangeBook.GUI.ViewModels;
using Xunit;

namespace MyLittleRangeBook.GUI.Tests
{
    public class SimpleRangeSessionsSortExpressionTests
    {
        [Fact]
        public void SortByEventDateExpression_ShouldBeDescending()
        {
            // Arrange & Act
            var expression = SimpleRangeSessionsSortExpression.SortBySessionDateExpression;

            // Assert
            Assert.True(expression.IsDescending);
            Assert.Equal("Event Date", expression.DisplayName);
        }

        [Fact]
        public void SortByFirearmNameExpression_ShouldBeAscending()
        {
            // Arrange & Act
            var expression = SimpleRangeSessionsSortExpression.SortByFirearmNameExpression;

            // Assert
            Assert.False(expression.IsDescending);
            Assert.Equal("Firearm Name", expression.DisplayName);
        }
    }
}
