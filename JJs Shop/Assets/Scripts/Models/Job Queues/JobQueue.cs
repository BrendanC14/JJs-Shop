using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue  {

    public Job GetJob(List<string> PriorityList, Employee e)
    {
        Job j = null;
        //Could be null
        for (int i = 0; i < PriorityList.Count - 1; i++)
        {
            if (PriorityList[i] == Words.Current.CheckoutQueue)
            {
                bool CheckoutManned = false;
                foreach (Employee em in World.Current.Employees)
                {
                    if (em.EmployeeID != e.EmployeeID &&
                        em.OnTill)
                    {
                        CheckoutManned = true;
                    }
                }
                if (!CheckoutManned)
                {
                    j = World.Current.CheckoutQueue.Dequeue();
                }
                if (j != null)
                {
                    return j;
                }
            }
            else if (PriorityList[i] == Words.Current.ConstructionQueue)
            {
                j = World.Current.ConstructionQueue.Dequeue();
                if (j != null)
                {
                    return j;
                }
            }
            else if (PriorityList[i] == Words.Current.StockQueue)
            {
                j = World.Current.StockQueue.Dequeue();
                if (j != null)
                {
                    return j;
                }
            }
            else if (PriorityList[i] == Words.Current.CostChangeQueue)
            {
                j = World.Current.CostChangeQueue.Dequeue();
                if (j != null)
                {
                    return j;
                }
            }
        }
        return j;
    }

    public void AbandonJob(Job j)
    {
        if (j.jobQueue == Words.Current.CheckoutQueue)
        {
            World.Current.CheckoutQueue.Enqueue(j);
        }
        else if (j.jobQueue == Words.Current.ConstructionQueue)
        {
            World.Current.ConstructionQueue.Enqueue(j);
        }
    }
    
}
