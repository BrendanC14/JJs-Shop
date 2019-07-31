using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckoutQueue {

    public Queue<Job> checkoutQueue;
    Action<Job> cbJobCreated;

    public CheckoutQueue()
    {
        checkoutQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        if (j.jobTime <= 0)
        {
            j.DoWork(0);
            return;
        }
        checkoutQueue.Enqueue(j);
        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (checkoutQueue.Count == 0)
        {
            //return new Job(World.Current.Checkouts[0].EmployeeTile,
            //    Words.Current.CheckoutQueue,
            //    Words.Current.Checkout,
            //   null,
            //    Numbers.Current.WaitAtCheckoutTime);
            return null;
        }
        return checkoutQueue.Dequeue();
    }

    public void Remove(Job j)
    {
        List<Job> jobs = new List<Job>(checkoutQueue);
        if (jobs.Contains(j) == false)
        {
            return;
        }
        jobs.Remove(j);
        checkoutQueue = new Queue<Job>(jobs);
    }


    public void RegisterJobCreationCallback(Action<Job> cb) { cbJobCreated += cb; }
    public void UnregisterJobCreationCallback(Action<Job> cb) { cbJobCreated -= cb; }
}
