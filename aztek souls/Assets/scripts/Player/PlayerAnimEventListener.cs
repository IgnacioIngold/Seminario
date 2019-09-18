using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventListener : MonoBehaviour
{
    public Player player;
    public Collider DamageCollider;
    public GameObject marker;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    //Eventos de Animación.

    //Globales.

    private void AllowInterrupt()
    {
        player.interruptAllowed = true;
    }

    private void DenyInterrupt()
    {
        player.interruptAllowed = false;
    }

    private void EnableDamage()
    {
        //print("AnimEvent PLAYER ON START ATTACK activado");
        DamageCollider.enabled = true;
    }

    private void DisableDamage()
    {
        //print("AnimEvent PLAYER ON FINIT ATTACK activado");
        DamageCollider.enabled = false;
    }

    private void AllowGetInput()
    {
        player.CurrentWeapon.CanGetInput(true);
        player.interruptAllowed = true;
        marker.SetActive(true);

    }
    private void DenyGetInput()
    {
        player.CurrentWeapon.CanGetInput(false);
        marker.SetActive(false);

    }

    /// <summary>
    /// Retorna la duración del clip actual.
    /// </summary>
    public float getAnimTime()
    {
        AnimationClip clip = anim.GetCurrentAnimatorClipInfo(0)[0].clip;
        print("Attack Clip: " + clip + " ClipGetted: " + clip.name);

        //print("currentClip is Correct!");
        //float passed = AnimTime - (AnimTime * transitionPassed);

        return clip.length;
    }
}
