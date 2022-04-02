using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _energyBar;
    [SerializeField] private GameObject _contentmentBar;

    private float _health = 100;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        _health -= 1.0f * Time.deltaTime;
        _healthBar.transform.localScale = new Vector3(_health / 100.0f, 1, 1);
    }
}
