using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public AudioClip music;
    
    void Start()
    {
        MusicManager.Instance.PlayLoop("bg1", music, 1);
    }
}
