using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using H2V.AudioManager.ScriptableObjects;
using H2V.AudioManager.ScriptableObjects.Events;
using H2V.ExtensionsCore.AssetReferences;
using H2V.ExtensionsCore.Events.ScriptableObjects;
using UnityEngine;

namespace H2V.AudioManager.AudioChannels
{
    [Serializable]
    public class BGMAudioChannel : BaseAudioChannel
    {
        [SerializeField] private AudioCueEventChannelSO _playAudioCueEvent;
        [SerializeField] private VoidEventChannelSO _stopAudioEvent;

        private AudioEmitter _audioEmitter;
        private AudioAssetReference _playingAudioRef;

        public override void Enable(AudioManager audioManager)
        {
            base.Enable(audioManager);
            _playAudioCueEvent.EventRaised += Play;
            _stopAudioEvent.EventRaised += Stop;
        }

        public override void Disable()
        {
            base.Disable();
            _playingAudioRef?.ReleaseAsset();
            _playAudioCueEvent.EventRaised -= Play;
            _stopAudioEvent.EventRaised -= Stop;
        }

        private async UniTask<AudioEmitter> Play(AudioCueSO audioCue)
        {
            if (audioCue.AudioClips.Length == 0)
            {
                Debug.LogWarning("BGMAudioChannel: Audio cue has no audio clips.");
                return null;
            }

            await StopLastAudio(audioCue);

            _audioEmitter = GetAudioEmitter();
            _audioEmitter.AudioSource.loop = true;
            audioCue.AudioConfigSO.ApplyTo(_audioEmitter);
            await InternalPlay(audioCue);
            return _audioEmitter;
        }

        private async UniTask InternalPlay(AudioCueSO audioCue)
        {
            _playingAudioRef?.ReleaseAsset();
            _playingAudioRef = audioCue.AudioClips.First();
            var audioClip = await _playingAudioRef.TryLoadAsset();
            await _audioEmitter.Play(audioClip);
        }

        private async UniTask StopLastAudio(AudioCueSO audioCue)
        {
            if (_audioEmitter == null) return;

            if (audioCue.AudioConfigSO.IsCrossFade)
            {
                _ = _audioEmitter.Stop();
                return;
            }

            await _audioEmitter.Stop();
        } 

        private void Stop()
        {
            _ = _audioEmitter.Stop();
        }
    }
}