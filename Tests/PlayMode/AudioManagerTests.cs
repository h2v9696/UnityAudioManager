using System.Collections;
using Cysharp.Threading.Tasks;
using H2V.AudioManager.ScriptableObjects;
using H2V.AudioManager.ScriptableObjects.Events;
using H2V.ExtensionsCore.AssetReferences;
using H2V.ExtensionsCore.Editor.Helpers;
using H2V.ExtensionsCore.Events.ScriptableObjects;
using NUnit.Framework;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.TestTools;

namespace H2V.AudioManager.Tests
{
    [TestFixture, Category("Integration Tests")]
    public class AudioManagerTests
    {
        private AudioCueEventChannelSO _playBGMEventChannel;
        private SFXAudioCueEventChannelSO _playSFXEventChannel;
        private VoidEventChannelSO _stopBGMEventChannel;
        private VoidEventChannelSO _stopSFXEventChannel;
        private AudioCueSO _bgmCue;
        private AudioCueSO _longBgmCue;
        private SFXAudioCueSO _sfxCue;
        private SFXAudioCueSO _multipleSfxCue;
        private AudioConfigSO _bgmAudioConfig;
        private AudioConfigSO _fadeBgmAudioConfig;
        private AudioConfigSO _sfxAudioConfig;
        private AudioManager _audioManager;

        private string _assetFolderPath = AudioTestHelper.ASSET_FOLDER_PATH;

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
            _fadeBgmAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                AudioTestHelper.FADE_BGM_AUDIO_CONFIG, _assetFolderPath);
            _sfxAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                AudioTestHelper.SFX_AUDIO_CONFIG, _assetFolderPath);

            var bgmAssetRef = AudioTestHelper.CreateAudioAssetReference(AudioTestHelper.TEST_BGM_GUID);
            var longBgmAssetRef = AudioTestHelper.CreateAudioAssetReference(AudioTestHelper.TEST_LONG_BGM_GUID);
            var sfxAssetRef = AudioTestHelper.CreateAudioAssetReference(AudioTestHelper.TEST_SFX_GUID);
            var sfx2AssetRef = AudioTestHelper.CreateAudioAssetReference(AudioTestHelper.TEST_SFX2_GUID);

            _bgmCue = AudioTestHelper.CreateAudioCue<AudioCueSO>(_bgmAudioConfig, bgmAssetRef);
            _longBgmCue = AudioTestHelper.CreateAudioCue<AudioCueSO>(_fadeBgmAudioConfig, longBgmAssetRef);
            _sfxCue = AudioTestHelper.CreateAudioCue<SFXAudioCueSO>(_sfxAudioConfig, sfxAssetRef);
            _multipleSfxCue = AudioTestHelper.CreateAudioCue<SFXAudioCueSO>(_sfxAudioConfig, sfxAssetRef, sfx2AssetRef);

            var audioManager = AssetFinder.FindAssetWithNameInPath<GameObject>(
                AudioTestHelper.AUDIO_MANAGER_PREFAB, _assetFolderPath);
            _audioManager = GameObject.Instantiate(audioManager).GetComponent<AudioManager>();
        }

        [SetUp]
        public void Setup()
        {
            _audioManager.gameObject.SetActive(true);
        }

        [UnityTest]
        public IEnumerator PlayBGM_HearBGMPlayed_AndLooping()
        {
            var uniTask = _playBGMEventChannel.RaiseEvent(_bgmCue);
            AudioSource audioSource = null;
            yield return uniTask.ToCoroutine(audioEmitter => 
            {
                audioSource = audioEmitter.AudioSource;
                Assert.IsTrue(audioSource.loop);
                Assert.IsTrue(audioSource.isPlaying);
            });
;
            yield return new WaitForSeconds(audioSource.clip.length * 2f);

            // After length still playing
            Assert.IsTrue(audioSource.isPlaying);
            _stopBGMEventChannel.RaiseEvent();
        }

        [UnityTest]
        public IEnumerator StopBGM_BGMStopped()
        {
            var uniTask = _playBGMEventChannel.RaiseEvent(_bgmCue);
            AudioEmitter audioEmitter = null;
            yield return uniTask.ToCoroutine(emitter => 
            {
                audioEmitter = emitter;
            });
            yield return new WaitForSeconds(0.5f);

            _stopBGMEventChannel.RaiseEvent();

            bool isTestStopped = false;
            audioEmitter.AudioStopped += () => 
            {
                isTestStopped = true;
                Assert.IsFalse(audioEmitter.AudioSource.isPlaying);
            };
            yield return new WaitUntil(() => isTestStopped);
        }

        [UnityTest]
        public IEnumerator PlayBGM_HearBGM_FadeInAndOut()
        {
            var uniTask = _playBGMEventChannel.RaiseEvent(_longBgmCue);
            AudioEmitter audioEmitter = null;
            yield return uniTask.ToCoroutine(emitter => 
            {
                audioEmitter = emitter;
            });
            yield return new WaitForSeconds(2f);

            _stopBGMEventChannel.RaiseEvent();

            bool isTestStopped = false;
            audioEmitter.AudioStopped += () => 
            {
                isTestStopped = true;
            };
            yield return new WaitUntil(() => isTestStopped);
            yield return new WaitForSeconds(0.5f);
        }

        [UnityTest]
        public IEnumerator PlayBGM_HearBGM_PlayAnother_CrossFadeToNewBGM()
        {
            _playBGMEventChannel.RaiseEvent(_longBgmCue);
            yield return new WaitForSeconds(2f);

            var uniTask = _playBGMEventChannel.RaiseEvent(_bgmCue);
            AudioEmitter audioEmitter = null;
            yield return uniTask.ToCoroutine(emitter => 
            {
                audioEmitter = emitter;
            });

            yield return new WaitForSeconds(2f);
            _stopBGMEventChannel.RaiseEvent();

            bool isTestStopped = false;
            audioEmitter.AudioStopped += () => 
            {   
                isTestStopped = true;
            };
            yield return new WaitUntil(() => isTestStopped);
            yield return new WaitForSeconds(.5f);
        }

        [UnityTest]
        public IEnumerator PlaySFX_HearSFXPlayed()
        {
            var uniTask = _playSFXEventChannel.RaiseEvent(_sfxCue);
            yield return uniTask.ToCoroutine();
        }

        [UnityTest]
        public IEnumerator PlaySFX_HearSFXsPlayed_InSequence()
        {
            _multipleSfxCue.SetPrivateProperty("_sequenceSelector", new SequentialSelector());
            var uniTask = _playSFXEventChannel.RaiseEvent(_multipleSfxCue);
            yield return uniTask.ToCoroutine();
        }

        [UnityTest]
        public IEnumerator PlaySFX_HearSFXsPlayed_InSequenceRepeating()
        {
            _multipleSfxCue.SetPrivateProperty("_sequenceSelector", new SequentialRepeatSelector());
            var uniTask = _playSFXEventChannel.RaiseEvent(_multipleSfxCue);
            UniTask.Create(async () => 
            {
                await UniTask.Delay(3000);
                _stopSFXEventChannel.RaiseEvent();
            });

            yield return uniTask.ToCoroutine();
            Assert.Pass();
        }

        [TearDown]
        public void TearDown()
        {
            _audioManager.gameObject.SetActive(false);
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.RemoveGroup(settings.FindGroup(AudioTestHelper.TEST_GROUP));

            Object.Destroy(_bgmCue);
            Object.Destroy(_sfxCue);
            Object.Destroy(_multipleSfxCue);
        }
    }
}