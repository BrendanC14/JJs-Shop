using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixtureActions  {

    public static void Door_UpdateAction( Fixture f, float deltaTime)
    {
        
        if (f.GetParameter("is_opening") >= 1)
        {
            f.ChangeParameter("openness", deltaTime * Numbers.Current.DoorOpenSpeed);
            if (f.GetParameter("openness") >= 1)
            {
                f.SetParameter("is_opening", 0);
            }
        }
        else
        {
            if (f.Tile.People.Count == 0 && f.Tile.Employees.Count == 0)
            {
                f.ChangeParameter("openness", deltaTime * (Numbers.Current.DoorOpenSpeed * -1));
            }
        }
        f.SetParameter("openness", Mathf.Clamp01(f.GetParameter("openness")));

        if (f.cbOnChanged != null)
        {
            f.cbOnChanged(f);
        }

    }

    public static Enterability Door_IsEnterable(Fixture f, Tile tileComingFrom)
    {
        if (!World.Current.ShopOpen && World.Current.CustomerDoors.Contains(f) &&
            !World.Current.ShopTiles.Contains(tileComingFrom))
        {
            return Enterability.Soon;
        }

        f.SetParameter("is_opening", 1);
        if (f.GetParameter("openness") >= 1)
        {
            return Enterability.Yes;
        }
        return Enterability.Soon;
    }

    public static void JobComplete_FurnitureBuilding(Job theJob)
    {
        World.Current.PlaceFixture(theJob.jobObjectType, theJob.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.pendingFurnitureJob = null;
    }
}
