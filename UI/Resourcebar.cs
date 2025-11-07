using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Resourcebar : MonoBehaviour
{
    public Slider ResourceSlider;
    public Slider EaseResourceSlider;
    public float MaxResource = 100f;
    public float Resource;
    public float LerpSpeed = 0.05f;

    public float MaxSliderValue = 100f;
    public float MinSliderValue = 0f;

    void Awake()
    {
        // Auto-assign if not already set
        if (ResourceSlider == null || EaseResourceSlider == null)
        {
            Slider[] sliders = GetComponentsInChildren<Slider>();
            if (sliders.Length >= 2)
            {
                ResourceSlider = sliders[0];
                EaseResourceSlider = sliders[1];
            }
            else
            {
                Debug.LogWarning("Not enough sliders found in Resourcebar!");
            }
        }
    }

    void Start()
    {
        Resource = MaxResource;
        SetSliderRange(MinSliderValue, MaxSliderValue);
        UpdateSliderValues();
    }

    void Update()
    {
        UpdateSliderValues();
        EaseResourceSlider.value = Mathf.Lerp(EaseResourceSlider.value, Resource, LerpSpeed);
    }

    public void SetResource(float value)
    {
        Resource = Mathf.Clamp(value, MinSliderValue, MaxSliderValue);
    }

    public void SetMaxResource(float maxValue)
    {
        MaxResource = maxValue;
        MaxSliderValue = maxValue;
        Resource = Mathf.Clamp(Resource, MinSliderValue, MaxSliderValue);
    }

    void UpdateSliderValues()
    {
        ResourceSlider.value = Resource;
    }

    public void SetSliderRange(float min, float max)
    {
        MinSliderValue = min;
        MaxSliderValue = max;

        if (ResourceSlider != null)
        {
            ResourceSlider.minValue = min;
            ResourceSlider.maxValue = max;
        }

        if (EaseResourceSlider != null)
        {
            EaseResourceSlider.minValue = min;
            EaseResourceSlider.maxValue = max;
        }
    }

    public float GetCurrentResource() => Resource;
    public float GetMaxResource() => MaxResource;
}
