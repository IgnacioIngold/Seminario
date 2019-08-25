using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public Image life;
    public Image stamina;

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
