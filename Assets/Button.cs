using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {
    public float TimeoutPeriod = 1.0f;

    private float _timeout;
    private Material _material;
    private Transform _collisionTransform;
    private Vector3 _initialPos;
    private Vector3 _pressedPos;
    private Color _initialColor;
    private bool _buttonPressed;


    // Start is called before the first frame update
    void Start() {
        _material = GetComponentInChildren<Renderer>().material;
        _collisionTransform = GetComponentInChildren<Collider>().transform;
        _initialPos = transform.localPosition;
        _pressedPos = _initialPos + 0.1f * Vector3.forward;
        _initialColor = _material.GetColor("_EmissionColor");
    }

    // Update is called once per frame
    void Update() {
        if (_buttonPressed) {
            _timeout += Time.deltaTime / TimeoutPeriod;
            transform.localPosition = Vector3.Lerp(_pressedPos, _initialPos, _timeout);
            _material.SetColor("_EmissionColor", 0.5f * _timeout * _initialColor);
            if (_timeout >= 1.0f) {
                // button released
                _timeout = 1.0f;
                _buttonPressed = false;
                transform.localPosition = _initialPos;
                _material.SetColor("_EmissionColor", _initialColor);
            }
        }
        else if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform == _collisionTransform) {
                    // button pressed
                    _buttonPressed = true;
                    _timeout = 0;
                    transform.localPosition = _pressedPos;
                    _material.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}