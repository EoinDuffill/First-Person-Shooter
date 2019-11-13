using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayViewer : MonoBehaviour
{

    public float weaponRange = 500f;

    private Camera fpsCamera;

    // Start is called before the first frame update
    void Start()
    {
        fpsCamera = Camera.main;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(fpsCamera != null)
        {
            Vector3 lineOrigin = fpsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            Debug.DrawRay(lineOrigin, fpsCamera.transform.forward * weaponRange, Color.green);
        }
        else
        {
            fpsCamera = Camera.main;
        }

        
    }
}
