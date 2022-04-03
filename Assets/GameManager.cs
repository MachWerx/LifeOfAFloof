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
    private float kSunsetPeriod = 3.0f;
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
        if (_gameMode == GameMode.GameIntro) {
            _sunAngle -= Time.deltaTime * kSunsetAngle / kSunrisePeriod;
            if (_sunAngle < 0)  {
                _sunAngle = 0;
            }
            _sunAxis.transform.localEulerAngles = new Vector3(_sunAngle, 0, 0);
            float atmosphere = Mathf.Lerp(kDayAtmosphereThickness, kSettingAtmosphereThickness, _sunAngle / kSunsetAngle);
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphere);
        }
        else if (_gameMode == GameMode.GameOutro) {
            _sunAngle += Time.deltaTime * kSunsetAngle / kSunsetPeriod;
            if (_sunAngle > kSunsetAngle) {
                _sunAngle = kSunsetAngle;
            }
            _sunAxis.transform.localEulerAngles = new Vector3(_sunAngle, 0, 0);
            float atmosphere = Mathf.Lerp(kDayAtmosphereThickness, kSettingAtmosphereThickness, _sunAngle / kSunsetAngle);
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphere);
        }
    }

    void SetGameMode(GameMode mode)
    {
        if (mode == _gameMode) {
            return;
        }

        // deactivate everything
        _mainMenu.SetActive(false);
        _dialogBox.SetActive(false);
        _healthBar.gameObject.SetActive(false);
        _energyBar.gameObject.SetActive(false);
        _contentmentBar.gameObject.SetActive(false);
        _healthButton.gameObject.SetActive(false);
        _energyButton.gameObject.SetActive(false);
        _contentmentButton.gameObject.SetActive(false);

        switch (mode) {
            case GameMode.Menu:
                _mainMenu.SetActive(true);
                break;
            case GameMode.GameIntro:
                _dialogBox.SetActive(true);
                _dialogText.text =
                    "You encounter a strange creature. Somehow you can sense two things:\n" +
                    "\n" +
                    "1) This is a \"floof\"\n" +
                    "2) They are dying";
                _nextText.text = "Approach the floof";
                break;
            case GameMode.GamePlaying:
                _healthBar.gameObject.SetActive(true);
                _healthBar.lifetime = 0;
                _healthBar.value = 1;
                _healthBar.decayRate = 0.2f;
                _healthButton.timeout = 0;
                _healthButton.timeoutPeriod = 1.0f;
                _healthButton.autoPress = false;
                _healthBoost = 0.1f;

                if (_gameStage >= 2) {
                    _healthButton.gameObject.SetActive(true);
                }
                if (_gameStage >= 3) {
                    _healthButton.timeoutPeriod = 0.5f;
                    _healthBoost = 0.07f;
                }
                if (_gameStage >= 3) {
                    _healthButton.timeoutPeriod = 0.3f;
                    _healthBoost = 0.05f;
                }
                if (_gameStage >= 4) {
                    _healthButton.autoPress = true;
                }

                break;
            case GameMode.GameOutro:
                _dialogBox.SetActive(true);
                _dialogText.text = $"The floof has died. They lived for {_healthBar.lifetime.ToString("F2")} seconds. ";
                _nextText.text = "Touch the floof's body";
                switch (_gameStage) {
                    case 1:
                        _dialogText.text += "But now you are filled with the knowledge of what you could have done. If only you could go back in time.";
                        break;
                    case 2:
                        _dialogText.text += "You have gained knowledge. You think you could comfort the floof more efficiently next time.";
                        break;
                    case 3:
                        _dialogText.text += "You are beginning to feel like you can understand the floof. You feel their presence in your mind.";
                        break;
                    case 4:
                        _dialogText.text += "You feel a sense of relaxation. You feel like you don't have to do anything for the moment.";
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

    void OnGameOver()
    {
        SetGameMode(GameMode.GameOutro);
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
