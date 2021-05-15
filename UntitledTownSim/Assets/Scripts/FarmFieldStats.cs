using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmFieldStats : MonoBehaviour
{
    //1 Unit of food = person no longer hungry
    [Tooltip("How many units of food this farm field provides.")]
    public int providedFood = 10;

    //AKA how many steps through its growing process
    [Tooltip("How grown this farm field currently is")]
    public int farmGrowthLevel = 0;

    [Tooltip("total number of growth steps in this farm field")]
    public int farmMaxGrowth = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    /// <summary>
    /// Increases the growth of the field by one
    /// </summary>
    public void IncreaseFieldGrowth()
    {
        //Increment the counter
        farmGrowthLevel++;

        //Do other animation work here
        //TODO



        if(farmGrowthLevel >= farmMaxGrowth)
        {
            //Reset the counter
            farmGrowthLevel = 0;


            //Do other animation work here
            //TODO
        }
    }
}
