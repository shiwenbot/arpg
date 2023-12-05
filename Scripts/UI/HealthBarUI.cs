using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    Transform prefabTransform;

    public Transform healthBarPoint;
    Image healthSlider;

    Transform cameraPos;

    CharacterStats characterStats;

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        characterStats.updateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        cameraPos = Camera.main.transform;

        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode == RenderMode.WorldSpace) 
            {
                prefabTransform = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = prefabTransform.GetChild(0).GetComponent<Image>();
                prefabTransform.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0) { Destroy(prefabTransform.gameObject); }
        prefabTransform.gameObject.SetActive(true);

        float sliderPercent = (float)currentHealth / maxHealth;       
        healthSlider.fillAmount = sliderPercent;
    }

    private void LateUpdate()
    {
        if(prefabTransform != null)
        {
            prefabTransform.position = healthBarPoint.position;
            prefabTransform.forward = -cameraPos.forward; // 保证血条一直朝着摄像机
        }
    }
}
