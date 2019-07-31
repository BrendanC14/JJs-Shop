using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WorkingDaySaveFile
{
    public DateTime Date;
    public DateTime OpeningTime;
    public DateTime ClosingTime;
    public Dictionary<int, DateTime> EmployeeStartTimes;
    public Dictionary<int, DateTime> EmployeeEndTimes;
    public double OrderCosts;
    public double CustomerCosts;


    public WorkingDaySaveFile(WorkingDay wd, int Num)
    {
        Date = wd.Date;
        OpeningTime = wd.OpeningTime;
        ClosingTime = wd.ClosingTime;
        if (Num != 0)
        {
            //We save today first, so if num is 0 we're saving today
            foreach (Transaction t in wd.transactions)
            {
                //Should only be one of each
                if (t.Type == Words.Current.CustomerPurchase)
                {
                    CustomerCosts = t.Cost;
                }
                else if (t.Type == Words.Current.ShopOrder)
                {
                    OrderCosts = t.Cost;
                }
            }
        }
        else
        {
            int i = 1;
            foreach (Transaction t in wd.transactions)
            {
                TransactionSaveFile save = new TransactionSaveFile(t, i);
                i++;
            }
        }

        EmployeeStartTimes = new Dictionary<int, DateTime>();
        foreach (KeyValuePair<Employee, DateTime> emDa in wd.EmployeesStartingTimes)
        {
            EmployeeStartTimes.Add(emDa.Key.EmployeeID, emDa.Value);
        }
        EmployeeEndTimes = new Dictionary<int, DateTime>();
        foreach (KeyValuePair<Employee, DateTime> emDa in wd.EmployeeEndTimes)
        {
            EmployeeEndTimes.Add(emDa.Key.EmployeeID, emDa.Value);
        }

        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString("WorkingDay" + Num, JSON);
    }

}
