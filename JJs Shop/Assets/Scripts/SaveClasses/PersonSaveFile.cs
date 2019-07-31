using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PersonSaveFile
{
    public int ID;
    public int X = 0;
    public int Y = 0;
    public bool WantsToShop;
    public float speed;
    public string SpriteName;

    public string VehicleType;
    public int VehicleStartX = 0;
    public int VehicleStartY = 0;
    public int VehicleEndX = 0;
    public int VehicleEndY = 0;
    public string VehicleColour = "";
    public int VehicleTileSize;

    public string currActivityName = "";
    public int currActivityX;
    public int currActivityY;
    public float currActivityTime;
    public string currActivityDirection;
    public float currActivityXMod;
    public float currActivityYMod;

    public List<String> ShoppingList;
    public List<String> ItemsInHand;
    public List<String> UnavailableItems;

    public float TooExpensivePerc;
    public float BuyTwoPerc;
    public float ExpPriceModifier;

    public PersonSaveFile(Person p)
    {
        ID = Numbers.Current.GetNextPersonID();
        if (p.currTile != null)
        {
            X = p.currTile.X;
            Y = p.currTile.Y;
        }
        WantsToShop = p.WantsToShop;
        speed = p.Speed;
        SpriteName = p.SpriteName;
        if (p.v != null)
        {
            VehicleType = p.v.VehicleType;
            VehicleStartX = p.v.currTile[0].X;
            VehicleStartY = p.v.currTile[0].Y;
            VehicleEndX = p.v.destTile.X;
            VehicleEndY = p.v.destTile.Y;
            VehicleColour = p.v.Colour;
            VehicleTileSize = p.v.TileSize;
        }
        if (p.currActivity != null)
        {
            currActivityName = p.currActivity.ActivityName;
            currActivityX = p.currActivity.tile.X;
            currActivityY = p.currActivity.tile.Y;
            currActivityTime = p.currActivity.TimeTakes;
            currActivityDirection = convertDirection(p.currActivity.FacingDirection);
            currActivityXMod = p.currActivity.XModifier;
            currActivityYMod = p.currActivity.YModifier;
        }
        ShoppingList = new List<string>();
        foreach (Item i in p.ShoppingList)
        {
            ShoppingList.Add(i.Name);
        }
        ItemsInHand = new List<string>();
        foreach (Item i in p.ItemsInHand)
        {   
            ItemsInHand.Add(i.Name);
        }
        UnavailableItems = new List<string>();
        foreach(Item i in p.UnavailableItems)
        {
            UnavailableItems.Add(i.Name);
        }

        string JSON = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("Person" + ID.ToString(), JSON);
    }

    string convertDirection(Direction d)
    {
        if (d == Direction.Left)
        {
            return "Left";
        }
        else if (d == Direction.Up)
        {
            return "Up";
        }
        else if (d == Direction.Down)
        {
            return "Down";
        }

        return "Right";
    }
}
