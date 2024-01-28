using H2V.AudioManager.ScriptableObjects;
using NUnit.Framework;
using UnityEditor;

namespace H2V.AudioManager.Tests.ScriptableObjects
{
    [TestFixture, Category("Smoke Tests")]
    public class AudioCueSOTests
    {
        [Test]
        public void AudioCueSOs_CreatedCorrectly()
        {
            var guids = AssetDatabase.FindAssets("t:AudioCueSO");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<AudioCueSO>(path);
                foreach (var audioClipRef in so.AudioClips)
                {
                    Assert.IsNotEmpty(audioClipRef.AssetGUID,
                        $"{path} has no audio clip asset reference.");
                }
            }
        }
    }
}