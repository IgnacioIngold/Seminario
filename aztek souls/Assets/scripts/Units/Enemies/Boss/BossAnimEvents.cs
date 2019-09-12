using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.Serialization;

public class BossAnimEvents : MonoBehaviour
{
    public const string DataPath = "/Resources/Data";
    public const string FileAndExtention = "BossColliderAnims.json";
    public const string CompleteDataPath = "/Resources/Data/BossColliderAnims.json";

    public Dictionary<int, TransformValues> ColliderTransformKeyFrame = new Dictionary<int, TransformValues>();

    public BigCursed boss;
    public Collider DamageCollider;

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
        public Quaternion rotation;
        public Vector3 localScale;
    }

    public void loadData(string dataPath)
    {
        if (ColliderTransformKeyFrame == null)
            ColliderTransformKeyFrame = new Dictionary<int, TransformValues>();

        ColliderTransformKeyFrame.Clear();

        string completePath = Application.dataPath + CompleteDataPath;
        DataPairContainter data = FullSerialization.Deserialize<DataPairContainter>(completePath, false);

        for (int i = 0; i < data.Keys.Count; i++)
            ColliderTransformKeyFrame.Add(data.Keys[i], data.values[i]);
    }
    public void SaveData()
    {
        string completePath = Application.dataPath + CompleteDataPath;
        DataPairContainter data = new DataPairContainter() { Keys = new List<int>(), values = new List<TransformValues>()};

        foreach (var pair in ColliderTransformKeyFrame)
        {
            data.Keys.Add(pair.Key);
            data.values.Add(pair.Value);
        }

        data.Serialize(completePath, false);
    }

    void StartBasicCombo()
    {
        //Inicié el combo básico.

    }

    //Primer ataque básico.
    void RightPunchStart_AE()
    {
        
    }
    void RighPunchEnd_AE()
    {

    }

    //Segundo Ataque Básico.
    void LeftPunchStart()
    {

    }
    void LeftPunchEnd()
    {

    }

    //Tercer Ataque básico.
    void SwipeDownStart()
    {

    }
    void SwipeDownEnd()
    {

    }


    void EndBasicCombo()
    {
        //Termine el combo básico.
    }

    //High Jump.
    void HighJumpLandStart()
    {

    }
    void HighJumpLandEnd()
    {

    }
}
