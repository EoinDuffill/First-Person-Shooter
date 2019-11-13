using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public enum Ammo { Pistol, Rifle, SMG, Sniper, Shotgun, Launcher }
    public enum StatTypes { Damage, ClipSize, FireRate, Accuracy, FirstShotAccuracy, Recoil, ReloadSpeed}
    public enum LootQuality { Bad, Standard, Good, Great }

    public const int AMMO_PISTOL_DEFAULT = 10;
    public const int AMMO_RIFLE_DEFAULT = 30;
    public const int AMMO_SMG_DEFAULT = 40;
    public const int AMMO_SHOTGUN_DEFAULT = 6;
    public const int AMMO_SNIPER_DEFAULT = 5;
    public const int AMMO_LAUNCHER_DEFAULT = 2;

    public const int COMMON = 0;
    public const int UNCOMMON = 1;
    public const int RARE = 2;
    public const int EPIC = 3;
    public const int LEGENDARY = 4;

    public const int NOT_ELEMENTAL = 0;
    public const int ELECTRIC = 25;
    public const int FLAME = 26;
    public const int ACID = 27;
    public const int POISON = 28;

    public const int CLIP_CURTAILED = -10;
    public const int CLIP_STANDARD = 0;
    public const int CLIP_EXTENDED = 10;

    public const int POWER_WEAK = -10;
    public const int POWER_STANDARD = 0;
    public const int POWER_STRONG = 10;
    public const int POWER_POWERFUL = 25;

    public const int FIRE_MODE_AUTO = 3;
    public const int FIRE_MODE_BURST = 2;
    public const int FIRE_MODE_SINGLE = 1;
    public const int FIRE_MODE_BOLT = 0;

    public static readonly int[] RIFLE_FIRE_MODES = { FIRE_MODE_AUTO, FIRE_MODE_BURST, FIRE_MODE_SINGLE};
    public static readonly int[] PISTOL_FIRE_MODES = { FIRE_MODE_SINGLE};
    public static readonly int[] SMG_FIRE_MODES = { FIRE_MODE_AUTO };

    public static readonly int[] WEIGHTS_BAD = { 50, 25, 15, 5, 1 };

    public static readonly int[] WEIGHTS_STANDARD = { 20, 15, 10, 4, 1 };

    public static readonly int[] WEIGHTS_GOOD = { 5, 3, 3, 3, 1 };

    public static readonly int[] WEIGHTS_GREAT = { 1, 2, 3, 4, 5 };

    public const int BARREL_SHORT = -10;
    public const int BARREL_NORMAL = 0;
    public const int BARREL_LONG = 10;

    public const int BARREL_ATTACH_NONE = 0;
    public const int BARREL_ATTACH_SILENCER = 10;

    public const int SIZE_HASTY = 25;
    public const int SIZE_NORMAL = 0;
    public const int SIZE_SLUGGISH = 15;

    public const int NO_STOCK = 0;
    public const int STOCK = 10;

    
}

public static class HelperFunctions
{
    //Accuracy: value 0 - 100 representing maximum angle deviating from Vector3.forward in an inversly proportional manner, maxArc: value 0 - 1 representing how much angle is considered, 1: 0 degrees,0: 180 degrees
    public static Vector3 fireInAccuracy(float accuracy, float maxArc)
    {

        
        float limit = maxArc * (1 - (accuracy / 100f));

        float randomRange1 = Random.Range(0f, 1f);
        int magnitude = (int)(2 * (Random.Range(0, 2) - 0.5f));
        randomRange1 = (Mathf.Pow(20, randomRange1 - 1) - (1 / 20f)) * magnitude;

        float randomRange2 = Random.Range(0f, 1f);
        magnitude = (int)(2 * (Random.Range(0, 2) - 0.5f));
        randomRange2 = (Mathf.Pow(20, randomRange2 - 1) - (1 / 20f)) * magnitude;

        //Calculating the raycast direction
        Vector3 direction = new Vector3(
            randomRange1 * limit,
            randomRange2 * limit,
            1f
        );


        return direction;
    }

}

public class GameController : MonoBehaviour
{

    public Transform gun;
    public Transform pistol;

    public WeaponGenerator weaponGenerator;

    // Start is called before the first frame update
    void Start()
    {
        weaponGenerator = GetComponent<WeaponGenerator>();

        for (int i = 0; i < 25; i++)
        {
           
            //weaponGenerator.generateWeapon(new List<Constants.Ammo>() {Constants.Ammo.Rifle}, new Vector3(-10, 2f, 4 * i), Quaternion.Euler(0, 90, -10));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
