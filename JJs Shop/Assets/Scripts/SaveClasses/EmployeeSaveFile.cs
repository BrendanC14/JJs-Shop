using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EmployeeSaveFile
{
    public int TileX = 0;
    public int TileY = 0;
    public string Name;
    public int ID;
    public DateTime DOB;
    public double WageExpectations;
    public int PrefHours;
    public int currJobX;
    public int currJobY;
    public string currJobQueue = "";
    public float currJobTime;
    public string ItemNeeded;
    public float NumberItemNeeded;
    public List<string> PriList;
    public string ItemInHand;
    public int NumItemsInHand = 0;
    public int Experience;
    public int Flexibility;
    public int Reliability;
    public int CheckoutAbility;
    public int RestockAbility;
    public int CleanAbility;
    public int StartingHour;
    public int EndHour;
    public double Wage;
    public bool OnShift;
    public bool CalledInLate;
    public bool TimeToLeave;
    public float speed;
    

    public EmployeeSaveFile(Employee e)
    {
        Name = e.Name;
        ID = e.EmployeeID;
        DOB = e.DOB;
        WageExpectations = e.WageExpectations;
        PrefHours = e.PrefHours;
        if (e.currTile != null)
        {
            TileX = e.currTile.X;
            TileY = e.currTile.Y;
        }
        if (e.currJob != null)
        { 
            currJobX = e.currJob.tile.X;
            currJobY = e.currJob.tile.Y;
            currJobQueue = e.currJob.jobQueue;
            currJobTime = e.currJob.jobTime;
            if (currJobQueue == Words.Current.StockQueue)
            {
                ItemNeeded = e.ItemNeeded.Name;
                NumberItemNeeded = e.NumberItemNeeded;
            }
        }
        PriList = e.PriorityList;
        if (e.ItemsInHand.Count > 0)
        {
            ItemInHand = e.ItemsInHand[0].Name;
            NumItemsInHand = e.ItemsInHand.Count;
        }
        Experience = e.Experience;
        Flexibility = e.Flexibility;
        Reliability = e.Reliabilitiy;
        CheckoutAbility = e.CheckoutAbility;
        RestockAbility = e.RestockAbility;
        CleanAbility = e.CleanAbilitiy;
        StartingHour = e.StartingHour;
        EndHour = e.EndHour;
        Wage = e.Wage;
        OnShift = e.OnShift;
        CalledInLate = e.CalledInLate;
        TimeToLeave = e.TimeToLeave;
        speed = e.Speed;

        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString("Employee" + ID.ToString(), JSON);

    }
}
