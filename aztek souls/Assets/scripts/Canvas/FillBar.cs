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
    public void FadeDeactivate(float timeInSeconds = 1f)
    {
        StartCoroutine(FadeAndDeactivate(timeInSeconds));
    }

    IEnumerator FadeAndDeactivate(float duration)
    {
        Fill.CrossFadeAlpha(0.05f, duration, false);
        BackGround.CrossFadeAlpha(0.05f, duration, false);
        yield return new WaitForSeconds(duration + 1f);
        this.gameObject.SetActive(false);
    }
}
