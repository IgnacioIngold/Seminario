using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ComboInput
{
    public Tuple<int, Inputs>[] inputBind;

    public ComboInput(Tuple<int, Inputs>[] inputBinds)
    {
        inputBind = inputBinds;
    }
}

public class FeedbackRitmo : MonoBehaviour
{
    public Action OnComboSuccesfullyStart = delegate { };
    public Action OnComboCompleted = delegate { };
    public Action OnComboFailed = delegate { };
    public Action TimeEnded = delegate { };

    [Header("Sistema de Ritmo")]
    public ParticleSystem VulnerableMarker;       // Indica la tecla que debemos presionar.
    public ParticleSystem ButtonHitConfirm;       // Confirma que la tecla fue presionada.
    public Color LightColor;                      // Indica que se debe presionar el input Light.
    public Color HeavyColor;                      // Indica que se debe presionar el input Strong.


    [Header("Vulnerability Window")]
    public Dictionary<int, ComboInput> vulnerabilityCombos;
    protected int _currentComboIndex = 0;
    protected int _currentAttackIndex = 0;

    public bool AttackCoincided = false;

    public bool SuccesfullHit = false;
    Inputs LastPresedInput = Inputs.none;

    public float ComboWindow = 1.5f;

    bool displayVulnerability = false;
    float _currentTime = 0;
    private bool activeState;

    private void Awake()
    {
        VulnerableMarker.gameObject.SetActive(false);
        ButtonHitConfirm.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (displayVulnerability)
        {
            _currentTime -= Time.deltaTime;

            if (_currentTime <= 0)
            {
                print("Se acabó el tiempo.");
                TimeEnded();

                HideVulnerability();
                Reset();
            }
        }
    }

    private void Reset()
    {
        displayVulnerability = false;
        _currentTime = 0;

        SuccesfullHit = false;
        _currentAttackIndex = 0;
        LastPresedInput = Inputs.none;
    }

    public void FeedPressedInput(Inputs input)
    {
        LastPresedInput = input;

        var vul = vulnerabilityCombos[_currentComboIndex].inputBind[_currentAttackIndex];
        //Debug.LogWarning(string.Format("Vulnerabilidad Actual: ID {0} y tipo {1}, Índice acumulado: {2}, Del Combo {3}", vul.Item1, vul.Item2, _currentAttackIndex, _currentComboIndex ));

        if (input == GetCurrentVulnerabilityInput())
        {
            OnComboSuccesfullyStart();
            _currentTime = ComboWindow + 1f;
            HideVulnerability();
            Display_CorrectButtonHitted();
            //print("===================== PLAYER TOCA UNA TECLA =====================\nEl botón presionado coíncide con la vulnerabilidad");
        }
        else
        {
            HideVulnerability();
            Reset();
            //print("===================== PLAYER TOCA UNA TECLA =====================\nEl botón presionado NO coíncide con la vulnerabilidad");
        }
    }
    //Cuando el objetivo recive un hit hacemos algo.
    public void HitRecieved(int AttackID, Inputs input)
    {
        //print("====================== HIT RECIBIDO ======================");
        var currentCB = vulnerabilityCombos[_currentComboIndex].inputBind[_currentAttackIndex];

        //Si el último input recibido coíncide con el input del hit.
        bool coincidedWithLast = input == LastPresedInput;
        bool coincidedWithCurrent = AttackID == currentCB.Item1 && input == currentCB.Item2;
        AttackCoincided = coincidedWithCurrent && coincidedWithLast;

        //print(string.Format("Coincidió con el ultimo input {0} y con el primero {1}", coincidedWithLast, coincidedWithCurrent));
        //Debug.LogWarning(string.Format("ID ingresante es {0}", AttackID));
        //Debug.LogWarning(string.Format("El ínput del golpe es igual a la vulnerabilidad y al ulimo botón presionado.\nResumen ataque ingresante - ID: {0}, inputType {1} -\nResumen combo Actual -ID: {2}, InputType {3} -\nExtra: - Input Anterior registrado: {4}"
        //                        , AttackID, input, currentCB.Item1, currentCB.Item2, LastPresedInput));

        if (AttackCoincided)
        {
            if (_currentAttackIndex == GetCurrentMaxIndex())
            {
                //print("COMBO COMPLETADO BITCHES");
                OnComboCompleted();
                displayVulnerability = false;
                Reset();
            }
            else
            {
                _currentTime = ComboWindow;
                _currentAttackIndex++;
                ShowVulnerability();
            }

            LastPresedInput = Inputs.none;
        }
        else
        {
            //Cuando no coincide.
            OnComboFailed();
            displayVulnerability = false;
            Reset();
        }
    }

