using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Job  {

    public Tile tile;
    public float jobTime;

    public string jobObjectType
    {
        get; protected set;
    }
    public string jobQueue;
    public Fixture fixturePrototype;
    public Fixture fixture;

    public Action<Job> cbJobComplete;
    public Action<Job> cbJobCancel;
    public Action<Job> cbJobWorked;

    public Job (Tile tile,
        string jobQueue,
        string jobObjectType,
        Action<Job> cbJobComplete, 
        float jobTime)
    {
        this.tile = tile;
        this.jobQueue = jobQueue;
        this.jobObjectType = jobObjectType;
        this.cbJobComplete = cbJobComplete;
        this.jobTime = jobTime;
    }
    public Job(JobSaveFile savedJob)
    {
        tile = World.Current.GetTileAt(savedJob.X, savedJob.Y);
        jobTime = savedJob.jobTime;
        jobObjectType = savedJob.jobObjectType;
        jobQueue = savedJob.jobQueue;
        cbJobComplete = savedJob.cbJobComplete;
        cbJobCancel = savedJob.cbJobCancelled;
        cbJobWorked = savedJob.cbJobWorked;
    }

    protected Job (Job other)
    {
        this.tile = other.tile;
        this.jobQueue = other.jobQueue;
        this.jobObjectType = other.jobObjectType;
        this.cbJobComplete = other.cbJobComplete;
        this.jobTime = other.jobTime;
    }

    virtual public Job Clone()
    {
        return new Job(this);
    }

    public void DoWork(float workTime)
    {
        jobTime -= workTime;
        if (cbJobWorked != null)
        {
            cbJobWorked(this);
        }
        if (jobTime <= 0)
        {
            if (cbJobComplete != null)
            {
                cbJobComplete(this);
            }
        }
    }

    public void RegisterJobCompleteCallback(Action<Job> cb) { cbJobComplete += cb; }
    public void RegisterJobCancelCallback(Action<Job> cb) { cbJobCancel += cb; }
    public void UnregisterJobCompleteCallback(Action<Job> cb) { cbJobComplete -= cb; }
    public void UnregisterJobCancelCallback(Action<Job> cb) { cbJobCancel -= cb; }
    public void RegisterJobWorkedCallback(Action<Job> cb) { cbJobWorked += cb; }
    public void UnregisterJobWorkedCallback(Action<Job> cb) { cbJobWorked -= cb; }


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
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("jobQueue", jobQueue);
        writer.WriteAttributeString("jobTime", jobTime.ToString());
    }

    public void ReadXml(XmlReader reader)
    {

    }
}
