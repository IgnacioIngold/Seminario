﻿using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public Image life;
    public Image stamina;

    public Image lifeBackGround;
    public Image staminaBackGround;

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

    public void FadeOut(float timeInSeconds)
    {
        life.CrossFadeAlpha(0, timeInSeconds, false);
        lifeBackGround.CrossFadeAlpha(0, timeInSeconds, false);

        stamina.CrossFadeAlpha(0, timeInSeconds, false);
        staminaBackGround.CrossFadeAlpha(0, timeInSeconds, false);
    }

    public void FadeIn(float timeInSeconds)
    {
        life.CrossFadeAlpha(1, timeInSeconds, false);
        lifeBackGround.CrossFadeAlpha(1, timeInSeconds, false);

        stamina.CrossFadeAlpha(1, timeInSeconds, false);
        staminaBackGround.CrossFadeAlpha(1, timeInSeconds, false);
    }

}
