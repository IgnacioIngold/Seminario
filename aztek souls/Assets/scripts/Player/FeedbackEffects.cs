﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class FeedbackEffects : MonoBehaviour
{
    public Animator UIAnims;
    public RawImage BloodSplatEffect;
    public float HitDuration = 1f;

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
        //Si la vida es menor a cierto porcentaje
        //Reduzco el color grading
            //Saturación [1 -> 0]
    }

    public void GetHit()
    {
        StartCoroutine(DamageEffect());
    }

    IEnumerator DamageEffect()
    {
        //Necesito una duración.
        float remaining = HitDuration;

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
