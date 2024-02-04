using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using H2V.AudioManager.ScriptableObjects;
using H2V.AudioManager.ScriptableObjects.Events;
using H2V.ExtensionsCore.AssetReferences;
using H2V.ExtensionsCore.Events.ScriptableObjects;
using UnityEngine;

namespace H2V.AudioManager.AudioChannels
{
    [Serializable]
    public class SFXAudioChannel : BaseAudioChannel
    {
        [SerializeField] private SFXAudioCueEventChannelSO _playSFXAudioCueEvent;
        [SerializeField] private VoidEventChannelSO _stopAudioEvent;

        private List<AudioAssetReference> _audioAssetReferences = new();
        private List<AudioEmitter> _audioEmitters = new();
        private bool _isStopped = false;

        public override void Enable(AudioManager audioManager)
        {
            base.Enable(audioManager);

            _playSFXAudioCueEvent.EventRaised += Play;
            _stopAudioEvent.EventRaised += Stop;
        }

        public override void Disable()
        {
            base.Disable();
            _playSFXAudioCueEvent.EventRaised -= Play;
            _stopAudioEvent.EventRaised -= Stop;
            ReleaseAudioAssetReferences();
        }

        private async UniTask<AudioEmitter> Play(SFXAudioCueSO audioCue)
        {
            if (audioCue.AudioClips.Length == 0)
            {
                Debug.LogWarning("SFXAudioChannel: Audio cue has no audio clips.");
                return null;
            }

            return await InternalPlay(audioCue);
        }

        private async UniTask<AudioEmitter> InternalPlay(SFXAudioCueSO audioCue)
        {
            var audioClipSelector = audioCue.CreateAudioSelector();
            AudioClip audioClip;
            var audioEmitter = GetAudioEmitter();
            _audioEmitters.Add(audioEmitter);

            audioCue.AudioConfigSO.ApplyTo(audioEmitter);
            do 
            {
                if (_isStopped) break;

                audioClip = await GetAudioClip(audioClipSelector);

                await audioEmitter.Play(audioClip);
                if (audioClip == null) continue;

                await UniTask.WaitForSeconds(audioClip.length);
            } 
            while (audioClip != null);

            _audioEmitters.Remove(audioEmitter);
            ReleaseAudioAssetReferences();
            _isStopped = false;
            return audioEmitter;
        }

        private async UniTask<AudioClip> GetAudioClip(AudioClipSelector audioClipSelector)
        {
            var audioRef = audioClipSelector.GetAudioClipRef();
            if (audioRef == null) return null;
            _audioAssetReferences.Add(audioRef);
            return await audioRef.TryLoadAsset();
        }

        private void Stop()
        {
            _isStopped = true;
            foreach (var audioEmitter in _audioEmitters)
            {
                _ = audioEmitter.Stop();
            }
            _audioEmitters.Clear();
        }

        private void ReleaseAudioAssetReferences()
        {
            foreach (var audioAssetReference in _audioAssetReferences)
            {
                audioAssetReference.ReleaseAsset();
            }
            _audioAssetReferences.Clear();
        }
    }
}