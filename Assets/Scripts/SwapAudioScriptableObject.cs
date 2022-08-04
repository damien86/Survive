using UnityEngine;

[CreateAssetMenu(fileName = "SwapAudio", menuName = "Data/SwapAudio")]
public class SwapAudioScriptableObject : ScriptableObject {
    public AudioClip Music = null;
    public AudioClip Ambience = null;
}
