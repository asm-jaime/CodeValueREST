using NUnit.Framework;

namespace Tests;


[TestFixture]
class SolutionTests
{
    [Test]
    [TestCase(2, 2, 4)]
    [TestCase(0, 0, 0)]
    public void TestCalculate(int a, int b, int expected)
    {
        // Arrange
    }
}
