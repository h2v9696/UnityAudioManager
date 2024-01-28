using NUnit.Framework;
using System.Collections.Generic;

namespace H2V.AudioManager.Tests.IndexSelectors
{
    [TestFixture, Category("Unit Tests")]
    public class RandomNoRepeatSelectorTests
    {
        private IIndexSelector _selector = new RandomNoRepeatSelector();
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
        public void GetNextIndex_ListHasOneElement_ReturnsMinusOne()
        {
            var result = _selector.GetNextIndex(_oneElementList, 0);
            Assert.AreEqual(-1, result);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void GetNextIndex_ListHasMultipleElements_ReturnsRandomIndex()
        {
            var result = _selector.GetNextIndex(_multipleElementList, 0);
            Assert.IsTrue(result >= 0 && result < _multipleElementList.Count);
        }

        [Test]
        public void GetNextIndex_CurrentIndexIsMinusOne_ReturnsRandomIndex()
        {
            var result = _selector.GetNextIndex(_multipleElementList, -1);
            Assert.IsTrue(result >= 0 && result < _multipleElementList.Count);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void GetNextIndex_NextIndexIs_DifferentFromCurrentIndex(int currentIndex)
        {
            var result = _selector.GetNextIndex(_multipleElementList, currentIndex);
            Assert.IsTrue(_multipleElementList[currentIndex] != _multipleElementList[result]);
        }
    }
}