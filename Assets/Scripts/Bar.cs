using System;
using UnityEngine;

public class Bar : MonoBehaviour {
    public float decayRate;
    public float lifetime;
    public float value
    {
        get { return _value; }
        set {
            _value = Mathf.Clamp01(value);
            if (_value == 0) {
                OnBarDepleted?.Invoke();
            }
            Vector3 scale = transform.localScale;
            scale.x = _value * _initialLength;
            transform.localScale = scale;
            _material.SetColor("_EmissionColor", _value * _initialColor);
        }
    }
    public Action OnBarDepleted;

    private float _value;
    private Material _material;
    private float _initialLength;
    private Color _initialColor;

    private void Awake() {
        _material = GetComponentInChildren<Renderer>().material;
        _initialLength = transform.localScale.x;
        _initialColor = _material.GetColor("_EmissionColor");
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        float decayAmount = decayRate * Time.deltaTime;
        if (decayAmount < value) {
            lifetime += Time.deltaTime;
        } else {
            lifetime += value / decayRate;
        }
        value -= decayAmount;
    }
}
