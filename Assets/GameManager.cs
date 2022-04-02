using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Bar _healthBar;
    [SerializeField] private Bar _energyBar;
    [SerializeField] private Bar _contentmentBar;

    [SerializeField] private Button _healthButton;
    [SerializeField] private Button _energyButton;
    [SerializeField] private Button _contentmentButton;

    // Start is called before the first frame update
    void Start() {
        _healthBar.value = 1.0f;
        _healthBar.decayRate = 0.2f;
        _healthButton.OnButtonPressed += OnHealthButtonPressed;
    }

    // Update is called once per frame
    void Update() {
    }

    void OnHealthButtonPressed() {
        _healthBar.value += 0.1f;
    }
}
