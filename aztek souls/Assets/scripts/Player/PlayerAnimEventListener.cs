using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerAnimEventListener : MonoBehaviour
{
    public Player player;
    public Collider DamageCollider;
    public GameObject marker;
    public ParticleSystem tail;
    Animator anim;
    audioManager _AM;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        _AM = GetComponent<audioManager>();
    }

    //Eventos de Animación.

    //Globales.

    void PlaySound(String source)
    {
        _AM.Play(source);
    }
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

    }
    private void DenyGetInput()
    {
        player.CurrentWeapon.CanGetInput(false);

        if (marker.activeInHierarchy) marker.SetActive(false);
    }

    public void GetHurtEvent()
    {
        player.CameraShake.Play();
    }
    public void playTails()
    {
        if (tail.isPlaying)
            tail.Stop();
        tail.Play();
    }
}
