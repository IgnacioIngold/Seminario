using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class FeedbackEffects : MonoBehaviour
{
    public Animator UIAnims;
    public Image BloodSplatEffect;
    Vignette _vig;
    ChromaticAberration _ca;
    ColorGrading _cg;


    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetHit()
    {

    }

    IEnumerator ShowHit()
    {
        //Seteo las weas
        yield return null;
    }
}
