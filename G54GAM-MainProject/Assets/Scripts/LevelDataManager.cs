using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataManager : MonoBehaviour
{
    public static LevelDataManager instance = null;

    public Inventory inventoryToSave = null;
    public bool inventSaved = false;

    public bool shield = false;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
            //if not, set instance to this
            instance = this;
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            Destroy(gameObject);
        }
            // Then destroy this. This enforces the singleton pattern,
            // meaning there can only ever be one instance of a ScoreManager.
            
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    public void death()
    {

        Transform objects = gameObject.transform.Find("Objects");
        Transform cam = Camera.main.transform;

        cam.parent = objects;
    }

    public void reset()
    {
        inventSaved = false;
        //Camera.main.transform.parent = gameObject.transform;

        Transform objects = gameObject.transform.Find("Objects");
        Transform cam = Camera.main.transform;

        cam.parent = objects;
    }

    public void setData(Inventory inventory)
    {
        inventSaved = true;
        inventoryToSave = inventory;
        if(inventory.shield != null)
        {
            shield = true;
        }

        Transform objects = gameObject.transform.Find("Objects");
        Transform cam = Camera.main.transform;

        cam.parent = objects;
        
        
    }
}
