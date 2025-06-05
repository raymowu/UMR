using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;
    void Start()
    {
        StartCoroutine(AddGoldOverTime());   
    }

    IEnumerator AddGoldOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GameManager.Instance.AddGoldCount(gameObject, 1);
        }
    }
    public void UpdateGold(int goldCount)
    {
        goldText.text = goldCount + "";
    }
}
