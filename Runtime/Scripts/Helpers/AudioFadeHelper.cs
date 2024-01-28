
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace H2V.AudioManager.Helpers
{
    public struct FadeConfig
    {
        public float Duration;
        public bool IsIgnoreTimeScale;

        public FadeConfig(float duration, bool isIgnoreTimeScale = false)
        {
            Duration = duration;
            IsIgnoreTimeScale = isIgnoreTimeScale;
        }
    }

    public static class AudioFadeHelper
    {
        /// <summary>
        /// </summary>
        /// <param name="fadeTime">Fade-in time in seconds</param>
        /// <returns></returns>
        public static async UniTask FadeIn(AudioSource source, FadeConfig fadeConfig)
        {
            var fadeTime = fadeConfig.Duration;
            // To prevent a division by zero
            if (fadeTime <= 0) return; 
            float timer = 0;
            float cachedVolume = source.volume;

            while (timer < fadeTime)
            {
                timer += fadeConfig.IsIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

                source.volume = Mathf.Lerp(0, cachedVolume, timer / fadeTime);
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="fadeTime">Fade-out time in seconds</param>
        /// <returns></returns>
        public static async UniTask FadeOut(AudioSource source, FadeConfig fadeConfig)
        {
            var fadeTime = fadeConfig.Duration;
            if (fadeTime <= 0) return; 
            
            float timer = 0;
            float cachedVolume = source.volume;

            while (timer < fadeTime)
            {
                timer += fadeConfig.IsIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

                source.volume = Mathf.Lerp(cachedVolume, 0, timer / fadeTime);
                await UniTask.Yield();
            }
            source.Stop();
        }
    }
}