using H2V.AudioManager.ScriptableObjects;
using UnityEngine;

namespace H2V.AudioManager.AudioChannels
{
    public interface IAudioChannel
    {
        void Enable(AudioManager audioManager);
        void Disable();
    }
}