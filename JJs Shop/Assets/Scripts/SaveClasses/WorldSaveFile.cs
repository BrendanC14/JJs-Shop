using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WorldSaveFile 
{
    public float DeliveryTime;
    public double Balance;
    public string ItemToDeliver;
    public int QuantityToDeliver;

    public WorldSaveFile(World w)
    {
        DeliveryTime = w.DeliveryTime;
        Balance = w.Balance;
        if (DeliveryTime > 0)
        {
            ItemToDeliver = w.ItemToDeliver.Name;
            QuantityToDeliver = w.QuantityToDeliver;
        }


        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString("World", JSON);
    }
}
