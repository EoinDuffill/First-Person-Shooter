using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    public Inventory inventory;

    float verticalVelocity = 0;

    public float jumpSpeed = 5;
    public float crouchModifier = 0.5f;
    public float sprintModifier = 1.6f;
    public float moveSpeed = 5;

    public float health = 43f;
    public float healthMax = 100f;

    private bool playerDead = false;
    public bool hasShield = true;
    public float shield = 100f;
    private float timeDmgLastTaken = 0;
    public float shieldRegenSpeed = 5f;
    public float shieldRegenTimeout = 10f;

    //Normal vector from 
    public Vector3 hitNormal;

    //Audio Source to play background music too
    public AudioSource audioSource;

    public EventHandler<float> HealthChange;
    public EventHandler<float> ShieldChange;
    public EventHandler<bool> DamageTaken;

    float y = 0f;
    CharacterController characterController;
    
    void Awake()
    {
        if (LevelDataManager.instance != null)
        {

            if (LevelDataManager.instance.inventSaved)
            {
                

                Debug.Log("trying");

                inventory.ammoItems = LevelDataManager.instance.inventoryToSave.ammoItems;
                inventory.items = LevelDataManager.instance.inventoryToSave.items;
                inventory.shield = LevelDataManager.instance.inventoryToSave.shield;
                inventory.itemSlots = LevelDataManager.instance.inventoryToSave.itemSlots;
                if (LevelDataManager.instance.shield)
                {
                    //LevelDataManager.instance.transform.GetChild(0).parent = Camera.main.transform;
                    hasShield = true;
                }
                
                Transform objects = LevelDataManager.instance.transform.GetChild(0);
                
                Transform cam = GameObject.Find("Player").transform.Find("Main Camera");
                Destroy(cam.gameObject);

                Transform newParent = GameObject.Find("Player").transform;
                Transform oldParent = objects.transform;
                while (oldParent.childCount > 0)
                {
                    Debug.Log("Moving:" + oldParent.GetChild(oldParent.childCount - 1));
                    oldParent.GetChild(oldParent.childCount - 1).SetParent(newParent, false);
                }
                

                foreach (Transform child in GameObject.Find("Player").transform.Find("Main Camera"))
                {
                    
                    if (child.GetComponent<WeaponController>() != null)
                    {
                        child.GetComponent<WeaponController>().reassignWeaponMaster();
                        child.GetComponent<EquipableItem>().onPickup();
                    }
                }
                
                LevelDataManager.instance.inventoryToSave = inventory;
                
                
            }
            else
            {
                foreach (Transform child in LevelDataManager.instance.transform.Find("Objects"))
                {
                    Destroy(child.gameObject);
                }
            }

            
           
        }
    }

    // Use this for initialization
    void Start()
    {

        //Reset cam pos
        
        Camera.main.transform.localPosition = new Vector3(0, 1, 0);
        Camera.main.transform.localRotation = Quaternion.identity;

        Screen.lockCursor = true;

        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        updateHealth();

        
    }

    void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Lock Mouse
        if (Input.GetKey(KeyCode.Escape))
        {
            Screen.lockCursor = false;
        }
        else if (inventory.inventoryOpen)
        {
            Screen.lockCursor = false;
        }
        else
        {
            Screen.lockCursor = true;
        }


        if (!inventory.inventoryOpen)
        {
            // rotate the player object about the Y axis
            float rotation = Input.GetAxis("Mouse X") * 2;
            transform.Rotate(0, rotation, 0);
            // rotate the camera (the player's "head") about its X axis

            float updown = Input.GetAxis("Mouse Y") * 2;
            if (y + updown > 90 || y + updown < -90)
            {
                updown = 0;
            }

            y += updown;
            Camera.main.transform.Rotate(-updown, 0, 0);
        }


        // moving forwards and backwards
        float forwardSpeed;
        float sprintValue = 1;
        float crouchValue = 1;
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            crouchValue = crouchModifier;
            characterController.height = 2f;
            Camera.main.transform.localPosition = new Vector3(0, characterController.height / 3f,0);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            characterController.height = 3f;
            Camera.main.transform.localPosition = new Vector3(0, characterController.height / 3f, 0);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("MoveVertical") > 0)
        {
            sprintValue = sprintModifier;
            
        }    
        forwardSpeed = (crouchValue * sprintValue * moveSpeed) * Input.GetAxis("MoveVertical");
        
        // moving left to right
        float lateralSpeed = crouchValue * moveSpeed * Input.GetAxis("MoveHorizontal");
        // apply gravity
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        
        if (getSlopeAngle() <= characterController.slopeLimit && characterController.isGrounded )
        {
            if (Input.GetButton("Jump"))
            {
                verticalVelocity = jumpSpeed;
            }
            else
            {
                verticalVelocity = 0;
            }         
        }
        //Make sure player is above water
        drownCheck();

        //Calculate current shield ammount;
        handleShield();

        //Item Pickup
        if (Input.GetKeyDown(KeyCode.F) )
        {   
            inventory.pickUpItem();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            inventory.dropItem();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            inventory.switchSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            inventory.switchSlot(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            inventory.switchSlot(2);
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            inventory.reloadEquipped();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventory.toggleShowInventory();
        }

        if (Input.GetButton("Fire1") && !inventory.inventoryOpen)
        {
            inventory.fireSelected();
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            inventory.finishFire();
        }

        Vector3 speed = new Vector3(lateralSpeed, verticalVelocity, forwardSpeed);
        speed = transform.rotation * speed;
        characterController.Move(speed * Time.deltaTime);


 
    }

    public void drownCheck()
    {
        RaycastHit hit;
        int waterLayerMask = 1 << 4;

        //Camera.main.transform.position;
        if (Physics.Raycast(Camera.main.transform.position, Vector3.up, out hit, 1f, waterLayerMask))
        {
            playerDeath();
            Debug.Log("Drowned");
        }
    }

    public void handleShield()
    {
        if (hasShield)
        {
            if(Time.time >= timeDmgLastTaken + shieldRegenTimeout && shield < 100f)
            {
                shield += shieldRegenSpeed * Time.deltaTime;
                if (shield > 100) shield = 100f;
                updateShield();
            }
        }
    }

    //Return the angle between the up vector and the last characterController hit normal vector
    public float getSlopeAngle()
    {
        
        return Mathf.Abs(Vector3.Angle(Vector3.up, hitNormal));
    }

    public void takeDamage(float damage)
    {
        //Damage shield, and take away left over from health
        float healthDamage = shieldDamage(damage);

        if(DamageTaken != null)
        {
            if (healthDamage > 0)
            {
                DamageTaken.Invoke(this, true);
            }
            else
            {
                DamageTaken.Invoke(this, false);
            }
            
        }

        health -= healthDamage;

        if (health <= 0)
        {
            health = 0;
            playerDeath();
        }

        
        updateHealth();
        updateShield();

    }

    public void healPlayer(float heal)
    {
        health += heal;
        if(health > healthMax)
        {
            health = healthMax;
        }
        updateHealth();
    }

    public float shieldDamage(float damage)
    {
        timeDmgLastTaken = Time.time;
        shield -= damage;
        if(shield < 0)
        {
            float overFlow = Mathf.Abs(shield);
            shield = 0;
            return overFlow;
        }
        else
        {
            return 0f;
        }
        
    }

    private void updateHealth()
    {
        if (HealthChange != null)
        {
            HealthChange.Invoke(this, health);
        }
    }
    private void updateShield()
    {
        if (ShieldChange != null)
        {
            ShieldChange.Invoke(this, shield);
        }
    }

    public void playerDeath()
    {
        if (!playerDead)
        {

            Debug.Log("dead");

            playerDead = true;
            GameObject.Find("LevelEnd").GetComponent<LevelSwitch>().reloadLevel(SceneManager.GetActiveScene().buildIndex);
        }
        
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.GetComponent<IInventoryItem>() != null)
        {
            inventory.unHoverItem(other.gameObject.GetComponent<IInventoryItem>());
        }
        if (other.gameObject.GetComponent<LootBoxController>() != null)
        {
            other.gameObject.GetComponent<LootBoxController>().closeBox();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        inventory.closestItem(other.gameObject.GetComponent<IInventoryItem>());
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<IInventoryItem>() != null)
        {
            inventory.hoverItem(other.gameObject.GetComponent<IInventoryItem>());

        }

        if (other.gameObject.GetComponent<LootBoxController>() != null)
        {
            other.gameObject.GetComponent<LootBoxController>().openBox();
        }

        if(other.gameObject.GetComponent<MusicSource>() != null)
        {
            MusicSource musicSource = other.gameObject.GetComponent<MusicSource>();
            if ((musicSource.fade == MusicSource.Fade.FadeIn || musicSource.fade == MusicSource.Fade.NoFade) && audioSource.clip == null || (audioSource.clip != null && audioSource.clip.name != musicSource.music.name))
            {
                musicSource.init(audioSource);
            }

            if(musicSource.fade == MusicSource.Fade.FadeOut && audioSource.clip != null && audioSource.isPlaying)
            {
                musicSource.init(audioSource);
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }
    
}
