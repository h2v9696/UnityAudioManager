using Cysharp.Threading.Tasks;
using H2V.ExtensionsCore.AssetReferences;
using UnityEngine;

namespace H2V.AudioManager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SFXAudioCue", menuName = "H2V/Audio Manager/SFX Audio Cue")]
    public class SFXAudioCueSO : AudioCueSO
    {
        [SerializeReference, SubclassSelector] private IIndexSelector _sequenceSelector
            = new RandomOnceSelector();
        public IIndexSelector SequenceSelector => _sequenceSelector;

        public AudioClipSelector CreateAudioSelector() => new(this);
    }
    
    /// <summary>
    /// Create this selector from group to get audio clip from group.
    /// </summary>
    public class AudioClipSelector
    {
        public int CurrentIndex { get; private set; } = -1;
        private SFXAudioCueSO _sfxAudioCue;

        public AudioClipSelector(SFXAudioCueSO audioCue)
        {
            _sfxAudioCue = audioCue;
        }

        public AudioAssetReference GetAudioClipRef()
        {
            CurrentIndex = _sfxAudioCue.SequenceSelector
                .GetNextIndex(_sfxAudioCue.AudioClips, CurrentIndex);
            if (CurrentIndex < 0) return default;
            return _sfxAudioCue.AudioClips[CurrentIndex];
        }

        public async UniTask<AudioClip> GetAudioClipAsync()
        {
            var audioClipRef = GetAudioClipRef();
            if (audioClipRef == default) return default;
            var audioClip = await audioClipRef.TryLoadAsset();
            return audioClip;
        }
    }
}