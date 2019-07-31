using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StockQueue  {

    public Queue<Job> stockQueue;
    Action<Job> cbJobCreated;

    public StockQueue()
    {
        stockQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        if (j.jobTime == 0) 
        {
            j.DoWork(0);
            return;
        }
        stockQueue.Enqueue(j);
        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }
 
    public Job Dequeue()
    {
        if (stockQueue.Count == 0)
        {
            return null;
        }
        Job j = stockQueue.Dequeue();
        if (j.jobTime == -1)
        {// -1 means the time needs to be decided at the time of Dequeue (so now)
            j.jobTime = (j.tile.Fixture.MaxShelfSpace - j.tile.Fixture.GetItemsOnShelf().Count) / j.tile.Fixture.ItemOnShelf.ShelfSpace;
        }
        return j;
    }

    public void Remove(Job j)
    {
        List<Job> jobs = new List<Job>(stockQueue);
        if (jobs.Contains(j) == false)
        {
            return;
        }
        jobs.Remove(j);
        stockQueue = new Queue<Job>(jobs);
    }


    public void RegisterJobCreationCallback(Action<Job> cb) { cbJobCreated += cb; }
    public void UnregisterJobCreationCallback(Action<Job> cb) { cbJobCreated -= cb; }
}
