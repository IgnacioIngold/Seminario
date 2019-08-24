using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public Image life;
    public Image stamina;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    public void UpdateHeathBar(float currentHealth, float maxHealth)
    {
        var pct = currentHealth / maxHealth;

        life.fillAmount = pct;
    }

    public void UpdateStamina(float Stamina, float MaxStamina)
    {
        var pct = Stamina / MaxStamina;

        stamina.fillAmount = pct;
    }
}
