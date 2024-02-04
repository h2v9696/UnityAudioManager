using H2V.AudioManager.ScriptableObjects;
using H2V.ExtensionsCore.AssetReferences;
using H2V.ExtensionsCore.Editor.Helpers;
using UnityEngine;

namespace H2V.AudioManager.Tests
{
    public static class AudioTestHelper
    {
        public static string ASSET_FOLDER_PATH = "Packages/h2v.audio-manager";
        public static string PLAY_BGM_EVENT = "PlayBGMAudioCueEvent";
        public static string PLAY_SFX_EVENT = "PlaySFXAudioCueEvent";
        public static string STOP_BGM_EVENT = "StopBGMEvent";
        public static string STOP_SFX_EVENT = "StopSFXEvent";
        public static string BGM_AUDIO_CONFIG = "BGMAudioConfig";
        public static string FADE_BGM_AUDIO_CONFIG = "FadeBGMAudioConfig";
        public static string SFX_AUDIO_CONFIG = "SFXAudioConfig";

        public static string TEST_BGM_GUID = "950089bf58994a043a61e2060b1cb28b";
        public static string TEST_LONG_BGM_GUID = "2e153112fce38474fad339dc94db8d7b";
        public static string TEST_SFX_GUID = "6d8086b3fec929146bc7cadd7b215bbe";
        public static string TEST_SFX2_GUID = "bc8b14fe934502843ae79651d680841d";
        public static string TEST_GROUP = "TestGroup";

        public static string AUDIO_MANAGER_PREFAB = "AudioManager";
        
        public static AudioAssetReference CreateAudioAssetReference(string guid)
        {
            var assetRef = new AudioAssetReference(guid);
            AddressableExtensions.SetObjectToAddressableGroup(guid, TEST_GROUP);
            return assetRef;
        }

        public static T CreateAudioCue<T>(AudioConfigSO audioConfig, params AudioAssetReference[] assetRefs)
            where T : AudioCueSO
        {
            var audioCue = ScriptableObject.CreateInstance<T>();
            audioCue.SetPrivateArrayProperty("_audioClips", assetRefs);
            audioCue.SetPrivateProperty("AudioConfigSO", audioConfig, true);
            return audioCue;
        }
    }
}