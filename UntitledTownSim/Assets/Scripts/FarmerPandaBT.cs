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


    //Basically, how many random places on the farm plot should the farmer mover to before 1 task of "Farming" is complete?
    private int timesToCycle;
    //The tracker for that
    private int timesCycled = 0;

    //The waypoint of the entrance to the field
    private Transform plotWaypoint;
    //The location of the farmers food drop-off point. This belongs to the field they farm.
    private GameObject foodDropoff;

    // Start is called before the first frame update
    protected override void Start()
    {
        //Call base.Start() because we still want to get all the references it does.
        base.Start();


        //Set the cycle times equal to what the farm is
        timesToCycle = farmPlot.GetComponent<FarmFieldStats>().farmMaxGrowth;

        //Later, if we have different crops, we'll need to reset timesToCycle when switching


        //Get the transform of the plot's waypoint
        plotWaypoint = farmPlot.transform.GetChild(0);
        //Get the GameObject of the dropoff stall.
        foodDropoff = farmPlot.transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Move to the farm plot
    /// </summary>
    [Task]
    public void FindFarmPlot()
    {
        //This is where we'll have the "farming" sequence be broke if they're hungry, at the beginning
        if (isAgentThirsty || isAgentHungry)
        {
            Debug.Log("I should be failing this move to farming");
            Task.current.Fail();
        }

        //Tell the agent to move to the waypoint of the farm plot
        agent.SetDestination(plotWaypoint.position);
        
        Task.current.Succeed();
    }


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

                //Tell the farm field that it's been grown
                farmPlot.GetComponent<FarmFieldStats>().IncreaseFieldGrowth();

                Debug.Log("Still Farming");
            }
            else
            {
                Debug.Log("Should be done farming");
                //So reset the speed and times cycled for the future
                timesCycled = 0;
                curSpeed = maxSpeed;
                agent.speed = curSpeed;

                //Add to the farmer the food that one cycle of this field provides
                carriedFood += farmPlot.GetComponent<FarmFieldStats>().providedFood;


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


    /// <summary>
    /// Move to the food dropoff location
    /// </summary>
    [Task]
    public void FindFoodDropOff()
    {
        //We're going to not break here if they just became hungry, and instead just allow them to dropoff food. This ensures it becomes avaliable to eat, should it then be the only source
        /*
        if (isAgentThirsty || isAgentHungry)
        {
            Debug.Log("I should be failing this move to farming");
            Task.current.Fail();
        }
        */


        //Instead we break if we have no food to drop-off
        if(carriedFood <= 0)
        {
            ///Debug.Log("No Food to drop off");
            Task.current.Fail();
            return;
        }

        //Otherwise...

        //Tell the agent to move to the waypoint of the food dropoff, which is nested
        agent.SetDestination(foodDropoff.transform.GetChild(0).position);

        Task.current.Succeed();

    }

    [Task]
    public void DropOffFood()
    {
        //Drop the food off into the stall.
        carriedFood = foodDropoff.GetComponent<FarmStall>().DepositFood(carriedFood);
        Debug.Log("Dropped off food");

        //Next task
        Task.current.Succeed();
    }




    /// <summary>
    /// Updates all stored values to use the provided plot's data.
    /// Essentially, change the farmers assigned plot to this plot.
    /// </summary>
    /// <param name="newPlot">The new plot</param>
    public void UpdateFarmPlot(GameObject newPlot)
    {
        throw new System.NotImplementedException();
    }
}
