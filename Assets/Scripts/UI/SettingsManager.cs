using MoreMountains.Tools;
using ProPixelizer;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    [Header("Volume")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("PixelSize")]
    [SerializeField] private Slider pixelSizeSlider;
    [SerializeField] private Material[] pixelMaterials;
    [Range(1, 5)][SerializeField] private int currentPixelSize = 3;

    [Header("ColorblindMode")]
    [SerializeField] private TMP_Dropdown colorBlindDropdown;
    [SerializeField] private ColorBlindMode colorBlindController;

    [Header("Refs")]
    [SerializeField] private MMSoundManager soundManger;

    private void Start()
    {
        //Call to make sure everythings set
        ChangeGraphicsQuality();
        ChangeMasterVolume();
        ChangePixelSize();
    }

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMasterVolume()
    {
        soundManger.SetVolumeMaster(masterVolumeSlider.value);
    }

    public void ChangePixelSize()
    {
        foreach(var material in pixelMaterials)
        {
            currentPixelSize = Mathf.Clamp((int)(pixelSizeSlider.value), 1, 5);

            //check if has propperty and set it
            if (material.HasProperty("_PixelSize"))
            {
                material.SetFloat("_PixelSize", currentPixelSize);
            }
            else
            {
                Debug.LogWarning($"Material does not have a property named {"_PixelSize"}");
            }
        }
    }

    public void ChangeColorBlindMode()
    {
        string value = colorBlindDropdown.value.ToString();
        if(!Enum.TryParse(value, out colorBlindController.colorBlindMode))
        {
            Debug.LogError($"{value} is not an option");
        }
        Debug.Log(colorBlindController.colorBlindMode);
        colorBlindController.UpdateColorBlindMode();
    }
}
