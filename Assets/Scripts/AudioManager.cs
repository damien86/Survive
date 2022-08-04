using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioType {
    Master,
    Music,
    Ambience,
    Dialogue,
    SFX,
    Footsteps,
    Weapon,
    Enemy,
    None
}

public class AudioManager : MonoBehaviour
{
    #region Variables
    // All public functions in this class can be accessed by typing AudioManager.Instance.<nameOfFunction> from any class
    public static AudioManager Instance;
    public static bool IsDestroyed { 
        get { return Instance == null; } 
    }

    [Header("Mixing")]
    [SerializeField] private AudioMixer _gameMixer = null;
    [SerializeField] private AudioMixerGroup[] _mixerGroups = null;
    [SerializeField] private AudioMixerSnapshot[] _musicSnapshots = null;
    [SerializeField] private AudioMixerSnapshot[] _ambienceSnapshots = null;
    private AudioMixerSnapshot _currentMusicSnapshot;

    [Header("Background")]
    [SerializeField] private AudioSource[] _musicSources = null;
    [SerializeField] private AudioSource[] _ambienceSources = null;
    [SerializeField] private SwapAudioScriptableObject _menuAudio = null;
    [SerializeField] private SwapAudioScriptableObject _inGameAudio = null;

    [Header("Voiceovers")]
    [SerializeField] private AudioSource[] _dialogueSources = null;

    [Header("SFX")]
    [SerializeField] private AudioSource _footStepSource = null;
    [SerializeField] private AudioSource _weaponSource = null;
    [SerializeField] private AudioClip[] _zombieAttackClips = null;
    [SerializeField] private AudioClip[] _zombieDeathClips = null;
    [SerializeField] private AudioClip[] _zombieNoiseClips = null;
    [SerializeField] private AudioClip[] _zombieSpawnClips = null;
    [SerializeField] private AudioClip[] _healthPickupClips = null;
    [SerializeField] private Transform _sfxParent = null;
    [SerializeField] private GameObject _sourcePrefab = null;
    [SerializeField] private int _numberOfSources = 1;

    private List<AudioSource> _soundEffectSources = new List<AudioSource>();
    private AudioSource _currentDialogueSource = null;
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

