using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StatLine
{
    public Sprite attributeImage;
    public string attributeName;
    public float attributeValue;
    public bool betterLarger;
    public Constants.StatTypes statType;

    public StatLine(Sprite im, string name, float val, bool bLarger, Constants.StatTypes type)
    {
        attributeImage = im;
        attributeName = name;
        attributeValue = val;
        betterLarger = bLarger;
        statType = type;
    }
}


public class WeaponCreator : MonoBehaviour
{
    public struct weaponPart
    {
        public bool active;
        public string tag;

        public weaponPart(bool b, string t)
        {
            active = b;
            tag = t;
        }
    }

    public float spawnWeight;
    public float damage;
    public int clipSize;
    public float fireRate;
    public float accuracy;
    public float firstShotAccuracy;
    public float recoil;
    public int fireType;
    private float reloadTime;

    public Sprite damageIcon;
    public Sprite accuracyIcon;
    public Sprite clipSizeIcon;
    public Sprite recoilIcon;
    public Sprite reloadIcon;
    public Sprite fireRateIcon;

    //Stats to display on weapon hover
    private EquipableItemStats stats ;

    //If not full auto, weapon requires multiple clicks for multiple shots
    public bool waitForFireInput = false;

    //Shot variables
    public float weaponRange = 1000f;
    public Transform shotTransform;
    public Transform viewTransform;

    //Weapon type governed by its ammo type, 1:1 relationship
    public Constants.Ammo weaponType;
    public string weaponName;

    //Image of the weapon
    public Sprite itemImage;
    //Decal left on bullet impact
    public GameObject bulletDecal;

    //The Weapons animator
    private Animator anim;

    //Sound clip
    public AudioClip shotSilenced;
    public AudioClip shotUnSilenced;

    public weaponPart[] weaponParts;

    private bool Grip = true;
    private bool Clip = true;
    private bool ClipCurtailed = false;
    private bool ClipExtended = false;
    private bool Barrel = true;
    private bool BarrelShort = false;
    private bool BarrelLong = false;
    private bool Stock = false;
    //Silencer public as required outside of class
    public bool Silencer = false;
    private bool Detail1 = false;
    private bool Detail2 = false;
    private bool Detail3 = false;
    private bool Haste = false;
    private bool Slow = false;

    private string rarity = "";
    private string elemental = "";
    private string sizeModifier = "";
    private string barrelAttachment = "";
    private string barrelType = "";
    private string clipCapacity = "";
    private string stock = "";
    private string power = "";
    private string fireMode = "";

    private Color colorRarity = new Color(0, 0, 0);

