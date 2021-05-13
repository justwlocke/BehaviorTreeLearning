using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Tooltip("The building this waypoint is for.")]
    public GameObject building;

    [Tooltip("Everything that is currently \"inside\" this building")]
    public List<GameObject> thingsInsideBuilding = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Make an object enter the building
    /// </summary>
    /// <param name="enteringObject">The entering Object</param>
    public void EnterBuilding(GameObject enteringObject)
    {
        thingsInsideBuilding.Add(enteringObject);

        //Turn off colliders
        foreach (Collider c in enteringObject.GetComponents<Collider>())
        {
            c.enabled = false;
        }





        //Make the object invisible (But not disabled)
        enteringObject.GetComponent<MeshRenderer>().enabled = false;
        //have to do this because for ***some*** reason, all the body parts on a person are seperated
        MeshRenderer[] allRenderers = enteringObject.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer mesh in allRenderers)
        {
            mesh.enabled = false;
        }
    }

    /// <summary>
    /// Remove the object from the house and tell it it's of the house
    /// </summary>
    /// <param name="exitingObject">The Object to leave</param>
    public void ExitBuilding(GameObject exitingObject)
    {
        thingsInsideBuilding.Remove(exitingObject);

        //Make the object visible again (But not disabled)
        exitingObject.GetComponent<MeshRenderer>().enabled = true;
        //have to do this because for ***some*** reason, all the body parts on a person are seperated
        MeshRenderer[] allRenderers = exitingObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in allRenderers)
        {
            mesh.enabled = true;
        }




        //Turn on colliders
        foreach (Collider c in exitingObject.GetComponents<Collider>())
        {
            c.enabled = true;
        }

        //If this is an agent
        if (exitingObject.GetComponent<PandaBTScripts>() != null)
        {
            //Tell them they're out of the house
            exitingObject.GetComponent<PandaBTScripts>().ExitedHouse();
        }
    }




}
