using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class WeaponWindow : EditorWindow
{
    List<AttackNode> AllNodes;
    private GUIStyle Style;
    public string NewNodeName;
    private float ToolbarHeight = 100f;

    public AttackNode selectedNode;

    //Para el paneo.
    private bool _panningScreen;
    private Vector2 _graphPan;
    private Vector2 _originalMousePosition;
    private Vector2 _prevPan;
    private Rect _graphRect;

    public GUIStyle wrapTextFieldStyle;

    [MenuItem("CustomTools/Weapon Editor")]
    public static void OpenWindow()
    {
        var mysef = GetWindow<WeaponWindow>();
        //mysef.title = "Hola";
        mysef.minSize = new Vector2(400, 400);
        mysef.maxSize = new Vector2(600, 600);
        
        mysef.AllNodes = new List<AttackNode>();
        mysef.Style = new GUIStyle();
        mysef.Style.fontSize = 20;
        mysef.Style.alignment = TextAnchor.MiddleCenter;
        mysef.Style.fontStyle = FontStyle.BoldAndItalic;

        mysef._graphPan = new Vector2(0, mysef.ToolbarHeight);
        mysef._graphRect = new Rect(0, mysef.ToolbarHeight, 1000000, 100000);

        mysef.wrapTextFieldStyle = new GUIStyle(EditorStyles.textField);
        mysef.wrapTextFieldStyle.wordWrap = true;
    }

    public void OnGUI()
    {
        CheckMouseInput(Event.current);

        EditorGUILayout.BeginVertical(GUILayout.Height(100));
        EditorGUILayout.LabelField("Este es mi editor de Nodos", Style, GUILayout.Height(50));
            EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        NewNodeName = EditorGUILayout.TextField("Nombre: ", NewNodeName);
        EditorGUILayout.Space();
        if (GUILayout.Button("Create new Node", GUILayout.Height(30)))
            AddNode();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        //Le creo un cuadrado de color para el fondo, para que se vea mas interesante.
        _graphRect.x = _graphPan.x;
        _graphRect.y = _graphPan.y;
        EditorGUI.DrawRect(new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight), Color.gray);

        GUI.BeginGroup(_graphRect);
        BeginWindows();

        //Acá debería dibujar algo we.

        //Copy Paste
        var oriCol = GUI.backgroundColor;
        for (int i = 0; i < AllNodes.Count; i++)
        {
            foreach(var c in AllNodes[i].connected)
                Handles.DrawLine(new Vector2(AllNodes[i].myRect.position.x + AllNodes[i].myRect.width/2f, AllNodes[i].myRect.position.y + AllNodes[i].myRect.height/2f), new Vector2(c.myRect.position.x + c.myRect.width / 2f, c.myRect.position.y + c.myRect.height / 2f));
        }

        for (int i = 0; i < AllNodes.Count; i++)
        {
            if(AllNodes[i] == selectedNode)
                GUI.backgroundColor = Color.green;

            AllNodes[i].myRect = GUI.Window(i, AllNodes[i].myRect, DrawNode, AllNodes[i].nodeName);
            GUI.backgroundColor = oriCol;
        }
        

        EndWindows();
        GUI.EndGroup();
    }

    private void AddNode()
    {
        AllNodes.Add(new AttackNode(new Vector2( 0, 0), 200, 150, NewNodeName));
        NewNodeName = "";
        Repaint();
    }

    private void CheckMouseInput(Event currentEvent)
    {
        if (!_graphRect.Contains(currentEvent.mousePosition) || !(focusedWindow == this || mouseOverWindow == this))
            return;
        //Pan
        if (currentEvent.button == 2 && currentEvent.type == EventType.MouseDown)
        {
            _panningScreen = true;
            _prevPan = new Vector2(_graphPan.x, _graphPan.y);
            _originalMousePosition = currentEvent.mousePosition;
        }
        else if (currentEvent.button == 2 && currentEvent.type == EventType.MouseUp)
            _panningScreen = false;

        if (_panningScreen)
        {
            float newX, newY;
            newX = _prevPan.x + currentEvent.mousePosition.x + _originalMousePosition.x;
            newY = _prevPan.y + currentEvent.mousePosition.y + _originalMousePosition.y;

            _graphPan.x = newX > 0 ? 0 : newX;
            _graphPan.y = newY > 0 ? 0 : newY;

            Repaint();
        }

        //Muestro la context Menu
        if (currentEvent.button == 1 && currentEvent.type == EventType.MouseDown )
        {
            ContextMenuOpen();
        }

        //Selección del nodo.
        AttackNode currentNode = null;
        for (int i = 0; i < AllNodes.Count; i++)
        {
            AllNodes[i].CheckMouse(Event.current, _graphPan);
            if (AllNodes[i].OverNode) currentNode = AllNodes[i];
        }

        //Selección anterior.
        var prevSelected = currentNode;
        if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown)
        {
            if (currentNode != null)
                selectedNode = currentNode;
            else
                selectedNode = null;

            if (prevSelected != currentNode)
                Repaint();
        }

    }

    #region CONTEXT MENU
    private void ContextMenuOpen()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Mi primer item!"), false, PrimerItem);
        menu.AddDisabledItem(new GUIContent("un item desactivado :("));
        menu.AddItem(new GUIContent("LLENO DE OPCIONES/Como un supermercado"), false, PrimerItem);
        menu.AddDisabledItem(new GUIContent("LLENO DE OPCIONES/Como cuando tu amigo se deja el face abierto"));
        menu.ShowAsContext();
    }

    private void PrimerItem()
    {
        Debug.Log("no hago nada");
    }
    #endregion

    public void DrawNode(int id)
    {
        //le dibujamos lo que queramos al nodo...
        EditorGUILayout.BeginHorizontal();//==============================================================================

        EditorGUILayout.LabelField("Dialogo", GUILayout.Width(100));
        AllNodes[id].dialogo = EditorGUILayout.TextField(AllNodes[id].dialogo, wrapTextFieldStyle, GUILayout.Height(50));

        EditorGUILayout.EndHorizontal();//================================================================================

        AllNodes[id].duration = EditorGUILayout.FloatField("Duration", AllNodes[id].duration);
        var n = EditorGUILayout.TextField("Nodo:", "");
        if (n != "" && n != " ")
        {
            for (int i = 0; i < AllNodes.Count; i++)
            {
                if (AllNodes[i].nodeName == n)
                    AllNodes[id].connected.Add(AllNodes[i]);
            }
            Repaint();
        }

        if (!_panningScreen)
        {
            //esto habilita el arrastre del nodo.
            //pasandole como parámetro un Rect podemos setear que la zona "agarrable" a una específica.
            GUI.DragWindow();

            if (!AllNodes[id].OverNode) return;

            //clampeamos los valores para asegurarnos que no se puede arrastrar el nodo por fuera del "área" que nosotros podemos panear
            if (AllNodes[id].myRect.x < 0)
                AllNodes[id].myRect.x = 0;

            if (AllNodes[id].myRect.y < ToolbarHeight - _graphPan.y)
                AllNodes[id].myRect.y = ToolbarHeight - _graphPan.y;
        }
    }

}
