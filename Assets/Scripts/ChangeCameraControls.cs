using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ChangeCameraControls : MonoBehaviour
{
    [SerializeField] private float _intensity = 1f;
    [SerializeField] private float _shakeTime = 1f;

    private CinemachineVirtualCamera _cam = null;
    private CinemachinePOV _aimControls = null;
    private CinemachineBasicMultiChannelPerlin _shake = null;

    private bool _canCameraShake = false;
    private float _shakeTimer;
    private float _shakeTimerTotal;
    private float _startingIntensity;

    private void Start() {
        InitCam();
        Events.OnInvertedControlsChange += ChangeInvertedControls;
        Events.OnLookSensitivityChange += ChangeLookSensitity;
        Events.OnFOVChange += ChangeFOV;
        Events.OnCameraShakeChange += ChangeCameraShake;
        Events.OnHit += ShakeCamera;
    }

    private void OnDestroy() {
        Events.OnInvertedControlsChange -= ChangeInvertedControls;
        Events.OnLookSensitivityChange -= ChangeLookSensitity;
        Events.OnFOVChange -= ChangeFOV;
        Events.OnCameraShakeChange -= ChangeCameraShake;
    }

    private void Update() {
        if (_shakeTimer > 0) {
            _shakeTimer -= Time.deltaTime;
            if (_shakeTimer <= 0.0f) {
                _shake.m_AmplitudeGain = Mathf.Lerp(_startingIntensity, 0.0f, 1 - (_shakeTimer / _shakeTimerTotal));
            }
        }
    }

    private void InitCam() {
        _cam = GetComponent<CinemachineVirtualCamera>();
        _aimControls = _cam.GetCinemachineComponent<CinemachinePOV>();
        _shake = _cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        _aimControls.m_VerticalAxis.m_InvertInput = PreferenceManager.Instance.GetBoolPref("InvertedControls");

        float lookSensitivity = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
        _aimControls.m_VerticalAxis.m_MaxSpeed = lookSensitivity / 2f;
        _aimControls.m_HorizontalAxis.m_MaxSpeed = lookSensitivity;

        _cam.m_Lens.FieldOfView = PreferenceManager.Instance.GetFloatPref("FieldOfView");

        _canCameraShake = PreferenceManager.Instance.GetBoolPref("CameraShake");
    }

    private void ChangeInvertedControls(bool state) {
        _aimControls.m_VerticalAxis.m_InvertInput = state;
    }

    private void ChangeLookSensitity(float value) {
        _aimControls.m_VerticalAxis.m_MaxSpeed = value / 3f;
        _aimControls.m_HorizontalAxis.m_MaxSpeed = value;
    }

    private void ChangeFOV(float value) {
        _cam.m_Lens.FieldOfView = value;
    }

    private void ChangeCameraShake(bool state) {
        _canCameraShake = state;
    }

    private void ShakeCamera() {
        if (!_canCameraShake) {
            return;
        }

        _shake.ReSeed();
        _shake.m_AmplitudeGain = _intensity;

        _startingIntensity = _intensity;
        _shakeTimer = _shakeTime;
        _shakeTimerTotal = _shakeTime;
    }
}
