using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    WeaponMaster weaponMaster;

    public WeaponCreator weaponStats;

    public Camera fpsCamera;
    public WaitForSeconds shotDuration = new WaitForSeconds(0.01f);
    public AudioSource gunAudio;
    public LineRenderer laserSight;

    public int clipCurrent = 0;
    public float accuracyCurrent = 0;
    public float timeSinceLastShot = 0;
    public float normRecoil;

    public bool reloading = false;

    public void Start()
    {
        weaponMaster = GameObject.FindWithTag("Player").GetComponent<WeaponMaster>();
        laserSight = GetComponent<LineRenderer>();
        gunAudio = GetComponent<AudioSource>();
        fpsCamera = Camera.main;

        accuracyCurrent = weaponStats.firstShotAccuracy;
        normRecoil = (weaponStats.recoil / 1.5f) + 2;

        clipCurrent = weaponStats.clipSize;
        timeSinceLastShot = Time.time;
    }

    public void reassignWeaponMaster()
    {
        fpsCamera = Camera.main;
        weaponMaster = GameObject.FindWithTag("Player").GetComponent<WeaponMaster>();
    }

    public void calculateCurrentAccuracy()
    {
        accuracyCurrent += (normRecoil*6) * (Time.time-timeSinceLastShot);
        if (accuracyCurrent > weaponStats.firstShotAccuracy) accuracyCurrent = weaponStats.firstShotAccuracy;
    }

    public void fireWeapon()
    {
        clipCurrent--;
        accuracyCurrent -= normRecoil;
        if (accuracyCurrent < weaponStats.accuracy) accuracyCurrent = weaponStats.accuracy;
        timeSinceLastShot = Time.time;
    }

    public void finishReload()
    {
        reloading = false;
        weaponMaster.reloadFinish(this, weaponStats);
    }

    public void drawGun()
    {
        if (reloading)
        {
            
            reloading = false;
        }
    }
    
}
