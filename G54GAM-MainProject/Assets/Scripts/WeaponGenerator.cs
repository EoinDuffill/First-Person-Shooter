using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenerator : MonoBehaviour
{
    public Object[] objects = new Object[] { };
    public List<GameObject> weapons;
    public GameObject ammo;

    public static readonly int INDEX_POWER = 0;
    public static readonly int INDEX_ELEMENTAL = 1;
    public static readonly int INDEX_CLIP_SIZE = 2;
    public static readonly int INDEX_BARREL_TYPE = 3;
    public static readonly int INDEX_BARREL_ATTACHMENT = 4;
    public static readonly int INDEX_SIZE_MODIFIER = 5;
    public static readonly int INDEX_STOCK_TYPE = 6;

    public static readonly int NO_OF_MODIFIERS = 7;

    // Start is called before the first frame update
    void Start()
    {
        //Load all weapon prefabs
        objects = Resources.LoadAll("Prefabs/Weapons", typeof(GameObject));
        foreach(Object obj in objects)
        {
            if(obj as GameObject != null)
            {
                GameObject gObj = (GameObject)obj;
                if(gObj.GetComponent<WeaponCreator>() != null)
                {
                    weapons.Add(gObj);
                }
            }
        }
    }

    public void generateAmmo(List<Constants.Ammo> weaponTypes, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        
        

        int randType = Random.Range(0, weaponTypes.Count);
        InventoryItemAmmo ammoAttributes = ammo.GetComponent<InventoryItemAmmo>();

        ammoAttributes.ammoType = weaponTypes[randType];
        if(ammoAttributes.ammoType == Constants.Ammo.Pistol)
        {
            ammoAttributes.ammoQuantity = Constants.AMMO_PISTOL_DEFAULT;
        }
        else if (ammoAttributes.ammoType == Constants.Ammo.Rifle)
        {
            ammoAttributes.ammoQuantity = Constants.AMMO_RIFLE_DEFAULT;
        }
        else if (ammoAttributes.ammoType == Constants.Ammo.SMG)
        {
            ammoAttributes.ammoQuantity = Constants.AMMO_SMG_DEFAULT;
        }
        else if (ammoAttributes.ammoType == Constants.Ammo.Shotgun)
        {
            ammoAttributes.ammoQuantity = Constants.AMMO_SHOTGUN_DEFAULT;
        }
        else if (ammoAttributes.ammoType == Constants.Ammo.Sniper)
        {
            ammoAttributes.ammoQuantity = Constants.AMMO_SNIPER_DEFAULT;
        }
        else if (ammoAttributes.ammoType == Constants.Ammo.Launcher)
        {
            ammoAttributes.ammoQuantity = Constants.AMMO_LAUNCHER_DEFAULT;
        }

        Instantiate(ammo, spawnPosition, spawnRotation);
    }

    public void generateWeapon(List<Constants.Ammo> weaponTypes, int[] weights , Vector3 spawnPosition, Quaternion spawnRotation)
    {

        List<GameObject> weaponsAvailable = new List<GameObject>();
        GameObject spawnedWeapon = null ;
        float weightTotal = 0;

        //Determine valid weapons from the desired weapon types
        foreach(GameObject weapon in weapons)
        {
            WeaponCreator wc = weapon.GetComponent<WeaponCreator>();
            if (weaponTypes.Contains(wc.weaponType)){
                weaponsAvailable.Add(weapon);
                weightTotal += wc.spawnWeight;
            }
        }

        //Randomly gen one of the potential candidates
        float rng = Random.Range(0, weightTotal);
        foreach (GameObject weapon in weaponsAvailable)
        {
            WeaponCreator wc = weapon.GetComponent<WeaponCreator>();
            rng -= wc.spawnWeight;

            if (rng <= 0)
            {
                spawnedWeapon = wc.gameObject;
                break;
            }
        }
        
        spawnedWeapon = Instantiate(spawnedWeapon, spawnPosition, spawnRotation);
        WeaponStats weaponStats = new WeaponStats();
        Constants.Ammo weaponType = spawnedWeapon.GetComponent<WeaponCreator>().weaponType;

        //Always 5 weights, 
        int commonW = weights[0];
        int uncommonW = weights[1] + commonW;
        int rareW = weights[2] + uncommonW;
        int epicW = weights[3] + rareW;
        int legendaryW = weights[4] + epicW;

        
        
        int[] barrelAttachmentTypes = new int[] { Constants.BARREL_ATTACH_NONE, Constants.BARREL_ATTACH_SILENCER };
        int[] powerTypes = new int[] { Constants.POWER_WEAK, Constants.POWER_STANDARD, Constants.POWER_STRONG, Constants.POWER_POWERFUL };
        int[] clipTypes = new int[] { Constants.CLIP_CURTAILED, Constants.CLIP_STANDARD, Constants.CLIP_EXTENDED };
        int[] barrelTypes = new int[] { Constants.BARREL_SHORT, Constants.BARREL_NORMAL, Constants.BARREL_LONG };
        int[] sizeTypes = new int[] { Constants.SIZE_SLUGGISH, Constants.SIZE_NORMAL, Constants.SIZE_HASTY };
        int[] stockTypes = new int[] { Constants.NO_STOCK, Constants.STOCK };
        //TODO IMPLEMENT ELEMENTAL TYPES
        int[] elementalType = new int[] { Constants.NOT_ELEMENTAL };

        //Initialiser
        int[][] allAttributesLists = new int[][] { powerTypes, elementalType, barrelTypes, barrelAttachmentTypes, clipTypes, sizeTypes, stockTypes };
        //Ensure indexes allign with readonly definitions
        allAttributesLists[INDEX_POWER] = powerTypes;
        allAttributesLists[INDEX_ELEMENTAL] = elementalType;
        allAttributesLists[INDEX_BARREL_TYPE] = barrelTypes;
        allAttributesLists[INDEX_BARREL_ATTACHMENT] = barrelAttachmentTypes;
        allAttributesLists[INDEX_CLIP_SIZE] = clipTypes;
        allAttributesLists[INDEX_SIZE_MODIFIER] = sizeTypes;
        allAttributesLists[INDEX_STOCK_TYPE] = stockTypes;

        int minimumPoints = Constants.CLIP_CURTAILED + Constants.POWER_WEAK + Constants.BARREL_SHORT;
        int maximumPoints = Constants.CLIP_EXTENDED + Constants.POWER_POWERFUL + Constants.BARREL_LONG + Constants.BARREL_ATTACH_SILENCER + Constants.SIZE_HASTY + Constants.STOCK;
        int pointsRange = maximumPoints - minimumPoints;
        int points = 0;

        //APPLY WEAPON TYPE SPECIFIC MODIFIERS TO MODIFIER LIST
        //Rifle Generation
        if (weaponType == Constants.Ammo.Rifle)
        {
            
            float randFloat = Random.Range(0, 1f);
            if(randFloat < 0.5f)
            {
                weaponStats.fireMode = Constants.FIRE_MODE_AUTO;
            }
            else if(randFloat < 0.85)
            {
                weaponStats.fireMode = Constants.FIRE_MODE_BURST;
            }
            else
            {
                weaponStats.fireMode = Constants.FIRE_MODE_SINGLE;
            }
        }
        //Pistol Specific Settings
        else if (weaponType == Constants.Ammo.Pistol)
        {
            weaponStats.fireMode = Constants.PISTOL_FIRE_MODES[Random.Range(0, Constants.PISTOL_FIRE_MODES.Length)];
            allAttributesLists[INDEX_BARREL_TYPE] = new int []{ Constants.BARREL_NORMAL, Constants.BARREL_SHORT};
            allAttributesLists[INDEX_STOCK_TYPE] = new int[] { Constants.NO_STOCK};

            //Adjust min & max point cap based on pistol specifics
            maximumPoints -= Constants.STOCK;
            maximumPoints -= Constants.BARREL_LONG;
        }
        else if(weaponType == Constants.Ammo.SMG)
        {
            weaponStats.fireMode = Constants.SMG_FIRE_MODES[Random.Range(0, Constants.PISTOL_FIRE_MODES.Length)];
            allAttributesLists[INDEX_BARREL_TYPE] = new int[] { Constants.BARREL_NORMAL};
            //Adjust min & max point cap based on pistol specifics
            minimumPoints -= Constants.BARREL_SHORT;
            maximumPoints -= Constants.BARREL_LONG;
        }

        rng = Random.Range(0, legendaryW + 1);
        Debug.Log(commonW + ", " + uncommonW + ", " + rareW + ", " + epicW + ", " + legendaryW+", "+rng);

        //Determine rarity, and points limit for the weapon
        if (rng <= commonW)
        {
            //Common
            weaponStats.rarity = Constants.COMMON;
            points = Random.Range(minimumPoints, (int)(pointsRange * 0.15));
        }
        else if (rng <= uncommonW)
        {
            //Uncommon
            weaponStats.rarity = Constants.UNCOMMON;
            points = Random.Range((int)(pointsRange * 0.15), (int)(pointsRange * 0.3));
        }
        else if (rng <= rareW)
        {
            //Rare
            weaponStats.rarity = Constants.RARE;
            points = Random.Range((int)(pointsRange * 0.3), (int)(pointsRange * 0.5));
        }
        else if (rng <= epicW)
        {
            //Epic
            weaponStats.rarity = Constants.EPIC;
            points = Random.Range((int)(pointsRange * 0.5), (int)(pointsRange * 0.7));
        }
        else if (rng <= legendaryW)
        {
            //Legendary
            weaponStats.rarity = Constants.LEGENDARY;
            points = Random.Range((int)(pointsRange * 0.7), (int)(pointsRange * 1));

        }

        //Initialise for weapon's stats
        int[] weaponAttributeList = new int[NO_OF_MODIFIERS];
        weaponAttributeList[INDEX_POWER] = allAttributesLists[INDEX_POWER].Max();
        weaponAttributeList[INDEX_ELEMENTAL] = allAttributesLists[INDEX_ELEMENTAL].Max();
        weaponAttributeList[INDEX_BARREL_TYPE] = allAttributesLists[INDEX_BARREL_TYPE].Max();
        weaponAttributeList[INDEX_BARREL_ATTACHMENT] = allAttributesLists[INDEX_BARREL_ATTACHMENT].Max();
        weaponAttributeList[INDEX_CLIP_SIZE] = allAttributesLists[INDEX_CLIP_SIZE].Max();
        weaponAttributeList[INDEX_SIZE_MODIFIER] = allAttributesLists[INDEX_SIZE_MODIFIER].Max();
        weaponAttributeList[INDEX_STOCK_TYPE] = allAttributesLists[INDEX_STOCK_TYPE].Max();

        //Reduce the quality of each slot until it falls under the points limit for the weapon
        int attrNumber = allAttributesLists.Length;
        while (getPoints(weaponAttributeList) > points)
        {
            int i = Random.Range(0, attrNumber);
            selectLowerAttribute(ref weaponAttributeList[i], allAttributesLists[i]);
        }

        Debug.Log(getPoints(weaponAttributeList) + " - " +points);

        //Assign generated stats
        weaponStats.barrelAttachment = weaponAttributeList[INDEX_BARREL_ATTACHMENT];
        weaponStats.power = weaponAttributeList[INDEX_POWER];
        weaponStats.clipType = weaponAttributeList[INDEX_CLIP_SIZE];
        weaponStats.barrelType = weaponAttributeList[INDEX_BARREL_TYPE];
        weaponStats.sizeModifier = weaponAttributeList[INDEX_SIZE_MODIFIER];
        weaponStats.stock = weaponAttributeList[INDEX_STOCK_TYPE];
        weaponStats.elemental = weaponAttributeList[INDEX_ELEMENTAL];

        //Initialise the weapon creator with these stats
        spawnedWeapon.GetComponent<WeaponCreator>().Initialise(weaponStats);
    }

    public int getPoints(int[] weaponAttributeList)
    {
        int points = 0;

        foreach(int attri in weaponAttributeList)
        {
            points += attri;
        }

        return points;
    }

    public void selectLowerAttribute(ref int weaponAttr, int[] attributes)
    {
        List<int> lowerAttributes = new List<int>();

        for(int i = 0; i < attributes.Length; i++)
        {
            if(attributes[i] < weaponAttr)
            {
                lowerAttributes.Add(attributes[i]);
            }
        }

        int count = lowerAttributes.Count;
        if (count != 0)
        {
            int max = int.MinValue;
            foreach(int lowerAttri in lowerAttributes)
            {
                if(lowerAttri > max)
                {
                    max = lowerAttri;
                }
            }
            weaponAttr = max;
        }

    }
}
