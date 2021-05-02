using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Fuel
{
    Coal,
    Oil,
    Trash,
    Nuclear,
    EcoFriendly,
    CoalOrOil
}

[CreateAssetMenu(fileName = "New Powerplant", menuName = "Powerplant")]
public class Powerplant : ScriptableObject
{
    public int cost;
    public int citiesPowered;
    public Fuel fuelType;
    public int numberOfFuelRequired;
    public bool isPhaseThreeCard = false;
}
