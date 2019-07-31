using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Activity {
    public string ActivityName;
    public Tile tile;
    public float TimeTakes;
    public Direction FacingDirection;
    public float XModifier;
    public float YModifier;

    Action<Activity> cbActivityComplete;
    Action<Activity> cbActivityCancel;
    Action<Activity> cbActivityWorked;

    protected Activity (Activity other)
    {
        this.ActivityName = other.ActivityName;
        this.tile = other.tile;
        this.TimeTakes = other.TimeTakes;
        this.FacingDirection = other.FacingDirection;
        this.XModifier = other.XModifier;
        this.YModifier = other.YModifier;
    }

    virtual public Activity Clone()
    {
        return new Activity(this);
    }

    public Activity(string Name,
        Tile t,
        float Time,
        Direction DirectionToFace,
        Action<Activity> cbActivityComplete,
        Action<Activity> cbActivityCancel,
        float XMod = 0f,
        float YMod = 0f)
    {
        this.ActivityName = Name;
        this.tile = t;
        this.TimeTakes = Time;
        this.FacingDirection = DirectionToFace;
        this.cbActivityComplete = cbActivityComplete;
        this.cbActivityCancel = cbActivityCancel;
        this.XModifier = XMod;
        this.YModifier = YMod;

    }

    public void DoActivity(float workTime)
    {
        TimeTakes -= workTime;
        if (TimeTakes <= 0)
        {
            cbActivityComplete(this);
        }
    }
}
