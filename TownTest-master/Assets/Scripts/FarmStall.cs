using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmStall : MonoBehaviour
{

    public float storedFood = 0;


    /// <summary>
    /// Take food from the stall
    /// </summary>
    /// <param name="foodToTake">How much to take</param>
    /// <returns>If the action was sucessful</returns>
    public bool TakeFood(float foodToTake)
    {
        if(foodToTake > storedFood)
        {
            return false;
        }
        else
        {
            storedFood -= foodToTake;
            return true;
        }
    }

    /// <summary>
    /// Add food to this stall
    /// </summary>
    /// <param name="foodToAdd">How much to add</param>
    public void DepositFood(float foodToAdd)
    {
        storedFood += foodToAdd;
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
