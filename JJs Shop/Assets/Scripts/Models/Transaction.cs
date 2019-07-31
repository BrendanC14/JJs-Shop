using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Transaction {

    public string Type;
    public DateTime DateTimePurchase;
    public List<Purchase> ItemsBought;
    public double Cost; //This can be positive for customer purchases or negative for shop orders
    //This is total cost of the transaction

    public Transaction(string type, List<Item> Items)
    {
        ItemsBought = new List<Purchase>();
        Type = type;
        DateTimePurchase = WorldTime.Current.Date;
        if (Type == Words.Current.CustomerPurchase)
        {
            foreach (Item i in Items)
            {
                ItemsBought.Add(new Purchase(i, i.Price));
                Cost += i.Price;
            }
        }
        else if (Type == Words.Current.ShopOrder)
        {
            foreach (Item i in Items)
            {
                ItemsBought.Add(new Purchase(i, i.OrderPrice));
                Cost -= i.Price;
            }
        }


    }

    public Transaction(TransactionSaveFile savedTransaction)
    {
        Type = savedTransaction.Type;
        DateTimePurchase = savedTransaction.PurchaseTime;
        Cost = savedTransaction.Cost;

        foreach (KeyValuePair<string, double> item in savedTransaction.ItemsBought)
        {
            ItemsBought.Add(new Purchase(World.Current.GetItemFromShopShelf(item.Key), item.Value));
        }
    }
}
