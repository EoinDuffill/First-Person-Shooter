using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryItem
{
    string itemName { get; }

    Sprite itemImage { get; set; }

    Color itemColor { get; set; }
    
    
    bool inRange { get; }
    float range { get; }
    bool isObtained { get; }
    float dropTimeOutAmount { get; }

    bool pickUpReady();
    
    void updateHover();
    void onPickup();
    void onDrop();
}

public class OpenInventoryEventArgs : EventArgs
{
    public OpenInventoryEventArgs(bool invScreenOpen, bool itemOpen, AmmoItems ammoItems, ShieldItem shield, ref List<IInventoryItem> items)
    {
        this.invScreenOpen = invScreenOpen;
        this.itemOpen = itemOpen;
        this.ammoItems = ammoItems;
        this.shield = shield;
        this.items = items;
    }
    public bool invScreenOpen;
    public bool itemOpen;
    public AmmoItems ammoItems;
    public ShieldItem shield;
    public List<IInventoryItem> items;
}

public class EquipableInventEventArgs : EventArgs
{
    public EquipableInventEventArgs(EquipableItem item, int slot)
    {
        this.item = item;
        this.slot = slot;
    }
    public EquipableItem item;
    public int slot;
}

public class InventoryEventArgs : EventArgs
{
    public InventoryEventArgs(IInventoryItem item)
    {
        this.item = item;
    }
    public IInventoryItem item;
}

public class AmmoEventArgs : EventArgs
{
    public AmmoEventArgs(AmmoItem ammo, WeaponController weapon)
    {
        this.ammo = ammo;
        this.weapon = weapon;
    }
    public AmmoItem ammo;
    public WeaponController weapon;
}

public class WeaponEventArgs : EventArgs
{
    public WeaponEventArgs(WeaponCreator weaponCreator, WeaponController weaponController)
    {
        
        this.weaponStats = weaponCreator;
        this.weapon = weaponController;
    }
    public WeaponCreator weaponStats;
    public WeaponController weapon;



}