﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.Serialization;
using UnityEngine.Playables;

public class BossAnimEvents : MonoBehaviour
{
    public const string DataPath = "/Data";
    public const string FileAndExtention = "BossColliderAnims.json";
    public const string CompleteDataPath = "/Data/BossColliderAnims.json";

    public Dictionary<int, TransformValues> ColliderTransformKeyFrame = new Dictionary<int, TransformValues>();
    public PlayableDirector DeathACtions;

    public BigCursed Owner;
    public Collider DamageCollider;
    //public PlayableDirector FadeOut;


    [Serializable]
    public struct DataPairContainter
    {
        public List<int> Keys;
        public List<TransformValues> values;
    }

    [Serializable]
    public struct TransformValues
    {
        public Vector3 localPosition;
        public Quaternion LocalRotation;
        public Vector3 localScale;
    }

    public void loadData(string dataPath)
    {
        if (ColliderTransformKeyFrame == null)
            ColliderTransformKeyFrame = new Dictionary<int, TransformValues>();

        ColliderTransformKeyFrame.Clear();

        string completePath = Application.streamingAssetsPath + CompleteDataPath;
        DataPairContainter data = FullSerialization.Deserialize<DataPairContainter>(completePath, false);

        for (int i = 0; i < data.Keys.Count; i++)
            ColliderTransformKeyFrame.Add(data.Keys[i], data.values[i]);
    }
    public void SaveData()
    {
        string completePath = Application.streamingAssetsPath + CompleteDataPath;
        DataPairContainter data = new DataPairContainter() { Keys = new List<int>(), values = new List<TransformValues>()};

        foreach (var pair in ColliderTransformKeyFrame)
        {
            data.Keys.Add(pair.Key);
            data.values.Add(pair.Value);
        }

        data.Serialize(completePath, false);
    }

    //RunTimeLoad
    private void Awake()
    {
        Owner = GetComponent<BigCursed>();
        loadData(Application.streamingAssetsPath + CompleteDataPath);
    }

    #region BasicCombo

    //Primer ataque básico.
    void StartBasicCombo()
    {
        //Debug.LogWarning("Empezó el combo");
        //Inicié el combo básico.
        //Owner.SetVulnerabity(true);
    }
    void FirstBasicStart()
    {
        //Index 1
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[0].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[0].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[0].localScale;
        DamageCollider.enabled = true;
    }
    void FirstBasicEnd()
    {
        DamageCollider.enabled = false;
    }


    //Segundo Ataque Básico.
    void StartSecondAttack()
    {
        //Debug.LogWarning("StartSecond Attack event Run");
        Owner.SetAttackState(2);
    }
    void SecondBasicStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[1].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[1].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[1].localScale;
        DamageCollider.enabled = true;
    }
    void SecondBasicEnd()
    {
        DamageCollider.enabled = false;
    }

    //Tercer Ataque básico.
    void StartThirdAttack()
    {
        //print("Start Third Attack event Run");
        Owner.SetAttackState(3);
    }
    void ThirdBasicStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[2].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[2].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[2].localScale;
        DamageCollider.enabled = true;

        Owner.OnSmashParticle.Clear();
        Owner.SmashEmission.enabled = true;
        Owner.OnSmashParticle.Play();
    }
    void ThirdBasicEnd()
    {
        DamageCollider.enabled = false;
        Owner.SmashEmission.enabled = false;
    }

    void EndBasicCombo()
    {
        //Debug.LogWarning("Terminó el combo");
        //Termine el combo básico.
        Owner.SetAttackState(0);

        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[0].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[0].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[0].localScale;
    } 
    #endregion

    //High Jump.
    void HighJumpLandStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[3].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[3].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[3].localScale;
        DamageCollider.enabled = true;

        Owner.OnSmashParticle.Clear();
        Owner.SmashEmission.enabled = true;
        Owner.OnSmashParticle.Play();
    }
    void HighJumpLandEnd()
    {
        DamageCollider.enabled = false;
        Owner.SmashEmission.enabled = false;
    }

    void LowJumpLandStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[4].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[4].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[4].localScale;
        DamageCollider.enabled = true;

        Owner.OnSmashParticle.Clear();
        Owner.SmashEmission.enabled = true;
        Owner.OnSmashParticle.Play();
    }
    void LowJumpLandEnd()
    {
        DamageCollider.enabled = false;
        Owner.SmashEmission.enabled = false;
    }

    //Recovering from Smashed;
    void StandUp()
    {
        Owner.RiseFromSmash();
    }
    void unlockpassages()
    {
        DeathACtions.Play();
    }
}
