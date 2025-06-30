using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuScript : MonoBehaviour
{
    public Canvas MainCanvas;
    public Canvas SettingsCanvas;

    public AudioMixer audioMixer;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public Animator transition;

    public float transitionTime = 1f;

    public Vector2 referenceResolution = new Vector2(1920, 1080);

    void Start()
    {
        Time.timeScale = 1.0f;
        SetupCanvasScaler(MainCanvas);
        SetupCanvasScaler(SettingsCanvas);

        // Initialize sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // Set sliders to current volume levels (assume default is 0 dB)
        masterSlider.value = 1;
        musicSlider.value = 1;
        sfxSlider.value = 1;
    }

    void SetupCanvasScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }

    public void StartGame()
    {
        StartCoroutine(LoadStart());
    }

    IEnumerator LoadStart()
    {
        transition.SetTrigger("Fade");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(1);
    }

    public void DisableMainCanvas()
    {
        MainCanvas.gameObject.SetActive(false);
        SettingsCanvas.gameObject.SetActive(true);
    }

    public void DisableSettingsCanvas()
    {
        MainCanvas.gameObject.SetActive(true);
        SettingsCanvas.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}


