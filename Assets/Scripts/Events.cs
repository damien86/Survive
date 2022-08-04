public static class Events
{
    public delegate void HealthChange(float value);
    public static HealthChange OnHealthChange;

    public delegate void Hit();
    public static Hit OnHit;

    public delegate void Death();
    public static Death OnDeath;

    public delegate void BloomChange(float value);
    public static BloomChange OnBloomChange;

    public delegate void LookSensitivityChange(float value);
    public static LookSensitivityChange OnLookSensitivityChange;

    public delegate void InvertedControlsChange(bool state);
    public static InvertedControlsChange OnInvertedControlsChange;

    public delegate void CameraShakechange(bool state);
    public static CameraShakechange OnCameraShakeChange;

    public delegate void FOVChange(float value);
    public static FOVChange OnFOVChange;
}
