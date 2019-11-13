using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableItem : MonoBehaviour, IInventoryItem
{
    public string itemName { get; set; } = "";
    public Sprite itemImage { get; set; }
    public Color itemColor { get; set; }
    public EquipableItemStats statSheet { get; set; }
    public bool selected { get; private set; } = false;
    public bool inRange { get; private set; } = false;
    public float range { get; private set; }
    public bool isObtained { get; private set; } = false;
    public float dropTimeOutAmount { get; } = 1f;

    private float timeDropped = 0f;

    public void onPickup()
    {
        gameObject.transform.parent = GameObject.FindWithTag("MainCamera").transform;
        
        
        gameObject.transform.localPosition = gameObject.GetComponent<WeaponCreator>().viewTransform.localPosition;
        gameObject.transform.localRotation = gameObject.GetComponent<WeaponCreator>().viewTransform.localRotation;

        gameObject.GetComponent<Animator>().enabled = true;
        gameObject.GetComponent<Light>().enabled = false;

        isObtained = true;
        selected = true;

        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        if(gameObject.GetComponent<CapsuleCollider>() != null)
        {
            foreach (CapsuleCollider cc in gameObject.GetComponents<CapsuleCollider>())
            {
                cc.enabled = false;
            }
        }
        if(gameObject.GetComponent<BoxCollider>() != null)
        {
            foreach (BoxCollider bc in gameObject.GetComponents<BoxCollider>())
            {
                bc.enabled = false;
            }
        }
    }
    public void onDrop()
    {
        //Item no long on player
        gameObject.transform.parent = null;
        isObtained = false;
        timeDropped = Time.time;
        
        gameObject.GetComponent<Animator>().enabled = false;
        gameObject.GetComponent<Light>().enabled = true;

        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.right * 5, ForceMode.Impulse);
        }
        foreach (CapsuleCollider cc in gameObject.GetComponents<CapsuleCollider>())
        {
            cc.enabled = true;
        }
        foreach (BoxCollider bc in gameObject.GetComponents<BoxCollider>())
        {
            bc.enabled = true;
        }

    }

    public bool pickUpReady()
    {
        return Time.time > dropTimeOutAmount + timeDropped;
    }

    public void onSwitch(bool highlighted)
    {
        gameObject.GetComponent<Animator>().enabled = highlighted;
        gameObject.SetActive(highlighted);
        selected = highlighted;
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
