using H2V.AudioManager.ScriptableObjects;
using H2V.AudioManager.ScriptableObjects.Events;
using UnityEngine;

namespace H2V.AudioManager.AudioChannels
{
    public abstract class BaseAudioChannel : IAudioChannel
    {
        protected AudioManager _audioManager;

        public virtual void Enable(AudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public virtual void Disable() { }

        protected AudioEmitter GetAudioEmitter()
        {
            var pool = _audioManager.AudioEmitterPool;
            var audioEmitter = pool.GetItem();
            audioEmitter.Pool = pool;
            return audioEmitter;
        }
    }
}