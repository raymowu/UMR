using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MobHealthUI : MonoBehaviour
{
    public Canvas healthCanvas;
    public Slider healthSlider3D;
    public TMP_Text healthSlider3DText;
    
    void Update()
    {
        healthCanvas.gameObject.SetActive(GetComponent<MobPrefab>().IsDead ? false : true);
    }

    public void Start3DSlider(float maxValue)
    {
        healthSlider3D.maxValue = maxValue;
        healthSlider3D.value = maxValue;
        healthSlider3DText.text = maxValue + "/" + maxValue;
    }

    public void Update3DSlider(float maxValue, float value)
    {
        healthSlider3D.maxValue = maxValue;
        healthSlider3D.value = value;
        healthSlider3DText.text = value + "/" + maxValue;
    }
}
