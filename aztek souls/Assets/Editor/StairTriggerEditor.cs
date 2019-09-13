using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StairTrigger))]
public class StairTriggerEditor : Editor
{
    StairTrigger _i;

    //Warnings
    bool OrientationIsSetted = true;
    bool playerTagIsSetted = true;

    private void OnEnable()
    {
        _i = (StairTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Settings", style);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Stair Orientation:");
        _i.StairOrientation = (Transform)EditorGUILayout.ObjectField(_i.StairOrientation, typeof(Transform), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Player Tag:");
        _i.PlayerTag = EditorGUILayout.TagField(_i.PlayerTag);
        EditorGUILayout.EndHorizontal();

        if (_i.StairOrientation == null) OrientationIsSetted = false;
        if (_i.PlayerTag == "") playerTagIsSetted = false;

        if (!OrientationIsSetted || !playerTagIsSetted)
        {
            string warnings = "Hay elementos que no han sido asignados:";
            if (!OrientationIsSetted) warnings += "\nEl transform de la escalera no ha sido asignado.";
            if (!playerTagIsSetted) warnings += "\nEl Tag del jugador esta vacío";

            EditorGUILayout.HelpBox(warnings, MessageType.Error);
        }

        //base.OnInspectorGUI();
    }
}
