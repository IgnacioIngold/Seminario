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
   
    public float firstTime;
    public float SecondTime;
    public float FirstForce;
    public float SecondForce;

    public List<ParticleSystem> MyParticles = new List<ParticleSystem>();

    Animator anim;
    audioManager _AM;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        _AM = GetComponent<audioManager>();
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

    //Bloquea los inputs del jugador... lo usamos para que no reciba inputs durante una animación específica.
    void DisableInputs(int input)
    {
        if (input == 1)
            player._listenToInput = true;
        else
            player._listenToInput = false;
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
        player.CurrentWeapon.CanGetInput();
    }
    private void DenyGetInput()
    {
        player.CurrentWeapon.CanGetInput();

        if (marker.activeInHierarchy) marker.SetActive(false);
    }
    public void EndAnimation()
    {
        // print("PlayerAnimEventListener: CHUPAME LA PIJA LCDTM");
        player.EndAttackAnimation();
    }

    public void GetHurtEvent()
    {
        player.CameraShake.Play();
    }

    public void PlayMyParticles(int Index)
    {
        MyParticles[Index].Play();
    }

    public void playTails()
    {
        if (tail.isPlaying)
            tail.Stop();
        tail.Play();
    }

    public void PlaySound(String source)
    {
        _AM.Play(source);
    }
   
}
