using System.Collections.Generic;
using UnityEngine;

public class Floof : MonoBehaviour
{
    [SerializeField] private Bar _healthBar;
    private List<(Material, Color)> _materialsAndColors;
    private Transform _collisionTransform;
    private bool _isJumping;
    private Vector3 _initialPosition;
    private float _jumpTime;
    private float _jumpPeriod = 0.2f;
    private float kGravity = -9.8f;

    void Awake() {
        _materialsAndColors = new List<(Material, Color)>();
        foreach(Renderer renderer in GetComponentsInChildren<Renderer>()) {
            _materialsAndColors.Add((renderer.material, renderer.material.color));
        }
        _collisionTransform = GetComponent<Collider>().transform;
        _initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // update color
        foreach(var materialAndColor in _materialsAndColors) {
            materialAndColor.Item1.color = _healthBar.value * materialAndColor.Item2;
        }

        // check for jump
        if (Input.GetMouseButtonDown(0) && !_isJumping && _healthBar.value > 0) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform == _collisionTransform) {
                    // button pressed
                    _isJumping = true;
                    _jumpTime = 0;
                }
            }
        }

        // jumping
        if (_isJumping) {
            _jumpTime += Time.deltaTime;
            if (_jumpTime >= _jumpPeriod) {
                _jumpTime = _jumpPeriod;
                _isJumping = false;
            }
            float v = -0.5f * kGravity * _jumpPeriod;
            float height = 0 + v * _jumpTime + 0.5f * kGravity * _jumpTime * _jumpTime;
            transform.localPosition = _initialPosition + height * Vector3.up;
        }
    }
}
