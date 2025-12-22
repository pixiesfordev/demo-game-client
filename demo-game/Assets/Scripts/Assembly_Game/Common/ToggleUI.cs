using Scoz.Func;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class ToggleColorGroup {
    public Graphic graphic;
    public Color onColor = Color.white;
    public Color offColor = Color.white;
    public Color hoverColor = Color.white;
    public Color pressColor = Color.white;
    public Color disableColor = Color.white;
}

public class ToggleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    [SerializeField]
    private Toggle _toggle;
    [SerializeField]
    private AssetReference _audioClip;

    [SerializeField]
    private float _pressScale = 0.95f;
    [SerializeField]
    private float _smoothTime = 0.1f;
    [SerializeField]
    private ToggleColorGroup[] _groups;

    [SerializeField]
    private Image _imgSwitch;
    [SerializeField]
    private Sprite _spriteOn, _spriteOff, _spriteHover, _spritePress;

    private bool _isHover, _isPress, _isMobile;

    public bool IsOn {
        get => _toggle.isOn;
        set => _toggle.isOn = value;
    }

    public bool Interactable {
        get => _toggle.interactable;
        set {
            _toggle.interactable = value;
            for (int i = 0; i < _groups.Length; i++) {
                var graphic = _groups[i].graphic;
                Color color = value ? _toggle.isOn ? _groups[i].onColor : _groups[i].offColor : _groups[i].disableColor;
                graphic.color = color;
            }
        }
    }

    public void SetIsOnWithoutNotify(bool value) {
        _toggle.SetIsOnWithoutNotify(value);
        SetToggleIsOn(value);
    }

    public event Action<bool> OnValueChanged {
        add => _toggle.onValueChanged.AddListener(new UnityAction<bool>(value));
        remove => _toggle.onValueChanged.RemoveListener(new UnityAction<bool>(value));
    }

    //private void Start() {
    //    Init();
    //}

    public void Init() {
        _toggle.onValueChanged.AddListener(isOn => {
            //if (_isHover || _isPress) return;
            SetToggleIsOn(isOn);
            if (!string.IsNullOrEmpty(_audioClip.AssetGUID))
                AudioPlayer.PlayAudioByAudioAsset(MyAudioType.Sound, _audioClip);
        });

        SetToggleIsOn(_toggle.isOn);

        _isMobile = GameManager.IsMobileDevice;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!_toggle.interactable) return;
        if (_isMobile) return;

        _isHover = true;
        for (int i = 0; i < _groups.Length; i++) {
            var graphic = _groups[i].graphic;
            graphic.color = _groups[i].hoverColor;
        }
        //var startColors = _groups.Select(g => g.graphic.color).ToList();
        //SmoothTimer.ProgressTimer(_smoothTime).Subscribe(progress => {
        //    for (int i = 0; i < _groups.Length; i++) {
        //        _groups[i].graphic.color = Color.Lerp(startColors[i], _groups[i].hoverColor, progress);
        //    }
        //});
        SetSwitchSprite(_spriteHover);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!_toggle.interactable) return;
        if (_isMobile) return;

        _isHover = false;
        for (int i = 0; i < _groups.Length; i++) {
            var graphic = _groups[i].graphic;
            graphic.color = _toggle.isOn ? _groups[i].onColor : _groups[i].offColor;
        }
        //var startColors = _groups.Select(g => g.graphic.color).ToList();
        //SmoothTimer.ProgressTimer(_smoothTime).Subscribe(progress => {
        //    for (int i = 0; i < _groups.Length; i++) {
        //        _groups[i].graphic.color = Color.Lerp(startColors[i],
        //            _toggle.isOn ? _groups[i].onColor : _groups[i].offColor, progress);
        //    }
        //});
        SetSwitchSprite(_toggle.isOn ? _spriteOn : _spriteOff);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!_toggle.interactable) return;

        _isPress = true;
        for (int i = 0; i < _groups.Length; i++) {
            var graphic = _groups[i].graphic;
            graphic.color = _groups[i].pressColor;
        }
        transform.localScale = _pressScale * Vector3.one;
        //var startColors = _groups.Select(g => g.graphic.color).ToList();
        //SmoothTimer.ProgressTimer(_smoothTime).Subscribe(progress => {
        //    for (int i = 0; i < _groups.Length; i++) {
        //        _groups[i].graphic.color = Color.Lerp(startColors[i], _groups[i].pressColor, progress);
        //    }
        //    transform.localScale = Mathf.Lerp(1, _pressScale, progress) * Vector3.one;
        //});
        SetSwitchSprite(_spritePress);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!_toggle.interactable) return;

        _isPress = false;
        for (int i = 0; i < _groups.Length; i++) {
            var graphic = _groups[i].graphic;
            graphic.color = _toggle.isOn ? _groups[i].onColor : _groups[i].offColor;
        }
        transform.localScale = Vector3.one;
        //var startColors = _groups.Select(g => g.graphic.color).ToList();
        //SmoothTimer.ProgressTimer(_smoothTime).Subscribe(progress => {
        //    for (int i = 0; i < _groups.Length; i++) {
        //        _groups[i].graphic.color = Color.Lerp(startColors[i],
        //            _toggle.isOn ? _groups[i].onColor : _groups[i].offColor, progress);
        //    }
        //    transform.localScale = Mathf.Lerp(_pressScale, 1, progress) * Vector3.one;
        //});
        SetSwitchSprite(_toggle.isOn ? _spriteOn : _spriteOff);
    }

    private void SetToggleIsOn(bool isOn) {
        for (int i = 0; i < _groups.Length; i++) {
            var graphic = _groups[i].graphic;
            graphic.color = isOn ? _groups[i].onColor : _groups[i].offColor;
        }
        SetSwitchSprite(isOn ? _spriteOn : _spriteOff);
    }

    private void SetSwitchSprite(Sprite sprite) {
        if (_imgSwitch != null && sprite != null) {
            _imgSwitch.sprite = sprite;
        }
    }
}
