using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Camera _mainCam;

    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _revisitButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _endButton;

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
        GamePlaying,
        GameEnding,
        GameOutro,
        Ascending,
    }
    private GameMode _gameMode;

    private int _gameStage = 1;
    private float _sunAngle = 0;
    private float _initialAtmosphereThickness;
    private bool _ascensionUnlocked = false;
    private float _ascensionTime = 0.0f;
    private float kSunsetPeriod = 5.0f;
    private float kSunrisePeriod = 1.0f;
    private float kSunsetAngle = 120.0f;
    private float kDayAtmosphereThickness = 0.5f;
    private float kSettingAtmosphereThickness = 1.25f;
    private float kAscensionPeriod = 5.0f;

    private float _healthBoost = 0.1f;

    // Start is called before the first frame update
    void Start() {
        _newGameButton.OnButtonPressed += OnNewGameButtonPressed;
        _continueButton.OnButtonPressed += OnContinueButtonPressed;
        _revisitButton.OnButtonPressed += OnRevisitButtonPressed;
        _nextButton.OnButtonPressed += OnNextButtonPressed;
        _cancelButton.OnButtonPressed += OnCancelButtonPressed;
        _endButton.OnButtonPressed += OnEndButtonPressed;

        _healthBar.OnBarDepleted += OnGameOver;
        _healthButton.OnButtonPressed += OnHealthButtonPressed;
        _energyBar.OnBarDepleted += OnEnergyBarDepleted;
        _energyButton.OnButtonPressed += OnEnergyButtonPressed;
        _contentmentButton.OnButtonPressed += OnContentmentButtonPressed;

        _contentmentBar.OnBarFull += OnContentmentBarFull;

        _initialAtmosphereThickness = RenderSettings.skybox.GetFloat("_AtmosphereThickness");

        _gameMode = GameMode.GameOutro;
        SetGameMode(GameMode.Menu);

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

        if (Input.GetKeyDown(KeyCode.M))
        {
            // skip to the next section
            _healthBar.value = .01f;
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
        } else if (_gameMode == GameMode.Ascending) {
            _sunAngle += Time.deltaTime * kSunsetAngle / kSunsetPeriod;
            if (_sunAngle < kSunsetAngle)
            {
                float atmosphere = Mathf.Lerp(kDayAtmosphereThickness, kSettingAtmosphereThickness, _sunAngle / kSunsetAngle);
                RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphere);
            }
            else if (_sunAngle < 2 * kSunsetAngle)
            {
                float atmosphere = Mathf.Lerp(kDayAtmosphereThickness, kSettingAtmosphereThickness, 2 - _sunAngle / kSunsetAngle);
                RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphere);
            }
            else
            {
                _sunAngle = 2 * kSunsetAngle;
            }
            _sunAxis.transform.localEulerAngles = new Vector3(_sunAngle, 0, 0);

            _ascensionTime += Time.deltaTime / kAscensionPeriod;
            _mainCam.transform.localEulerAngles = new Vector3(
                Mathf.SmoothStep(10, -45, _ascensionTime), 0, 0);
        }
    }

    void SetGameMode(GameMode mode)
    {
        if (mode == _gameMode) {
            return;
        }

        // reset everything unless we're about to start a game
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", _initialAtmosphereThickness);
        _mainCam.transform.localEulerAngles = new Vector3(10, 0, 0);
        _mainMenu.SetActive(false);
        _continueButton.gameObject.SetActive(false);
        _revisitButton.gameObject.SetActive(false);
        _dialogBox.SetActive(false);
        _endButton.gameObject.SetActive(false);
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
                if (_ascensionUnlocked) {
                    _revisitButton.gameObject.SetActive(true);
                }
                if (_gameStage != 1) {
                    _continueButton.gameObject.SetActive(true);
                }
                _healthBar.value = 1;
                _sunAngle = 0;
                _sunAxis.transform.localEulerAngles = new Vector3(_sunAngle, 0, 0);
                RenderSettings.skybox.SetFloat("_AtmosphereThickness", kDayAtmosphereThickness);
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
                _contentmentBar.decayRate = -1.0f / 3.0f;

                if (_gameStage >= 2) {
                    _healthButton.gameObject.SetActive(true);
                    if (_gameStage == 2) {
                        _healthButton.Flash();
                    }
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
                    if (_gameStage == 5) {
                        _energyButton.Flash();
                    }
                }
                if (_gameStage >= 6) {
                    _energyBar.decayRate = -0.25f;
                    _contentmentButton.gameObject.SetActive(true);
                    _contentmentButton.SetPressed();
                    _healthBar.decayRate = 0.05f;
                    _healthButton.timeoutPeriod = 2.0f;
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
                if (_gameStage == 6)
                {
                    SetGameMode(GameMode.Ascending);
                    return;
                }
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

                if (_gameStage < 6) {
                    _gameStage += 1;
                }
                break;

            case GameMode.Ascending:
                // fade out all the UI
                _mainMenu.SetActive(false);
                _dialogBox.SetActive(false);
                _healthBar.gameObject.SetActive(false);
                _energyBar.gameObject.SetActive(false);
                _contentmentBar.gameObject.SetActive(false);
                _healthButton.gameObject.SetActive(false);
                _energyButton.gameObject.SetActive(false);
                _contentmentButton.gameObject.SetActive(false);
                _endButton.gameObject.SetActive(true);
                _gameStage = 1;
                _ascensionUnlocked = true;
                _ascensionTime = 0.0f;
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

    void OnContentmentBarFull() {
        _contentmentButton.Flash();
    }

    void OnContentmentButtonPressed() {
        _contentmentBar.value = 0.999f;
        _contentmentBar.decayRate = 0;

        SetGameMode(GameMode.Ascending);
    }

    void OnEndButtonPressed()
    {
        if (_gameMode == GameMode.Ascending) {
            SetGameMode(GameMode.Menu);
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
        _gameStage = 6;
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
