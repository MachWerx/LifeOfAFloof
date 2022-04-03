using System;
using UnityEngine;

public class Button : MonoBehaviour {
    [SerializeField] private Bar _linkedBar;
    public float timeout = 0;
    public float timeoutPeriod = 1.0f;
    public bool autoPress = false;
    public Action OnButtonPressed;

    private Material _material;
    private Transform _collisionTransform;
    private Vector3 _initialPos;
    private Vector3 _pressedPos;
    private Color _initialColor;
    private bool _buttonPressed;
    private bool _buttonPressQueued;

    private void Awake() {
        _material = GetComponentInChildren<Renderer>().material;
        _collisionTransform = GetComponentInChildren<Collider>().transform;
        _initialPos = transform.localPosition;
        _pressedPos = _initialPos + 0.1f * Vector3.forward;
        if (_material.HasProperty("_EmissionColor")) {
           _initialColor = _material.GetColor("_EmissionColor");
        }
    }

    private void OnEnable()
    {
        _buttonPressQueued = false;

        //if (_linkedBar == null) {
            timeout = 1;
            _buttonPressed = false;
            transform.localPosition = _initialPos;
            if (_material.HasProperty("_EmissionColor")) {
                _material.SetColor("_EmissionColor", _initialColor);
            }
        //} else {
        //    timeout = 0;
        //    _buttonPressed = true;
        //    transform.localPosition = _pressedPos;
        //    if (_material.HasProperty("_EmissionColor")) {
        //        _material.SetColor("_EmissionColor", Color.black);
        //    }
        //}

    }

    // Update is called once per frame
    void Update() {
        if (_buttonPressed) {
            if (_linkedBar == null) {
                timeout += Time.deltaTime / timeoutPeriod;
            } else {
                timeout = _linkedBar.value;
            }

            transform.localPosition = Vector3.Lerp(_pressedPos, _initialPos, timeout);
            if (_material.HasProperty("_EmissionColor")) {
                _material.SetColor("_EmissionColor", 0.5f * timeout * _initialColor);
            }
            if (timeout >= 1.0f) {
                // button released
                timeout = 1.0f;
                _buttonPressed = false;
                transform.localPosition = _initialPos;
                if (_material.HasProperty("_EmissionColor")) {
                    _material.SetColor("_EmissionColor", _initialColor);
                }
            }
        }

        bool buttonBeingPressedNow = autoPress;
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform == _collisionTransform) {
                    // button pressed
                    buttonBeingPressedNow = true;
                }
            }
        }

        if (buttonBeingPressedNow || !_buttonPressed && _buttonPressQueued) {
            if (_buttonPressed) {
                // button is already pressed, so queue another one
                _buttonPressQueued = true;
            } else {
                _buttonPressed = true;
                _buttonPressQueued = false;
                if (_linkedBar == null) {
                    timeout = 0;
                    transform.localPosition = _pressedPos;
                    _material.SetColor("_EmissionColor", Color.black);
                }
                OnButtonPressed?.Invoke();
            }
        }
    }
}
