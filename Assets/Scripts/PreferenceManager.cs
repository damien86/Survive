using UnityEngine;
using UnityEngine.InputSystem;

public class PreferenceManager : MonoBehaviour
{
    #region Variables
    // All public functions in this class can be accessed by typing PreferenceManager.Instance.<nameOfFunction> from any class
    public static PreferenceManager Instance;

    [SerializeField] private bool _debugMode = false;
    [SerializeField] private FloatPref[] _floatPrefs;
    [SerializeField] private BoolPref[] _boolPrefs;
    [SerializeField] private IntPref[] _intPrefs;
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

        InitPrefs();
    }

    private void Update() {
        //DEBUG
        if (_debugMode) {
            if (Keyboard.current.deleteKey.wasPressedThisFrame) {
                ResetPreferences();
            }
        }        
    }

    private void OnApplicationQuit() {
        PlayerPrefs.Save();
    }
    #endregion

    #region Private Methods
    private void ResetPreferences() {
        Debug.Log("Deleting all preferences");
        PlayerPrefs.DeleteAll();
        InitPrefs();
    }

    private void InitPrefs() {
        foreach (var pref in _floatPrefs) {
            if (!PlayerPrefs.HasKey(pref.Name)) {
                PlayerPrefs.SetFloat(pref.Name, pref.DefaultValue);
            }
        }

        foreach (var pref in _boolPrefs) {
            if (!PlayerPrefs.HasKey(pref.Name)) {
                PlayerPrefs.SetInt(pref.Name, pref.DefaultValue ? 1 : 0);
            }
        }

        foreach (var pref in _intPrefs) {
            if (!PlayerPrefs.HasKey(pref.Name)) {
                PlayerPrefs.SetInt(pref.Name, pref.DefaultValue);
            }
        }

        PlayerPrefs.Save();
    }
    #endregion

    #region Public Methods
    public void UpdatePreferences(string name, float value) {
        // Slider UI gameObjects should be named something like "Music_slider" and "Ambience_slider", the first word matching the PreferenceManager array strings
        string prefix = name.Substring(0, name.IndexOf("_"));

        if (PlayerPrefs.HasKey(prefix)) {
            PlayerPrefs.SetFloat(prefix, value);
        }
    }

    public void UpdatePreferences(string name, bool value) {        
        string prefix = name.Substring(0, name.IndexOf("_"));

        if (PlayerPrefs.HasKey(prefix)) {
            PlayerPrefs.SetInt(prefix, value ? 1 : 0);
        }
    }

    public void UpdatePreferences(string name, int value) {
        string prefix = name.Substring(0, name.IndexOf("_"));

        if (PlayerPrefs.HasKey(prefix)) {
            PlayerPrefs.SetInt(prefix, value);
        }
    }    

    public bool GetBoolPrefDefault(string name) {
        foreach (var pref in _boolPrefs) {
            if (pref.Name == name) {
                return pref.DefaultValue;
            }
        }

        return false;
    }

    public float GetFloatPrefDefault(string name) {
        foreach (var pref in _floatPrefs) {
            if (pref.Name == name) {
                return pref.DefaultValue;
            }
        }

        return 0f;
    }

    public int GetIntPrefDefault(string name) {
        foreach (var pref in _intPrefs) {
            if(pref.Name == name) {
                return pref.DefaultValue;
            }
        }

        return 0;
    }

    public float GetFloatPref(string name) {
        return PlayerPrefs.GetFloat(name, PreferenceManager.Instance.GetFloatPrefDefault(name));
    }

    public bool GetBoolPref(string name) {
        return PlayerPrefs.GetInt(name, PreferenceManager.Instance.GetBoolPrefDefault(name) == true ? 1 : 0) == 1 ? true : false;
    }

    public int GetIntPref(string name) {
        return PlayerPrefs.GetInt(name, PreferenceManager.Instance.GetIntPrefDefault(name));
    }
    #endregion
}

[System.Serializable]
public struct BoolPref {
    public string Name;
    public bool DefaultValue;

    public BoolPref(string name, bool defaultValue) {
        this.Name = name;
        this.DefaultValue = defaultValue;
    }
}

[System.Serializable]
public struct FloatPref {
    public string Name;
    public float DefaultValue;

    public FloatPref(string name, float defaultValue) {
        this.Name = name;
        this.DefaultValue = defaultValue;
    }
}

[System.Serializable]
public class IntPref {
    public string Name;
    public int DefaultValue;

    public IntPref(string name, int defaultValue) {
        this.Name = name;
        this.DefaultValue = defaultValue;
    }
}