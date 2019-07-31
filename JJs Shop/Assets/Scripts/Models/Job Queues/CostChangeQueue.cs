using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CostChangeQueue {

    public Queue<Job> costChangeQueue;
    Action<Job> cbJobCreated;

    public CostChangeQueue()
    {
        costChangeQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        if (j.jobTime == 0)
        {
            j.DoWork(0);
            return;
        }
        costChangeQueue.Enqueue(j);
        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (costChangeQueue.Count == 0)
        {
            return null;
        }
        Job j = costChangeQueue.Dequeue();
        return j;
    }

    public void Remove(Job j)
    {
        List<Job> jobs = new List<Job>(costChangeQueue);
        if (jobs.Contains(j) == false)
        {
            return;
        }
        jobs.Remove(j);
        costChangeQueue = new Queue<Job>(jobs);
    }
    
}
