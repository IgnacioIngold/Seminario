using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using EZCameraShake;

public class FeedbackEffects : MonoBehaviour
{
    public Animator UIAnims;
    public RawImage BloodSplatEffect;

    public float LowHealthPercentage = 0.3f;
    public float HitDuration = 1f;
    public float HitShakeMagnitude;
    public float HitShakeRoughness;
    public float HitShakeFadeInTime;
    public float HitShakeFadeOutTime;

    Vignette _vig;
    ChromaticAberration _ca;
    ColorGrading _cg;
    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
        player.OnGetHit += GetHit;
        var PP = Camera.main.GetComponent<PostProcessVolume>();

        if (!PP.profile.TryGetSettings(out _vig))
            print("No se encontró el vignette");
        if (!PP.profile.TryGetSettings(out _ca))
            print("No se encontró el Chromattic Aberration");
        if (!PP.profile.TryGetSettings(out _cg))
            print("No se encontró el ColorGrading");
    }

    // Update is called once per frame
    void Update()
    {
        float normalizedHealth = player.Health / player.MaxHealth;
        UpdateHP(normalizedHealth);
    }

    void UpdateHP(float HP)
    {
        if (HP > LowHealthPercentage)
        {
            _cg.saturation.value = 0;
            _cg.brightness.value = 0;
            _cg.contrast.value = 0;
        }

        if (HP < LowHealthPercentage)
        {
            float HAmmount = HP / LowHealthPercentage;

            //Si la vida es menor a cierto porcentaje, Reduzco el color grading
            _cg.saturation.value = Mathf.Lerp(0, -100, HAmmount);
            _cg.brightness.value = Mathf.Lerp(0, 80, HAmmount);
            _cg.contrast.value = Mathf.Lerp(0, 60, HAmmount);
        }

        if (HP == 0)
        {
            _cg.saturation.value = -100;
            _cg.brightness.value = 80;
            _cg.contrast.value = 60;
        }
    }

    public void GetHit()
    {
        StartCoroutine(DamageEffect());
    }

    IEnumerator DamageEffect()
    {
        //Necesito una duración.
        float remaining = HitDuration;
        CameraShaker.Instance.ShakeOnce(HitShakeMagnitude, HitShakeRoughness, HitShakeFadeInTime, HitShakeFadeOutTime);

        //Start -->
        //Seteo el Vignette.
            _vig.color.value = new Color(1, 0, 0);
            _vig.intensity.value = 1f;
            //_vig.rounded.value = true;

        Color initialColor = new Color(0, 0, 0);

        //Seteo el Chromatic Averration.
            _ca.intensity.value = 1f;


        //Llamo a la animación de la UI;
        UIAnims.SetTrigger("GetHit");

        while (remaining > 0)
        {
            remaining -= Time.deltaTime;
            if (remaining < 0) remaining = 0;

            //Vignette.
                _vig.intensity.value = Mathf.Lerp(0.18f, 1f, remaining);

            //Chromatic Aberration.
                _ca.intensity.value = Mathf.Lerp(0, 1, remaining);

            yield return null;
        }

        //Finish -->
        //Seteo el Vignette a los valores originales.ç
            _vig.color.value = initialColor;
            _vig.intensity.value = 0.18f;
            //_vig.rounded.value = false;

        //Seteo el Chromatic Aberration a los valores originales.
        _ca.intensity.value = 0f;
    }
}
