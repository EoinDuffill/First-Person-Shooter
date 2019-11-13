using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxController : MonoBehaviour
{
    public bool spawnedLoot = false;
    public GameController gameController;
    public WeaponGenerator weaponGenerator;
    public Transform[] weaponSpawns;
    public Transform[] ammoSpawns;
    public List<Constants.Ammo> spawnTypes;
    public Constants.LootQuality quality;
    private int[] weights;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        weaponGenerator = GameObject.FindWithTag("Player").GetComponent<WeaponGenerator>();

        if(quality == Constants.LootQuality.Bad)
        {
            weights = Constants.WEIGHTS_BAD;
        }
        else if(quality == Constants.LootQuality.Standard)
        {
            weights = Constants.WEIGHTS_STANDARD;
        }
        else if(quality == Constants.LootQuality.Good)
        {
            weights = Constants.WEIGHTS_GOOD;

        }
        else if(quality == Constants.LootQuality.Great)
        {
            weights = Constants.WEIGHTS_GREAT;
            
        }


        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openBox()
    {
        gameObject.GetComponent<Animator>().SetTrigger("OpenBox");
        if (!spawnedLoot)
        {
            
            for (int i = 0; i < weaponSpawns.Length; i++)
            {
                
                weaponGenerator.generateWeapon(spawnTypes, weights, weaponSpawns[i].position, weaponSpawns[i].rotation);
            }

            for (int i = 0; i < ammoSpawns.Length; i++)
            {
                weaponGenerator.generateAmmo(spawnTypes, ammoSpawns[i].position, ammoSpawns[i].rotation);
            }

            spawnedLoot = true;
        }
    }

    public void closeBox()
    {
        gameObject.GetComponent<Animator>().SetTrigger("CloseBox");

    }

}
