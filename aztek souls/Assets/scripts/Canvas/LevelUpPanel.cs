using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;
using TMPro;
using System;

public class LevelUpPanel : MonoBehaviour
{
    public event Action OnAccept = delegate { };
    public event Action OnCancel = delegate { };

    public int BloodForLevelUp = 1000;
    public float BloodForLevelUpIncreaseRate = 0.4f;

    [Header("Textos")]
    public TextMeshProUGUI CurrentLevel;
    public TextMeshProUGUI CurrentBlood;
    public TextMeshProUGUI BloodToLvlUp;

    //public TextMeshProUGUI CurrentVit;
    //public TextMeshProUGUI CurrentStr;
    //public TextMeshProUGUI CurrentDef;

    public TextMeshProUGUI newVit;
    public TextMeshProUGUI newStr;
    public TextMeshProUGUI newDef;

    [Header("Botones")]
    public Button Btn_VitIncrease;
    public Button Btn_VitDecrease;

    public Button Btn_StrIncrease;
    public Button Btn_StrDecrease;

    public Button Btn_DefIncrease;
    public Button Btn_DefDecrease;

    Stack<int> _vitBloodPaid;
    Stack<int> _strBloodPaid;
    Stack<int> _defBloodPaid;

    Player source;

    int originalAmmount = 0;

    int level = 1;
    int blood = 0;

    int _vitExtraPoints = 0;
    int _strExtraPoints = 0;
    int _defExtraPoints = 0;

    private void Awake()
    {
        source = FindObjectOfType<Player>();
        source.myStats.bloodForLevelUp = 1000;

        originalAmmount = BloodForLevelUp;
    }

    private void Update()
    {
        CurrentLevel.text = level.ToString();
        CurrentBlood.text = blood.ToString();
        BloodToLvlUp.text = BloodForLevelUp.ToString();

        //Los botones de incrementar se activan solo si hay suficiente sangre para subir de nivel.
        bool enoughBloodForLevelUp = blood >= BloodForLevelUp;

        Btn_VitIncrease.interactable = enoughBloodForLevelUp;
        Btn_StrIncrease.interactable = enoughBloodForLevelUp;
        Btn_DefIncrease.interactable = enoughBloodForLevelUp;

        Btn_VitDecrease.interactable = _vitExtraPoints > 0;
        Btn_StrDecrease.interactable = _strExtraPoints > 0;
        Btn_DefDecrease.interactable = _defExtraPoints > 0;
    }

    public void LoadData()
    {
        level = source.myStats.Nivel;
        blood = (int)source.myStats.Sangre;
        BloodForLevelUp = source.myStats.bloodForLevelUp;

        _vitBloodPaid = new Stack<int>();
        _strBloodPaid = new Stack<int>();
        _defBloodPaid = new Stack<int>();

        CurrentLevel.text = level.ToString();
        CurrentBlood.text = blood.ToString();
        BloodToLvlUp.text = BloodForLevelUp.ToString();

        //CurrentVit.text = source.myStats.Vitalidad.ToString();
        //CurrentStr.text = source.myStats.Fuerza.ToString();
        //CurrentDef.text = source.myStats.Resistencia.ToString();

        newVit.text = source.myStats.Vitalidad.ToString();
        newStr.text = source.myStats.Fuerza.ToString();
        newDef.text = source.myStats.Resistencia.ToString();
    }

    public void Accept()
    {
        if (_vitExtraPoints > 0 || _strExtraPoints > 0 || _defExtraPoints > 0)
        {
            //Añado los cambios al Player.
            source.myStats.Nivel = level;
            source.Blood = blood;
            source.myStats.bloodForLevelUp = BloodForLevelUp;
            source.myStats.Vitalidad = source.myStats.Vitalidad + _vitExtraPoints;
            source.myStats.Fuerza = source.myStats.Fuerza + _strExtraPoints;
            source.myStats.Resistencia = source.myStats.Resistencia + _defExtraPoints;

            _vitExtraPoints = 0;
            _strExtraPoints = 0;
            _defExtraPoints = 0;
        }

        OnAccept();
    }
    public void Cancel()
    {
        BloodForLevelUp = originalAmmount;
        _vitExtraPoints = 0;
        _strExtraPoints = 0;
        _defExtraPoints = 0;
        LoadData();
        OnCancel();
    }

    public void VitIncrease()
    {
        level++;
        _vitExtraPoints++;
        _vitBloodPaid.Push(BloodForLevelUp);
        blood -= BloodForLevelUp;
        BloodForLevelUp += (int)(BloodForLevelUp * BloodForLevelUpIncreaseRate);
        newVit.text = (source.myStats.Vitalidad + _vitExtraPoints).ToString();
    }
    public void VitDecrease()
    {
        level--;
        _vitExtraPoints--;
        int gainedBlood = _vitBloodPaid.Pop();
        blood += gainedBlood;
        BloodForLevelUp = gainedBlood;
        newVit.text = (source.myStats.Vitalidad + _vitExtraPoints).ToString();
    }

    public void StrIncrease()
    {
        level++;
        _strExtraPoints++;
        _strBloodPaid.Push(BloodForLevelUp);
        blood -= BloodForLevelUp;
        BloodForLevelUp += (int)(BloodForLevelUp * BloodForLevelUpIncreaseRate);
        newStr.text = (source.myStats.Fuerza + _strExtraPoints).ToString();
    }
    public void StrDecrease()
    {
        level--;
        _strExtraPoints--;
        int gainedBlood = _strBloodPaid.Pop();
        blood += gainedBlood;
        BloodForLevelUp = gainedBlood;
        newStr.text = (source.myStats.Fuerza + _strExtraPoints).ToString();
    }

    public void DefIncrease()
    {
        level++;
        _defExtraPoints++;
        _defBloodPaid.Push(BloodForLevelUp);
        blood -= BloodForLevelUp;
        BloodForLevelUp += (int)(BloodForLevelUp * BloodForLevelUpIncreaseRate);
        newDef.text = (source.myStats.Resistencia + _defExtraPoints).ToString();
    }
    public void DefDecrease()
    {
        level--;
        _defExtraPoints--;
        int gainedBlood = _defBloodPaid.Pop();
        blood += gainedBlood;
        BloodForLevelUp = gainedBlood;
        newDef.text = (source.myStats.Resistencia + _defExtraPoints).ToString();
    }
}
