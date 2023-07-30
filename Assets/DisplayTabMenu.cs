using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisplayTabMenu : MonoBehaviour
{
    public GameObject Panel;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Panel.gameObject.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            Panel.gameObject.SetActive(false);
        }
    }
}
