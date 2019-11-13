using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAllySpawner : MonoBehaviour
{
    public SquadController allySpawn;
    public float spawnTimer = 15f;

    private bool alliesDead = false;
    private float spawnAllies = 0f;

    public bool bossesDead = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        alliesLeft();
        bossesAlive();
        if(Time.time > spawnAllies && alliesDead && !bossesDead)
        {
            Debug.Log("Spawning");
            alliesDead = false;
            allySpawn.spawnSquad();
        }
    }

    public void bossesAlive()
    {
        bossesDead = true;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<EnemyController>())
            {
                if(child.GetComponent<EnemyController>().health > 0)
                {
                    bossesDead = false;
                }
            }
        }
        
    }

    public int alliesLeft()
    {
        int allies = 0;
        foreach(Transform child in allySpawn.transform)
        {
            if (child.GetComponent<EnemyController>())
            {
                allies++;
            }
        }

        if (!alliesDead && allies == 0)
        {
            alliesDead = true;
            spawnAllies = Time.time + spawnTimer;
        }

        return allies;

    }
}
