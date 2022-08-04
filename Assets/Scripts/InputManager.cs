using UnityEngine;
using UnityEngine.InputSystem;

public enum ActionMapName { Player, UI };

public class InputManager : MonoBehaviour
{
    #region Variables
    public static InputManager Instance;

    public static Vector2 MoveDirection { get; private set; }
    public static Vector2 LookDirection { get; private set; }
    public static bool IsSprintHeld { get; private set; }
    public static bool IsFireHeld { get; private set; }    

    public InputActionMap currentActionMap {
        get {
            return _currentActionMap;
        }
        set {
            var oldMap = _currentActionMap;
            _currentActionMap = null;
            oldMap?.Disable();

            _currentActionMap = value;
            _currentActionMap?.Enable();
        }
    }

    private InputActionMap _currentActionMap = null;
    private Controls _controls = null;
    [SerializeField] private ActionMapName _startingActionMap = ActionMapName.Player;

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
        
        InitInput();
    }

    public void Update() {
        MoveDirection = _controls.Player.Move.ReadValue<Vector2>();
        LookDirection = _controls.Player.Look.ReadValue<Vector2>();
    }

    private void OnEnable() {
        if (_controls != null) {
            _controls.Player.Sprint.started += _ => IsSprintHeld = true;
            _controls.Player.Sprint.canceled += _ => IsSprintHeld = false;

            _controls.Player.Fire.started += _ => IsFireHeld = true;
            _controls.Player.Fire.canceled += _ => IsFireHeld = false;

            _controls.Player.Pause.started += _ => UIManager.Instance.ChangeUITypeOnPausePress();
            _controls.UI.Pause.started += _ => UIManager.Instance.ChangeUITypeOnPausePress();

            _controls.Enable();
        }        
    }

    private void OnDisable() {
        if (_controls != null) {
            _controls.Disable();
            _controls.Player.Pause.started -= _ => UIManager.Instance.ChangeUITypeOnPausePress();
            _controls.UI.Pause.started -= _ => UIManager.Instance.ChangeUITypeOnPausePress();

            _controls.Player.Sprint.started -= _ => IsSprintHeld = true;
            _controls.Player.Sprint.canceled -= _ => IsSprintHeld = false;

            _controls.Player.Fire.started -= _ => IsFireHeld = true;
            _controls.Player.Fire.canceled -= _ => IsFireHeld = false;
        }        
    }
    #endregion

    #region Private Methods
    private void InitInput() {
        _controls = new Controls();
        _currentActionMap = _controls.asset.FindActionMap(_startingActionMap.ToString());
        SwitchCurrentActionMap(_startingActionMap.ToString());

        MoveDirection = Vector2.zero;
        IsSprintHeld = false;
        IsFireHeld = false;
    }

    #endregion

    #region Public Methods
    public void SwitchCurrentActionMap(string mapNameOrId) {
        var actionMap = _controls.asset.FindActionMap(mapNameOrId);

        if (actionMap == null) {
            Debug.LogError($"Cannot find action map '{mapNameOrId}' in actions '{_controls.asset}'", this);
            return;
        }

        currentActionMap = actionMap;
    }
    #endregion
}
