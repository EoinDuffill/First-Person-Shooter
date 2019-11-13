using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemAmmo : MonoBehaviour, IInventoryItem
{
    
    public Constants.Ammo ammoType;
    public int ammoQuantity = 0;

    public string itemName { get; private set; } = "";
    public Sprite itemImage { get; set; }
    public Color itemColor { get; set; }
    public string statSheet { get; set; } = "";
    public bool inRange { get; private set; } = false;
    public float range { get; private set; }
    public bool isObtained { get; private set; } = false;
    public float dropTimeOutAmount { get; } = 1f;

    private float timeDropped = 0f;


    public void Start()
    {
        
        itemName = ammoType.ToString()+" Ammo";
        statSheet = ammoQuantity +" "+ ammoType.ToString() + " Ammo";

        if (ammoType == Constants.Ammo.Rifle)
        {
            itemColor = Color.blue;
        }
        else if (ammoType == Constants.Ammo.Pistol)
        {
            itemColor = Color.yellow;
        }
        else if(ammoType == Constants.Ammo.SMG)
        {
            itemColor = Color.green;
        }
        else if(ammoType == Constants.Ammo.Sniper)
        {
            itemColor = Color.cyan;
        }
        else if(ammoType == Constants.Ammo.Shotgun)
        {
            itemColor = Color.red;
        }
        else if(ammoType == Constants.Ammo.Launcher)
        {
            itemColor = Color.magenta;
        }
        gameObject.GetComponent<Renderer>().material.color = itemColor;
    }

    public void onPickup()
    {
        gameObject.SetActive(false);
        isObtained = true;
    }
    public void onDrop()
    {

        isObtained = false;
        
    }

    public bool pickUpReady()
    {
        return Time.time > dropTimeOutAmount + timeDropped;
    }

    public void updateStatSheet()
    {
        statSheet = ammoQuantity + " " + ammoType.ToString() + " Ammo";
    }

    public void updateHover()
    {
        GameObject player = GameObject.FindWithTag("Player");
        range = Vector3.Distance(player.transform.position, transform.position);
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            inRange = true;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            inRange = false;
        }
    }
}
