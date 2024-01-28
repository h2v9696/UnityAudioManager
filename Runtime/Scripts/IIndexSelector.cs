using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace H2V.AudioManager
{
    public interface IIndexSelector
    {
        int GetNextIndex(IList list, int currentIndex);
    }

    [Serializable]
    /// <summary>
    ///  Randomly select index infinitely
    /// </summary>
    public class RandomSelector : IIndexSelector
    {
        public int GetNextIndex(IList list, int _)
        {
            if (list.Count == 0) return -1;
            return Random.Range(0, list.Count);
        }
    }

    [Serializable]
    /// <summary>
    ///  Select next index and stop when reach end
    /// </summary>
    public class SequentialSelector : IIndexSelector
    {
        public int GetNextIndex(IList list, int currentIndex)
        {
            if (list.Count == 0) return -1;
            if (currentIndex >= list.Count - 1) return -1;
            return (currentIndex + 1) % list.Count;
        }
    }

    [Serializable]
    /// <summary>
    ///  Select next index and repeat when reach end
    /// </summary>
    public class SequentialRepeatSelector : IIndexSelector
    {
        public int GetNextIndex(IList list, int currentIndex)
        {
            if (list.Count == 0) return -1;
            return (currentIndex + 1) % list.Count;
        }
    }

    [Serializable]
    /// <summary>
    ///  Randomly select index once
    /// </summary>
    public class RandomOnceSelector : IIndexSelector
    {
        public int GetNextIndex(IList list, int currentIndex)
        {
            if (list.Count == 0) return -1;
            if (currentIndex != -1) return -1;
            return Random.Range(0, list.Count);
        }
    }

    [Serializable]
    /// <summary>
    ///  Randomly select index without same as current index
    /// </summary>
    public class RandomNoRepeatSelector : IIndexSelector
    {
        public int GetNextIndex(IList list, int currentIndex)
        {
            if (list.Count == 0 || list.Count == 1) return -1;
            var indexes = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i != currentIndex) indexes.Add(i);
            }
            return indexes[Random.Range(0, indexes.Count)];
        }
    }
}