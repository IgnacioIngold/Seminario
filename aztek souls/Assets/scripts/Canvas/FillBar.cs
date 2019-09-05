using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    public Image Fill;
    public float MaxValue { get; set; } = 0f;

    public void UpdateDisplay(float currentValue)
    {
        float value = currentValue / MaxValue;
        Fill.fillAmount = value;
        //print("UpdateDisplay");
        //print("Ammount = " + value + " max Value is: " + MaxValue + " \nFinal Value is: " + Fill.fillAmount);
    }
}
