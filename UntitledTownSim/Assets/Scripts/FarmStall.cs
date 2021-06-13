using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmStall : MonoBehaviour
{

    public float storedFood = 0;
    [Tooltip("The Max amount of food this stall can store")]
    public float maxFood = 500;


    /// <summary>
    /// Take food from the stall
    /// </summary>
    /// <param name="foodToTake">How much to take</param>
    /// <returns>If the action was sucessful</returns>
    public float TakeFood(float foodToTake)
    {
        //If there is less food here than we want...
        if(foodToTake > storedFood)
        {
            //Take all that remains
            float allThatRemains = storedFood;
            storedFood = 0;
            return allThatRemains;
        }
        else
        {
            //Take the amount of food we want
            storedFood -= foodToTake;
            return foodToTake;
        }
    }

    /// <summary>
    /// Add food to this stall
    /// </summary>
    /// <param name="foodToAdd">How much to add</param>
    /// <returns>Any overflow food</returns>
    public float DepositFood(float foodToAdd)
    {
        //If we would add more food that can be stored...
        if(storedFood + foodToAdd > maxFood)
        {
            //Calc the overflow amount before we modify stored food
            float overflow = maxFood - storedFood;
            overflow = foodToAdd - overflow;
            //Max out the food
            storedFood = maxFood;

            //Return the extra food
            return overflow;
        }
        else
        {
            //Add all food
            storedFood += foodToAdd;
            //No overflow food
            return 0;
        }

        
    }


    /// <summary>
    /// Just check the Food Levels
    /// </summary>
    /// <returns>The amount stored</returns>
    public float CheckFoodLevels()
    {
        return storedFood;
    }
}
