using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;


public class FarmerPandaBT : PandaBTScripts
{

    [Header("Farmer Specific")]
    [Tooltip("The farm plot this farmer works on")]
    public GameObject farmPlot;


    // Start is called before the first frame update
    protected override void Start()
    {
        //Call base.Start() because we still want to get all the references it does.
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Move to the farm plot
    /// </summary>
    [Task]
    public void MoveToFarmPlot()
    {
        //This is where we'll have the "farming" sequence be broke if they're hungry, at the beginning
        if (isAgentThirsty || isAgentHungry)
        {
            Debug.Log("I should be failing this move to farming");
            Task.current.Fail();
        }

        //Tell the agent to move to the waypoint of the farm plot
        agent.SetDestination(farmPlot.transform.GetChild(0).position);
        //When it gets close, move to the next task
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }


    //Basically, how many random places on the farm plot should the farmer mover to before 1 task of "Farming" is complete?
    private int timesToCycle = 5;
    //The tracker for that
    private int timesCycled = 0;

    /// <summary>
    /// Farm or food by moving to random positions on the plot
    /// </summary>
    [Task]
    public void Farm()
    {
        //Don't want food or hunger to interrupt this task in the middle of it, farming's just too important you know?
        /*
        if (isAgentThirsty || isAgentHungry)
        {
            Debug.Log("I should be failing this random dest");
            Task.current.Fail();
        }
        */

        //This basically says "If it becomes night while farming, go to bed", because it will mark the task as done, go through the behavior tree again and find it's time to sleep.
        //Then it will pick back up at the same cycle spot in the morning, even though the task was "Succeeded"
        if (!WithinWorkingHours())
        {
            //Reset thier speed
            curSpeed = maxSpeed;
            agent.speed = curSpeed;
            Task.current.Succeed();
            return;
        }

        //Reduce the agent speed
        curSpeed = 1;
        agent.speed = curSpeed;

        //If the agent has reached it's destination
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            //If we still need to cycle more.
            if (timesCycled < timesToCycle)
            {
                //Go to a new spot on the field.
                SetNewRandomDestinationInFarmPlot();
                timesCycled++;

                Debug.Log("Still Farming");
            }
            else
            {
                Debug.Log("Should be done farming");
                //So reset the speed and times cycled for the future
                timesCycled = 0;
                curSpeed = maxSpeed;
                agent.speed = curSpeed;


                //Also get hungrier and thirstier
                thirstValue++;
                hungerValue++;

                //Update if this person is hungry or thirsty
                UpdateAgentStatus();

                //Succeed this task
                Task.current.Succeed();
            }
        }

    }
    



    /// <summary>
    /// Sets the current position to a new random one on the assigned farm plot.
    /// </summary>
    public void SetNewRandomDestinationInFarmPlot()
    {
        Vector3 dest;
        dest = GetRandomPositionOnPlot();
        //agent.transform.LookAt(curDest.transform);

        agent.SetDestination(dest);
    }




    /// <summary>
    /// Get a random position on the farm plot currently assigned to this GameObject
    /// </summary>
    /// <returns>The random position</returns>
    private Vector3 GetRandomPositionOnPlot()
    {
        Vector3 randomPos = new Vector3();
        //Store the bounds of the plot
        Bounds farmBounds = farmPlot.GetComponent<BoxCollider>().bounds;
        //Make sure we're on top of the farm plot, and not sunk within it
        randomPos.y = farmBounds.center.y + farmBounds.extents.y;

        //Create the randomness in this random position.
        randomPos.x = Random.Range((farmBounds.center.x - farmBounds.extents.x), (farmBounds.center.x + farmBounds.extents.x));

        randomPos.z = Random.Range((farmBounds.center.z - farmBounds.extents.z), (farmBounds.center.z + farmBounds.extents.z));


        return randomPos;
    }
}
