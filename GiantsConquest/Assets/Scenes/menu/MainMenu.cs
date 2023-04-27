using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{

    public Slider volumeSlider;
    public AudioListener al; // Add this line to reference the slider

    private void Start()
    {
        // Load the saved volume value, or set it to the default value (0.5) if it doesn't exist
        volumeSlider.value = PlayerPrefs.GetFloat("volume", 0.5f);

        // Apply the saved volume value to the AudioListener
        AudioListener.volume = volumeSlider.value;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void StartGame(){
        SceneManager.LoadScene("Game");
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("volume", volume);
    }
}
