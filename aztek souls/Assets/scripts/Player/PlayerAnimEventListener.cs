using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerAnimEventListener : MonoBehaviour
{
    public Player player;
    public Collider DamageCollider;
    public ParticleSystem tail;
    Animator anim;
    audioManager _AM;

    public List<ParticleSystem> MyParticles = new List<ParticleSystem>();

    public float firstTime;
    public float SecondTime;
    public float FirstForce;
    public float SecondForce;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        _AM = GetComponent<audioManager>();
    }

    //Eventos de Animación.

    //Globales.
    void StepPerform(int index)
    {
        if (index == 1)
            player.Step(FirstForce, firstTime);
        else
            player.Step(SecondForce, SecondTime);
    }
    void DisableInputs(int input)
    {
        if (input == 1)
            player._listenToInput = true;
        else
            player._listenToInput = false;
    }
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
        player.CurrentWeapon.CanGetInput();
        player.interruptAllowed = true;
    }
    private void DenyGetInput()
    {
        //player.CurrentWeapon.CanGetInput();
        //if (marker.activeInHierarchy) marker.SetActive(false);
    }
    public void EndAnimation()
    {
        print("PlayerAnimEventListener: CHUPAME LA PIJA LCDTM");
        player.EndAttackAnimation();
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
    public void PlayMyParticles(int Index)
    {
        MyParticles[Index].Play();
    }
}
