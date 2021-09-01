using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class MinerPandaBT : PandaBTScripts
{

    [Header("Miner Specific")]
    [Tooltip("The mine this miner works at")]
    public GameObject mine;


    //Basically, how long should the miner be in the mine before 1 task of "Mining" is complete?
    private int workTime;
    //The tracker for that
    private float currentWork = 0;

    //The waypoint of the entrance to the mine
    private Transform mineEntranceWaypoint;
    //The location of the miner food drop-off point. This belongs to the mine they mine in.
    private GameObject rockDropoff;



    // Start is called before the first frame update
    protected override void Start()
    {



        //Get the transform of the plot's waypoint
        mineEntranceWaypoint = mine.transform.GetChild(0);
        //Get the GameObject of the dropoff stall.
        rockDropoff = mine.GetComponent<ResourceMine>().GetDropOff();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Find and set the agents destination to their assigned mine entrance.
    /// </summary>
    [Task]
    public void FindMineEntrance()
    {

    }

    /// <summary>
    /// Mine for whatever resource the mine has by moving the agent inside of the mine.
    /// </summary>
    [Task]
    public void Mine()
    {
        //Will have to override the enter building task.


    }

    /// <summary>
    /// Find and set the agents destination to an avaliable drop off
    /// </summary>
    [Task]
    public void FindRockDropOff()
    {

    }

    //Transfer the stored rock to the drop off.
    [Task]
    public void DropOffRock()
    {

    }


}
