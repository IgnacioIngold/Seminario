using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    public enum InfoComponent
    {
        HealthBar,
        StaminaBar,
        BloodStat
    }
    public enum FadeAction : int
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

    /// <summary>
    /// Actualiza el Display de la cantidad de vida.
    /// </summary>
    /// <param name="currentHealth">Cantidad de vida actual.</param>
    /// <param name="maxHealth">Cantidad de vida Máxima.</param>
    public void UpdateHeathBar(float currentHealth, float maxHealth)
    {
        StopCoroutine("DelayedFadeHealthBar");
        if (!healthBarIsVisible)
        {
            var lifeAlphaChannel = life.color.a;
            var lifeBackGroundAlphaChannel = lifeBackGround.color.a;

            lifeAlphaChannel = 1;
            lifeBackGroundAlphaChannel = 1;
        }

        var pct = currentHealth / maxHealth;

        life.fillAmount = pct;
    }
    /// <summary>
    /// Actualiza el Display de la cantidad de Estamina.
    /// </summary>
    /// <param name="Stamina">Cantidad actual de la Estamina.</param>
    /// <param name="MaxStamina">Cantidad Máxima de la Estamina.</param>
    public void UpdateStamina(float Stamina, float MaxStamina)
    {
        StopCoroutine("DelayedFadeStaminaBar");
        print("Stamina is visible: " + staminaBarIsVisible + " co1or is: " + stamina.color.a );

        if (!staminaBarIsVisible)
        {
            var staminaAlphaChannel = stamina.color.a;
            var staminaBackGroundAlphaChannel = staminaBackGround.color.a;

            //print("LLegué hasta este punto");

            staminaAlphaChannel = 1;
            staminaBackGroundAlphaChannel = 1;
        }

        var pct = Stamina / MaxStamina;
        stamina.fillAmount = pct;
    }
    /// <summary>
    /// Actualiza el Display de la cantidad de Sangre Acumulada.
    /// </summary>
    /// <param name="Ammount"></param>
    public void UpdateBloodAmmount(int Ammount)
    {
        StopCoroutine("DelayedFadeBlood");
        if (!BloodAmmountIsVisible)
        {
            var bloodAlphaChannel = BloodAmmount.color.a;
            var bloodBackGroundAlphaChannel = BloodAmmountBackGround.color.a;

            bloodAlphaChannel = 1;
            bloodBackGroundAlphaChannel = 1;
        }

        BloodAmmount.text = Ammount.ToString();
    }
    /// <summary>
    /// Revela u Oculta todos los componentes en un lapso de tiempo.
    /// </summary>
    /// <param name="timeInSeconds"></param>
    public void FadeAll(FadeAction Action, float timeInSeconds)
    {
        life.CrossFadeAlpha((int) Action, timeInSeconds, false);
        lifeBackGround.CrossFadeAlpha((int)Action, timeInSeconds, false);

        stamina.CrossFadeAlpha((int)Action, timeInSeconds, false);
        staminaBackGround.CrossFadeAlpha((int)Action, timeInSeconds, false);

        BloodAmmount.CrossFadeAlpha((int)Action, timeInSeconds, false);
        BloodAmmountBackGround.CrossFadeAlpha((int)Action, timeInSeconds, false);
    }
    /// <summary>
    /// Muestra u Oculta un componente específico en x cantidad de Tiempo.
    /// </summary>
    /// <param name="ComponentType"></param>
    /// <param name="Action"></param>
    /// <param name="TimeInSeconds"></param>
    public void Fade(InfoComponent ComponentType, FadeAction Action, float TimeInSeconds)
    {
        switch (ComponentType)
        {
            case InfoComponent.HealthBar:
                life.CrossFadeAlpha((int)Action, TimeInSeconds, false);
                lifeBackGround.CrossFadeAlpha((int)Action, TimeInSeconds, false);
                break;
            case InfoComponent.StaminaBar:
                stamina.CrossFadeAlpha((int)Action, TimeInSeconds, false);
                staminaBackGround.CrossFadeAlpha((int)Action, TimeInSeconds, false);
                break;
            case InfoComponent.BloodStat:
                BloodAmmount.CrossFadeAlpha((int)Action, TimeInSeconds, false);
                BloodAmmountBackGround.CrossFadeAlpha((int)Action, TimeInSeconds, false);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Apaga inmediatamente todos los componentes.
    /// </summary>
    public void TurnOffAll()
    {
        FadeAll(FadeAction.FadeOut, 0);
    }
    /// <summary>
    /// Prende Inmediatamente todos los componentes.
    /// </summary>
    public void TurnOnAll()
    {
        FadeAll( FadeAction.FadeIn, 0);
    }
    /// <summary>
    /// Realiza un Fade luego de que se cumpla un Delay
    /// </summary>
    /// <param name="component">El tipo de componente cuyo Display queremos modificar.</param>
    /// <param name="Action">La acción a realizar.</param>
    /// <param name="DelayTime">El tiempo que pasa antes de realizar la acción.</param>
    /// <param name="FadeTime">El tiempo que dura la acción.</param>
    public void DelayedFade(InfoComponent component, FadeAction Action, float DelayTime, float FadeTime)
    {
        switch (component)
        {
            case InfoComponent.HealthBar:
                StartCoroutine(DelayedFadeHealthBar(Action, DelayTime, FadeTime));
                break;
            case InfoComponent.StaminaBar:
                StartCoroutine(DelayedFadeStaminaBar(Action, DelayTime, FadeTime));
                break;
            case InfoComponent.BloodStat:
                StartCoroutine(DelayedFadeBlood(Action, DelayTime, FadeTime));
                break;
            default:
                break;
        }
    }

    IEnumerator DelayedFadeHealthBar(FadeAction Action, float DelayTime, float FadeTime)
    {
        yield return new WaitForSeconds(DelayTime);
        Fade(InfoComponent.HealthBar, Action, FadeTime);
    }
    IEnumerator DelayedFadeStaminaBar(FadeAction Action, float DelayTime, float FadeTime)
    {
        yield return new WaitForSeconds(DelayTime);
        Fade( InfoComponent.StaminaBar, Action, FadeTime);
    }
    IEnumerator DelayedFadeBlood(FadeAction Action, float DelayTime, float FadeTime)
    {
        yield return new WaitForSeconds(DelayTime);
        Fade( InfoComponent.BloodStat, Action, FadeTime);
    }
}
