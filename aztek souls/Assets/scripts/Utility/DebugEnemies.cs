using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugEnemies : MonoBehaviour
{
    public BaseUnit[] EnemiesInScene;


    private void Awake()
    {
        EnemiesInScene = FindObjectsOfType<BaseUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        //Si revisas el código, acá solo tenés que cambiar el keycode :)
        if (Input.GetKeyDown(KeyCode.F2))
            foreach (var Enemy in EnemiesInScene)
                Enemy.gameObject.SetActive(!Enemy.gameObject.activeSelf);
    }


}
