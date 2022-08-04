using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum UIType {
    Main,
    Options,
    Credits,
    HUD,
    Pause,
    Dead
}

public class UIManager : MonoBehaviour {
    #region Variables
    //Singleton for global access to public methods, using UIManager.Instance.x where x is the public method name
    public static UIManager Instance;

    [Header("Intialization")]
    [SerializeField] private List<GameObject> _uiStates = null;
    [SerializeField] private UIType _startingUI = UIType.HUD;

    [Header("Audio")]
    [SerializeField] private AudioClip[] _highlightSound;
    [SerializeField] private AudioSource _audioSource;

    [Header("Main Menu")]
    [SerializeField] private Button _startButton = null;
    [SerializeField] private GameObject _mainButtons = null;
    [SerializeField] private GameObject _quitConfirmation = null;
    [SerializeField] private TextMeshProUGUI _highScoreText = null;

    [Header("Credits")]
    [SerializeField] private Button _creditsBackButton = null;
    [SerializeField] private Button _controlsButton = null;
    [SerializeField] private Button _backToOptionsButton = null;
    [SerializeField] private GameObject _controlsScreen = null;
    [SerializeField] private GameObject _preferencesScreen = null;
    [SerializeField] private Toggle _invertedControlsToggle = null;
    [SerializeField] private Toggle _cameraShakeToggle = null;
    [SerializeField] private Slider _masterVolume = null;
    [SerializeField] private Slider _musicVolume = null;
    [SerializeField] private Slider _ambienceVolume = null;
    [SerializeField] private Slider _sfxVolume = null;
    [SerializeField] private Slider _dialogueVolume = null;
    [SerializeField] private Slider _lookSensitivity = null;
    [SerializeField] private Slider _fieldOfView = null;
    [SerializeField] private Slider _bloom = null;
    [SerializeField] private TMP_Dropdown _qualityDropdown = null;

    [Header("Pause Menu")]
    [SerializeField] private Button _continueButton = null;
    [SerializeField] private Button _noConfirmationButton = null;
    [SerializeField] private GameObject _pauseInteractables = null;
    [SerializeField] private GameObject _mainConfirmation = null;
    [SerializeField] private Toggle _invertedControlsTogglePaused = null;
    [SerializeField] private Toggle _cameraShakeTogglePaused = null;
    [SerializeField] private Slider _masterVolumePaused = null;
    [SerializeField] private Slider _musicVolumePaused = null;
    [SerializeField] private Slider _ambienceVolumePaused = null;
    [SerializeField] private Slider _sfxVolumePaused = null;
    [SerializeField] private Slider _dialogueVolumePaused = null;
    [SerializeField] private Slider _lookSensitivityPaused = null;
    [SerializeField] private Slider _fieldOfViewPaused = null;
    [SerializeField] private Slider _bloomPaused = null;
    [SerializeField] private TMP_Dropdown _qualityDropdownPaused = null;

    [Header("HUD")]    
    [SerializeField] private GameObject _waveContainer = null;
    [SerializeField] private TextMeshProUGUI _waveText = null;
    [SerializeField] private GameObject _dialogueBox = null;
    [SerializeField] private TextMeshProUGUI _actorDialogue = null;
    [SerializeField] private Slider _healthSlider = null;
    [SerializeField] private Image _healthBarImage = null;
    [SerializeField] private Gradient _healthGradient = null;

    [Header("Dead")]
    [SerializeField] private GameObject _deadButtons = null;
    [SerializeField] private Button _retryButton = null;
    [SerializeField] private GameObject _deadQuitConfirmation = null;
    [SerializeField] private Button _deadNoQuitButton = null;
    [SerializeField] private TextMeshProUGUI _highScoreResultsText = null;

    private UIType _currentType;
    private GameObject _currentState = null;
    private GameObject _previousState = null;
    private GameObject _lastElementSelected = null;

    #endregion