    public void Initialise(WeaponStats weapon)
    {

        float fsaMultiplier = 1;
        float accuracyMultiplier = 1;
        float recoilMultiplier = 1;
        float damageMultiplier = 1;
        float fireRateMultiplier = 1;
        float clipMultiplier = 1;
        float reloadMultiplier = 1;

        //Get Reload time
        anim = GetComponent<Animator>();
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "GunReload")
            {
                reloadTime = clip.length;

            }
        }

        fireType = weapon.fireMode;
        if (weapon.fireMode == Constants.FIRE_MODE_AUTO)
        {

            fireMode = "Full Auto\n";
        }
        else if (weapon.fireMode == Constants.FIRE_MODE_BURST)
        {
            fireMode = "Burst Fire\n";
            waitForFireInput = true;
            fireRateMultiplier += addMultiplier(0.25f, 0.05f);
        }
        else if (weapon.fireMode == Constants.FIRE_MODE_SINGLE)
        {
            fireMode = "Single Fire\n";
            waitForFireInput = true;
            if(weaponType == Constants.Ammo.Rifle)
            {
                damageMultiplier += addMultiplier(0.4f, 0.1f);
            }
            
        }
        else if (weapon.fireMode == Constants.FIRE_MODE_BOLT)
        {
            fireMode = "Bolt Action\n";
            waitForFireInput = true;

        }

        //
        if (weapon.barrelType == Constants.BARREL_SHORT)
        {
            BarrelShort = true;
            Barrel = false;
            accuracyMultiplier -= addMultiplier(0.1f, 0.025f);
            barrelType = "Short Barrelled\n";
        }
        else if(weapon.barrelType == Constants.BARREL_LONG)
        {
            BarrelLong = true;
            Barrel = false;
            accuracyMultiplier += addMultiplier(0.1f, 0.025f);
            barrelType = "Long Barrelled\n";
        }
        
        //Modify visual clip size
        if(weapon.clipType == Constants.CLIP_CURTAILED)
        {
            ClipCurtailed = true;
            Clip = false;
            clipMultiplier -= addMultiplier(0.35f, 0.15f);
            clipCapacity = "Curtailed Magazine\n";

        }
        else if(weapon.clipType == Constants.CLIP_EXTENDED)
        {
            ClipExtended = true;
            Clip = false;
            clipMultiplier += addMultiplier(0.35f, 0.15f);
            clipCapacity = "Extended Magazine\n";
        }


        

        //
        if (weapon.stock == Constants.STOCK)
        {
            Stock = true;
            stock = "Stock\n";
            recoilMultiplier -= addMultiplier(0.1f, 0.05f);
            accuracyMultiplier += addMultiplier(0.1f, 0.025f);
            fsaMultiplier += addMultiplier(0.2f, 0.05f);
        }

        
        //Add detail with more power
        if(weapon.power == Constants.POWER_WEAK)
        {
            Detail1 = false;
            Detail2 = false;
            power = "Weak\n";
            damageMultiplier -= addMultiplier(0.25f, 0.05f);
            fireRateMultiplier -= addMultiplier(0.1f, 0.05f);
        }
        else if(weapon.power == Constants.POWER_STANDARD)
        {
            Detail1 = true;
            Detail2 = false;
        }
        else if(weapon.power == Constants.POWER_STRONG)
        {
            Detail1 = true;
            Detail2 = true;
            power = "Strong\n";

            damageMultiplier += addMultiplier(0.2f, 0.1f);
            fireRateMultiplier += addMultiplier(0.1f, 0.05f);
        }
        else if(weapon.power == Constants.POWER_POWERFUL)
        {
            Detail1 = true;
            Detail2 = true;
            Detail3 = true;
            power = "Powerful\n";

            damageMultiplier += addMultiplier(0.3f, 0.1f);
            fireRateMultiplier += addMultiplier(0.2f, 0.05f);
        }

        if(weapon.sizeModifier == Constants.SIZE_HASTY)
        {
            Haste = true;
            sizeModifier = "Hasty\n";
            fireRateMultiplier += addMultiplier(0.25f, 0.1f);
            reloadMultiplier += addMultiplier(0.5f, 0.1f);
        }
        else if(weapon.sizeModifier == Constants.SIZE_SLUGGISH)
        {
            Slow = true;
            sizeModifier = "Sluggish\n";
            fireRate = fireRate/1.25f;
            reloadMultiplier -= addMultiplier(0.25f, 0.1f);
            damageMultiplier += addMultiplier(0.1f, 0.05f);
        }

        //BARREL ATTACHMENTS
        if (weapon.barrelAttachment == Constants.BARREL_ATTACH_SILENCER)
        {
            Silencer = true;
            barrelAttachment = "Suppressed\n";
            fsaMultiplier += addMultiplier(0.2f, 0.05f);
        }

        //APPLY COLOR MOD TO GUN BAED ON RARITY
        if (weapon.rarity == Constants.COMMON)
        {
            colorRarity = new Color(0.5f, 0.5f, 0.5f);
            rarity = "Common\n"; 
            weaponName = "Common " + weaponName;
        }
        else if (weapon.rarity == Constants.UNCOMMON)
        {
            colorRarity = new Color(0.2f, 0.8f, 0.2f);
            rarity = "Uncommon\n";
            weaponName = "Uncommon " + weaponName;
        }
        else if (weapon.rarity == Constants.RARE)
        {
            colorRarity = new Color(0.2f, 0.2f, 0.9f);
            rarity = "Rare\n";
            weaponName = "Rare " + weaponName;
            damageMultiplier += addMultiplier(0.1f, 0.05f);
        }
        else if (weapon.rarity == Constants.EPIC)
        {
            colorRarity = new Color(0.66f, 0.25f, 1f);
            rarity = "Epic\n";
            weaponName = "Epic " + weaponName;
            damageMultiplier += addMultiplier(0.3f, 0.05f);
        }
        else if (weapon.rarity == Constants.LEGENDARY)
        {
            colorRarity = new Color(1f, 0.9f, 0.25f);
            rarity = "Legendary\n";
            weaponName = "Legendary " + weaponName;
            damageMultiplier += addMultiplier(0.6f, 0.05f);
        }

        //Assign reload multiplier
        reloadTime /= reloadMultiplier;
        anim.SetFloat("reloadMultiplier", reloadMultiplier);

        float accuracyMax = 100;
        float fsaMax = 100;
        float recoilMax = 10;

        damage *= damageMultiplier;
        clipSize = (int)(clipSize * clipMultiplier);
        fireRate *= fireRateMultiplier;



        accuracy = accuracy + ((accuracyMax - accuracy)*(accuracyMultiplier-1));
        recoil = recoil + ((recoilMax - recoil) * (recoilMultiplier - 1));
        firstShotAccuracy = firstShotAccuracy + ((fsaMax - firstShotAccuracy) * (fsaMultiplier - 1));




        gameObject.GetComponent<EquipableItem>().itemName = weaponName;
        gameObject.GetComponent<EquipableItem>().itemColor = colorRarity;
        gameObject.GetComponent<EquipableItem>().itemImage = itemImage;
        gameObject.GetComponent<EquipableItem>().statSheet = weaponStatSheet(weapon);
        gameObject.GetComponent<Light>().enabled = true;
        gameObject.GetComponent<Light>().color = colorRarity;
        gameObject.GetComponent<Light>().range = 1 + weapon.rarity/2f;
    }

    // Start is called before the first frame update
    void Start()
    {
        



        weaponParts = new weaponPart[16];

        weaponParts[0] = new weaponPart(Grip, "Grip");
        weaponParts[1] = new weaponPart(true, "Frame");
        weaponParts[2] = new weaponPart(Clip, "Clip");
        weaponParts[3] = new weaponPart(ClipCurtailed, "ClipCurtailed");
        weaponParts[4] = new weaponPart(ClipExtended, "ClipExtended");
        weaponParts[5] = new weaponPart(Barrel, "Barrel");
        weaponParts[6] = new weaponPart(BarrelShort, "BarrelShort");
        weaponParts[7] = new weaponPart(BarrelLong, "BarrelLong");
        weaponParts[8] = new weaponPart(Stock, "Stock");
        weaponParts[9] = new weaponPart(Silencer, "Silencer");
        weaponParts[10] = new weaponPart(Detail1, "Detail1");
        weaponParts[11] = new weaponPart(Detail2, "Detail2");
        weaponParts[12] = new weaponPart(Detail3, "Detail3");
        weaponParts[13] = new weaponPart(true, "Rarity");
        weaponParts[14] = new weaponPart(Haste, "Haste");
        weaponParts[15] = new weaponPart(Slow, "Slow");

        iterateOverChildren(transform);

        


    }

    void iterateOverChildren(Transform weapon)
    {
        foreach(Transform weaponComponent in weapon)
        {

            if (weaponComponent.childCount > 0)
            {
                iterateOverChildren(weaponComponent);
            }

            for (int i = 0; i < weaponParts.Length; i++)
            {
                if (weaponParts[i].tag == weaponComponent.tag)
                {
                    if (weaponParts[i].active)
                    {

                        weaponComponent.gameObject.SetActive(true);
                        if(weaponComponent.GetComponent<Renderer>() != null && weaponComponent.tag == "Rarity")
                        {
                            weaponComponent.GetComponent<Renderer>().material.color = colorRarity;
                        }
                        //When the barrel is found, activate the respective shot transform for the weapon, as it depends on the barrel length
                        if(weaponComponent.tag == "Barrel" || weaponComponent.tag == "BarrelLong" || weaponComponent.tag == "BarrelShort")
                        {

                            if (Silencer)
                            {
                                foreach (Transform silentT in weaponComponent)
                                {

                                    if (silentT.tag == "Silencer")
                                    {

                                        foreach (Transform t in silentT)
                                        {

                                            if (t.tag == "ShotTransform")
                                            {
                                                t.gameObject.SetActive(true);
                                                shotTransform = t;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (Transform t in weaponComponent)
                                {

                                    if (t.tag == "ShotTransform")
                                    {
                                        t.gameObject.SetActive(true);
                                        shotTransform = t;
                                    }
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        weaponComponent.gameObject.SetActive(false);
                    }
                }
            }           
        }
    }

    public float addMultiplier(float val, float plusMinus)
    {
        return val + Random.Range(-plusMinus, plusMinus);
    }

    EquipableItemStats weaponStatSheet(WeaponStats weapon)
    {

        damageIcon = Resources.Load<Sprite>("Sprites/damageIcon");
        accuracyIcon = Resources.Load<Sprite>("Sprites/accuracyIcon");
        clipSizeIcon = Resources.Load<Sprite>("Sprites/clipSizeIcon");
        recoilIcon = Resources.Load<Sprite>("Sprites/recoilIcon");
        reloadIcon = Resources.Load<Sprite>("Sprites/reloadIcon");
        fireRateIcon = Resources.Load<Sprite>("Sprites/fireRateIcon");

        stats = gameObject.GetComponent<EquipableItemStats>();

        stats.statLines.Add(new StatLine(damageIcon, "Damage", damage, true, Constants.StatTypes.Damage));
        stats.statLines.Add(new StatLine(fireRateIcon, "Fire Rate", fireRate, true, Constants.StatTypes.FireRate));
        stats.statLines.Add(new StatLine(reloadIcon, "Reload Speed", reloadTime, false, Constants.StatTypes.ReloadSpeed));
        stats.statLines.Add(new StatLine(recoilIcon, "Recoil", recoil, false, Constants.StatTypes.Recoil));
        stats.statLines.Add(new StatLine(clipSizeIcon, "Clip Size", clipSize, true, Constants.StatTypes.ClipSize));
        stats.statLines.Add(new StatLine(accuracyIcon, "Accuracy", accuracy, true, Constants.StatTypes.Accuracy));
        stats.statLines.Add(new StatLine(accuracyIcon, "First Shot", firstShotAccuracy, true, Constants.StatTypes.FirstShotAccuracy));

        if(rarity != "")
        {
            stats.extraAttributes.Add(rarity);

        }
        if (fireMode != "")
        {
            stats.extraAttributes.Add(fireMode);

        }
        if (power != "")
        {
            stats.extraAttributes.Add(power);

        }
        if (sizeModifier != "")
        {
            stats.extraAttributes.Add(sizeModifier);

        }
        if (barrelAttachment != "")
        {
            stats.extraAttributes.Add(barrelAttachment);

        }
        if (barrelType != "")
        {
            stats.extraAttributes.Add(barrelType);

        }
        if (stock != "")
        {
            stats.extraAttributes.Add(stock);

        }
        if(clipCapacity != "")
        {
            stats.extraAttributes.Add(clipCapacity);
        }
        
       
        return stats;
    }

    
}
