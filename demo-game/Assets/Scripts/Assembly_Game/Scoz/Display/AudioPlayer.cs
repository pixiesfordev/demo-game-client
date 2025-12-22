using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;

namespace Scoz.Func {
    public partial class AudioPlayer : MonoBehaviour {
        public static AudioPlayer Instance;
        public static bool IsInit;
        static List<AudioSource> SoundList;
        static List<AudioSource> MusicList;
        static List<AudioSource> VoiceList;
        static GameObject MySoundObject;
        static GameObject MyVoiceObject;
        static GameObject MyMusicObject;

        static AudioSource CurPlayAudio;


        static AudioMixer MyAudioMixer = null;
        static string MusicGroup = "Music";
        static string SoundGroup = "Sound";
        static string VoiceGroup = "Voice";
        static int MaxAttenuation = -30;//最高降低音量為把Volume調整為-30，低於-30就會設定不開音量
        public static float MusicVolumeRatio { get; private set; }
        public static bool MuteMusic { get; private set; } = false;
        public static float SoundVolumeRatio { get; private set; }
        public static bool MuteSound { get; private set; } = false;
        public static float VoiceVolumeRatio { get; private set; }
        public static bool MuteVoice { get; private set; } = false;

        public static void Caeate() {
            GameObject gameObject = new GameObject("AudioPlayer");
            var ap = gameObject.AddComponent<AudioPlayer>();
            ap.Init();
        }

        public void Init() {
            if (IsInit)
                return;
            Instance = this;
            IsInit = true;
            MyAudioMixer = Resources.Load<AudioMixer>("Prefabs/Common/MyAudioMixer");
            DontDestroyOnLoad(gameObject);

            SoundList = new List<AudioSource>();
            MusicList = new List<AudioSource>();
            VoiceList = new List<AudioSource>();

            MySoundObject = CreatePlayerObject("SoundPlayer", SoundGroup, SoundList);
            MyMusicObject = CreatePlayerObject("MusicPlayer", MusicGroup, MusicList);
            MyVoiceObject = CreatePlayerObject("VoicePlayer", VoiceGroup, VoiceList);
        }

        static GameObject CreatePlayerObject(string _name, string _group, List<AudioSource> _list) {
            var go = new GameObject(_name);
            go.transform.SetParent(Instance.transform);

            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(_group)[0];
            _list.Add(src);

            return go;
        }

        static float Clamp01(float _value) {
            if (_value > 1) return 1;
            if (_value < 0) return 0;
            return _value;
        }

        static float GetAttenuation(float _volume) {
            float attenuation = (MaxAttenuation * (1 - _volume));
            if (attenuation <= MaxAttenuation) {
                attenuation = -80;
            } else {
                if (attenuation > 0)
                    attenuation = 0;
            }
            return attenuation;
        }

        static void SetVolumeInternal(MyAudioType audioType, float _volume) {
            _volume = Clamp01(_volume);
            float attenuation = GetAttenuation(_volume);
            _volume = MyMath.Round(_volume, 2);

            switch (audioType) {
                case MyAudioType.Music:
                    MusicVolumeRatio = _volume;
                    if (MyAudioMixer != null) MyAudioMixer.SetFloat("MusicVol", attenuation);
                    break;
                case MyAudioType.Sound:
                    SoundVolumeRatio = _volume;
                    if (MyAudioMixer != null) MyAudioMixer.SetFloat("SoundVol", attenuation);
                    break;
                case MyAudioType.Voice:
                    VoiceVolumeRatio = _volume;
                    if (MyAudioMixer != null) MyAudioMixer.SetFloat("VoiceVol", attenuation);
                    break;
                default:
                    break;
            }
        }

        static List<AudioSource> GetList(MyAudioType _type) {
            switch (_type) {
                case MyAudioType.Music:
                    return MusicList;
                case MyAudioType.Sound:
                    return SoundList;
                default:
                    return VoiceList;
            }
        }

