using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using H2V.AudioManager.ScriptableObjects;
using H2V.AudioManager.ScriptableObjects.Events;
using H2V.ExtensionsCore.AssetReferences;
using H2V.ExtensionsCore.Editor.Helpers;
using H2V.ExtensionsCore.Events.ScriptableObjects;
using NUnit.Framework;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.TestTools;

namespace H2V.AudioManager.Tests
{
    [TestFixture, Category("Integration Tests")]
    public class AudioSettingSOTests
    {
        private const string AUDIO_SETTING_SO = "AudioSetting";
        private const string MASTER_GROUP_NAME = "Master";
        private const string BGM_GROUP_NAME = "BGM";
        private const string SFX_GROUP_NAME = "SFX";
        private const string AUDIO_MIXER_NAME = "AudioMixer";

        private AudioCueEventChannelSO _playBGMEventChannel;
        private SFXAudioCueEventChannelSO _playSFXEventChannel;
        private VoidEventChannelSO _stopBGMEventChannel;
        private VoidEventChannelSO _stopSFXEventChannel;

        private AudioCueSO _bgmCue;
        private SFXAudioCueSO _sfxCue;
        private AudioConfigSO _bgmAudioConfig;
        private AudioConfigSO _sfxAudioConfig;
        private AudioSettingSO _audioSetting;
        private AudioMixerGroup _masterGroup;
        private AudioMixerGroup _bgmGroup;
        private AudioMixerGroup _sfxGroup;

        private string _assetFolderPath = AudioTestHelper.ASSET_FOLDER_PATH;
        private static float[] _volumes = new float[] { 0, 0.5f, 1f };
        private static bool[] _isMuted = new bool[] { true, false };


        private AudioEmitter _audioEmitter;
        private bool _isTestStopped;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _playBGMEventChannel = AssetFinder.FindAssetWithNameInPath<AudioCueEventChannelSO>(
                AudioTestHelper.PLAY_BGM_EVENT, _assetFolderPath);
            _playSFXEventChannel = AssetFinder.FindAssetWithNameInPath<SFXAudioCueEventChannelSO>(
                AudioTestHelper.PLAY_SFX_EVENT, _assetFolderPath);
            _stopBGMEventChannel = AssetFinder.FindAssetWithNameInPath<VoidEventChannelSO>(
                AudioTestHelper.STOP_BGM_EVENT, _assetFolderPath);
            _stopSFXEventChannel = AssetFinder.FindAssetWithNameInPath<VoidEventChannelSO>(
                AudioTestHelper.STOP_SFX_EVENT, _assetFolderPath);
            _bgmAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                AudioTestHelper.BGM_AUDIO_CONFIG, _assetFolderPath);
            _sfxAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                AudioTestHelper.SFX_AUDIO_CONFIG, _assetFolderPath);
            _audioSetting = AssetFinder.FindAssetWithNameInPath<AudioSettingSO>(
                AUDIO_SETTING_SO, _assetFolderPath);

            var audioMixer = AssetFinder.FindAssetWithNameInPath<AudioMixer>(
                AUDIO_MIXER_NAME, _assetFolderPath);
            _masterGroup = audioMixer.FindMatchingGroups(MASTER_GROUP_NAME).FirstOrDefault();
            _bgmGroup = audioMixer.FindMatchingGroups(BGM_GROUP_NAME).FirstOrDefault();
            _sfxGroup = audioMixer.FindMatchingGroups(SFX_GROUP_NAME).FirstOrDefault();

            var bgmAssetRef = AudioTestHelper.CreateAudioAssetReference(AudioTestHelper.TEST_BGM_GUID);
            var sfxAssetRef = AudioTestHelper.CreateAudioAssetReference(AudioTestHelper.TEST_SFX_GUID);

            _bgmCue = AudioTestHelper.CreateAudioCue<AudioCueSO>(_bgmAudioConfig, bgmAssetRef);
            _sfxCue = AudioTestHelper.CreateAudioCue<SFXAudioCueSO>(_sfxAudioConfig, sfxAssetRef);

            var audioManager = AssetFinder.FindAssetWithNameInPath<GameObject>(
                AudioTestHelper.AUDIO_MANAGER_PREFAB, _assetFolderPath);
            GameObject.Instantiate(audioManager).GetComponent<AudioManager>();
        }

        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            var uniTask = _playBGMEventChannel.RaiseEvent(_bgmCue);
            _sfxCue.SetPrivateProperty("_sequenceSelector", new SequentialRepeatSelector());
            _playSFXEventChannel.RaiseEvent(_sfxCue);
            yield return uniTask.ToCoroutine(emitter => 
            {
                _audioEmitter = emitter;
            });
        }

        [UnityTest]
        public IEnumerator SetVolume_MasterGroup_AllGroupsVolumeChanged(
            [ValueSource(nameof(_volumes))] float volume)
        {
            _audioSetting.SetVolume(_masterGroup, volume);
            Assert.AreEqual(volume, _audioSetting.GroupMapping[_masterGroup].Volume);

            yield return new WaitForSeconds(2f);
        }

        [UnityTest]
        public IEnumerator SetVolume_BGMGroup_BGMVolumeChanged(
            [ValueSource(nameof(_volumes))] float volume)
        {
            _stopSFXEventChannel.RaiseEvent();
            _audioSetting.SetVolume(_bgmGroup, volume);
            Assert.AreEqual(volume, _audioSetting.GroupMapping[_bgmGroup].Volume);

            yield return new WaitForSeconds(2f);
        }

        [UnityTest]
        public IEnumerator SetVolume_SFXGroup_SFXVolumeChanged(
            [ValueSource(nameof(_volumes))] float volume)
        {
            _stopBGMEventChannel.RaiseEvent();
            _audioSetting.SetVolume(_sfxGroup, volume);
            Assert.AreEqual(volume, _audioSetting.GroupMapping[_sfxGroup].Volume);

            yield return new WaitForSeconds(2f);
        }

        [UnityTest]
        public IEnumerator Mute_AllSoundMuted(
            [ValueSource(nameof(_isMuted))] bool isMuted)
        {
            _audioSetting.SetMute(_masterGroup, isMuted);
            Assert.AreEqual(isMuted, _audioSetting.GroupMapping[_masterGroup].IsMuted);

            yield return new WaitForSeconds(1f);
        }

        [UnityTest]
        public IEnumerator UnMute_AllSoundPlaying_VolumeEqualToBeforeMuted()
        {
            float beforeVolume = 0.75f;
            _audioSetting.SetVolume(_masterGroup, beforeVolume);
            _audioSetting.SetMute(_masterGroup, true);

            yield return new WaitForSeconds(1f);
            _audioSetting.SetMute(_masterGroup, false);
            Assert.AreEqual(beforeVolume, _audioSetting.GroupMapping[_masterGroup].Volume);
            yield return new WaitForSeconds(1f);
        }


        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            _stopBGMEventChannel.RaiseEvent();
            _stopSFXEventChannel.RaiseEvent();

            _isTestStopped = false;
            _audioEmitter.AudioStopped += () => 
            {
                _isTestStopped = true;
            };
            yield return new WaitUntil(() => _isTestStopped);
            yield return new WaitForSeconds(0.5f);
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.RemoveGroup(settings.FindGroup(AudioTestHelper.TEST_GROUP));

            Object.Destroy(_bgmCue);
            Object.Destroy(_sfxCue);
        }
    }
}