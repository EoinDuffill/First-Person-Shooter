using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadableItem : MonoBehaviour, IInventoryItem
{
    //Vars to set in editor
    public string name = "";
    public Sprite itemIcon;
    

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
        itemName = name;
        itemImage = itemIcon;
    }

    public void onPickup()
    {
        gameObject.SetActive(false);

        //Set pos of player
        gameObject.transform.parent = GameObject.FindWithTag("MainCamera").transform;
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        gameObject.transform.localRotation = Quaternion.identity;

        isObtained = true;
        
    }
    public void onDrop()
    {
        gameObject.SetActive(true);

        gameObject.transform.parent = null;

        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 5, ForceMode.Impulse);
        }
        
        isObtained = false;

    }

    public bool pickUpReady()
    {
        return Time.time > dropTimeOutAmount + timeDropped;
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