        static string GetGroupName(MyAudioType _type) {
            switch (_type) {
                case MyAudioType.Music:
                    return MusicGroup;
                case MyAudioType.Sound:
                    return SoundGroup;
                default:
                    return VoiceGroup;
            }
        }

        public static float GetVolume(MyAudioType audioType) {
            return audioType == MyAudioType.Music ? MusicVolumeRatio :
                   audioType == MyAudioType.Sound ? SoundVolumeRatio :
                   audioType == MyAudioType.Voice ? VoiceVolumeRatio : 0f;
        }

        public static bool GetMute(MyAudioType audioType) {
            return audioType == MyAudioType.Music ? MuteMusic :
                   audioType == MyAudioType.Sound ? MuteSound :
                   audioType == MyAudioType.Voice ? MuteVoice : false;
        }

        public static void SetMute(MyAudioType audioType, bool mute) {
            switch (audioType) {
                case MyAudioType.Music:
                    SetMusicMute(mute);
                    break;
                case MyAudioType.Sound:
                    SetSoundMute(mute);
                    break;
                case MyAudioType.Voice:
                    SetVoiceMute(mute);
                    break;
                default:
                    break;
            }
        }

        public static void SetVolume(MyAudioType audioType, float volume) {
            if (MyAudioMixer == null)
                return;

            volume = Clamp01(volume);
            volume = MyMath.Round(volume, 2);

            SetVolumeInternal(audioType, volume);
        }

        /// <summary>
        /// 傳入0~1
        /// </summary>
        public static void SetMusicVolume(float _volume) {
            SetVolumeInternal(MyAudioType.Music, _volume);
        }
        /// <summary>
        /// 傳入0~1
        /// </summary>
        public static void SetSoundVolume(float _volume) {
            SetVolumeInternal(MyAudioType.Sound, _volume);
        }
        public static void SetVoiceVolume(float _volume) {
            SetVolumeInternal(MyAudioType.Voice, _volume);
        }

        static void StopAll(List<AudioSource> _list) {
            for (int i = 0; i < _list.Count; i++) {
                _list[i].Stop();
            }
        }

        static void StopByExactNames(List<AudioSource> _list, params string[] _names) {
            if (_names == null || _names.Length == 0)
                return;

            var set = new HashSet<string>(_names);
            for (int i = 0; i < _list.Count; i++) {
                var src = _list[i];
                if (!src.isPlaying || src.clip == null)
                    continue;
                if (set.Contains(src.clip.name)) {
                    src.Stop();
                }
            }
        }

        static bool IncludingNameInternal(List<AudioSource> _list, bool _ignoreNullNames, params string[] _names) {
            for (int i = 0; i < _list.Count; i++) {
                var src = _list[i];
                if (!src.isPlaying || src.clip == null)
                    continue;

                for (int j = 0; j < _names.Length; j++) {
                    if (_ignoreNullNames && _names[j] == null)
                        continue;

                    if (src.clip.name.Contains(_names[j])) {
                        return true;
                    }
                }
            }
            return false;
        }

        static void StopIncludingNameInternal(List<AudioSource> _list, bool _ignoreNullNames, params string[] _names) {
            for (int i = 0; i < _list.Count; i++) {
                var src = _list[i];
                if (!src.isPlaying || src.clip == null)
                    continue;

                for (int j = 0; j < _names.Length; j++) {
                    if (_ignoreNullNames && _names[j] == null)
                        continue;

                    if (src.clip.name.Contains(_names[j])) {
                        src.Stop();
                    }
                }
            }
        }

        public static void StopAllSounds_static() {
            StopAll(SoundList);
        }

        public static void SetMusicMute(bool _mute) {
            MuteMusic = _mute;
        }
        public static void SetSoundMute(bool _mute) {
            MuteSound = _mute;
        }
        public static void SetVoiceMute(bool _mute) {
            MuteVoice = _mute;
        }

