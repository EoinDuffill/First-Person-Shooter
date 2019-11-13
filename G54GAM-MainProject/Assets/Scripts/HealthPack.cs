using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour, IInventoryItem
{
    public float healAmount = 25f;
    

    public string itemName { get; private set; } = "";
    public Sprite itemImage { get; set; }
    public Color itemColor { get; set; }
    public string statSheet { get; set; } = "";
    public bool inRange { get; private set; } = false;
    public float range { get; private set; }
    public bool isObtained { get; private set; } = false;
    public float dropTimeOutAmount { get; } = 1f;

    private float timeDropped = 0f;
    
    private PlayerController player;

    public void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        itemName = "Health Pack ("+healAmount+")";
    }

    public void onPickup()
    {
        player.GetComponent<PlayerController>().healPlayer(healAmount);
        gameObject.SetActive(false);
        isObtained = true;
    }
    public void onDrop()
    {

        isObtained = false;

    }

    public bool pickUpReady()
    {
        //Can pick up if health is not max
        return player.health < player.healthMax;
    }

    public void updateHover()
    {
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
