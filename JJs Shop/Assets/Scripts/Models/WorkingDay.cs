using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


public class WorkingDay : IXmlSerializable  {

    public DateTime Date;
    public List<Transaction> transactions;
    public DateTime OpeningTime;
    public DateTime ClosingTime;
    public Dictionary<Employee, DateTime> EmployeesStartingTimes;
    public Dictionary<Employee, DateTime> EmployeeEndTimes;

    public WorkingDay(DateTime date, DateTime open, DateTime close, Dictionary<Employee, DateTime> eStartTimes, Dictionary<Employee, DateTime> eEndTimes)
    {
        Date = date;
        transactions = new List<Transaction>();
        OpeningTime = open;
        ClosingTime = close;
        EmployeesStartingTimes = new Dictionary<Employee, DateTime>(eStartTimes);
        EmployeeEndTimes = new Dictionary<Employee, DateTime>(eEndTimes);
    }

    public WorkingDay(WorkingDaySaveFile savedWD, bool Today)
    {
        Date = savedWD.Date;
        OpeningTime = savedWD.OpeningTime;
        ClosingTime = savedWD.ClosingTime;

        EmployeesStartingTimes = new Dictionary<Employee, DateTime>();
        foreach (KeyValuePair<int, DateTime> emp in savedWD.EmployeeStartTimes)
        {
            EmployeesStartingTimes.Add(World.Current.GetEmployeeWithID(emp.Key), emp.Value);
        }
        EmployeeEndTimes = new Dictionary<Employee, DateTime>();
        foreach (KeyValuePair<int, DateTime> emp in savedWD.EmployeeEndTimes)
        {
            EmployeeEndTimes.Add(World.Current.GetEmployeeWithID(emp.Key), emp.Value);
        }

        if (Today)
        {

            int i = 1;
            do
            {
                string json = PlayerPrefs.GetString("Transaction" + i);
                Transaction t = new Transaction(JsonUtility.FromJson<TransactionSaveFile>(json));
                i++;
            } while (PlayerPrefs.HasKey("Transaction" + i));
        }
        else
        {
            Transaction custPurch = new Transaction(Words.Current.CustomerPurchase, new List<Item>());
            Transaction shopOrder = new Transaction(Words.Current.ShopOrder, new List<Item>());

            custPurch.Cost = savedWD.CustomerCosts;
            shopOrder.Cost = savedWD.OrderCosts;

            transactions.Add(custPurch);
            transactions.Add(shopOrder);

        }

    }



    //////////////////////////////////////////////////////////////////////////////////////
    /////               
    /////                           SAVING & LOADING
    /////       
    ///////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("Date", Date.ToString());
        writer.WriteStartElement("Transacstions");
        foreach (Transaction t in transactions)
        {
            writer.WriteStartElement("Transaction");
            writer.WriteAttributeString("Type", t.Type);
            writer.WriteAttributeString("DateTimePurchase", t.DateTimePurchase.ToString());
            writer.WriteAttributeString("Cost", t.Cost.ToString());

            writer.WriteStartElement("ItemsBought");
            foreach (Purchase p in t.ItemsBought)
            {
                writer.WriteStartElement("Purchase");
                writer.WriteAttributeString("Item", p.i.Name);
                writer.WriteAttributeString("Cost", p.Cost.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        writer.WriteAttributeString("OpeningTime", OpeningTime.ToString());
        writer.WriteAttributeString("ClosingTime", ClosingTime.ToString());

        writer.WriteStartElement("EmployeeStartTimes");
        foreach( KeyValuePair<Employee, DateTime> kp in EmployeesStartingTimes)
        {
            writer.WriteStartElement("Employee");
            writer.WriteAttributeString("Name", kp.Key.Name);
            writer.WriteAttributeString("StartTime", kp.Value.ToString());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("EmployeeEndTimes");
        foreach (KeyValuePair<Employee, DateTime> kp in EmployeeEndTimes)
        {
            writer.WriteStartElement("Employee");
            writer.WriteAttributeString("Name", kp.Key.Name);
            writer.WriteAttributeString("EndTimes", kp.Value.ToString());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

    }

    public void ReadXml(XmlReader reader)
    {

    }
}
