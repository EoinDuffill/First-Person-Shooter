using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{

    public int squadSize = 0;

    public GameObject Enemy;
    public List<EnemyController> enemies = new List<EnemyController>();
    public List<Transform> spawnPoints = new List<Transform>();
    public List<Waypoint> routeStarts = new List<Waypoint>();

    // Start is called before the first frame update
    void Start()
    {
        spawnSquad();

        
    }

    public void spawnSquad()
    {
        enemies = new List<EnemyController>();
        spawnPoints = new List<Transform>();
        routeStarts = new List<Waypoint>();

        Transform spParent = transform.Find("EnemySpawnPoints");
        foreach (Transform spawnP in spParent)
        {
            spawnPoints.Add(spawnP);
        }

        Transform routesTransform = transform.Find("WaypointRoutes");
        foreach (Transform routeTransform in routesTransform)
        {
            Transform startPoint = routeTransform.GetChild(0);
            if (startPoint.GetComponent<Waypoint>() != null)
            {
                routeStarts.Add(startPoint.GetComponent<Waypoint>());
            }
        }

        //Make sure squad size does not exceed the number of spawn points
        if (squadSize > spawnPoints.Count)
        {
            squadSize = spawnPoints.Count;
        }


        int routeIndex = 0;
        for (int i = 0; i < squadSize; i++)
        {
            //Ensure we assign a valid index for route
            if (routeIndex >= routeStarts.Count)
            {
                routeIndex = 0;
            }

            GameObject EnemyObject = Instantiate(Enemy, spawnPoints[i].position, Quaternion.identity, gameObject.transform);
            EnemyController ec = EnemyObject.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.waypoint = routeStarts[routeIndex];
                enemies.Add(ec);

                routeIndex++;
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
