using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityHolder : MonoBehaviour
{
    public Node thisCityNode;
    public int partOfNumberOfRegions = 3;
    [HideInInspector] public bool isBlocked = false;
    public GameObject blocked;

    public void BlockUnusedCities(int numberOfRegions)
    {
        if (partOfNumberOfRegions > numberOfRegions)
        {
            isBlocked = true;
            Instantiate(blocked, thisCityNode.position, Quaternion.identity);
        }
    }
}
