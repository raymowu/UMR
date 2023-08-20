using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Canvas healthCanvas;
    public Slider healthSlider3D;
    public Slider healthSlider2D;

    void Update()
    {
        healthCanvas.gameObject.SetActive(GetComponent<PlayerPrefab>().IsDead ? false : true);
    }

    public void Start3DSlider(float maxValue)
    {
        healthSlider3D.maxValue = maxValue;
        healthSlider3D.value = maxValue;
    }

    public void Start2DSlider(float maxValue)
    {
        healthSlider2D.maxValue = maxValue;
        healthSlider2D.value = maxValue;
    }

    public void Update3DSlider(float maxValue, float value)
    {
        healthSlider3D.maxValue = maxValue;
        healthSlider3D.value = value;
    }

    public void Update2DSlider(float maxValue, float value)
    {
        healthSlider2D.maxValue = maxValue;
        healthSlider2D.value = value;
    }
}
