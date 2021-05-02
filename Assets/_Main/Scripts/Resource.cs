using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public Fuel fuelType;
    public int count;

    public Resource()
    {

    }

    public Resource(Fuel f, int c)
    {
        fuelType = f;
        count = c;
    }
}
