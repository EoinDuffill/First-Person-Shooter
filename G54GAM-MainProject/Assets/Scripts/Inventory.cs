using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AmmoItem
{
    public Constants.Ammo ammo;
    public int quantity;
    public int maxQuantity;
    public Sprite ammoImage;

    public AmmoItem(Constants.Ammo ammo, int quantity, int maxQuantity, Sprite ammoImage)
    {
        this.ammo = ammo;
        this.quantity = quantity;
        this.maxQuantity = maxQuantity;
        this.ammoImage = ammoImage;
    }
}

public class AmmoItems 
{
    public AmmoItem[] ammoItems;
    public Sprite ammoImage;

    public AmmoItems()
    {
        ammoImage = Resources.Load<Sprite>("Sprites/damageIcon");
        ammoItems = new AmmoItem[] {
        new AmmoItem(Constants.Ammo.Rifle, 0, 300,ammoImage),
        new AmmoItem(Constants.Ammo.Pistol, 0, 120,ammoImage),
        new AmmoItem(Constants.Ammo.SMG, 0, 450,ammoImage),
        new AmmoItem(Constants.Ammo.Sniper, 0, 64,ammoImage),
        new AmmoItem(Constants.Ammo.Shotgun, 0, 64,ammoImage),
        new AmmoItem(Constants.Ammo.Launcher, 0, 10,ammoImage) };
    }

    public int addAmmo(Constants.Ammo ammoType, int quantity, bool change)
    {
        int changedBy = 0;
        int startingAmmo = 0;
        for (int i = 0; i < ammoItems.Length; i++)
        {
            if (ammoItems[i].ammo == ammoType)
            {
                startingAmmo = ammoItems[i].quantity;

                ammoItems[i].quantity += quantity;
                if(ammoItems[i].quantity > ammoItems[i].maxQuantity)
                {
                    ammoItems[i].quantity = ammoItems[i].maxQuantity;
                }
                else if (ammoItems[i].quantity < 0)
                {
                    ammoItems[i].quantity = 0;
                }

                changedBy = Math.Abs(ammoItems[i].quantity - startingAmmo);

                if (!change)
                {
                    ammoItems[i].quantity = startingAmmo;
                }

                break;
            }
        }

        return changedBy;
    }

    public AmmoItem getAmmoItem(Constants.Ammo ammoType)
    {
        //Guranteed to match as enum non nullable
        AmmoItem ammo = ammoItems[0];
        for (int i = 0; i < ammoItems.Length; i++)
        {
            if(ammoType == ammoItems[i].ammo)
            {
                ammo = ammoItems[i];
            }
        }
        return ammo;
    }

    public int requestAmmo(Constants.Ammo ammoType, int quantity)
    {
        return addAmmo(ammoType, -quantity, true);
    }

    public bool ammoAvailable(Constants.Ammo ammoType)
    {
        return (addAmmo(ammoType, -1, false) > 0);
    }
}

public class Inventory : MonoBehaviour
{

    public List<IInventoryItem> items = new List<IInventoryItem>();

    public AmmoItems ammoItems = null;

    public ShieldItem shield = null;

    public struct ItemSlot
    {
        public readonly string[] itemTypes;
        public EquipableItem item;
        public ItemSlot(string[] types, EquipableItem i)
        {
            itemTypes = types;
            item = i;
        }
    }

    private readonly static string[] stdWeaponSlotTypes = { Constants.Ammo.Rifle.ToString(), Constants.Ammo.SMG.ToString(), Constants.Ammo.Shotgun.ToString(), Constants.Ammo.Sniper.ToString(), Constants.Ammo.Launcher.ToString()};
    private readonly static string[] pistolWeaponSlotTypes = { Constants.Ammo.Pistol.ToString() };
    public ItemSlot[] itemSlots = { new ItemSlot(stdWeaponSlotTypes, null), new ItemSlot(stdWeaponSlotTypes, null), new ItemSlot(pistolWeaponSlotTypes, null) };

