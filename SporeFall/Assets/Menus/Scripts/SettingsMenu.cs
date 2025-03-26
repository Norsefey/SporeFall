using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Buttons")]
    [SerializeField] private GameObject masterVolume;
    [SerializeField] private Button masterButton;
    [SerializeField] private GameObject musicVolume;
    [SerializeField] private Button musicButton;
    [SerializeField] private GameObject sfxVolume;
    [SerializeField] private Button sfxButton;
    [SerializeField] private GameObject aimSensitivity;
    [SerializeField] private Button aimButton;

    [Header("Sliders")]
    [SerializeField] private GameObject masterSlider;
    [SerializeField] private GameObject musicSlider;
    [SerializeField] private GameObject sfxSlider;
    [SerializeField] private GameObject sensitivitySlider;

    private bool isSliderSelected = false;
    private bool canSelect = false;
    private GameObject lastSelected;
    private bool isGamepadConnected = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Gamepad.all.Count == 0)
        {
            isGamepadConnected = false;
            masterButton.interactable = false;
            musicButton.interactable = false;
            sfxButton.interactable = false;
            aimButton.interactable = false;
            Debug.Log("No gamepad on startup");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (isSliderSelected)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame && canSelect)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(lastSelected);
                Debug.Log("Returning to button");
            }
        }

        if (Gamepad.all.Count > 0 && isGamepadConnected == false)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(masterVolume);
            isGamepadConnected = true;
            masterButton.interactable = true;
            musicButton.interactable = true;
            sfxButton.interactable = true;
            aimButton.interactable = true;
            Debug.Log("Gamepad connected");
        }

        //this one doesn't work right now
        if (Gamepad.all.Count == 0 && isGamepadConnected == true)
        {
            EventSystem.current.SetSelectedGameObject(null);
            isGamepadConnected = false;
            masterButton.interactable = false;
            musicButton.interactable = false;
            sfxButton.interactable = false;
            aimButton.interactable = false;
            Debug.Log("Gamepad disconnected");
        }
    }

    public void SelectMasterVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(masterSlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = masterVolume;
        StartCoroutine(SelectionCooldown());
        Debug.Log("Master volume selected");
    }

    public void SelectMusicVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(musicSlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = musicVolume;
        StartCoroutine(SelectionCooldown());
    }

    public void SelectSFXVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sfxSlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = sfxVolume;
        StartCoroutine(SelectionCooldown());
    }

    public void SelectAimSensitivity()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sensitivitySlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = aimSensitivity;
        StartCoroutine(SelectionCooldown());
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log(volume);
        audioMixer.SetFloat("masterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        Debug.Log(volume);
        audioMixer.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        Debug.Log(volume);
        audioMixer.SetFloat("sfxVolume", volume);
    }

    IEnumerator SelectionCooldown()
    {
        yield return new WaitForSeconds(.1f);
        canSelect = true;
        Debug.Log("Can select slider level");
    }
}
