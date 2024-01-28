using Cysharp.Threading.Tasks;
using H2V.ExtensionsCore.Events.ScriptableObjects;
using UnityEngine;

namespace H2V.AudioManager.ScriptableObjects.Events
{
    [CreateAssetMenu(fileName = "AudioCueEvent", menuName = "H2V/Audio Manager/Events/Audio Cue Event Channel")]
    public class AudioCueEventChannelSO : GenericReturnEventChannelSO<AudioCueSO, UniTask<AudioEmitter>>
    {}
}