        public static void StopSounds(params string[] _soundNames) {
            StopByExactNames(SoundList, _soundNames);
        }
        public static bool IncludingNameSound(params string[] _soundNames) {
            return IncludingNameInternal(SoundList, false, _soundNames);
        }
        public static void StopIncludingNameSounds(params string[] _soundNames) {
            StopIncludingNameInternal(SoundList, false, _soundNames);
        }
        public static bool IncludingNameVoice(params string[] _VoiceNames) {
            return IncludingNameInternal(VoiceList, false, _VoiceNames);
        }
        public static void StopIncludingNameVoices(params string[] _voiceNames) {
            StopIncludingNameInternal(VoiceList, true, _voiceNames);
        }
        public static void StopAllVoices_static() {
            StopAll(VoiceList);
        }
        public static void StopVoices(params string[] _voiceNames) {
            StopByExactNames(VoiceList, _voiceNames);
        }
        public static void StopAllMusic_static() {
            StopAll(MusicList);
        }

        static void FadeOutInternal(List<AudioSource> _list, string _name, float _duration) {
            foreach (var src in _list) {
                if (src.isPlaying && src.clip != null && src.clip.name == _name) {
                    float startVol = src.volume;
                    src
                        .DOFade(0f, _duration)
                        .OnComplete(() => {
                            src.Stop();
                            src.volume = startVol;  // 還原原始音量
                        });
                }
            }
        }

        /// <summary>
        /// 音樂淡出並停止
        /// </summary>
        public static void FadeOutMusic(string _name, float _duration) {
            if (!IsInit || _duration <= 0f) {
                StopMusics(_name);
                return;
            }
            FadeOutInternal(MusicList, _name, _duration);
        }
        /// <summary>
        /// 音效淡出並停止
        /// </summary>
        public static void FadeOutSound(string _name, float _duration) {
            if (!IsInit || _duration <= 0f) {
                StopSounds(_name);
                return;
            }
            FadeOutInternal(SoundList, _name, _duration);
        }

        public static void StopMusics(params string[] _musicNames) {
            StopByExactNames(MusicList, _musicNames);
        }
        /// <summary>
        /// 某音樂是否還在播放中
        /// </summary>
        public static bool IncludingNameMusics(params string[] _musicNames) {
            return IncludingNameInternal(MusicList, false, _musicNames);
        }

        static AudioSource GetApplicableSource(List<AudioSource> _list) {
            CurPlayAudio = null;
            for (int i = 0; i < _list.Count; i++) {
                if (!_list[i].isPlaying) {
                    CurPlayAudio = _list[i];
                    return CurPlayAudio;
                }
            }
            return CurPlayAudio;
        }

        static AudioSource GetNewSource(GameObject _obj, string _group, List<AudioSource> _list) {
            CurPlayAudio = _obj.AddComponent<AudioSource>();
            CurPlayAudio.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(_group)[0];
            _list.Add(CurPlayAudio);
            return CurPlayAudio;
        }

        static AudioSource GetApplicableSoundSource() {
            return GetApplicableSource(SoundList);
        }
        static AudioSource GetNewSoundSource() {
            return GetNewSource(MySoundObject, SoundGroup, SoundList);
        }
        static AudioSource GetApplicableVoiceSource() {
            return GetApplicableSource(VoiceList);
        }
        static AudioSource GetNewVoiceSource() {
            return GetNewSource(MyVoiceObject, VoiceGroup, VoiceList);
        }
        static AudioSource GetApplicableMusicSource() {
            return GetApplicableSource(MusicList);
        }
        static AudioSource GetNewMusicSource() {
            return GetNewSource(MyMusicObject, MusicGroup, MusicList);
        }

        static AudioSource GetOrCreateAudioSource(MyAudioType _type) {
            switch (_type) {
                case MyAudioType.Music:
                    return GetApplicableMusicSource() ?? GetNewMusicSource();
                case MyAudioType.Sound:
                    return GetApplicableSoundSource() ?? GetNewSoundSource();
                case MyAudioType.Voice:
                    return GetApplicableVoiceSource() ?? GetNewVoiceSource();
                default:
                    return null;
            }
        }

