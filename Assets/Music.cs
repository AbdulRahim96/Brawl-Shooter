using UnityEngine;
using UnityEngine.UI;

public class Music : MonoBehaviour
{
    private AudioSource audioSource;
    public Toggle musicToggle;

    private void Awake()
    {
        audioSource  = GetComponent<AudioSource>();
        bool isOn = PlayerPrefsExtra.GetBool("music", true);
        audioSource.mute = !isOn;

        if(musicToggle != null)
        {
            musicToggle.isOn = !isOn;
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }
    }

    public void ToggleMusic(bool isOn)
    {
        audioSource.mute = isOn;
        PlayerPrefsExtra.SetBool("music", !isOn);
    }
}
