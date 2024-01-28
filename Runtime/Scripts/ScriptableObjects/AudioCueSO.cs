
using Cysharp.Threading.Tasks;
using H2V.ExtensionsCore.AssetReferences;
using UnityEngine;

namespace H2V.AudioManager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AudioCue", menuName = "H2V/Audio Manager/Audio Cue")]
    public class AudioCueSO : ScriptableObject
    {
        [SerializeField] private AudioAssetReference[] _audioClips = default;
        public AudioAssetReference[] AudioClips => _audioClips;

        [Tooltip("Leave if null to use default audio config")]
        [field: SerializeField] public AudioConfigSO AudioConfigSO { get; private set; }
    }
}