using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConstructionQueue {

    public Queue<Job> constructionQueue;

    Action<Job> cbJobCreated;
    
    public ConstructionQueue()
    {
        constructionQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        if (j.jobTime <= 0)
        {
            j.DoWork(0);
            return;
        }

        constructionQueue.Enqueue(j);

        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (constructionQueue.Count == 0)
        {
            return null;
        }
        return constructionQueue.Dequeue();
    }

    public void RegisterJobCreationCallback(Action<Job> cb) { cbJobCreated += cb; }
    public void UnregisterJobCreationCallback(Action<Job> cb) { cbJobCreated -= cb; }

    public void Remove(Job j)
    {
        List<Job> jobs = new List<Job>(constructionQueue);

        if (jobs.Contains(j) == false)
        {
            return;
        }
        jobs.Remove(j);
        constructionQueue = new Queue<Job>(jobs);
    }

}
