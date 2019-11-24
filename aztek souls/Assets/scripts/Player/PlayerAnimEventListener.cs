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

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    //Eventos de Animación.

    //Globales.
    void StepPerform(int index)
    {
        if(index == 1)
            player.Step(FirstForce,firstTime);
        else
            player.Step(SecondForce,SecondTime);
        
    }
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
        //player.CurrentWeapon.CanGetInput();

        if (marker.activeInHierarchy) marker.SetActive(false);
    }

    public void GetHurtEvent()
    {
        player.CameraShake.Play();
    }
    public void PlayTails()
    {
        if (tail.isPlaying)
            tail.Stop();
        tail.Play();
    }
    public void PlayMyParticles(int Index)
    {
        MyParticles[Index].Play();
    }

    public void EndAnimation()
    {
        print("PlayerAnimEventListener: CHUPAME LA PIJA LCDTM");
        player.EndAttackAnimation();
    }
}
