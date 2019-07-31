using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TransactionSaveFile
{
    public string Type;
    public DateTime PurchaseTime;
    public Dictionary<string, double> ItemsBought;
    public double Cost;

    public TransactionSaveFile(Transaction t, int ID)
    {
        Type = t.Type;
        PurchaseTime = t.DateTimePurchase;
        Cost = t.Cost;

        foreach (Purchase p in t.ItemsBought)
        {
            ItemsBought.Add(p.i.Name, p.Cost);
        }
        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString("Transaction" + ID.ToString(), JSON);
    }
}
