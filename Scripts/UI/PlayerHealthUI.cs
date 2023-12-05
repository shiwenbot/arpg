using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    Image healthSlider;
    Image expSlider;

    private void Awake()
    {
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent <Image>();
    }

    private void Update()
    {
        updateHealth();
        updateExp();
    }

    void updateHealth()
    {
        float sliderPercent = (float)GameManager.Instance.characterStats.CurrentHealth / GameManager.Instance.characterStats.MaxHealth;
        healthSlider.fillAmount = sliderPercent;
    }
    
    void updateExp()
    {
        float sliderPercent = (float)GameManager.Instance.characterStats.characterData.currentExp / GameManager.Instance.characterStats.characterData.baseExp;
        expSlider.fillAmount = sliderPercent;
    }
}
