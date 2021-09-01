using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMine : Building
{
    [Header("Mine Specific")]
    public GameObject rockDropOff;



    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetDropOff()
    {
        return rockDropOff;
    }
}
