using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDCanvas : MonoBehaviour
{

    public Inventory inventory;
    public PlayerController playerController;

    public GameObject equipableStatsGround;
    public GameObject equipableStatsEquipped;
    public GameObject equipableStatLine;
    public GameObject equipableExtraAttri;
    public GameObject inventoryItemLine;
    public GameObject inventoryAmmoLine;

    public Canvas worldSpaceCanvas;

    public Button closeItemButton;

    public Sprite imageGreaterThan;
    public Sprite imageLessThan;
    public Sprite imageEqualTo;

    public Sprite healthDamage;
    public Sprite shieldDamage;

    public float alpha = 0;
    public float alphaDecayRate = 1f;
    Transform damageTransform;
    Image damageTakenImage;

    public bool bossFightUI = false;
    public EnemyController bossController = null;
    public RectTransform bossHealth;
    public Text bossStatus;
    public float startWidth;

    // Use this for initialization
    void Start()
    {
        inventory = GameObject.Find("Player/Inventory").GetComponent<Inventory>();

        damageTransform = transform.Find("DamageTaken");
        damageTakenImage = damageTransform.GetComponent<Image>();

        equipableStatsGround.SetActive(false);
        equipableStatsEquipped.SetActive(false);

        inventory.ItemEquip += InventoryItemEquipped;
        inventory.ItemHover += InventoryItemHover;
        inventory.ItemSelect += InventoryItemSelect;
        inventory.AmmoChange += InvenytoryAmmoChange;
        inventory.AmmoNull += InvenytoryAmmoNull;
        inventory.OpenInventory += InventoryOpen;

        playerController.HealthChange += ChangeHealth;
        playerController.ShieldChange += ChangeShield;
        playerController.DamageTaken += DamageTaken;
    }

    void OnDestroy()
    {
        inventory.ItemEquip -= InventoryItemEquipped;
        inventory.ItemHover -= InventoryItemHover;
        inventory.ItemSelect -= InventoryItemSelect;
        inventory.AmmoChange -= InvenytoryAmmoChange;
        inventory.AmmoNull -= InvenytoryAmmoNull;
        inventory.OpenInventory -= InventoryOpen;

        playerController.HealthChange -= ChangeHealth;
        playerController.ShieldChange -= ChangeShield;
        playerController.DamageTaken -= DamageTaken;
    }

    public void Update()
    {
        //Make world space canvas look in the correct direction to toward the camera
        worldSpaceCanvas.transform.LookAt((2 * worldSpaceCanvas.transform.position - Camera.main.transform.position));

        if (alpha > 0)
        {
            alpha -= alphaDecayRate * Time.deltaTime;
            if(alpha < 0)
            {
                alpha = 0;
            }
            damageTakenImage.color = new Color(damageTakenImage.color.r, damageTakenImage.color.g, damageTakenImage.color.b, alpha);
        }

        //handle boss fight ui
        if (bossFightUI)
        {
            handleBoss();
        }
    }

    public void handleBoss()
    {
        
        bossHealth.sizeDelta = new Vector2((bossController.health / bossController.healthMax) * startWidth, bossHealth.rect.height);
        bossStatus.text = "Boss Status: "+bossController.stateMachine.getStateName();

        if (bossController.health <= 0)
        {
            endBossFight();
        }

    }

    public void endBossFight()
    {
        bossFightUI = false;
        bossStatus.text = "Boss Defeated";
    }

    public void startBossFight(EnemyController enemyController)
    {
        bossFightUI = true;
        bossController = enemyController;
        transform.Find("BossFight").gameObject.SetActive(true);
        bossHealth = transform.Find("BossFight").Find("BossHealth").GetComponent<RectTransform>();
        bossStatus = transform.Find("BossFight").Find("BossStatus").GetComponent<Text>();
        startWidth = bossHealth.rect.width;
    }

    public void InventoryOpen(object sender, OpenInventoryEventArgs e)
    {

        GameObject inventoryHUD = gameObject.transform.Find("Inventory").gameObject;
        foreach (Transform child in inventoryHUD.transform.Find("ItemView").Find("Viewport").Find("Content").transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in inventoryHUD.transform.Find("AmmoStatus").transform)
        {
            Destroy(child.gameObject);
        }

        if (e.invScreenOpen)
        {
            inventoryHUD.SetActive(true);

            int itemRowOffset = 50;
            int baseOffset = 16;

            //Find content of scrollview
            GameObject items = inventoryHUD.transform.Find("ItemView").Find("Viewport").Find("Content").gameObject;

            items.GetComponent<RectTransform>().sizeDelta = new Vector2(items.GetComponent<RectTransform>().rect.x, itemRowOffset * e.items.Count + baseOffset);

            GameObject ammoStatus = inventoryHUD.transform.Find("AmmoStatus").gameObject;

            int i = 0;
            foreach(IInventoryItem item in e.items)
            {
                GameObject itemLine = Instantiate(inventoryItemLine, items.transform, false);
                itemLine.transform.localPosition -= new Vector3(0, itemRowOffset * i, 0);
                GameObject itemImage = itemLine.transform.Find("ItemImage").gameObject;
                itemImage.GetComponent<Image>().sprite = item.itemImage;
                itemImage.transform.Find("ItemName").GetComponent<Text>().text = item.itemName;

                
                Button itemOpen = itemLine.transform.Find("ButtonOpen").GetComponent<Button>();
                Button itemDrop = itemOpen.transform.Find("ButtonDrop").GetComponent<Button>();
                itemOpen.onClick.AddListener(() => openItem(item));
                itemDrop.onClick.AddListener(() => inventory.dropSpecifiedItem(item));
                i++;
            }

            i = 0;
            foreach(AmmoItem ammoItem in e.ammoItems.ammoItems)
            {
                if(ammoItem.quantity > 0)
                {
                    GameObject ammoLine = Instantiate(inventoryAmmoLine, ammoStatus.transform, false);
                    ammoLine.transform.localPosition -= new Vector3(0, 32 * i, 0);
                    GameObject ammoImage = ammoLine.transform.Find("AmmoImage").gameObject;
                    ammoImage.GetComponent<Image>().sprite = ammoItem.ammoImage;
                    ammoImage.transform.Find("AmmoInfo").GetComponent<Text>().text = ammoItem.ammo.ToString()+" Ammo: "+ammoItem.quantity+" - Max("+ammoItem.maxQuantity+")";

                    i++;
                }
                
            }
            
            
        }
        else
        {
            
            inventoryHUD.SetActive(false);
            
        }

        if (!e.itemOpen)
        {
            closeItem();
        }

    }

    public void openItem(IInventoryItem item)
    {
        if (inventory.itemOpen)
        {
            closeItemButton.onClick.Invoke();
        }

        if (item as ReadableItem != null)
        {
            ReadableItem readable = (ReadableItem)item;
            Canvas canvas = readable.gameObject.transform.Find("Canvas").GetComponent<Canvas>();
            canvas.gameObject.SetActive(true);
            canvas.transform.parent = GameObject.FindWithTag("Player").transform;

            //closeItemButton = null;
            closeItemButton = canvas.transform.Find("Panel").Find("ButtonClose").GetComponent<Button>();
            closeItemButton.onClick.AddListener(() => closeReadableItem(readable, canvas));
            inventory.itemOpen = true;
        }
    }

    public void closeItem()
    {
        if(closeItemButton != null)
        {
            closeItemButton.onClick.Invoke();
            inventory.itemOpen = false;
        }
        
    }

    public void closeReadableItem(ReadableItem item, Canvas canvas)
    {
        canvas.transform.parent = item.gameObject.transform;
        canvas.gameObject.SetActive(false);
        inventory.itemOpen = false;
    }

    public void DamageTaken(object sender, bool health)
    {
        alpha = 0.75f;
        if (health)
        {
            damageTakenImage.sprite = healthDamage;
        }
        else
        {
            damageTakenImage.sprite = shieldDamage;
        }

    }

    public void ChangeHealth(object sender, float e)
    {
        Transform panel = transform.Find("Health");
        panel.GetChild(0).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(e).ToString();
    }
    public void ChangeShield(object sender, float e)
    {
        Transform panel = transform.Find("Shield");
        panel.GetChild(0).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(e).ToString();
    }

    public void InvenytoryAmmoChange(object sender, AmmoEventArgs e)
    {
        Transform panel = transform.Find("Ammo");
        Transform ammoMax = panel.GetChild(0);
        Transform ammoCurrent = ammoMax.GetChild(0);

        Text textAmmoMax = ammoMax.GetComponent<Text>();
        Text textAmmoCurrent = ammoCurrent.GetComponent<Text>();
        if (textAmmoMax != null && textAmmoCurrent != null)
        {
            textAmmoCurrent.text = e.weapon.clipCurrent.ToString();
            textAmmoMax.text = "/ " + e.ammo.quantity;
        }
    }

    public void InvenytoryAmmoNull(object sender, EventArgs e)
    {
        Transform panel = transform.Find("Ammo");
        Transform ammoMax = panel.GetChild(0);
        Transform ammoCurrent = ammoMax.GetChild(0);

        Text textAmmoMax = ammoMax.GetComponent<Text>();
        Text textAmmoCurrent = ammoCurrent.GetComponent<Text>();
        if (textAmmoMax != null && textAmmoCurrent != null)
        {
            textAmmoMax.text = "/ --";
            textAmmoCurrent.text = "--";
        }

    }

    public void InventoryItemSelect(object sender, EquipableInventEventArgs e)
    {
        Transform panel = transform.Find("Slots");

        Transform slot = panel.transform.GetChild(e.slot);
        Image image = slot.GetComponent<Image>();
        if (image != null)
        {

            Color clr = slot.GetChild(0).GetComponent<Image>().color;
            if (e.item.selected)
            {
                slot.GetChild(0).GetComponent<Image>().color = new Color(clr.r, clr.g, clr.b, 0.9f);
            }
            else
            {
                slot.GetChild(0).GetComponent<Image>().color = new Color(clr.r, clr.g, clr.b, 100f / 255);
            }
        }
    }

    private void InventoryItemEquipped(object sender, EquipableInventEventArgs e)
    {
        Transform panel = transform.Find("Slots");
       
        Transform slot = panel.transform.GetChild(e.slot);

        Image image = slot.GetComponent<Image>();
        if (image != null)
        {
            if (e.item != null)
            {
                
                image.enabled = true;
                image.sprite = e.item.itemImage;
                image.color = e.item.itemColor;
                slot.GetChild(0).GetComponent<Image>().color = new Color(e.item.itemColor.r, e.item.itemColor.g, e.item.itemColor.b, 0.9f);
                
            }
            else
            {
                slot.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 100f/255);
                image.enabled = false;
            }
        }
    }

    private void InventoryItemHover(object sender, InventoryEventArgs e)
    {
        Transform panel = transform.Find("WeaponInfo");
        Text text = panel.Find("WeaponComparison").Find("ItemPickUpText").GetComponent<Text>();

        //Disable UI for ground & equipped compare
        equipableStatsGround.SetActive(false);
        equipableStatsEquipped.SetActive(false);

        //Set up pick up prompt text
        if (text != null)
        {
            if (e.item != null && inventory.ableToPickUp(e.item))
            {
                if (e.item as InventoryItemAmmo != null)
                {
                    InventoryItemAmmo aItem = (InventoryItemAmmo)e.item;
                    text.text = "Press F to Pick Up " + aItem.itemName + " (" + aItem.ammoQuantity + ")";
                }
                else
                {
                    text.text = "Press F to Pick Up " + e.item.itemName;
                }
                text.enabled = true;
            }
            else
            {
                text.enabled = false;
            }
        }
        
        //Destroy old comparison UI, as changes have been made
        for (int i = 1; i < equipableStatsGround.transform.childCount; i++)
        {
            Destroy(equipableStatsGround.transform.GetChild(i).gameObject);
        }
        for (int i = 1; i < equipableStatsEquipped.transform.childCount; i++)
        {
            Destroy(equipableStatsEquipped.transform.GetChild(i).gameObject);
        }

        if (e.item != null && inventory.ableToPickUp(e.item))
        {
            if (e.item as EquipableItem != null)
            {
                EquipableItem eItem = (EquipableItem)e.item;

                Transform comparisonPanel = panel.Find("WeaponComparison");
                
                equipableStatsGround.SetActive(true);
                equipableStatsGround.transform.GetChild(0).GetComponent<Text>().text = eItem.itemName;
                
                //Equipped item retrieved
                EquipableItem equippedItem = inventory.itemSlots[inventory.getCurrentSlot()].item;
                
                if (equippedItem != null)
                {
                    equipableStatsEquipped.SetActive(true);
                    equipableStatsEquipped.transform.GetChild(0).GetComponent<Text>().text = equippedItem.itemName;

                    //Generate Comparison UI for equipped item
                    expandEquiableStats(equippedItem, eItem, equipableStatsEquipped);

                }
                //Disable UI for equipped compare
                else
                {
                    equipableStatsEquipped.SetActive(false);
                }
                //Generate Comparison UI for ground item
                expandEquiableStats(eItem, equippedItem, equipableStatsGround);
            }
        }
    }

    public void expandEquiableStats(EquipableItem item, EquipableItem itemCompare, GameObject root)
    {
        int startOffset = -46;
        int offset = startOffset;

        if(item != null)
        {
            foreach (StatLine statLine in item.statSheet.statLines)
            {
                GameObject statline = Instantiate(equipableStatLine, root.transform, false);
                statline.GetComponent<RectTransform>().localPosition = new Vector3(0, offset, 0);
                //Get Statline children
                GameObject attrImage = statline.transform.GetChild(0).gameObject;
                GameObject compImage = statline.transform.GetChild(1).gameObject;

                attrImage.GetComponent<Image>().sprite = statLine.attributeImage;
                attrImage.transform.GetChild(0).GetComponent<Text>().text = statLine.attributeName;

                compImage.GetComponent<Image>().sprite = imageGreaterThan;

                //Set statline image, if better, worse or equal to other item
                if (itemCompare != null)
                {
                    foreach (StatLine sl in itemCompare.statSheet.statLines)
                    {
                        if (sl.statType == statLine.statType)
                        {
                            //If equal then display --
                            bool equal = sl.attributeValue == statLine.attributeValue;
                            //If ground value is larger then it is better
                            bool groundBetter = sl.attributeValue > statLine.attributeValue;
                            //Unless smaller values are desired, inwhich invert
                            if (!statLine.betterLarger) groundBetter = !groundBetter;

                            if (!equal && groundBetter)
                            {
                                compImage.GetComponent<Image>().sprite = imageLessThan;
                            }
                            else if (!equal && !groundBetter)
                            {
                                compImage.GetComponent<Image>().sprite = imageGreaterThan;
                            }
                            else
                            {
                                compImage.GetComponent<Image>().sprite = imageEqualTo;
                            }
                        }
                    }
                }
                compImage.transform.GetChild(0).GetComponent<Text>().text = statLine.attributeValue.ToString("0.0");
                offset -= 28;
            }

            offset += 16;
            string text = "";
            int count = 0;
            foreach (string attr in item.statSheet.extraAttributes)
            {
                text += attr;
                count++;
            }
            //Create ui text prefab for extra attributes
            GameObject extraText = Instantiate(equipableExtraAttri, root.transform, false);
            extraText.GetComponent<RectTransform>().sizeDelta = new Vector2(extraText.GetComponent<RectTransform>().rect.width, 24 * count);
            extraText.GetComponent<Text>().text = text;
            extraText.GetComponent<RectTransform>().localPosition = new Vector3(5, offset, 0);

            worldSpaceCanvas.transform.position = item.transform.position + new Vector3(0, 1f, 0);

            root.GetComponent<RectTransform>().sizeDelta = new Vector2(root.GetComponent<RectTransform>().rect.width, extraText.GetComponent<RectTransform>().rect.height + Math.Abs(offset) + 10);

        }

    }
}