        static void PlayAudio(MyAudioType _type, AudioSource _source, AudioClip _clip) {
            _source.clip = _clip;

            if (GetMute(_type))
                return;

            var list = GetList(_type);
            if (!list.Contains(_source)) {
                list.Add(_source);
            }
            _source.Play(0);
        }


        public static void PlayAudioByPath(
            MyAudioType _type,
            string _path,
            Action<string> _cb = null,
            bool _loop = false,
            float _pitch = 1,
            bool _restartIfExists = false
        ) {
            if (!IsInit || string.IsNullOrEmpty(_path))
                return;

            AddressablesLoader.GetAudio(_type, _path, audio => {
                if (audio == null) {
                    WriteLog.LogWarning("不存在的音檔:" + _path);
                    return;
                }

                // 若要求重播同名，先搜尋現有列表
                if (_restartIfExists) {
                    var list = GetList(_type);
                    var existing = list.Find(src =>
                        src.clip != null &&
                        src.clip.name == audio.name &&
                        src.isPlaying
                    );
                    if (existing != null) {
                        CurPlayAudio = existing;
                        // 重新播放
                        existing.Stop();
                        PlayAudio(_type, existing, audio);
                        existing.loop = _loop;
                        existing.pitch = _pitch;
                        _cb?.Invoke(audio.name);
                        return;
                    }
                }

                CurPlayAudio = GetOrCreateAudioSource(_type);

                // 播放音檔
                PlayAudio(_type, CurPlayAudio, audio);
                CurPlayAudio.loop = _loop;
                CurPlayAudio.pitch = _pitch;
                _cb?.Invoke(audio.name);
            });
        }

        public static void PlayAudioByAudioAsset(MyAudioType _type, AssetReference _clip, bool _loop = false, float _pitch = 1, Action<AudioClip, AsyncOperationHandle> _cb = null) {
            if (!IsInit)
                return;
            if (_clip == null) {
                WriteLog.LogWarning("不存在的音檔");
                return;
            }
            MyAudioType type = _type;
            bool loop = _loop;
            float pitch = _pitch;
            AddressablesLoader.GetAudioClipByRef(_clip, (clip, handle) => {
                PlayAudioByAudioClip(type, clip, loop, pitch);
                _cb?.Invoke(clip, handle);
            });
        }
        public static void PlayAudioByAudioClipAsset(MyAudioType _type, AssetReference _clip, Action<AudioSource> _cb, bool _loop = false) {
            if (!IsInit)
                return;
            if (_clip == null) {
                WriteLog.LogWarning("不存在的音檔");
                return;
            }
            MyAudioType type = _type;
            bool loop = _loop;
            AddressablesLoader.GetAudioClipByRef(_clip, (clip, handle) => {
                AudioSource audioSource = PlayAudioByAudioClip(type, clip, loop);
                _cb?.Invoke(audioSource);
            });
        }
        public static AudioSource PlayAudioByAudioClip(MyAudioType _type, AudioClip _clip, bool _loop = false, float _pitch = 1) {
            if (!IsInit || _clip == null) {
                if (_clip == null) WriteLog.LogWarning("不存在的音檔");
                return null;
            }

            // 取出或新增合適的 AudioSource
            CurPlayAudio = GetOrCreateAudioSource(_type);

            var src = CurPlayAudio;
            src.playOnAwake = false;
            src.pitch = _pitch;
            src.outputAudioMixerGroup = MyAudioMixer.FindMatchingGroups(GetGroupName(_type))[0];

            if (_loop) {
                src.loop = true;
                double startTime = AudioSettings.dspTime + 0.1;
                src.clip = _clip;
                src.PlayScheduled(startTime);
            } else {
                src.loop = false;
                src.clip = _clip;
                src.Play();
            }

            return src;
        }

        public static void SetMusicPitch(float _pitchValue) {
            if (GetApplicableMusicSource() == null)
                GetNewMusicSource();
            foreach (AudioSource audioSource in MusicList) {
                audioSource.pitch = _pitchValue;
            }
        }
        public void StopAllVoice() {
            StopAllVoices_static();
        }
        public void StopMusic() {
            StopAllMusic_static();
        }
    }
}
