using System;
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

    public BigCursed boss;
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
        loadData(Application.streamingAssetsPath + CompleteDataPath);
    }

    void StartBasicCombo()
    {
        //Inicié el combo básico.
    }

    //Primer ataque básico.
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
    void ThirdBasicStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[2].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[2].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[2].localScale;
        DamageCollider.enabled = true;

        boss.OnSmashParticle.Clear();
        boss.SmashEmission.enabled = true;
        boss.OnSmashParticle.Play();
    }
    void ThirdBasicEnd()
    {
        DamageCollider.enabled = false;
        boss.SmashEmission.enabled = false;
    }

    void EndBasicCombo()
    {
        //Termine el combo básico.
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[0].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[0].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[0].localScale;
    }

    //High Jump.
    void HighJumpLandStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[3].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[3].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[3].localScale;
        DamageCollider.enabled = true;

        boss.OnSmashParticle.Clear();
        boss.SmashEmission.enabled = true;
        boss.OnSmashParticle.Play();
    }

    void HighJumpLandEnd()
    {
        DamageCollider.enabled = false;
        boss.SmashEmission.enabled = false;
    }

    void LowJumpLandStart()
    {
        DamageCollider.transform.localPosition = ColliderTransformKeyFrame[4].localPosition;
        DamageCollider.transform.localRotation = ColliderTransformKeyFrame[4].LocalRotation;
        DamageCollider.transform.localScale = ColliderTransformKeyFrame[4].localScale;
        DamageCollider.enabled = true;

        boss.OnSmashParticle.Clear();
        boss.SmashEmission.enabled = true;
        boss.OnSmashParticle.Play();
    }
    void LowJumpLandEnd()
    {
        DamageCollider.enabled = false;
        boss.SmashEmission.enabled = false;
    }
    void unlockpassages()
    {
      
        DeathACtions.Play();
    }
}
