using NUnit.Framework;
using System.Collections.Generic;

namespace H2V.AudioManager.Tests.IndexSelectors
{
    [TestFixture, Category("Unit Tests")]
    public class SequentialRepeatSelectorTests
    {
        private IIndexSelector _selector = new SequentialRepeatSelector();
        private List<int> _emptyList = new();
        private List<int> _oneElementList = new() { 0 };
        private List<int> _multipleElementList = new() { 0, 1, 2 };

        [Test]
        public void GetNextIndex_ListIsEmpty_ReturnsMinusOne()
        {
            var result = _selector.GetNextIndex(_emptyList, 0);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void GetNextIndex_ListHasOneElement_ReturnsZero()
        {
            var result = _selector.GetNextIndex(_oneElementList, 0);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetNextIndex_ListHasMultipleElements_ReturnsNextIndex()
        {
            var result = _selector.GetNextIndex(_multipleElementList, 0);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetNextIndex_CurrentIndexIsLastElement_ReturnsZero()
        {
            var result = _selector.GetNextIndex(_multipleElementList, 2);
            Assert.AreEqual(0, result);
        }
    }
}