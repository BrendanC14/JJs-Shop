using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Purchase {

    public Item i;
    public double Cost;

    public Purchase(Item item, double c)
    {
        i = item;
        Cost = c;
    }
}
