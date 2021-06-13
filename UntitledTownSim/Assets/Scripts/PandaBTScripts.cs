using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class PandaBTScripts : MonoBehaviour
{
    protected NavMeshAgent agent;
    GameObject[] waypoints;
    protected GameObject curDest;


    [Tooltip("The max/default speed of the agent")]
    public float maxSpeed = 3.5f;
    //The current speed of the agent, which can be affected by hunger
    protected float curSpeed;

    [Header("Thirst Details")]
    public float thirstValue = 0;
    public bool isAgentThirsty = false;

    [Header("Hunger Details")]
    public float hungerValue = 0;
    public bool isAgentHungry = false;
    [Tooltip("Not for eating! Just moving from place to place")]
    public float carriedFood = 0;
    [Tooltip("The max amount of food this agent can carry on their person")]
    public float maxCarriedFood = 25;


    [Header("Time-Based Details")]
    [Tooltip("What hour of the day it is. Set by Lighting Manager")]
    [SerializeField, Range(0, 24)] private int TimeOfDay;

    [Tooltip("The period of time in which this agent will be active. Inclusive")]
    public Vector2 preferredWorkingHours = new Vector2();

    //This should be a waypoint that has an extra script attached to it with the GameObject that this waypoint is for attached to it.
    [Tooltip("Where the villager will go when they are outside of their working hours")]
    public GameObject restingLocation;

    protected virtual void Start()
    {
        //Set certain agent values
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0.6f;
        //Set the speeds
        curSpeed = maxSpeed;
        agent.speed = curSpeed;

        //Get all of the waypoints
        waypoints = GameObject.FindGameObjectsWithTag("waypoint");
        if (waypoints.Length <= 0) Debug.Log("No waypoints found!");
    }

    /// <summary>
    /// Called by TimeManager to tell the agent what hour it is.
    /// </summary>
    /// <param name="hour">The hour</param>
    public void UpdateTime(int hour)
    {
        TimeOfDay = hour;

        //If the behaviour tree is off
        if (!this.GetComponent<PandaBehaviour>().enabled)
        {
            //And the agent is in the working hours...
            if (WithinWorkingHours())
            {
                //That means they're probably asleep in a building, so wake up, get up, get out there.
                restingLocation.GetComponent<Building>().ExitBuilding(this.gameObject);
            }
        }
    }


    [Task]
    public void SetNewRandomDestination()
    {
        if (isAgentThirsty || isAgentHungry)
        {
            Debug.Log("I should be failing this random dest");
            Task.current.Fail();
        }
        Vector3 dest; 
        if (waypoints.Length > 0)
        {
            curDest = waypoints[Random.Range(0, waypoints.Length - 1)];
            agent.transform.LookAt(curDest.transform);
            dest = curDest.transform.position;
        }
        else
        {
            dest = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        }

        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void MoveToNewDestination()
    {
        if (Task.isInspected) // showing in the inspector 
        {
            
            //If there is no current destination...
            if (curDest == null)
            {
                //Don't freak out and break the agent, instead just alert us.
                //Debug.Log("The inspected agent had no destination for a moment. This could be intentional.");
            }
            else
            {
            
                Task.current.debugInfo = string.Format(curDest.ToString() + " t= {0:0.00}", Time.time);
            }
        }
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            //Make the agent more thirsty every time it moves
            thirstValue++;
            //make the agent more hungry every time it moves
            hungerValue++;
            //If we're sorta hungry
            if (hungerValue >= 5)
            {
                //Reduce the agent speed little by little
                curSpeed = maxSpeed - (hungerValue / 10) - 0.5f;
                agent.speed = curSpeed;
            }

            //Update if this person is hungry or thirsty
            UpdateAgentStatus();
            Task.current.Succeed();
        }
    }


    /// <summary>
    /// Find a well, which we know because it starts with Well
    /// </summary>
    [Task]
    public void FindWell()
    {
        //Debug.Log("Finding a well");
        //If the agent is not thirsty, end this sequence
        if (!isAgentThirsty)
        {
            //Debug.Log("Not thirsty");
            Task.current.Fail();
            return;
        }


        Vector3 dest = Vector3.zero;
        if (waypoints.Length > 0)
        {
            //Check all the waypoints for an avaliable well.
            foreach(GameObject waypoint in waypoints)
            {
                if (waypoint.name.StartsWith("WellWaypoint"))
                {
                    curDest = waypoint;
                    agent.transform.LookAt(curDest.transform);
                    dest = curDest.transform.position;
                }
            }
            //If there isn't a well avaliable, fall back to a random destination
            if(dest == Vector3.zero)
            {
                SetNewRandomDestination();
            }
            
        }
        else
        {
            SetNewRandomDestination();
        }


        agent.SetDestination(dest);
        Task.current.Succeed();
    }


    /// <summary>
    /// Find a MarketStall, which we know because it starts with MarketWaypoint
    /// </summary>
    [Task]
    public void FindMarketStallWithFood()
    {
        //Debug.Log("Finding a stall");

        if (!isAgentHungry)
        {
            //Debug.Log("Not Hungry");
            Task.current.Fail();
            return;
        }


        Vector3 dest = Vector3.zero;
        if (waypoints.Length > 0)
        {
            //Check all the waypoints for an avaliable MarketStall
            foreach (GameObject waypoint in waypoints)
            {
                if (waypoint.name.StartsWith("MarketWaypoint"))
                {
                    //If the market stall we found has no food in it, give up and try the next stall
                    if (waypoint.transform.parent.gameObject.GetComponent<MarketStall>().CheckFoodLevels() <= 0)
                    {
                        //So do nothing here
                    }
                    //if it has any food at all...
                    else 
                    {
                        //Perhaps reserve that food for ourself so it doesn't get taken by someone else?


                        //Set it as the destination for pathfinding
                        curDest = waypoint;
                        agent.transform.LookAt(curDest.transform);
                        dest = curDest.transform.position;
                    }
                }
            }
            //If there isn't a Market Stall avaliable, fall back to a random destination
            if (dest == Vector3.zero)
            {
                SetNewRandomDestination();
            }

        }
        else
        {
            SetNewRandomDestination();
        }


        agent.SetDestination(dest);
        Task.current.Succeed();
    }




    //Make sure to wiggle the time in the lighting manager back and forth when adding new agents
    /// <summary>
    /// Head to the agents resting location
    /// </summary>
    [Task]
    public void GoToBed()
    {
        if (WithinWorkingHours())
        {
            //Debug.Log("Still within working hours");
            Task.current.Fail();
            return;
        }
        //Eventually maybe an ovveride if they're very tired they'll go to bed anyways.

        //By this point we know we're outside of this agents working hours, so they need to move to their resting location.
        curDest = restingLocation;
        agent.SetDestination(curDest.transform.position);
        Task.current.Succeed();
        //After this will be another task where they enter/exit a house.
    }


    [Task]
    public void EnterHouse()
    {
        //If we make it to this task, we will always do it.
        restingLocation.GetComponent<Building>().EnterBuilding(this.gameObject);

        //Succed in this task
        Task.current.Succeed();
        //Then stop running through tasks, as the agent is "in" the building
        this.GetComponent<PandaBehaviour>().enabled = false;
    }


    public void ExitedHouse()
    {
        //Start running the tree again as the agent is out of the house
        this.GetComponent<PandaBehaviour>().enabled = true;
    }

    /// <summary>
    /// Drink a liquid, which will impart the drinks effects on the agent.
    /// Gonna be only water and reduing thirst though xD
    /// </summary>
    [Task]
    public void Drink()
    {
        //Reset the thirst variables
        thirstValue = 0;
        isAgentThirsty = false;


        Task.current.Succeed();
    }


    /// <summary>
    /// Eat an item, which will impart the items effects on the agent.
    /// Gonna be only "food" and reduing hunger though xD
    /// </summary>
    [Task]
    public void Eat()
    {
        if(curDest.transform.parent.gameObject.GetComponent<MarketStall>() == null)
        {
            Debug.Log("Current Destination is not a market stall");
            Task.current.Fail();
            return;
        }

        //Get enough food to satisfy our hunger, or all the food there is
        hungerValue = hungerValue - curDest.transform.parent.gameObject.GetComponent<MarketStall>().TakeFood(hungerValue);


        //Resolve the hunger variables
        UpdateAgentStatus();
        //Reset the speeds to normal
        agent.speed = maxSpeed;
        curSpeed = maxSpeed;

        Task.current.Succeed();
    }

    /// <summary>
    /// Check this agents hunger and thirst to see if they've reached the threshold to be considered "hungry" or "thirsty"
    /// </summary>
    protected void UpdateAgentStatus()
    {
        if(hungerValue >= 10)
        {
            isAgentHungry = true;
        }

        if(thirstValue >= 5)
        {
            isAgentThirsty = true;
        }
    }


    /// <summary>
    /// Is the time within the agents preferred working hours?
    /// </summary>
    /// <returns>True if it is, False if it is not</returns>
    protected bool WithinWorkingHours()
    {
        //Day Worker
        if (preferredWorkingHours.x < preferredWorkingHours.y)
        {
            //If the current time *is* within the preferred hours to work...
            if ((TimeOfDay >= preferredWorkingHours.x) && (TimeOfDay <= preferredWorkingHours.y))
            {
                //Then return true
                return true;
            }
            else
            {
                return false;
            }
        }
        //Night Owl
        else if (preferredWorkingHours.x > preferredWorkingHours.y)
        {
            //If the current time *is* within the preferred hours to work...
            if (!((TimeOfDay <= preferredWorkingHours.x) && (TimeOfDay >= preferredWorkingHours.y)))
            {
                //Then return true

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Search for a farm stall with food in it.
    /// Start of the tree to move food from a farm stall to a market stall
    /// </summary>
    [Task]
    protected void TransferFarmFood()
    {

        Vector3 dest = Vector3.zero;
        if (waypoints.Length > 0)
        {
            //Check all the waypoints for an avaliable MarketStall
            foreach (GameObject waypoint in waypoints)
            {
                if (waypoint.name.StartsWith("FarmStallWaypoint"))
                {
                    //If the farmstall we found has no food in it, give up and try the next stall
                    if (waypoint.transform.parent.gameObject.GetComponent<FarmStall>().CheckFoodLevels() <= 0)
                    {
                        //So do nothing here
                    }
                    //if it has any food at all...
                    else
                    {
                        //Perhaps reserve that food for ourself so it doesn't get taken by someone else?


                        //Set it as the destination for pathfinding
                        curDest = waypoint;
                        agent.transform.LookAt(curDest.transform);
                        dest = curDest.transform.position;
                    }
                }
            }
            //If there isn't a Farm Stall avaliable, this task failed
            if (dest == Vector3.zero)
            {
                Task.current.Fail();
                return;
            }

        }
        else
        {
            Task.current.Fail();
            return;
        }


        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    /// <summary>
    /// Pickup food from the farm stall
    /// </summary>
    [Task]
    protected void PickupFood()
    {
        //Fail if we're not at a farm stall
        if (curDest.transform.parent.gameObject.GetComponent<FarmStall>() == null)
        {
            Debug.Log("Current Destination is not a farm stall");
            Task.current.Fail();
            return;
        }

        //Get all the food we can carry, or as much as is in the farm stall
        carriedFood = carriedFood + curDest.transform.parent.gameObject.GetComponent<FarmStall>().TakeFood(maxCarriedFood - carriedFood);


        //Then find the market stall to drop the food off at.
        Task.current.Succeed();
    }



    /// <summary>
    /// Simple bool function for the behavior tree
    /// </summary>
    /// <returns></returns>
    [Task]
    protected bool HasFood()
    {
        if (carriedFood > 0) return true;
        else return false;
    }

    /// <summary>
    /// Find a market stall
    /// </summary>
    [Task]
    protected void FindMarketStall(){

        Vector3 dest = Vector3.zero;
        if (waypoints.Length > 0)
        {
            //Check all the waypoints for an avaliable MarketStall
            foreach (GameObject waypoint in waypoints)
            {
                if (waypoint.name.StartsWith("MarketWaypoint"))
                {
                    //If the market stall we found is full of food, give up and try the next stall
                    if (waypoint.transform.parent.gameObject.GetComponent<MarketStall>().CheckFoodLevels() >= waypoint.transform.parent.gameObject.GetComponent<MarketStall>().maxFood)
                    {
                        //So do nothing here
                    }
                    //if it has any room at all...
                    else
                    {
                        //Perhaps reserve that space for ourself so it doesn't get taken by someone else?


                        //Set it as the destination for pathfinding
                        curDest = waypoint;
                        agent.transform.LookAt(curDest.transform);
                        dest = curDest.transform.position;
                    }
                }
            }
            //If there isn't a market Stall avaliable, this task failed
            if (dest == Vector3.zero)
            {
                Task.current.Fail();
                return;
            }

        }
        else
        {
            Task.current.Fail();
            return;
        }


        agent.SetDestination(dest);
        Task.current.Succeed();

    }

    /// <summary>
    /// Deposit any carried food into the market stall we just arrived at
    /// </summary>
    [Task]
    protected void DepositFood()
    {
        //Fail if we're not at a market stall
        if (curDest.transform.parent.gameObject.GetComponent<MarketStall>() == null)
        {
            Debug.Log("Current Destination is not a market stall");
            Task.current.Fail();
            return;
        }

        //Drop off all the food that we can, even if it's not all of it, because the tree will loop if there is more food to be dropped off.
        carriedFood = curDest.transform.parent.gameObject.GetComponent<MarketStall>().DepositFood(carriedFood);

        Task.current.Succeed();
    }

}

