using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    public Image Fill;
    public Image BackGround;
    public float MaxValue { get; set; } = 0f;

    public void UpdateDisplay(float currentValue)
    {
        float value = currentValue / MaxValue;
        Fill.fillAmount = value;
        //print("UpdateDisplay");
        //print("Ammount = " + value + " max Value is: " + MaxValue + " \nFinal Value is: " + Fill.fillAmount);
    }
    public void SetApha(float alphaValue)
    {
        BackGround.canvasRenderer.SetAlpha(alphaValue);
        Fill.canvasRenderer.SetAlpha(alphaValue);
    }

    public void FadeIn(float duration = 1f)
    {
        Fill.CrossFadeAlpha(1f, duration, false);
        BackGround.CrossFadeAlpha(1f, duration, false);
    }

    public void FadeOut(float duration = 1f)
    {
        Fill.CrossFadeAlpha(0f, duration, false);
        BackGround.CrossFadeAlpha(0f, duration, false);
    }
}
