using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BossAnimEvents))]
public class BossAnimRecieverEditor : Editor
{
    BossAnimEvents inspected;
    bool foldout;

    Color selectedColor;
    AnimationCurve bla;
    private int selectedLayer;
    private Gradient selectedGradient = new Gradient();
    private string selectedTag;
    private string historyText;
    private Bounds selectedBounds;

    SerializedProperty ColliderValues;

    private void OnEnable()
    {
        inspected = (BossAnimEvents)target;

        ColliderValues = serializedObject.FindProperty("DamageCollider");
        //Cargo la posible data.
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //Dibujo antes del base.
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();                          //Inicio grupo Horizontal.

            GUI.color = Color.green;
            if (GUILayout.Button("Copy Target Collider Settings"))
            {
                //Expando con esto :D
            }
            GUI.color = Color.grey;

            EditorGUILayout.FloatField(0f);

        GUILayout.EndHorizontal();                              //Fin Grupo Horizontal.
        GUILayout.Space(10f);

        /*
         * GUILayout.Button("TestButton");
            GUILayout.Button("TestButton");
            GUILayout.Button("TestButton");
         */

        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Info");
        if (foldout)
        {
            selectedColor = EditorGUILayout.ColorField("Pick a color", Color.white);
            bla = EditorGUILayout.CurveField("Pick a Curve (?)", bla);
            selectedLayer = EditorGUILayout.LayerField(selectedLayer);
            selectedGradient = EditorGUILayout.GradientField(selectedGradient);
            selectedTag = EditorGUILayout.TagField(selectedTag);
            historyText = EditorGUILayout.TextArea(historyText);
            selectedBounds = EditorGUILayout.BoundsField(selectedBounds);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(ColliderValues);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        GUI.color = Color.white;
    }

    [ContextMenu("Add Current Collider Settings")]
    void CopyColliderSettings()
    {
        if (inspected.DamageCollider == null)
        {
            print("El collider no esta Seteado Salamin");
            return;
        }

        string Settings = "Damage Collider Current Settings:\n" +
                                 "Position = ({0};{1};{2})\n" +
                                 "Rotation = ({3};{4};{5})\n" +
                                 "Scale = ({6};{7};{8})";

        Transform col = inspected.DamageCollider.transform;
        Settings = string.Format(Settings,
                                 col.localPosition.x, col.localPosition.y, col.localPosition.z,
                                 col.rotation.x, col.rotation.y, col.rotation.z,
                                 col.localScale.x, col.localScale.y, col.localScale.z);

        print(Settings);
    }

    void print(object message)
    {
        MonoBehaviour.print(message);
    }

}
