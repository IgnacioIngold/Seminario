using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BossAnimEvents))]
public class BossAnimRecieverEditor : Editor
{
    BossAnimEvents inspected;
    bool foldout;
    int selectedIndex = 0;

    //Warnings
    bool AEReciever = true;
    bool colliderSetted = true;
    bool FolderExists = true;
    bool FileExists = true;

    //Debug.
    bool FoldOut_DebugMessages = true;
    bool DebugMessages = true;
    bool CopiedSettings = false;

    SerializedProperty ColliderValues;

    private void OnEnable()
    {
        inspected = (BossAnimEvents)target;

        //Chequear que el collider este seteado.
        AEReciever = inspected.boss != null;
        colliderSetted = inspected.DamageCollider != null;

        CheckRelevantData();

        if (FolderExists && FileExists)
            inspected.loadData(BossAnimEvents.CompleteDataPath);

        //cambiar el valor de selected index segun la cantidad de elementos cargados.
        if (inspected.ColliderTransformKeyFrame == null)
            inspected.ColliderTransformKeyFrame = new Dictionary<int, BossAnimEvents.TransformValues>();
        selectedIndex = inspected.ColliderTransformKeyFrame.Count;

    }

    private void CheckRelevantData()
    {
        selectedIndex = inspected.ColliderTransformKeyFrame.Count;

        //Chequear que el collider y el Observador esten seteados.
        AEReciever = inspected.boss != null;
        colliderSetted = inspected.DamageCollider != null;

        //Chequear que la carpeta exista y el archivo existe.
        FolderExists = Directory.Exists(Application.dataPath + BossAnimEvents.DataPath);
        FileExists = File.Exists(Application.dataPath + BossAnimEvents.CompleteDataPath);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CheckRelevantData();

        #region Mensajes de advertencia
        //Mostramos los mensajes de advertencia en caso de que la data mas relevante tenga algún problema.
        if (!colliderSetted || !AEReciever)
        {
            string Advertencies = "Algunos elementos no han sido seteados todavía!:";
            if (!AEReciever) Advertencies += "\nEl Observador no ha sido seteado.";
            if (!colliderSetted) Advertencies += "\nEl collider no ha sido seteado.";
            Advertencies += "\nEsto es obligatorio.";

            EditorGUILayout.HelpBox(Advertencies, MessageType.Error);
        }

        if (!FolderExists || !FileExists)
        {
            string Advertencies = "There is a problem with the Data!";
            if (!FolderExists) Advertencies += "\nData Path folder does not exists";
            if (!FileExists) Advertencies += "\nData File does not exists";

            EditorGUILayout.HelpBox(Advertencies, MessageType.Warning);
            if (GUILayout.Button("Create new DataFile"))
            {
                Directory.CreateDirectory(Application.dataPath + BossAnimEvents.DataPath);
                inspected.SaveData();
                AssetDatabase.Refresh();

                if (DebugMessages)
                    MonoBehaviour.print("Archivo y Path creados");
            }
        } 
        #endregion

        EditorGUI.BeginDisabledGroup(!colliderSetted || !AEReciever);
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();                          //Inicio grupo Horizontal.

            GUI.color = Color.green;
            if (GUILayout.Button("Copy Target Collider Settings") && colliderSetted)
            {
            #region Debug Copied Settings
            if (CopiedSettings)
            {
                string Settings = "Damage Collider Current Settings:\n" +
                                         "Position = ({0};{1};{2})\n" +
                                         "Rotation = ({3};{4};{5})\n" +
                                         "Scale = ({6};{7};{8})";

                Transform col = inspected.DamageCollider.transform;
                Settings = string.Format(Settings,
                                         col.localPosition.x, col.localPosition.y, col.localPosition.z,
                                         col.rotation.x, col.rotation.y, col.rotation.z,
                                         col.localScale.x, col.localScale.y, col.localScale.z);

                Debug.LogFormat(Settings);
            } 
            #endregion

                //Guardo los valores en un contenedor.
                BossAnimEvents.TransformValues serializedValues = new BossAnimEvents.TransformValues()
                {
                    localPosition = inspected.DamageCollider.transform.localPosition,
                    rotation = inspected.DamageCollider.transform.rotation,
                    localScale = inspected.DamageCollider.transform.localScale
                };

                //Los añado al diccionario.
                if (inspected.ColliderTransformKeyFrame.ContainsKey(selectedIndex))
                {
                    inspected.ColliderTransformKeyFrame[selectedIndex] = serializedValues;
                    if (DebugMessages)
                        MonoBehaviour.print("Los valores han sido reemplazados.");
                }
                else
                {
                    inspected.ColliderTransformKeyFrame.Add(selectedIndex, serializedValues);
                    if (DebugMessages)
                        MonoBehaviour.print("Los valores han sido añadidos.");
                }
            }
            GUI.color = Color.grey;

            selectedIndex = EditorGUILayout.IntField(selectedIndex);

        GUILayout.EndHorizontal();                              //Fin Grupo Horizontal.
        GUILayout.Space(10f);

        if (GUILayout.Button("Save Data"))
        {
            if (DebugMessages) MonoBehaviour.print("Data has been saved...");
            inspected.SaveData();
        }

        EditorGUI.EndDisabledGroup();

        #region Debug

        //Enable And Debug Messages
        FoldOut_DebugMessages = EditorGUILayout.Foldout(FoldOut_DebugMessages, "Debug Messages");
        if (FoldOut_DebugMessages)
        {
            DebugMessages = EditorGUILayout.ToggleLeft("Print Debug Messages", DebugMessages);
            CopiedSettings = EditorGUILayout.ToggleLeft("Print Copied Values.", CopiedSettings);
        }

        #endregion

        GUI.color = Color.white;
    }

    //=========================================================================================

    //[ContextMenu("Add New Transform Collider Setting")]
    //void CopyColliderSettings()
    //{
    //    if (inspected.DamageCollider == null)
    //    {
    //        Debug.LogError("El collider no esta Seteado Salamin");
    //        return;
    //    }

    //    string Settings = "Damage Collider Current Settings:\n" +
    //                             "Position = ({0};{1};{2})\n" +
    //                             "Rotation = ({3};{4};{5})\n" +
    //                             "Scale = ({6};{7};{8})";

    //    Transform col = inspected.DamageCollider.transform;
    //    Settings = string.Format(Settings,
    //                             col.localPosition.x, col.localPosition.y, col.localPosition.z,
    //                             col.rotation.x, col.rotation.y, col.rotation.z,
    //                             col.localScale.x, col.localScale.y, col.localScale.z);

    //    Debug.LogWarning(Settings);
    //}
}