        InitAudio();        
    }

    private void Start() {
        InitMixer();
    }
    #endregion

    #region Private Methods
    private void InitMixer() {
        _currentMusicSnapshot = _musicSnapshots[0];
        _currentDialogueSource = _dialogueSources[0];

        //Set volumes for mixers based on PlayerPrefs
        _gameMixer.SetFloat("Master", PreferenceManager.Instance.GetFloatPref("Master"));
        foreach (var mixer in _mixerGroups) {
            if(!_gameMixer.SetFloat(mixer.name, PreferenceManager.Instance.GetFloatPref(mixer.name))) {
                Debug.Log("parameter not found");
            }

            mixer.audioMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
        }
    }

    private void InitSource(AudioSource[] sources, AudioType audioType = AudioType.None) {
        AudioMixerGroup group = null;
        string audioTypeStr = audioType.ToString();
        bool playOnAwake = true;
        bool loop = false;

        if(audioType != AudioType.None) {
            foreach (var mixer in _mixerGroups) {
                if (mixer.name == audioTypeStr) {
                    group = mixer;
                    break;
                }
            }
        }

        switch (audioType) {
            case AudioType.Ambience:
                loop = true;
                sources[0].clip = _menuAudio.Ambience;
                sources[0].Play();
                break;
            case AudioType.Music:
                loop = true;
                sources[0].clip = _menuAudio.Music;
                sources[0].Play();
                break;
            case AudioType.Dialogue:
                break;
            case AudioType.SFX:
                break;
            default:
                break;
        }

        foreach (var source in sources) {
            source.playOnAwake = playOnAwake;
            source.loop = loop;
            source.volume = 1f;
            if(source.outputAudioMixerGroup == null) {
                source.outputAudioMixerGroup = group;
            }           
        }
    }

    private void InitAudio() {
        InitSource(_musicSources, AudioType.Music);
        InitSource(_ambienceSources, AudioType.Ambience);
        InitSource(_dialogueSources, AudioType.Dialogue);

        if(_soundEffectSources.Count == 0) {
            for (int i = 0; i < _numberOfSources; i++) {
                _soundEffectSources.Add(CreateNewSource(_sfxParent));
            }
        }
    }

    private AudioSource CreateNewSource(Transform parent) {
        GameObject clone = Instantiate(_sourcePrefab, transform.position, Quaternion.identity, parent);
        AudioSource source = clone.GetComponent<AudioSource>();
        
        if (source != null) {
            AudioSource[] sources = new AudioSource[] { source };
            InitSource(sources, AudioType.SFX);
        }

        clone.SetActive(false);
        return source;
    }

    private IEnumerator SourceCleanup(AudioSource source, bool isDialogue = false, bool attachToParent = false) {
        while (isDialogue && (source.isPlaying || source.time != 0)) {
            yield return null;
        }

        if (source == null) {
            yield break;
        }

        float counter = 0.5f;
        while (counter > 0) {
            counter -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (!source.isPlaying || Time.timeScale == 0) {
            source.spatialBlend = 0f;
            source.clip = null;
            if (attachToParent) {
                source.gameObject.transform.SetParent(_sfxParent);
            }
            source.gameObject.SetActive(false);

            if (isDialogue) {
                UIManager.Instance.EndDialogue();
            }
        }
    }
    #endregion

    #region Public Functions
    /* Public functions to access private AudioManager data or perform actions */
    public void CleanupAllSFXSources() {
        foreach (AudioSource source in _soundEffectSources) {
            if (source == null) {
                if (source.gameObject.activeInHierarchy) {
                    StartCoroutine(SourceCleanup(source, false, true));
                }                
            }
        }
    }

    public void ZombieAttack(GameObject zombie) {
        AudioClip spawnClip = _zombieAttackClips[UnityEngine.Random.Range(0, _zombieAttackClips.Length)];
        PlayEffect(spawnClip, AudioType.SFX, false, zombie.transform, false);
    }

    public void ZombieSpawn(GameObject zombie) {
        AudioClip spawnClip = _zombieSpawnClips[UnityEngine.Random.Range(0, _zombieSpawnClips.Length)];
        PlayEffect(spawnClip, AudioType.SFX, false, zombie.transform, false)
        ;
    }

    public void ZombieDeath(GameObject zombie) {
        AudioClip spawnClip = _zombieDeathClips[UnityEngine.Random.Range(0, _zombieDeathClips.Length)];
        PlayEffect(spawnClip, AudioType.SFX, false, zombie.transform, false);
    }

    public void ZombieNoise(GameObject zombie) {
        AudioClip spawnClip = _zombieNoiseClips[UnityEngine.Random.Range(0, _zombieNoiseClips.Length)];
        PlayEffect(spawnClip, AudioType.SFX, false, zombie.transform, false);
    }

    public void HealthPickup(GameObject pickup) {
        AudioClip pickupClip = _healthPickupClips[UnityEngine.Random.Range(0, _healthPickupClips.Length)];
        PlayEffect(pickupClip, AudioType.SFX, false, pickup.transform, true);
    }

    public void PlayPlayerEffect(AudioClip clip, AudioType sourceToUse, bool wantToPitch = false) {
        if (clip != null) {
            float pitch = wantToPitch ? UnityEngine.Random.Range(0.1f, 3f) : 1f;
            
            if (sourceToUse == AudioType.Footsteps) {
                _footStepSource.pitch = pitch;
                _footStepSource.PlayOneShot(clip);
            }
            else if (sourceToUse == AudioType.Weapon) {
                _weaponSource.pitch = pitch;
                _weaponSource.PlayOneShot(clip);
            }
        }        
    }

    public void PlayEffect(AudioClip clip, AudioType sourceToUse = AudioType.None, bool wantToPitch = false, Transform worldPosition = null, bool attachToParent = false) {
        if (clip != null) {
            if (sourceToUse == AudioType.None || sourceToUse == AudioType.SFX) {
                int clipAlreadyInUse = 0;
                float pitch = wantToPitch ? UnityEngine.Random.Range(0.1f, 3f) : 1f;

                // Object pooling through some AudioSources to find an unused one
                foreach (var source in _soundEffectSources) {
                    if(source != null) {
                        if (source.clip == clip) {
                            clipAlreadyInUse++;
                        }

                        if (!source.gameObject.activeInHierarchy) { //if a source is inactive, use it.
                            if (worldPosition != null) {
                                if (attachToParent) {
                                    source.gameObject.transform.SetParent(worldPosition);
                                }

                                source.spatialBlend = 1f;
                                source.gameObject.transform.position = worldPosition.position;                                                                    
                            }

                            // Load the AudioClip and play it, cleaning itself up once it has completed
                            source.enabled = true;
                            source.pitch = pitch;
                            source.clip = clip;

                            if (source.gameObject.activeInHierarchy) {
                                source.Play();
                            }
                            else {
                                source.gameObject.SetActive(true);
                            }

                            StartCoroutine(SourceCleanup(source, false, attachToParent));
                            return;
                        }
                    }                    
                }

                // If all AudioSources are in use, we need to create a new source to play the clip with
                // We'll store the new source with the others to use later if necessary
                AudioSource newSource = CreateNewSource(_sfxParent);
                _soundEffectSources.Add(newSource);

                if (worldPosition != null) {
                    if (attachToParent) {
                        newSource.gameObject.transform.SetParent(worldPosition);
                    }

                    newSource.spatialBlend = 1f;
                    newSource.gameObject.transform.position = worldPosition.position;
                }

                newSource.enabled = true;
                newSource.pitch = pitch;
                newSource.clip = clip;

                if (newSource.gameObject.activeInHierarchy) {
                    newSource.Play();
                }
                else {
                    newSource.gameObject.SetActive(true);
                }

                StartCoroutine(SourceCleanup(newSource));              
            }
            else if (sourceToUse == AudioType.Dialogue) {
                _currentDialogueSource.clip = clip;

                if (!_currentDialogueSource.gameObject.activeInHierarchy) {
                    _currentDialogueSource.gameObject.SetActive(true);
                }
                else {
                    _currentDialogueSource.Play();
                }

                StartCoroutine(SourceCleanup(_currentDialogueSource));
            }
        }
    }

    public void SwapSnapshots(SwapAudioScriptableObject so, float delay = 1f) {
        if(_currentMusicSnapshot == _musicSnapshots[0]) {
            _currentMusicSnapshot = _musicSnapshots[1];

            _musicSources[1].clip = so.Music;
            _ambienceSources[1].clip = so.Ambience;

            _musicSnapshots[1].TransitionTo(delay);
            _musicSources[1].Play();
            _ambienceSnapshots[1].TransitionTo(delay);
            _ambienceSources[1].Play();

            _musicSources[0].Stop();
            _ambienceSources[0].Stop();
            _musicSources[0].clip = null;
            _ambienceSources[0].clip = null;
        }
        else {
            _currentMusicSnapshot = _musicSnapshots[0];

            _musicSources[0].clip = so.Music;
            _ambienceSources[0].clip = so.Ambience;

            _musicSnapshots[0].TransitionTo(delay);
            _musicSources[0].Play();
            _ambienceSnapshots[0].TransitionTo(delay);
            _ambienceSources[0].Play();

            _musicSources[1].Stop();
            _ambienceSources[1].Stop();
            _musicSources[1].clip = null;
            _ambienceSources[1].clip = null;
        }
    }

    public void SpeakVoiceLine(DialogueScriptableObject dialogue) {
        UIManager.Instance.SpeakVoiceLine(dialogue);

        if (!_currentDialogueSource.gameObject.activeInHierarchy || _currentDialogueSource.clip != dialogue.DialogueClip) {
            _currentDialogueSource = dialogue.Actor == Actor.Radio ? _dialogueSources[0] : _dialogueSources[1];
            _currentDialogueSource.clip = dialogue.DialogueClip;

            if (!_currentDialogueSource.gameObject.activeInHierarchy) {
                _currentDialogueSource.gameObject.SetActive(true);
            }
            else {
                _currentDialogueSource.Play();
            }

            StartCoroutine(SourceCleanup(_currentDialogueSource, true));
        }
    }

    public bool IsDialoguePlaying() {
        if (_currentDialogueSource.gameObject.activeInHierarchy) {
            return _currentDialogueSource.isPlaying;
        }

        return false;
    }

    public void PauseDialogue(bool state) {
            _musicSources[0].ignoreListenerPause = state;
            _musicSources[1].ignoreListenerPause = state;
            _ambienceSources[0].ignoreListenerPause = state;
            _ambienceSources[1].ignoreListenerPause = state;
            
            if (state) {
                _currentDialogueSource.Pause();
            }
            else {
                _currentDialogueSource.UnPause();
            }

            AudioListener.pause = state;
    }

    public void SetVolume(string name, float value, bool updatePref = true) {
        string prefix = name.Substring(0, name.IndexOf("_"));
        AudioType type = (AudioType)Enum.Parse(typeof(AudioType), prefix);        

        switch (type) {
            case AudioType.Master:               
            case AudioType.Music:
            case AudioType.Ambience:
            case AudioType.Dialogue:
            case AudioType.SFX:
                _gameMixer.SetFloat(type.ToString(), value);
                break;
            case AudioType.None:
            default:
                break;
        }

        if (updatePref) {
            PreferenceManager.Instance.UpdatePreferences(name, value);
        }
    }

    public void StartGame() {
        SwapSnapshots(_inGameAudio, 1f);
    }

    public void BackToMain() {
        SwapSnapshots(_menuAudio, 1f);
    }

    public void Reset(bool resetToMain = false) { 
        if (_currentDialogueSource != null) {
            _currentDialogueSource.Stop();
            _currentDialogueSource.clip = null;
        }        

        if (resetToMain && _menuAudio != null) {
            SwapSnapshots(_menuAudio, 1f);
        }

        AudioListener.pause = false;
    }

    public void ReturnSourceToParent(AudioSource[] sources) {
        foreach (var source in sources) {
            source.transform.parent = _sfxParent;
            StartCoroutine(SourceCleanup(source));
        }
    }
    public void ReturnSourceToParent(AudioSource source) {
        source.transform.parent = _sfxParent;
        StartCoroutine(SourceCleanup(source));
    }
    #endregion    
}
