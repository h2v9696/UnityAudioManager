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
        private const string ASSET_FOLDER_PATH = "Packages/h2v.audio-manager";
        private const string PLAY_BGM_EVENT = "PlayBGMAudioCueEvent";
        private const string PLAY_SFX_EVENT = "PlaySFXAudioCueEvent";
        private const string STOP_BGM_EVENT = "StopBGMEvent";
        private const string STOP_SFX_EVENT = "StopSFXEvent";
        private const string BGM_AUDIO_CONFIG = "BGMAudioConfig";
        private const string FADE_BGM_AUDIO_CONFIG = "FadeBGMAudioConfig";
        private const string SFX_AUDIO_CONFIG = "SFXAudioConfig";

        private const string TEST_BGM_GUID = "950089bf58994a043a61e2060b1cb28b";
        private const string TEST_LONG_BGM_GUID = "2e153112fce38474fad339dc94db8d7b";
        private const string TEST_SFX_GUID = "6d8086b3fec929146bc7cadd7b215bbe";
        private const string TEST_SFX2_GUID = "bc8b14fe934502843ae79651d680841d";
        private const string TEST_GROUP = "TestGroup";

        private const string AUDIO_MANAGER_PREFAB = "AudioManager";

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


        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _playBGMEventChannel = AssetFinder.FindAssetWithNameInPath<AudioCueEventChannelSO>(
                PLAY_BGM_EVENT, ASSET_FOLDER_PATH);
            _playSFXEventChannel = AssetFinder.FindAssetWithNameInPath<SFXAudioCueEventChannelSO>(
                PLAY_SFX_EVENT, ASSET_FOLDER_PATH);
            _stopBGMEventChannel = AssetFinder.FindAssetWithNameInPath<VoidEventChannelSO>(
                STOP_BGM_EVENT, ASSET_FOLDER_PATH);
            _stopSFXEventChannel = AssetFinder.FindAssetWithNameInPath<VoidEventChannelSO>(
                STOP_SFX_EVENT, ASSET_FOLDER_PATH);
            _bgmAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                BGM_AUDIO_CONFIG, ASSET_FOLDER_PATH);
            _fadeBgmAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                FADE_BGM_AUDIO_CONFIG, ASSET_FOLDER_PATH);
            _sfxAudioConfig = AssetFinder.FindAssetWithNameInPath<AudioConfigSO>(
                SFX_AUDIO_CONFIG, ASSET_FOLDER_PATH);

            var bgmAssetRef = CreateAudioAssetReference(TEST_BGM_GUID);
            var longBgmAssetRef = CreateAudioAssetReference(TEST_LONG_BGM_GUID);
            var sfxAssetRef = CreateAudioAssetReference(TEST_SFX_GUID);
            var sfx2AssetRef = CreateAudioAssetReference(TEST_SFX2_GUID);

            _bgmCue = CreateAudioCue<AudioCueSO>(_bgmAudioConfig, bgmAssetRef);
            _longBgmCue = CreateAudioCue<AudioCueSO>(_fadeBgmAudioConfig, longBgmAssetRef);
            _sfxCue = CreateAudioCue<SFXAudioCueSO>(_sfxAudioConfig, sfxAssetRef);
            _multipleSfxCue = CreateAudioCue<SFXAudioCueSO>(_sfxAudioConfig, sfxAssetRef, sfx2AssetRef);

            var audioManager = AssetFinder.FindAssetWithNameInPath<GameObject>(
                AUDIO_MANAGER_PREFAB, ASSET_FOLDER_PATH);
            _audioManager = GameObject.Instantiate(audioManager).GetComponent<AudioManager>();
        }

        [SetUp]
        public void Setup()
        {
            _audioManager.gameObject.SetActive(true);
        }

        private AudioAssetReference CreateAudioAssetReference(string guid)
        {
            var assetRef = new AudioAssetReference(guid);
            AddressableExtensions.SetObjectToAddressableGroup(guid, TEST_GROUP);
            return assetRef;
        }

        private T CreateAudioCue<T>(AudioConfigSO audioConfig, params AudioAssetReference[] assetRefs)
            where T : AudioCueSO
        {
            var audioCue = ScriptableObject.CreateInstance<T>();
            audioCue.SetPrivateArrayProperty("_audioClips", assetRefs);
            audioCue.SetPrivateProperty("AudioConfigSO", audioConfig, true);
            return audioCue;
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
            settings.RemoveGroup(settings.FindGroup(TEST_GROUP));

            Object.Destroy(_bgmCue);
            Object.Destroy(_sfxCue);
            Object.Destroy(_multipleSfxCue);
        }
    }
}