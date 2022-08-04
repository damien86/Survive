using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class ChangeVolume : MonoBehaviour
{
    private Volume _volume;
    private VolumeProfile _profile;
    private Bloom _bloom;

    private void Start() {
        InitBloom();
        Events.OnBloomChange += ModifyBloomValue;
    }

    private void OnDestroy() {
        Events.OnBloomChange -= ModifyBloomValue;
    }

    private void InitBloom() {
        _volume = GetComponent<Volume>();
        _profile = _volume.sharedProfile;

        if (!_profile.TryGet(out _bloom)) {
            _bloom = _profile.Add<Bloom>(false);
        }

        _bloom.active = true;
        _bloom.intensity.overrideState = true;
        _bloom.intensity.value = PlayerPrefs.GetFloat("Bloom", PreferenceManager.Instance.GetFloatPrefDefault("Bloom"));
    }

    private void ModifyBloomValue(float value) {
        value = Mathf.Clamp(value, 0f, 1f);
        _bloom.intensity.value = value;
    }
}
