using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public static Dialogue Instance;
    private Queue<DialogueScriptableObject> _dialogueQueue = new Queue<DialogueScriptableObject>();    

    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else if (Instance != null) {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {        
        if (_dialogueQueue.Count != 0 && !AudioManager.Instance.IsDialoguePlaying()) {
            DialogueScriptableObject dialogue = _dialogueQueue.Dequeue();
            AudioManager.Instance.SpeakVoiceLine(dialogue);
        }
    }

    public void AddToQueue(DialogueScriptableObject dialogue) {
        _dialogueQueue.Enqueue(dialogue);        
    }
}
