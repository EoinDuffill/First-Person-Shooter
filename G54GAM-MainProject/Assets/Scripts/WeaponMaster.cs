using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMaster : MonoBehaviour
{
    public Inventory inventory;
    
    private float nextFire = 0f;
    RaycastHit hit;
    private int layerMask;
    private bool canShootAgain = true;
    private bool shotFinish = true;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = (1 << 9) + (1 << 4);

        inventory = transform.Find("Inventory").GetComponent<Inventory>();

        inventory.WeaponReload += reload;
        inventory.WeaponShoot += shoot;
        inventory.WeaponFinishShoot += finishShoot;
    }

    public void reloadFinish(WeaponController weaponController, WeaponCreator weaponStats)
    {
        int ammoAvailable = inventory.ammoItems.requestAmmo(weaponStats.weaponType, weaponStats.clipSize - weaponController.clipCurrent);

        weaponController.clipCurrent += ammoAvailable;
        inventory.weaponUpdate();

    }

    public void reload(object sender, WeaponEventArgs e)
    {
        bool ammoAvailable = inventory.ammoItems.ammoAvailable(e.weaponStats.weaponType);

        if (!e.weapon.reloading && ammoAvailable && e.weapon.clipCurrent != e.weaponStats.clipSize)
        {
            e.weapon.reloading = true;
            
            //Start reload animation
            e.weapon.GetComponent<Animator>().SetTrigger("Reload");
        }

    }

    public void finishShoot(object sender, WeaponEventArgs e)
    {

        canShootAgain = true;
    }

    public void shoot(object sender, WeaponEventArgs e)
    {
        if(e.weaponStats.fireType == Constants.FIRE_MODE_BURST)
        {
            if (canShootAgain && shotFinish)
            {
                StartCoroutine(BurstFire(e));
            }
        }
        else if(canShootAgain)
        {
            fire(e);
        }

        if (e.weaponStats.waitForFireInput)
        {
            canShootAgain = false;
        }

    }
    
    private bool readyToFire()
    {
        return Time.time > nextFire;
    }

    private void fire(WeaponEventArgs e)
    {
        if(!e.weapon.reloading && e.weapon.clipCurrent > 0 && Time.time > nextFire)
        {
            //Calculate time at which another shot can be fired
            nextFire = Time.time + (60f / e.weaponStats.fireRate);

            //Begin shooting animation
            e.weapon.GetComponent<Animator>().SetTrigger("Shoot");

            //Start shot effect coroutine
            StartCoroutine(ShotEffect(e.weapon));

            e.weapon.laserSight.SetPosition(0, e.weaponStats.shotTransform.position);

            Vector3 rayOrigin = e.weapon.fpsCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));

            //Re-calculate accuracy for current shot
            e.weapon.calculateCurrentAccuracy();

            Vector3 direction = HelperFunctions.fireInAccuracy(e.weapon.accuracyCurrent, 1/6f);

            direction = e.weapon.fpsCamera.transform.TransformDirection(direction.normalized);

            //Shoot one bullet
            e.weapon.fireWeapon();
            if (Physics.Raycast(rayOrigin, direction, out hit, e.weaponStats.weaponRange, ~layerMask))
            {
                e.weapon.laserSight.SetPosition(1, hit.point);
                GameObject decal = Instantiate(e.weaponStats.bulletDecal, hit.point + (hit.normal * 0.025f), Quaternion.FromToRotation(Vector3.forward, hit.normal));
                decal.transform.parent = hit.collider.gameObject.transform;
            }
            else
            {
                e.weapon.laserSight.SetPosition(1, direction * e.weaponStats.weaponRange);
            }



            //Calculate if an enemy was hit, if so damage it
            Collider enemy = hit.collider;
            if (enemy != null)
            {


                if (enemy.GetComponent<EnemyController>() != null)
                {
                    enemy.GetComponent<EnemyController>().Damage(e.weaponStats.damage, gameObject);

                }
            }

        }

    }

    private IEnumerator BurstFire(WeaponEventArgs e)
    {
        shotFinish = false;

        for(int i = 0; i < 3; i++)
        {
            yield return new WaitUntil(() => readyToFire());
            fire(e);
            inventory.weaponUpdate();
        }

        shotFinish = true;
        nextFire = Time.time + (( 3*60f )/ e.weaponStats.fireRate);
    }

    private IEnumerator ShotEffect(WeaponController weapon)
    {
        if (weapon.weaponStats.Silencer)
        {
            weapon.gunAudio.clip = weapon.weaponStats.shotSilenced;
        }
        else
        {
            weapon.gunAudio.clip = weapon.weaponStats.shotUnSilenced;
        }
        weapon.gunAudio.Play();
        weapon.laserSight.enabled = true;
        
        yield return weapon.shotDuration;

        weapon.laserSight.enabled = false;


    }
}
