using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    Image _life;

    // Start is called before the first frame update
    void Awake()
    {
        _life = GetComponent<Image>();
    }

    // Update is called once per frame
    public void UpdateHeathBar(float currentHealth, float maxHealth)
    {
        var pct = currentHealth / maxHealth;

        _life.fillAmount = pct;
    }
}
