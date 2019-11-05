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
    public float FirstForce;
    public float SecondForce;

    public List<ParticleSystem> MyParticles = new List<ParticleSystem>();
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    //Eventos de Animación.

    //Globales.
    void StepPerform(int index)
    {
        if (index == 1)
            player.Step(FirstForce);
        else
            player.Step(SecondForce);
    }
    void disableInputs(int input)
    {
        if (input == 0)
        {
            print("disabling input");
            player.listenToInput = false;
            player.running = false;
            player.clamped = true;
            player.moving = false;
        }
        else
        if (input == 1)
        {
            print("enabling input");
            player.listenToInput = true;
            player.moving = true;
            player.clamped = false;
        }
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
    public void PlayMyParticles(int Index)
    {
        MyParticles[Index].Play();
    }
}
