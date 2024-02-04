using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using H2V.ExtensionsCore.EditorTools;


#if UNITY_EDITOR
using UnityEditor;
# endif

namespace H2V.AudioManager.ScriptableObjects
{
    /// <summary>
    /// Setting audio mixer group's volume, mute 
    /// </summary>
    public class AudioSettingSO : ScriptableObject
    {
        [SerializeField] private AudioMixer _audioMixer;
        public AudioMixer AudioMixer => _audioMixer;

        [SerializeField] private AudioMixerGroupSetting[] _audioMixerGroupSettings;
        public AudioMixerGroupSetting[] AudioMixerGroupSettings => _audioMixerGroupSettings;

        [SerializeField] private float _defaultVolume = 0.5f;

        public Dictionary<AudioMixerGroup, AudioMixerGroupSetting> GroupMapping
            { get; private set; } = new();

        private void OnEnable()
        {
            GroupMapping.Clear();
            foreach (var setting in _audioMixerGroupSettings)
            {
                GroupMapping.Add(setting.MixerGroup, setting);
            }

            SetMixerGroupVolume();
        }

        public void SetVolume(AudioMixerGroup group, float volume)
        {
            if (!GroupMapping.TryGetValue(group, out var setting))
                return;
            setting.Volume = volume;
        }

        public void SetMute(AudioMixerGroup group, bool isMute)
        {
            if (!GroupMapping.TryGetValue(group, out var setting))
                return;
            setting.IsMuted = isMute;
        }

        private void SetMixerGroupVolume()
        {
            foreach (var setting in _audioMixerGroupSettings)
            {
                setting.Volume = _defaultVolume;
            }
        }

# if UNITY_EDITOR
        private void OnValidate()
        {
            if (_audioMixerGroupSettings.Length > 0)
            {
                SetMixerGroupVolume();
                return;
            }

            FindAndAddAllGroups();
            OnEnable();
        }

        private void FindAndAddAllGroups()
        {
            var groups = _audioMixer.FindMatchingGroups(string.Empty);
            var exposedParams = GetExposedParameters();

            _audioMixerGroupSettings = new AudioMixerGroupSetting[0];
            foreach (var group in groups)
            {
                var exposedVolumeName = $"{group.name}Volume";
                if (!exposedParams.Contains(exposedVolumeName))
                {
                    Debug.LogError($"Exposed Param {exposedVolumeName} not found");
                    Debug.LogWarning($"Please expose volume parameter of {group.name} and rename it to {exposedVolumeName}");
                    return;
                }
                var setting = new AudioMixerGroupSetting(group, exposedVolumeName);
                ArrayUtility.Add(ref _audioMixerGroupSettings, setting);
            }
        }

        private List<string> GetExposedParameters()
        {
            var parameters = (Array) _audioMixer.GetType().GetProperty("exposedParameters")
                .GetValue(_audioMixer, null);
    
            List<string> exposedParams = new();
            for(int i = 0; i < parameters.Length; i++)
            {
                var obj = parameters.GetValue(i);
                var Param = (string) obj.GetType().GetField("name").GetValue(obj);
                exposedParams.Add(Param);
            }
            return exposedParams;
        }
# endif
    }

    /// <summary>
    /// Linear lerping volume and mute 
    /// </summary>
    [Serializable]
    public class AudioMixerGroupSetting
    {
        private static float VolumeStep = 20f;
        private static float MinVolume = 0.0001f;

        [field: SerializeField, ReadOnly] public AudioMixerGroup MixerGroup { get; private set; }
        [field: SerializeField, ReadOnly] public string ExposedVolumeName { get; private set; }

        [Range(0f, 1f)]
        [SerializeField] private float _volume = 0.5f;
        [SerializeField] private bool _isMuted = false;

        public AudioMixer AudioMixer => MixerGroup.audioMixer;
        private float _beforeMutedVolume;

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                AudioMixer.SetFloat(ExposedVolumeName, 
                    Mathf.Log10(_volume <= 0 ? MinVolume : _volume) * VolumeStep);
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                if (_isMuted) 
                    _beforeMutedVolume = Volume;
                Volume = _isMuted ? 0 : _beforeMutedVolume;
            }
        }

        public AudioMixerGroupSetting(AudioMixerGroup mixerGroup, string exposedVolumeName)
        {
            MixerGroup = mixerGroup;
            ExposedVolumeName = exposedVolumeName;
        }
    }
}