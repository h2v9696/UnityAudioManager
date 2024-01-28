using Cysharp.Threading.Tasks;
using H2V.ExtensionsCore.Events.ScriptableObjects;
using UnityEngine;

namespace H2V.AudioManager.ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "SFXAudioCueEvent",menuName = "H2V/Audio Manager/Events/SFX Audio Cue Event Channel")]
    public class SFXAudioCueEventChannelSO : GenericReturnEventChannelSO<SFXAudioCueSO, UniTask<AudioEmitter>>
    {}
}