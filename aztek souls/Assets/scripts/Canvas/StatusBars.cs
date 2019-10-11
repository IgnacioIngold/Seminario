using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusBars : MonoBehaviour
{
    public enum InfoComponent
        {
            HealthBar,
            StaminaBar,
            BloodStat
        }
    public enum FadeType : int
        {
            FadeOut = 0,
            FadeIn = 1
        }

    public Image life;
    public Image stamina;

    public Image lifeBackGround;
    public Image staminaBackGround;

    public TextMeshProUGUI BloodAmmount;
    public Image BloodAmmountBackGround;

    public bool healthBarIsVisible { get => life.color.a == 1; }
    public bool staminaBarIsVisible { get => stamina.color.a == 1; }
    public bool BloodAmmountIsVisible { get => BloodAmmountBackGround.color.a == 1; }

    bool _healthIsFadingOut = false;
    bool _staminaIsFadingOut = false;
    bool _bloodIsFadingOut = false;

    float currentHealthFadeTime = 0;
    float currentStaminaFadeTime = 0;
    float currentBloodFadeTime = 0;

    Func<FadeType, float, float> fadeOperation = (opType, Ammount) => opType == FadeType.FadeIn ? Ammount : -Ammount;

    /// <summary>
    /// Actualiza el Display de la cantidad de vida.
    /// </summary>
    /// <param name="currentHealth">Cantidad de vida actual.</param>
    /// <param name="maxHealth">Cantidad de vida Máxima.</param>
    public void m_UpdateHeathBar(float currentHealth, float maxHealth)
    {
        if (_healthIsFadingOut) StopCoroutine("DelayedFadeHealthBar");
        if (!healthBarIsVisible)
            m_SetComponentAlpha(InfoComponent.HealthBar, 1);

        var pct = currentHealth / maxHealth;

        life.fillAmount = pct;
    }
    /// <summary>
    /// Actualiza el Display de la cantidad de Estamina.
    /// </summary>
    /// <param name="Stamina">Cantidad actual de la Estamina.</param>
    /// <param name="MaxStamina">Cantidad Máxima de la Estamina.</param>
    public void m_UpdateStamina(float Stamina, float MaxStamina)
    {

        if (!staminaBarIsVisible)
            StartCoroutine(FadeStamina(FadeType.FadeIn, 1));
        else
        if (_staminaIsFadingOut)
        {
            StopCoroutine(FadeStamina(FadeType.FadeOut, currentStaminaFadeTime));
            m_SetComponentAlpha(InfoComponent.StaminaBar, 1);
            _staminaIsFadingOut = false;
        }

        var pct = Stamina / MaxStamina;
        stamina.fillAmount = pct;
    }
    /// <summary>
    /// Actualiza el Display de la cantidad de Sangre Acumulada.
    /// </summary>
    /// <param name="Ammount"></param>
    public void m_UpdateBloodAmmount(int Ammount)
    {
        if (_bloodIsFadingOut) StopCoroutine("DelayedFadeBlood");
        if (!BloodAmmountIsVisible)
            m_SetComponentAlpha(InfoComponent.BloodStat, 1);

        BloodAmmount.text = Ammount.ToString();
    }
    /// <summary>
    /// Muestra u Oculta un componente específico en x cantidad de Tiempo.
    /// </summary>
    /// <param name="ComponentType"></param>
    /// <param name="Action"></param>
    /// <param name="TimeInSeconds"></param>
    public void m_Fade(InfoComponent ComponentType, FadeType Action, float TimeInSeconds)
    {
        if (TimeInSeconds <= 0)
        {
            m_SetComponentAlpha(ComponentType, Action == FadeType.FadeIn ? 1 : 0);
            return;
        }

        switch (ComponentType)
        {
            case InfoComponent.HealthBar:
                if (!_healthIsFadingOut) StartCoroutine(FadeHealth(Action, TimeInSeconds));
                break;
            case InfoComponent.StaminaBar:
                if (!_staminaIsFadingOut) StartCoroutine(FadeStamina(Action, TimeInSeconds));
                break;
            case InfoComponent.BloodStat:
                if (!_bloodIsFadingOut) StartCoroutine(FadeBlood(Action, TimeInSeconds));
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Realiza un Fade luego de que se cumpla un Delay
    /// </summary>
    /// <param name="component">El tipo de componente cuyo Display queremos modificar.</param>
    /// <param name="Action">La acción a realizar.</param>
    /// <param name="DelayTime">El tiempo que pasa antes de realizar la acción.</param>
    /// <param name="FadeTime">El tiempo que dura la acción.</param>
    public void m_DelayedFade(InfoComponent component, FadeType Action, float DelayTime, float FadeTime)
    {
        switch (component)
        {
            case InfoComponent.HealthBar:
                if (!_healthIsFadingOut)
                    StartCoroutine(DelayedFadeHealthBar(Action, DelayTime, FadeTime));
                break;
            case InfoComponent.StaminaBar:
                if (!_staminaIsFadingOut)
                    StartCoroutine(DelayedFadeStaminaBar(Action, DelayTime, FadeTime));
                break;
            case InfoComponent.BloodStat:
                if (!_bloodIsFadingOut)
                    StartCoroutine(DelayedFadeBlood(Action, DelayTime, FadeTime));
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Revela u Oculta todos los componentes en un lapso de tiempo.
    /// </summary>
    /// <param name="timeInSeconds"></param>
    public void m_FadeAll(FadeType Action, float timeInSeconds)
    {
        if (!_healthIsFadingOut) StartCoroutine(FadeHealth(Action, timeInSeconds));
        if (!_staminaIsFadingOut) StartCoroutine(FadeStamina(Action, timeInSeconds));
        if (!_bloodIsFadingOut) StartCoroutine(FadeBlood(Action, timeInSeconds));
    }
    /// <summary>
    /// Prende Inmediatamente todos los componentes.
    /// </summary>
    public void m_TurnOnAll()
    {
        m_SetComponentAlpha(InfoComponent.HealthBar, 1);
        m_SetComponentAlpha(InfoComponent.StaminaBar, 1);
        m_SetComponentAlpha(InfoComponent.BloodStat, 1);
    }
    /// <summary>
    /// Apaga inmediatamente todos los componentes.
    /// </summary>
    public void m_TurnOffAll()
    {
        m_SetComponentAlpha(InfoComponent.HealthBar, 0);
        m_SetComponentAlpha(InfoComponent.StaminaBar, 0);
        m_SetComponentAlpha(InfoComponent.BloodStat, 0);
    }
    /// <summary>
    /// Permite setear directamente el valor del canal Alpha de un Componente.
    /// </summary>
    /// <param name="component">Tipo de componente</param>
    /// <param name="AlphaValue">Nuevo valor del Canal Alpha</param>
    public void m_SetComponentAlpha(InfoComponent component, float AlphaValue)
    {
        switch (component)
        {
            case InfoComponent.HealthBar:
                var lifeColor = life.color;
                var lifeBackGroundColor = lifeBackGround.color;

                lifeColor.a = AlphaValue;
                lifeBackGroundColor.a = AlphaValue;

                life.color = lifeColor;
                lifeBackGround.color = lifeBackGroundColor;
                break;
            case InfoComponent.StaminaBar:
                var staminaColor = stamina.color;
                var staminaBackGroundColor = staminaBackGround.color;

                staminaColor.a = AlphaValue;
                staminaBackGroundColor.a = AlphaValue;

                stamina.color = staminaColor;
                staminaBackGround.color = staminaBackGroundColor;
                break;
            case InfoComponent.BloodStat:
                var bloodColor = BloodAmmount.color;
                var bloodBackGroundColor = BloodAmmountBackGround.color;

                bloodColor.a = AlphaValue;
                bloodBackGroundColor.a = AlphaValue;

                BloodAmmount.color = bloodColor;
                BloodAmmountBackGround.color = bloodBackGroundColor;
                break;
            default:
                break;
        }
    }

    //============================================ CORRUTINAS ==========================================================================

    IEnumerator FadeHealth(FadeType fadeType, float FadeTime)
    {
        _healthIsFadingOut = true;
        currentHealthFadeTime = FadeTime;

        var lifeColor = life.color;
        var lifeBackGroundColor = lifeBackGround.color;

        float remainigFadeTime = FadeTime;
        float fadeAmmount = Time.deltaTime;

        float AlphaValue = life.color.a;
        while (remainigFadeTime > 0)
        {
            remainigFadeTime -= Time.deltaTime;

            AlphaValue += fadeOperation(fadeType, fadeAmmount);
            AlphaValue = Mathf.Clamp(AlphaValue, 0, 1);

            lifeColor.a = AlphaValue;
            lifeBackGroundColor.a = AlphaValue;

            life.color = lifeColor;
            lifeBackGround.color = lifeBackGroundColor;

            if (fadeType == FadeType.FadeIn && AlphaValue == 1) break;
            else if (fadeType == FadeType.FadeOut && AlphaValue == 0) break;

            yield return new WaitForEndOfFrame();
        }
        _healthIsFadingOut = false;
    }
    IEnumerator FadeStamina(FadeType fadeType, float FadeTime)
    {
        _staminaIsFadingOut = fadeType == FadeType.FadeOut;
        currentStaminaFadeTime = FadeTime;

        var staminaColor = stamina.color;
        var staminaBackGroundColor = staminaBackGround.color;

        float remainigFadeTime = FadeTime;
        float fadeAmmount = Time.deltaTime;

        float AlphaValue = stamina.color.a;
        while (remainigFadeTime > 0)
        {
            remainigFadeTime -= Time.deltaTime;

            AlphaValue += fadeOperation(fadeType, fadeAmmount);
            AlphaValue = Mathf.Clamp(AlphaValue, 0, 1);

            staminaColor.a = AlphaValue;
            staminaBackGroundColor.a = AlphaValue;

            stamina.color = staminaColor;
            staminaBackGround.color = staminaBackGroundColor;

            if (fadeType == FadeType.FadeIn && AlphaValue == 1) break;
            else if (fadeType == FadeType.FadeOut && AlphaValue == 0) break;

            yield return new WaitForEndOfFrame();
        }

        _staminaIsFadingOut = false;
    }
    IEnumerator FadeBlood(FadeType fadeType, float FadeTime)
    {
        _bloodIsFadingOut = true;
        currentBloodFadeTime = FadeTime;

        var bloodColor = BloodAmmount.color;
        var bloodBackGroundColor = BloodAmmountBackGround.color;

        float remainigFadeTime = FadeTime;
        float fadeAmmount = Time.deltaTime;

        float AlphaValue = BloodAmmount.color.a;
        while (remainigFadeTime > 0)
        {
            remainigFadeTime -= Time.deltaTime;

            AlphaValue += fadeOperation(fadeType, fadeAmmount);
            AlphaValue = Mathf.Clamp(AlphaValue, 0, 1);

            bloodColor.a = AlphaValue;
            bloodBackGroundColor.a = AlphaValue;

            stamina.color = bloodColor;
            staminaBackGround.color = bloodBackGroundColor;

            if (fadeType == FadeType.FadeIn && AlphaValue == 1) break;
            else if (fadeType == FadeType.FadeOut && AlphaValue == 0) break;

            yield return new WaitForEndOfFrame();
        }

        _bloodIsFadingOut = false;
    }

    IEnumerator DelayedFadeHealthBar(FadeType Action, float DelayTime, float FadeTime)
    {
        yield return new WaitForSeconds(DelayTime);
        m_Fade(InfoComponent.HealthBar, Action, FadeTime);
    }
    IEnumerator DelayedFadeStaminaBar(FadeType Action, float DelayTime, float FadeTime)
    {
        yield return new WaitForSeconds(DelayTime);
        m_Fade( InfoComponent.StaminaBar, Action, FadeTime);
    }
    IEnumerator DelayedFadeBlood(FadeType Action, float DelayTime, float FadeTime)
    {
        yield return new WaitForSeconds(DelayTime);
        m_Fade( InfoComponent.BloodStat, Action, FadeTime);
    }
}
