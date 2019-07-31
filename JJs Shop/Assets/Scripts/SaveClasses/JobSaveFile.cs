using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JobSaveFile
{
    public int X;
    public int Y;
    public float jobTime;
    public string jobObjectType;
    public string jobQueue;
    public Action<Job> cbJobComplete;
    public Action<Job> cbJobWorked;
    public Action<Job> cbJobCancelled;

    public JobSaveFile(Job j,int Num)
    {
        X = j.tile.X;
        Y = j.tile.Y;
        jobObjectType = j.jobObjectType;
        jobTime = j.jobTime;
        jobQueue = j.jobQueue;
        cbJobComplete = j.cbJobComplete;
        cbJobWorked = j.cbJobWorked;
        cbJobCancelled = j.cbJobCancel;

        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString("Job" + Num, JSON);
    }

}
