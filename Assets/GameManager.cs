using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _revisitButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _cancelButton;

    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _dialogBox;
    [SerializeField] private TMPro.TextMeshPro _dialogText;
    [SerializeField] private TMPro.TextMeshPro _nextText;

    [SerializeField] private Bar _healthBar;
    [SerializeField] private Bar _energyBar;
    [SerializeField] private Bar _contentmentBar;

    [SerializeField] private Button _healthButton;
    [SerializeField] private Button _energyButton;
    [SerializeField] private Button _contentmentButton;

    [SerializeField] private Transform _sunAxis;

    private enum GameMode {
        Menu,
        GameIntro,
        GameStarting,
        GamePlaying,
        GameEnding,
        GameOutro
    }
    private GameMode _gameMode;

    private int _gameStage;
    private float _sunAngle = 0;
    private float _initialAtmosphereThickness;
    private float kSunsetPeriod = 5.0f;
    private float kSunrisePeriod = 1.0f;
    private float kSunsetAngle = 120.0f;
    private float kDayAtmosphereThickness = 0.5f;
    private float kSettingAtmosphereThickness = 1.25f;

    private float _healthBoost = 0.1f;

    // Start is called before the first frame update
    void Start() {
        _newGameButton.OnButtonPressed += OnNewGameButtonPressed;
        _continueButton.OnButtonPressed += OnContinueButtonPressed;
        _revisitButton.OnButtonPressed += OnRevisitButtonPressed;
        _nextButton.OnButtonPressed += OnNextButtonPressed;
        _cancelButton.OnButtonPressed += OnCancelButtonPressed;

        _healthBar.OnBarDepleted += OnGameOver;
        _healthButton.OnButtonPressed += OnHealthButtonPressed;
        _energyBar.OnBarDepleted += OnEnergyBarDepleted;
        _energyButton.OnButtonPressed += OnEnergyButtonPressed;

        _gameMode = GameMode.GameOutro;
        SetGameMode(GameMode.Menu);

        _initialAtmosphereThickness = RenderSettings.skybox.GetFloat("_AtmosphereThickness");
    }

    private void OnDestroy()
    {
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", _initialAtmosphereThickness);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.N)) {
            // skip to the next section
            _healthBar.value = 0;
            _sunAngle = kSunsetAngle;
        }

        if (_gameMode == GameMode.GameIntro) {
            _sunAngle -= Time.deltaTime * kSunsetAngle / kSunrisePeriod;
            if (_sunAngle < 0)  {
                _sunAngle = 0;
            }
            _sunAxis.transform.localEulerAngles = new Vector3(_sunAngle, 0, 0);
            float atmosphere = Mathf.Lerp(kDayAtmosphereThickness, kSettingAtmosphereThickness, _sunAngle / kSunsetAngle);
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphere);
        } else if (_gameMode == GameMode.GameEnding) {
            _sunAngle += Time.deltaTime * kSunsetAngle / kSunsetPeriod;
            if (_sunAngle >= kSunsetAngle) {
                _sunAngle = kSunsetAngle;
                SetGameMode(GameMode.GameOutro);
            }
            _sunAxis.transform.localEulerAngles = new Vector3(_sunAngle, 0, 0);
            float atmosphere = Mathf.Lerp(kDayAtmosphereThickness, kSettingAtmosphereThickness, _sunAngle / kSunsetAngle);
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphere);
        } else if (_gameMode == GameMode.GamePlaying) {
            if (_gameStage >= 5) {
                if (_energyBar.value < 1.0f) {
                    _healthBar.decayRate = 0.05f;
                    _healthButton.timeoutPeriod = 2.0f;
                } else {
                    _healthBar.decayRate = 0.2f;
                    _healthButton.timeoutPeriod = 0.5f;
                }
            }
        }
    }

    void SetGameMode(GameMode mode)
    {
        if (mode == _gameMode) {
            return;
        }

        // deactivate everything unless we're about to start a game
        _mainMenu.SetActive(false);
        _dialogBox.SetActive(false);
        _healthBar.gameObject.SetActive(false);
        _energyBar.gameObject.SetActive(false);
        _contentmentBar.gameObject.SetActive(false);
        if (mode != GameMode.GamePlaying) {
            _healthButton.gameObject.SetActive(false);
            _energyButton.gameObject.SetActive(false);
            _contentmentButton.gameObject.SetActive(false);
        }

        switch (mode) {
            case GameMode.Menu:
                _mainMenu.SetActive(true);
                break;
            case GameMode.GameIntro:
                _dialogBox.SetActive(true);
                switch (_gameStage) {
                    case 1:
                        _dialogText.text =
                            "You encounter a strange creature. Somehow you can sense two things:\n" +
                            "\n" +
                            "1) This is a \"floof\"\n" +
                            "2) They are dying";
                        break;
                    case 2:
                        _dialogText.text = "Time has suddenly rewinded. This time you feel like you could help the floof.";
                        break;
                    case 3:
                        _dialogText.text = "You think you could be more effective this time.";
                        break;
                    case 4:
                        _dialogText.text = "The floof is connected to you. Think calming thoughts.";
                        break;
                    case 5:
                        _dialogText.text = "Help ease the floof's mind.";
                        break;
                    case 6:
                        _dialogText.text = "Breathe in and out with the floof's rhythm.";
                        break;
                }
                _nextText.text = "Approach the floof";

                _healthBar.lifetime = 0;
                _healthBar.value = 1;
                _healthBar.decayRate = 0.2f;
                _healthButton.timeout = 0;
                _healthButton.timeoutPeriod = 1.0f;
                _healthButton.autoPress = false;
                _healthBoost = 0.1f;
                _energyBar.value = 1;
                _energyBar.decayRate = -1.0f;
                _contentmentBar.value = 0;
                _contentmentBar.decayRate = -1.0f / 30.0f;

                if (_gameStage >= 2) {
                    _healthButton.gameObject.SetActive(true);
                }
                if (_gameStage >= 3) {
                    _healthButton.timeoutPeriod = 0.5f;
                    _healthBoost = 0.07f;
                }
                if (_gameStage >= 4) {
                    _healthButton.autoPress = true;
                }
                if (_gameStage >= 5) {
                    _energyButton.gameObject.SetActive(true);
                }
                if (_gameStage >= 6) {
                    _energyBar.decayRate = -0.25f;
                    _contentmentButton.gameObject.SetActive(true);
                    _energyButton.autoPress = true;
                }
                break;

            case GameMode.GamePlaying:
                _healthBar.gameObject.SetActive(true);
                if (_gameStage >= 5) {
                    _energyBar.gameObject.SetActive(true);
                }
                if (_gameStage >= 6) {
                    _contentmentBar.gameObject.SetActive(true);
                }
                break;

            case GameMode.GameEnding:
                break;

            case GameMode.GameOutro:
                _dialogBox.SetActive(true);
                _dialogText.text = $"The floof lived for {_healthBar.lifetime.ToString("F2")} seconds.\n\n";
                _nextText.text = "Touch the floof's body";
                switch (_gameStage) {
                    case 1:
                        _dialogText.text += "But you feel like you have learned something from being in their presence.";
                        break;
                    case 2:
                        _dialogText.text += "You could not save the floof but you have a better understanding of their body works.";
                        break;
                    case 3:
                        _dialogText.text += "You had started to feel a connection with the floof.";
                        break;
                    case 4:
                        _dialogText.text += "You sense that the floof was trying to tell you something.";
                        break;
                    case 5:
                        _dialogText.text += "You felt the presence of the floof in your mind.";
                        break;
                }

                _gameStage += 1;
                break;
        }

        _gameMode = mode;
    }

    void OnHealthButtonPressed() {
        _healthBar.value += _healthBoost;
    }

    void OnEnergyBarDepleted() {
        _energyBar.decayRate = -Mathf.Abs(_energyBar.decayRate);
    }

    void OnEnergyButtonPressed() {
        if (_energyBar.value == 1) {
            _energyBar.value = 0.999f;
            _energyBar.decayRate = Mathf.Abs(_energyBar.decayRate);
        }
    }

    void OnGameOver() {
        SetGameMode(GameMode.GameEnding);
    }

    void OnNewGameButtonPressed() {
        _gameStage = 1;
        SetGameMode(GameMode.GameIntro);
    }

    void OnContinueButtonPressed() {
        SetGameMode(GameMode.GameIntro);
    }

    void OnRevisitButtonPressed() {
        _gameStage = 10;
        SetGameMode(GameMode.GameIntro);
    }

    void OnNextButtonPressed() {
        if (_gameMode == GameMode.GameIntro) {
            SetGameMode(GameMode.GamePlaying);
        } else if (_gameMode == GameMode.GameOutro) {
            SetGameMode(GameMode.GameIntro);
        }
    }

    void OnCancelButtonPressed() {
        if (_gameMode == GameMode.GameIntro) {
            SetGameMode(GameMode.Menu);
        } else if (_gameMode == GameMode.GameOutro) {
            SetGameMode(GameMode.Menu);
        }
    }
}
