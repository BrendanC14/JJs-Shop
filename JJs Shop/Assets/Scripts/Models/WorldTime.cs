using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldTime {

    public static WorldTime Current { get; protected set; }
    public DateTime Date = new DateTime(2018, 12, 31, 23, 00, 0);
    
    public WorldTime()
    {
        Current = this;
        //Numbers.Current.UpdateOpeningTime(Date.Year, Date.Month, Date.Day);
       // Numbers.Current.UpdateClosingTime(Date.Year, Date.Month, Date.Day);
    }

    public void AddMinute()
    {
        Date = Date.AddMinutes(1);
    }
}