    #region Unity Events
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != null) {
            Destroy(this.gameObject);
        }        
    }

    private void Start() {
        InitUI();
        UIEventSubs(true);
    }

    private void OnDestroy() {
        UIEventSubs(false);
    }

    private void Update() {
        if (_currentType != UIType.HUD) {
            if (EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_lastElementSelected);
            }
        }
    }
    #endregion

    #region Private Methods
    private void UIEventSubs(bool isStart) {
        if (isStart) {
            // Subscribe to Events here
            Events.OnHealthChange += OnPlayerHealthChange;
            Events.OnDeath += OnPlayerDeath;
        }
        else {
            // Unsubscribe Events here
            Events.OnHealthChange -= OnPlayerHealthChange;
            Events.OnDeath -= OnPlayerDeath;
        }
    }    

    private void InitUI() {
        //Need to make sure that the UI SFX does not get paused when the game pauses
        _audioSource.ignoreListenerPause = true;

        //Reset all UI in case something was left open in-editor
        foreach (GameObject state in _uiStates) {
            state.SetActive(false);
        }

        //Set default values
        _quitConfirmation.SetActive(false);
        _waveContainer.SetActive(false);
        _dialogueBox.SetActive(false);
        _controlsScreen.SetActive(false);
        _preferencesScreen.SetActive(true);
        _deadQuitConfirmation.SetActive(false);
        UpdateOptions(true);

        //Find and set starting UI
        _currentState = FindUI(_startingUI);
        if (_currentState != null) {
            UpdateUI(_startingUI);
            FocusOnButton(_startingUI);
        }

        //Finish up
        _currentType = _startingUI;
        ChangeUIStateConfigs(_startingUI);        
    }

    private void OnPlayerHealthChange(float healthPercentage) {
        //Slider value needs to match %
        _healthSlider.value = healthPercentage * 100f;

        //Determine the color needed from the gradient and assign it to the image
        Color32 newColor = _healthGradient.Evaluate(_healthBarImage.fillAmount);
        _healthBarImage.color = newColor;
    }

    private void OnPlayerDeath() {
        OnPlayerHealthChange(1f);
        GameManager.Instance.HighScore();
        _deadButtons.SetActive(true);
        AudioManager.Instance.CleanupAllSFXSources();
        UpdateUI(UIType.Dead);
    }

    private void ChangeUIStateConfigs(UIType type) {
        if (type == UIType.HUD) {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputManager.Instance.SwitchCurrentActionMap(ActionMapName.Player.ToString());
        }
        else if (type == UIType.Main || type == UIType.Dead) {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            InputManager.Instance.SwitchCurrentActionMap(ActionMapName.UI.ToString());
        }
        else {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            InputManager.Instance.SwitchCurrentActionMap(ActionMapName.UI.ToString());
        }
    }

    private void FocusOnButton(UIType type) {
        GameObject button = null;

        switch (type) {
            case UIType.Main:
                button = _startButton.gameObject;
                break;
            case UIType.Options:
                button = _controlsButton.gameObject;
                break;
            case UIType.Credits:
                button = _creditsBackButton.gameObject;
                break;
            case UIType.Pause:
                button = _continueButton.gameObject;
                break;
            case UIType.Dead:
                button = _retryButton.gameObject;
                break;
            case UIType.HUD:
            default:
                break;
        }

        if (button != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }

            EventSystem.current.SetSelectedGameObject(button);
            HighlightButton(button);
        }        
    }

    private GameObject FindUI(UIType type) {
        foreach (var state in _uiStates) {
            if (state.name.Contains(type.ToString())) {
                return state;
            }
        }

        return null;
    }

    private void UpdateOptions(bool isMainOptions = true) {
        if (isMainOptions) {
            _invertedControlsToggle.isOn = PreferenceManager.Instance.GetBoolPref("InvertedControls");
            _cameraShakeToggle.isOn = PreferenceManager.Instance.GetBoolPref("CameraShake");
            _masterVolume.value = PreferenceManager.Instance.GetFloatPref("Master");
            _musicVolume.value = PreferenceManager.Instance.GetFloatPref("Music");
            _ambienceVolume.value = PreferenceManager.Instance.GetFloatPref("Ambience");
            _sfxVolume.value = PreferenceManager.Instance.GetFloatPref("SFX");
            _dialogueVolume.value = PreferenceManager.Instance.GetFloatPref("Dialogue");
            _lookSensitivity.value = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
            _fieldOfView.value = PreferenceManager.Instance.GetFloatPref("FieldOfView");
            _bloom.value = PreferenceManager.Instance.GetFloatPref("Bloom");
            _qualityDropdown.value = PreferenceManager.Instance.GetIntPref("Quality");
        }
        else {
            _invertedControlsTogglePaused.isOn = PreferenceManager.Instance.GetBoolPref("InvertedControls");
            _cameraShakeTogglePaused.isOn = PreferenceManager.Instance.GetBoolPref("CameraShake");
            _masterVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Master");
            _musicVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Music");
            _ambienceVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Ambience");
            _sfxVolumePaused.value = PreferenceManager.Instance.GetFloatPref("SFX");
            _dialogueVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Dialogue");
            _lookSensitivityPaused.value = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
            _fieldOfViewPaused.value = PreferenceManager.Instance.GetFloatPref("FieldOfView");
            _bloomPaused.value = PreferenceManager.Instance.GetFloatPref("Bloom");
            _qualityDropdownPaused.value = PreferenceManager.Instance.GetIntPref("Quality");
        }  
    }

    private IEnumerator HideDialogue(float delay) {
        yield return new WaitForSeconds(delay);
        _dialogueBox.SetActive(false);
    }
    #endregion

    #region Public Methods
    public UIType GetCurrentType() {
        return _currentType;
    }

    public void ChangeUITypeOnPausePress() {
        if (_currentType == UIType.Pause) {
            PauseMenu(false);
        }
        else if (_currentType == UIType.HUD) {
            PauseMenu(true);
        }
    }

    public void SpeakVoiceLine(DialogueScriptableObject dialogue) {
        _actorDialogue.text = dialogue.Transcript;

        if (!_dialogueBox.activeInHierarchy) {
            _dialogueBox.SetActive(true);
        }
    }

    public void EndDialogue() {
        StartCoroutine(HideDialogue(0.5f));
    }

    public void UpdateUI(UIType type) {
        _previousState = _currentState;
        _currentState = FindUI(type);

        if (_currentState != null) {
            _previousState.SetActive(false);
            _currentState.SetActive(true);

            _currentType = type;
            FocusOnButton(type);            
            ChangeUIStateConfigs(type);
        }
    }

    public void PauseMenu(bool gamePaused) {
        if (gamePaused) {
            UpdateOptions(false);
            AudioManager.Instance.PauseDialogue(true);
            UpdateUI(UIType.Pause);
        }
        else {
            AudioManager.Instance.PauseDialogue(false);
            UpdateUI(UIType.HUD);
        }
    }

    public void HideWaveText() {
        _waveText.text = "";

        if (_waveContainer.activeInHierarchy) {
            _waveContainer.SetActive(false);
        }
    }

    public void RevealWaveText(int wave) {
        _waveText.text = "Wave " + wave.ToString();

        if (!_waveContainer.activeInHierarchy) {
            _waveContainer.SetActive(true);
        }
    }

    public void UpdateHighScore(int score) {
        _highScoreResultsText.text = "High Score: " + score.ToString() + " Waves";
        _highScoreText.text = "High Score: " + score.ToString() + " Waves";
    }

    #endregion

    #region UI Event Triggers
    public void ElementHighlighted() {
        if(_highlightSound != null) {
            if (_highlightSound.Length > 0) {
                int index = Random.Range(0, _highlightSound.Length);

                _audioSource.clip = _highlightSound[index];
                _audioSource.Play();
            }            
        }
    }

    public void SetLastElementSelected(GameObject element) {
        _lastElementSelected = element;
    }

    public void HighlightButton(GameObject button) {
        Image buttonImage = button.gameObject.GetComponent<Image>();
        if (buttonImage != null) {
            var currentColor = buttonImage.color;

            Color32 color = new Color32((byte)(currentColor[0] * 255f), (byte)(currentColor[1] * 255f), (byte)(currentColor[2] * 255f), 255);
            buttonImage.color = color;
        }
    }

    public void UnHighlightButton(GameObject button) {
        Image buttonImage = button.gameObject.GetComponent<Image>();
        if (buttonImage != null) {
            var currentColor = buttonImage.color;

            Color32 color = new Color32((byte)(currentColor[0] * 255f), (byte)(currentColor[1] * 255f), (byte)(currentColor[2] * 255f), 0);
            buttonImage.color = color;
        }
    }    
   
    public void StartGame() {
        OnPlayerHealthChange(1f);
        SceneManager.LoadScene(1);
        
        UpdateUI(UIType.HUD);
        AudioManager.Instance.StartGame();
        GameManager.Instance.RestartWave();
    }

    public void GoToCredits() {
        UpdateUI(UIType.Credits);
    }

    public void BackToMain() {
        SceneManager.LoadScene(0);
        UpdateUI(UIType.Main);
    }

    public void BackToMainFromPause() {
        _pauseInteractables.SetActive(true);
        _mainConfirmation.SetActive(false);

        SceneManager.LoadScene(0);        
        UpdateUI(UIType.Main);
        AudioManager.Instance.BackToMain();
    }

    public void RetryFromDead() {
        SceneManager.LoadScene(1);
        OnPlayerHealthChange(1f);
        UpdateUI(UIType.HUD);
        GameManager.Instance.RestartWave();
    }

    public void MainMenuFromDead() {
        _deadQuitConfirmation.SetActive(false);
        SceneManager.LoadScene(0);        
        UpdateUI(UIType.Main);
        AudioManager.Instance.BackToMain();
    }

    public void GoToControls() {
        _preferencesScreen.SetActive(false);
        _controlsScreen.SetActive(true);

        if (_backToOptionsButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }                
            EventSystem.current.SetSelectedGameObject(_backToOptionsButton.gameObject);
            HighlightButton(_backToOptionsButton.gameObject);
        }
    }

    public void GoToOptionsFromControls() {
        _controlsScreen.SetActive(false);
        _preferencesScreen.SetActive(true);

        if (_controlsButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }
            EventSystem.current.SetSelectedGameObject(_controlsButton.gameObject);
            HighlightButton(_controlsButton.gameObject);
        }
    }

    public void GoToOptionsFromMain() {
        UpdateOptions(true);
        UpdateUI(UIType.Options);
    }

    public void AreYouSure(Button focusedButton) {
        if (_currentType == UIType.Main) {
            _mainButtons.SetActive(false);
            _deadQuitConfirmation.SetActive(false);
            _quitConfirmation.SetActive(true);            
        }
        else if (_currentType == UIType.Pause) {
            _pauseInteractables.SetActive(false);
            _deadQuitConfirmation.SetActive(false);
            _mainConfirmation.SetActive(true);
        }
        else if (_currentType == UIType.Dead) {
            _deadButtons.SetActive(false);            
            _quitConfirmation.SetActive(false);
            _deadQuitConfirmation.SetActive(true);
        }

        if (focusedButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }                
            EventSystem.current.SetSelectedGameObject(focusedButton.gameObject);
            HighlightButton(focusedButton.gameObject);
        }
    }

    public void YesQuit() {
        Debug.LogWarning("Quit");
        Application.Quit();
    }

    public void NoQuit(Button focusedButton) {
        if (_currentType == UIType.Main) {
            _quitConfirmation.SetActive(false);
            _mainButtons.SetActive(true);
        }
        else if (_currentType == UIType.Pause) {
            _mainConfirmation.SetActive(false);
            _pauseInteractables.SetActive(true);
        }
        else if (_currentType == UIType.Dead) {            
            _deadQuitConfirmation.SetActive(false);
            _deadButtons.SetActive(true);
        }

        if (focusedButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }                
            EventSystem.current.SetSelectedGameObject(focusedButton.gameObject);
            HighlightButton(focusedButton.gameObject);
        }
    }

    public void ChangeAudio(Slider slider) {
        AudioManager.Instance.SetVolume(slider.name, slider.value, true);
    }

    public void ChangeVolume(Slider slider) {
        Events.OnBloomChange?.Invoke(slider.value);
        PreferenceManager.Instance.UpdatePreferences(slider.name, slider.value);
    }

    public void ChangeLookSensitivity(Slider slider) {
        Events.OnLookSensitivityChange?.Invoke(slider.value);
        PreferenceManager.Instance.UpdatePreferences(slider.name, slider.value);
    }

    public void ChangeInvertedControls(Toggle toggle) {
        Events.OnInvertedControlsChange?.Invoke(toggle.isOn);
        PreferenceManager.Instance.UpdatePreferences(toggle.name, toggle.isOn);
    }

    public void ChangeCameraShake(Toggle toggle) {
        Events.OnCameraShakeChange?.Invoke(toggle.isOn);
        PreferenceManager.Instance.UpdatePreferences(toggle.name, toggle.isOn);
    }

    public void ChangeFOV(Slider slider) {
        Events.OnFOVChange?.Invoke(slider.value);
        PreferenceManager.Instance.UpdatePreferences(slider.name, slider.value);
    }

    public void ChangeQualitySettings(TMP_Dropdown dropdown) {
        QualitySettings.SetQualityLevel(dropdown.value);
        PreferenceManager.Instance.UpdatePreferences(dropdown.name, dropdown.value);
    }
    #endregion
}
