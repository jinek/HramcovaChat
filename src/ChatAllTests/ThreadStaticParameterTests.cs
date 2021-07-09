using ChatHelpers;
using Xunit;

namespace ChatAllTests
{
    public class ThreadStaticParameterTests
    {
        [Fact]
        public void TestOneThreadHasParameter()
        {
            const int valueToCheck = 2;

            using (TestClass.Value.StartParameterRegion(valueToCheck))
            {
                Assert.Equal(valueToCheck, TestClass.Value.CurrentValue);
            }

            Assert.Equal(default, TestClass.Value.CurrentValue);
        }


        [Fact]
        public void OneThreadTwoParameters()
        {
            const int value1 = 30;
            const int value2 = 40;

            using (TestClass.Value.StartParameterRegion(value1))
            {
                Assert.Equal(value1, TestClass.Value.CurrentValue);
                Assert.Equal(default, TestClass.SecondValue.CurrentValue);

                using (TestClass.SecondValue.StartParameterRegion(value2))
                {
                    Assert.Equal(value1, TestClass.Value.CurrentValue);
                    Assert.Equal(value2, TestClass.SecondValue.CurrentValue);
                }

                Assert.Equal(value1, TestClass.Value.CurrentValue);
                Assert.Equal(default, TestClass.SecondValue.CurrentValue);
            }

            Assert.Equal(default, TestClass.Value.CurrentValue);
            Assert.Equal(default, TestClass.SecondValue.CurrentValue);
        }

        private static class TestClass
        {
            public static readonly ThreadStaticParameter<int> Value = new();
            public static readonly ThreadStaticParameter<int> SecondValue = new();
        }
    }
}