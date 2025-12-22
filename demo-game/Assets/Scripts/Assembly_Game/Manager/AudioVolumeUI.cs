using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AudioAnimation {
    [SerializeField]
    private float _duration = 0.3f;
    [SerializeField]
    private float _widthMuteInner = 75;
    [SerializeField]
    private float _widthMuteOuter = 87;
    [SerializeField]
    private float _audioCenterPos = 15;
    [SerializeField]
    private Color _activeColor = Color.white;
    [SerializeField]
    private Color _inactiveColor = Color.white;
    [SerializeField]
    private AnimationCurve _posCurve;

    [SerializeField]
    private Image _imgMuteInner;
    [SerializeField]
    private Image _imgMuteOuter;
    [SerializeField]
    private Image _imgAudio;
    [SerializeField]
    private Image _imgAudioWave1;
    [SerializeField]
    private Image _imgAudioWave2;

    public bool IsMute { get; private set; }

    public void SetMute(bool isMute) {
        IsMute = isMute;
        var startColor = isMute ? _activeColor : _inactiveColor;
        var endColor = isMute ? _inactiveColor : _activeColor;
        float offset = _widthMuteOuter - _widthMuteInner;
        float startWidth = 0, endWidth = 0, startPos = 0, endPos = 0;
        if (isMute) {
            // 開始前將斜線的寬度設為0
            _imgMuteInner.gameObject.SetActive(true);
            _imgMuteOuter.gameObject.SetActive(true);
            _imgMuteInner.rectTransform.sizeDelta = new Vector2(0, _imgMuteInner.rectTransform.sizeDelta.y);
            _imgMuteOuter.rectTransform.sizeDelta = new Vector2(offset, _imgMuteOuter.rectTransform.sizeDelta.y);

            endWidth = _widthMuteInner;
            endPos = _audioCenterPos;
        } else {
            startWidth = _widthMuteInner;
            startPos = _audioCenterPos;
        }

        // 動畫
        SmoothTimer.ProgressTimer(_duration).Subscribe(progress => {
            progress = _posCurve.Evaluate(progress);
            // 顏色動畫
            _imgMuteInner.color = Color.Lerp(startColor, endColor, progress);
            _imgAudio.color = Color.Lerp(startColor, endColor, progress);

            // 斜線動畫
            Vector2 sizeDeltaInner = _imgMuteInner.rectTransform.sizeDelta;
            Vector2 sizeDeltaOuter = _imgMuteOuter.rectTransform.sizeDelta;
            sizeDeltaInner.x = Mathf.Lerp(startWidth, endWidth, progress);
            sizeDeltaOuter.x = Mathf.Lerp(startWidth, endWidth, progress) + offset;
            _imgMuteInner.rectTransform.sizeDelta = sizeDeltaInner;
            _imgMuteOuter.rectTransform.sizeDelta = sizeDeltaOuter;

            // 置中動畫
            Vector2 pos = _imgAudio.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(startPos, endPos, progress);
            _imgAudio.rectTransform.anchoredPosition = pos;
        }, () => {
            if (!isMute) {
                _imgMuteInner.gameObject.SetActive(false);
                _imgMuteOuter.gameObject.SetActive(false);
            }
        });
    }

    public void SetVolume(float volume) {
        float alpha1 = Mathf.Clamp01(volume * 2);
        float alpha2 = Mathf.Clamp01((volume - 0.5f) * 2);
        Color color1 = _imgAudioWave1.color;
        Color color2 = _imgAudioWave2.color;
        color1.a = alpha1;
        color2.a = alpha2;
        _imgAudioWave1.color = color1;
        _imgAudioWave2.color = color2;
    }
}

public class AudioVolumeUI : MonoBehaviour {
    [SerializeField]
    private AudioAnimation _audioAnimation;
    [SerializeField]
    private MyAudioType _audioType;
    [SerializeField]
    private ToggleUI _toggleMute;
    [SerializeField]
    private Slider _volumeSlider;
    [SerializeField, Range(0, 1)]
    private float _testValue;

    private float _volume;

    public void Init() {
        _toggleMute.Init();
        _toggleMute.OnValueChanged += MuteAudio;
        _volumeSlider.onValueChanged.AddListener(SetVolume);

        _volume = AudioPlayer.GetVolume(_audioType);
        _volumeSlider.SetValueWithoutNotify(_volume);
        SetVolume(_volume);
    }

    private void MuteAudio(bool isOn) {
        if (!isOn && _volume == 0) {
            _volume = 0.05f;
        }
        _volumeSlider.SetValueWithoutNotify(isOn ? 0 : _volume);
        _audioAnimation.SetMute(isOn);
        _audioAnimation.SetVolume(isOn ? 0 : _volume);
        AudioPlayer.SetVolume(_audioType, isOn ? 0 : _volume);
    }

    private void SetVolume(float value) {
        //if (_audio != null) {
        //    _audio.volume = value;
        //}
        _volume = value;

        _toggleMute.SetIsOnWithoutNotify(value == 0);
        if (_audioAnimation.IsMute != (value == 0)) {
            _audioAnimation.SetMute(value == 0);
        }
        _audioAnimation.SetVolume(value);
        AudioPlayer.SetVolume(_audioType, value);
    }
}
