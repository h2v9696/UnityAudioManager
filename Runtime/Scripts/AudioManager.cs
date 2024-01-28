using H2V.AudioManager.AudioChannels;
using UnityEngine;

namespace H2V.AudioManager
{
    /// <summary>
    /// Manager for all audio channels and provider anythings that needs to play audio
    /// </summary>
    [RequireComponent(typeof(AudioEmitterPool))]
    public class AudioManager : MonoBehaviour
    {
        [field: SerializeField] 
        public AudioEmitterPool AudioEmitterPool { get; private set; }

        [SerializeReference, SubclassSelector]
        private IAudioChannel[] _audioChannels;

        private void OnValidate()
        {
            if (AudioEmitterPool == null)
                AudioEmitterPool = GetComponent<AudioEmitterPool>();
        }

        private void OnEnable()
        {
            foreach (var audioChannel in _audioChannels)
            {
                audioChannel.Enable(this);
            }
        }

        private void OnDisable()
        {
            foreach (var audioChannel in _audioChannels)
            {
                audioChannel.Disable();
            }

            AudioEmitterPool.ReleaseAll();
        }
    }
}