using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public class ColorBlindMode : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;

    [Header("ColorBlind Settings")]
    public ColorBlindModes colorBlindMode = ColorBlindModes.Normal;

    private ColorBlindModes oldColorBlindMode;
    [HideInInspector] public ChannelMixer channelMixer;

    private void Start()
    {
        oldColorBlindMode = colorBlindMode;

        // Ensure the Volume is assigned
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume is not assigned!");
            return;
        }

        GetChannelMixer();
    }

    public void GetChannelMixer()
    {
        // Get the Volume Profile
        VolumeProfile profile = globalVolume.profile;

        // Check if the Channel Mixer effect exists in the profile
        if (!profile.TryGet<ChannelMixer>(out channelMixer))
        {
            Debug.LogWarning($"A 'Channel Mixer' on 'Volume' : {globalVolume}, dose not exist. Adding one...");
            channelMixer = profile.Add<ChannelMixer>(true); // Add a new Channel Mixer
        }
    }

    private void Update()
    {
        if(oldColorBlindMode != colorBlindMode)
        {
            oldColorBlindMode = colorBlindMode;
            UpdateColorBlindMode();
        }
    }

    public void UpdateColorBlindMode()
    {
        //set channel mix settings to fit selected colorblindness
        if(colorBlindMode == ColorBlindModes.Normal)
        {
            SetChannelMixerValues(
                new Vector3(100, 0, 0),
                new Vector3(0, 100, 0),
                new Vector3(0, 0, 100)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Achromatomaly)
        {
            SetChannelMixerValues(
                new Vector3(61.8f, 32, 6.2f),
                new Vector3(16.3f, 77.5f, 6.2f),
                new Vector3(16.3f, 32, 51.6f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Achromatopsia)
        {
            SetChannelMixerValues(
                new Vector3(29.9f, 58.7f, 11.4f),
                new Vector3(29.9f, 58.7f, 11.4f),
                new Vector3(29.9f, 58.7f, 11.4f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Deuteranomaly)
        {
            SetChannelMixerValues(
                new Vector3(80, 20, 0),
                new Vector3(25.833f, 74.167f, 0),
                new Vector3(0, 14.167f, 85.833f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Deuteranopia)
        {
            SetChannelMixerValues(
                new Vector3(62.5f, 37.5f, 0f),
                new Vector3(70f, 30f, 0),
                new Vector3(0, 30f, 70f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Protanomaly)
        {
            SetChannelMixerValues(
                new Vector3(81.667f, 18.333f, 0),
                new Vector3(33.333f, 66.667f, 0f),
                new Vector3(0f, 12.5f, 87.5f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Protanopia)
        {
            SetChannelMixerValues(
                new Vector3(56.667f, 43.333f, 0),
                new Vector3(55.833f, 44.167f, 0),
                new Vector3(0, 24.167f, 75.833f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Tritanomaly)
        {
            SetChannelMixerValues(
                new Vector3(96.667f, 3.333f, 0),
                new Vector3(0f, 73.333f, 26.667f),
                new Vector3(0, 18.333f, 81.667f)
                );
        }
        else if (colorBlindMode == ColorBlindModes.Tritanopia)
        {
            SetChannelMixerValues(
                new Vector3(95, 5, 0),
                new Vector3(0, 43.333f, 56.667f),
                new Vector3(0, 47.5f, 52.5f)
                );
        }
    }

    public void SetChannelMixerValues(Vector3 red, Vector3 green, Vector3 blue)
    {
        if (channelMixer == null)
        {
            Debug.LogWarning("Channel Mixer is not available. Make sure it's added to the Volume Profile.");
            return;
        }

        // Set the red, green, and blue channels
        channelMixer.redOutRedIn.value = red.x;
        channelMixer.redOutGreenIn.value = red.y;
        channelMixer.redOutBlueIn.value = red.z;

        channelMixer.greenOutRedIn.value = green.x;
        channelMixer.greenOutGreenIn.value = green.y;
        channelMixer.greenOutBlueIn.value = green.z;

        channelMixer.blueOutRedIn.value = blue.x;
        channelMixer.blueOutGreenIn.value = blue.y;
        channelMixer.blueOutBlueIn.value = blue.z;

        Debug.Log("Channel Mixer values updated!");
    }

}

