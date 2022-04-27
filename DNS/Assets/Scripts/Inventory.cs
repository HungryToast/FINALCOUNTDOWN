using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Inventory : MonoBehaviour
{
    private int wood;
    private int food;
    private float water;

    public int GetWood()
    {
        return wood;
    }

    public int SetWood(int gain)
    {
        wood += gain;
        return wood;
    }

    public int GetFood()
    {
        return food;
    }

    public int SetFood(int gain)
    {
        return food += gain;
    }

    public float GetWater()
    {
        return water;
    }

    public float SetWater(int gain)
    {
        return water += gain;
    }

}