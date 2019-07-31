using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildMode
{
    FLOOR,
    FIXTURE,
    DECONSTRUCT
}

public class BuildModeController : MonoBehaviour
{

    public BuildMode buildMode;
    public string buildModeObjectType;

    // Use this for initialization
    void Start()
    {
        Debug.Log("");
    }

    // Update is called once per frame
    void Update()
    {

    }
    

    public void DoBuild(Tile t)
    {
        if (buildMode == BuildMode.FIXTURE)
        {
            string fixtureType = buildModeObjectType;
            if (World.Current.IsFixturePlacementValid(fixtureType, t) &&
                t.pendingFurnitureJob == null)
            {
                Job j;
                if (World.Current.FixtureJobPrototypes.ContainsKey(fixtureType))
                {
                    j = World.Current.FixtureJobPrototypes[fixtureType].Clone();
                    j.tile = t;
                }
                else
                {
                    Debug.LogError("There's no fixture job prototype for this");
                    j = new Job(t, Words.Current.ConstructionQueue, fixtureType, FixtureActions.JobComplete_FurnitureBuilding, 10f);
                }

                j.fixturePrototype = World.Current.FixturePrototypes[fixtureType];

                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                World.Current.ConstructionQueue.Enqueue(j);


            }
        }
    }

    public void BuildButtonPressed()
    {

    }
}
