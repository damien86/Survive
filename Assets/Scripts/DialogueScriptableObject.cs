using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Data/Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    public Actor Actor;
    [TextArea] public string Transcript;
    public AudioClip DialogueClip;

}

public enum Actor {
    Radio,
    Voices
}