    private int currentSlot = 0;
    public bool itemOpen = false;
    public bool inventoryScreenOpen = false;
    public bool inventoryOpen = false;
    public bool inventoryInit = false;

    public List<IInventoryItem> hoveredItems = new List<IInventoryItem>();
    public IInventoryItem currentClosestItem = null;

    public event EventHandler<EquipableInventEventArgs> ItemEquip;
    public event EventHandler<InventoryEventArgs> ItemHover;
    public event EventHandler<EquipableInventEventArgs> ItemSelect;
    public event EventHandler<WeaponEventArgs> WeaponReload;
    public event EventHandler<WeaponEventArgs> WeaponShoot;
    public event EventHandler<WeaponEventArgs> WeaponFinishShoot;
    public event EventHandler<AmmoEventArgs> AmmoChange;
    public event EventHandler AmmoNull;
    public event EventHandler<OpenInventoryEventArgs> OpenInventory;

    public void Start()
    {
        if(ammoItems == null)
        {
            ammoItems = new AmmoItems();
        }
       
    }

    public void Update()
    {
        if (!inventoryInit)
        {
            inventoryInit = true;
            weaponIconUpdate();
        }
    }

    public void toggleShowInventory()
    {
        //If an item is open close it, otherwise toggle invent screen open/closed
        if (itemOpen)
        {
            itemOpen = false;
        }
        else
        {
            inventoryScreenOpen = !inventoryScreenOpen;
        }


        //If either inventory component is open, then inventory is open
        if(itemOpen || inventoryScreenOpen)
        {
            inventoryOpen = true;

        }
        else
        {
            inventoryOpen = false;
        }

        if (OpenInventory != null)
        {
            OpenInventory.Invoke(this, new OpenInventoryEventArgs(inventoryScreenOpen ,itemOpen,ammoItems, shield, ref items));
        }
    }

    public void updateInventory()
    {
        //Set to closed, toggle will turn on and update inventory
        inventoryOpen = true;
        if (OpenInventory != null)
        {
            OpenInventory.Invoke(this, new OpenInventoryEventArgs(inventoryScreenOpen, itemOpen, ammoItems, shield, ref items));
        }
    }

    public void switchSlot(int slot)
    {
        currentSlot = slot;
        for(int i = 0; i < itemSlots.Length; i++)
        {
            if(itemSlots[i].item != null)
            {
                itemSlots[i].item.onSwitch(i == slot);
                if(ItemSelect != null)
                {
                    ItemSelect.Invoke(this, new EquipableInventEventArgs(itemSlots[i].item, i));
                    
                }

            }
        }
        weaponUpdate();
        hoverUpdate();
    }