    //============================== SISTEMA DE RITMO =========================================

    /// <summary>
    /// Permite Añadir una vulnerabilidad.
    /// </summary>
    /// <param name="index">El índice de la vulnerabilidad, empieza en 0.</param>
    /// <param name="AttackTypes"> Un array de tipos de ataque para cada ataque.</param>
    public void AddVulnerability(int index, Tuple<int, Inputs>[] inputBinds)
    {
        if (vulnerabilityCombos == null)
            vulnerabilityCombos = new Dictionary<int, ComboInput>();

        ComboInput data = new ComboInput(inputBinds);

        if (!vulnerabilityCombos.ContainsKey(index))
            vulnerabilityCombos.Add(index, data);
        else
            vulnerabilityCombos[index] = data;
    }
    /// <summary>
    /// Permite setear _currentVulnerabilityCombo.
    /// </summary>
    /// <param name="ComboIndex">El índice de la vulnerabilidad</param>
    public void SetCurrentVulnerabilityCombo(int ComboIndex)
    {
        if (vulnerabilityCombos.ContainsKey(ComboIndex))
            _currentComboIndex = ComboIndex;
        else
            _currentComboIndex = 0;
    }

    //-------------------------------- Display ------------------------------------------------

    /// <summary>
    /// Feedback que muestra que se presionó el boton correcto.
    /// </summary>
    public void Display_CorrectButtonHitted()
    {
        ButtonHitConfirm.Play();
    }
    /// <summary>
    /// Feedback que muestra que se presionó el boton incorrecto.
    /// </summary>
    public void Display_IncorrectButtonHitted()
    {
        //Por ahora no tenemos una particula que muestre lo contrario.
        ButtonHitConfirm.Stop();
    }

    /// <summary>
    /// Muestra la particula de Vulnerabilidad, utilizando _currentVulnerabilityCombo y _AttacksRecieved como referencia.
    /// </summary>
    public void ShowVulnerability()
    {
        var ParticleSystem = VulnerableMarker.main;
        switch (GetCurrentVulnerabilityInput())
        {
            case Inputs.light:
                ParticleSystem.startColor = LightColor;
                break;
            case Inputs.strong:
                ParticleSystem.startColor = HeavyColor;
                break;
            default:
                ParticleSystem.startColor = LightColor;
                break;
        }

        VulnerableMarker.gameObject.SetActive(true);
        displayVulnerability = true;
        _currentTime = ComboWindow;
    }
    /// <summary>
    /// Oculta la particula que indica la vulnerabilidad
    /// </summary>
    public virtual void HideVulnerability()
    {
        VulnerableMarker.gameObject.SetActive(false);
    }

    //--------------------------------- Utilidad ----------------------------------------------

    /// <summary>
    /// Devuelve el valor Input de la vulnerabilidad actual.
    /// </summary>
    /// <returns></returns>
    protected Inputs GetCurrentVulnerabilityInput()
    {
        return vulnerabilityCombos[_currentComboIndex].inputBind[_currentAttackIndex].Item2;
    }
    protected int GetCurrentMaxIndex()
    {
        return Mathf.Clamp(vulnerabilityCombos[_currentComboIndex].inputBind.Length - 1, 0, int.MaxValue);
    }
}
