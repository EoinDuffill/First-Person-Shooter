using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decalController : MonoBehaviour
{
    private float spawnTime;
    public float timeAlive;

    // Start is called before the first frame update
    void Start()
    {
        spawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time  > spawnTime + timeAlive)
        {
            Destroy(gameObject);
        }
    }
}