    public void weaponIconUpdate()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (ItemEquip != null)
            {
                ItemEquip.Invoke(this, new EquipableInventEventArgs(itemSlots[i].item, i));
            }
        }


            
        switchSlot(0);
    }

    //Called when the weapon or ammo undergoes a change
    public void weaponUpdate()
    {
        //If current slot has a weapon, signal to the hud to update the ammo counts
        //Otherwise reset the ammo hud
        EquipableItem item = itemSlots[currentSlot].item;
        if(item != null)
        {
            WeaponCreator weaponC = item.gameObject.GetComponent<WeaponCreator>();
            WeaponController weapon = item.gameObject.GetComponent<WeaponController>();
            if (weaponC != null && weapon != null && AmmoChange != null)
            {
                AmmoChange.Invoke(this, new AmmoEventArgs(ammoItems.getAmmoItem(weaponC.weaponType), weapon));
            }
            else if (AmmoNull != null)
            {
                AmmoNull.Invoke(this, null);
            }
        }
        else
        {
            if( AmmoNull != null)
            {
                AmmoNull.Invoke(this, null);
            }
            
        }
        

    }

    public void hoverUpdate()
    {
        if (ItemHover != null)
        {
            ItemHover.Invoke(this, new InventoryEventArgs(closestItem()));
        }
    }

    public void reloadEquipped()
    {
        EquipableItem item = itemSlots[currentSlot].item;
        if (item != null)
        {
            WeaponCreator weaponC = item.gameObject.GetComponent<WeaponCreator>();
            WeaponController weapon = item.gameObject.GetComponent<WeaponController>();
            if (WeaponReload != null && weapon != null && weaponC != null)
            {
                WeaponReload.Invoke(this, new WeaponEventArgs(weaponC, weapon));
            }
        }
        weaponUpdate();
    }

    public void fireSelected()
    {
        EquipableItem item = itemSlots[currentSlot].item;
        if(item != null)
        {
            WeaponCreator weaponC = item.gameObject.GetComponent<WeaponCreator>();
            WeaponController weapon = item.gameObject.GetComponent<WeaponController>();
            if (WeaponShoot != null && weaponC != null && weapon != null)
            {
                WeaponShoot.Invoke(this, new WeaponEventArgs(weaponC, weapon));
            }

        }
        weaponUpdate();
    }

    public void finishFire()
    {
        EquipableItem item = itemSlots[currentSlot].item;
        if (item != null)
        {
            WeaponCreator weaponC = item.gameObject.GetComponent<WeaponCreator>();
            WeaponController weapon = item.gameObject.GetComponent<WeaponController>();
            if (WeaponShoot != null && weaponC != null && weapon != null)
            {
                WeaponFinishShoot.Invoke(this, new WeaponEventArgs(weaponC, weapon));
            }

        }
        weaponUpdate();
    }

    public void pickUpItem()
    {
        IInventoryItem item = closestItem();
        if (ableToPickUp(item))
        {
            if(item as EquipableItem != null)
            {
                EquipableItem eItem = (EquipableItem)item;
                //Find best slot for the item to go into
                int bestSlot = findBestSlot(eItem);
                if (bestSlot >= 0)
                {
                    //Switch slots to the best one 
                    switchSlot(bestSlot);
                    //Drop item in that slot if there is one
                    if (itemSlots[bestSlot].item != null)
                    {
                        dropItem();
                    }
                    //Equip item methods
                    if (ItemEquip != null)
                    {
                        ItemEquip.Invoke(this, new EquipableInventEventArgs(eItem, bestSlot));
                    }
                    eItem.onPickup();
                    //Place item in slot
                    itemSlots[bestSlot].item = eItem;
                    unHoverItem(item);
                }
            }
            else if(item as InventoryItemAmmo != null)
            {
                InventoryItemAmmo ammoItem = (InventoryItemAmmo)item;

                int ammoChange = ammoItems.addAmmo(ammoItem.ammoType, ammoItem.ammoQuantity, true);
                ammoItem.ammoQuantity -= ammoChange;

                if (ammoItem.ammoQuantity == 0)
                {
                    ammoItem.onPickup();
                    unHoverItem(item);
                }
                else
                {
                    ammoItem.updateStatSheet();
                }
            }
            else if(item as ShieldItem != null)
            {
                ShieldItem shieldItem = (ShieldItem) item;
                if (shield == null)
                {
                    //Pickup new shield
                    shieldItem.onPickup();
                    shield = shieldItem;
                }
                else
                {
                    //Switch shield
                    shield.onDrop();
                    shieldItem.onPickup();
                    shield = shieldItem;
                }
            }
            else if(item as ReadableItem != null)
            {
                ReadableItem readableItem = (ReadableItem) item;
                readableItem.onPickup();
                items.Add(readableItem);
            }
            else if(item as HealthPack != null)
            {
                HealthPack healthPack = (HealthPack)item;
                healthPack.onPickup();
            }
        }

        weaponUpdate();
        hoverUpdate();
        if (inventoryOpen)
        {
            updateInventory();
        }
    }
    public void dropItem()
    {
        if(itemSlots[currentSlot].item != null)
        {
            

            if (ItemEquip != null)
            {
                ItemEquip.Invoke(this, new EquipableInventEventArgs(null, currentSlot));
            }
            itemSlots[currentSlot].item.onDrop();
            itemSlots[currentSlot].item = null;
        }

        weaponUpdate();
        hoverUpdate();
    }

    public void dropSpecifiedItem(IInventoryItem item)
    {
        item.onDrop();
        items.Remove(item);
        if (inventoryOpen)
        {
            updateInventory();
        }
    }

    public void hoverItem(IInventoryItem item)
    {
        hoveredItems.Remove(item);
        if (!item.isObtained)
        {
            hoveredItems.Add(item);
            item.updateHover();
            if (ItemHover != null)
            {
                ItemHover.Invoke(this, new InventoryEventArgs(closestItem()));
            }
        }    
    }

    public void unHoverItem(IInventoryItem item)
    {
        hoveredItems.Remove(item);
        if (ItemHover != null)
        {
            ItemHover.Invoke(this, new InventoryEventArgs(closestItem()));
        }
    }

    public int getCurrentSlot()
    {
        return currentSlot;
    }

    public int findBestSlot(IInventoryItem item)
    {
        bool currEligible = false;
        bool openSlot = false;
        int bestSlot = -1;

        if (item as EquipableItem != null)
        {
            EquipableItem ei = (EquipableItem)item;
            WeaponCreator wc = ei.gameObject.GetComponent<WeaponCreator>();
            if( wc != null)
            {
                //Check if the current slot is either guranteed best slot or eligible to be
                foreach (string s in itemSlots[currentSlot].itemTypes)
                {
                    if (s.Contains(wc.weaponType.ToString()))
                    {
                        if (itemSlots[currentSlot].item == null)
                        {
                            return currentSlot;
                        }
                        currEligible = true;
                        bestSlot = currentSlot;
                    }
                }

                //Choose which slot is best, with the previous information about the current slot (as it is preferable)
                for (int i = itemSlots.Length - 1; i >= 0; i--)
                {
                    foreach (string s in itemSlots[i].itemTypes)
                    {
                        if (s.Contains(wc.weaponType.ToString()))
                        {
                            //Slots with no items in
                            if (itemSlots[i].item == null)
                            {
                                openSlot = true;
                                bestSlot = i;
                            }
                            //Slots with items in if current slot isn't eligible to take the item
                            if (!openSlot && !currEligible)
                            {
                                bestSlot = i;
                            }
                        }
                    }
                }
            }                  
        }
        return bestSlot;
    }

    public IInventoryItem closestItem()
    {
        return closestItem(null);
    }

    public IInventoryItem closestItem(IInventoryItem item)
    {
        float closestRange = float.MaxValue;
        bool closestRangeChange = false;
        if (item != null && !hoveredItems.Contains(item))
        {
            hoveredItems.Add(item);
        }

        if (!ableToPickUp(currentClosestItem))
        {
            currentClosestItem = null;
        }
        else if(currentClosestItem != null)
        {
            currentClosestItem.updateHover();
            closestRange = currentClosestItem.range;
        }

        foreach (IInventoryItem i in hoveredItems)
        {
            i.updateHover();
            if (i.range < closestRange && ableToPickUp(i))
            {
                closestRange = i.range;
                currentClosestItem = i;
                closestRangeChange = true;
            }
        }

        //If new closest item, we must alert the HUD
        if (closestRangeChange && ItemHover != null)
        {
            ItemHover.Invoke(this, new InventoryEventArgs(closestItem()));
        }

        if (currentClosestItem != null && ableToPickUp(currentClosestItem))
        {
            return currentClosestItem;
        }
        return null;
    }

    public bool ableToPickUp(IInventoryItem item)
    {
        return (item != null && item.inRange && !item.isObtained && item.pickUpReady());
    }
}
