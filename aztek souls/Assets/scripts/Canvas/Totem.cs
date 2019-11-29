using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using UnityEngine.SceneManagement;


public class Totem : MonoBehaviour
{
    public SceneInterface ScnInt;
    bool LevelUpPanelWasOpened = false;
    public string levelUpAxis;
    public Canvas canvas;
    

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            if (!LevelUpPanelWasOpened)
                canvas.gameObject.SetActive(true);

            if(Input.GetButtonDown(levelUpAxis))
            {
                canvas.gameObject.SetActive(!canvas.gameObject.activeInHierarchy);
                Context.LevelupPanel = !Context.LevelupPanel;
                LevelUpPanelWasOpened = Context.LevelupPanel;
                
                ScnInt.EnableDisableLevelUpPanel(Context.LevelupPanel);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(canvas.gameObject.activeInHierarchy)
         canvas.gameObject.SetActive(false);
    }

 }
