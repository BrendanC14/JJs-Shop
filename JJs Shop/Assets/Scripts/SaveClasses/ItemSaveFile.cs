using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemSaveFile
{
    public string Name;
    public int ID;
    public double PrevPrice;
    public double Price;
    public double OrderPrice;
    public bool DeliverPending;
    public double NewPrice;
    public int PrevNotBuying;
    public int PrevNeutral;
    public int PrevHappy;
    public int PrevOuttaStock;
    public int PrevWouldaBoughtTwo;
    public int PrevBoughtTwo;
    public int NotBuying;
    public int Neutral;
    public int Happy;
    public int OuttaStock;
    public int WouldaBoughtTwo;
    public int BoughtTwo;
    public bool Refrigerated;
    public int ShelfSpace;
    public int ShopShelfX;
    public int ShopShelfY;
    public int StockShelfX;
    public int StockShelfY;

    public ItemSaveFile(Item i, string ListToSave)
    {
        Name = i.Name;
        ID = i.ID;
        PrevPrice = i.PrevPrice;
        Price = i.Price;
        OrderPrice = i.OrderPrice;
        DeliverPending = i.DeliveryPending;
        NewPrice = i.NewPrice;
        PrevNotBuying = i.PrevNotBuying;
        PrevNeutral = i.PrevNeutral;
        PrevHappy = i.PrevHappy;
        PrevOuttaStock = i.PrevOuttaStock;
        PrevWouldaBoughtTwo = i.PrevWouldaBoughtTwo;
        PrevBoughtTwo = i.PrevBoughtTwo;
        NotBuying = i.NotBuying;
        Neutral = i.Neutral;
        Happy = i.Happy;
        OuttaStock = i.OuttaStock;
        WouldaBoughtTwo = i.WouldaBoughtTwo;
        BoughtTwo = i.BoughtTwo;
        Refrigerated = i.Refrigerated;
        ShelfSpace = i.ShelfSpace;
        ShopShelfX = i.ShopShelf.Tile.X;
        ShopShelfY = i.ShopShelf.Tile.Y;
        StockShelfX = i.StockShelf.Tile.X;
        StockShelfY = i.StockShelf.Tile.Y;

        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString(ListToSave + ID.ToString(), JSON);

    }
}
