using MyLittleRangeBook.GUI.ViewModels;
using Xunit;

namespace MyLittleRangeBook.GUI.Tests
{
    public class SimpleRangeEventsSortExpressionTests
    {
        [Fact]
        public void SortByEventDateExpression_ShouldBeDescending()
        {
            // Arrange & Act
            var expression = SimpleRangeEventsSortExpression.SortByEventDateExpression;

            // Assert
            Assert.True(expression.IsDescending);
            Assert.Equal("Event Date", expression.DisplayName);
        }

        [Fact]
        public void SortByFirearmNameExpression_ShouldBeAscending()
        {
            // Arrange & Act
            var expression = SimpleRangeEventsSortExpression.SortByFirearmNameExpression;

            // Assert
            Assert.False(expression.IsDescending);
            Assert.Equal("Firearm Name", expression.DisplayName);
        }
    }
}
