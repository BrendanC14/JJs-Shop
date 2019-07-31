using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpriteController : MonoBehaviour
{

    Dictionary<Vehicle, GameObject> VehicleGameObjectMap;
    Dictionary<string, Sprite> VehicleSpriteMap;
    float TimeIndicating;

    // Use this for initialization
    void Start()
    {
        LoadSprites();

        VehicleGameObjectMap = new Dictionary<Vehicle, GameObject>();
        World.Current.AddVehicleCreatedCallback(OnVehicleCreated);
        World.Current.AddVehicleRemovedCallback(OnVehicleRemoved);

        foreach (Vehicle v in World.Current.Vehicles)
        {
            OnVehicleCreated(v);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadSprites()
    {
        VehicleSpriteMap = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Fixtures");

        foreach (Sprite s in sprites)
        {
            VehicleSpriteMap[s.name] = s;
        }
    }

    void OnVehicleCreated(Vehicle v)
    {
        GameObject vehGo = new GameObject();
        VehicleGameObjectMap.Add(v, vehGo);

        vehGo.name = v.VehicleType;
        if (!(v.X == 0 || v.X == World.Current.Width - 1 || v.Y == 0 || v.Y == World.Current.Height - 1))
        {

            vehGo.transform.position = new Vector3(v.X, v.Y, 0);
        }

        vehGo.transform.SetParent(this.transform, true);
        vehGo.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = vehGo.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = Words.Current.VehicleLayer;
        OnVehicleChangedDirection(v);

        v.AddVehicleChangedPositionCallback(OnVehicleChangedPosition);
        v.AddVehicleChangedDirectionCallback(OnVehicleChangedDirection);
        //v.AddVehicleIndicatingCallback(OnVehicleIndicating);

    }

    void OnVehicleChangedPosition(Vehicle v)
    {
        if (VehicleGameObjectMap.ContainsKey(v) == false)
        {
            return;
        }
        GameObject vehGo = VehicleGameObjectMap[v];

        if (v.X == 0 || v.X == World.Current.Width - 1 || v.Y == 0 || v.Y == World.Current.Height - 1)
        {
            return;
        }
        vehGo.transform.position = new Vector3(v.X, v.Y);
    }

    void OnVehicleChangedDirection(Vehicle v)
    {
        if (VehicleGameObjectMap.ContainsKey(v) == false)
        {
            return;
        }

        GameObject vehGo = VehicleGameObjectMap[v];
        string SpriteName = v.VehicleType + " " + v.Direction;
        if (v.Colour != null) { SpriteName += " " + v.Colour; }
        SpriteRenderer sr = vehGo.GetComponent<SpriteRenderer>();
        sr.sprite = VehicleSpriteMap[SpriteName];


        if (v.Direction == Direction.Up || v.Direction == Direction.Down)
        {
            vehGo.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        }
        else { vehGo.transform.localScale = new Vector3(0.5f, 0.5f, 1); }

    }

    void OnVehicleIndicating(Vehicle v)
    {


        Direction Turning;
        if (v.Direction == Direction.Left || v.Direction == Direction.Right)
        {
            if (v.thirdTile.Y > v.currTile[0].Y)
            {
                Turning = Direction.Right;
            }
            else { Turning = Direction.Left; }
        }
        else
        {
            if (v.thirdTile.X > v.currTile[0].X)
            {
                Turning = Direction.Left;
            }
            else { Turning = Direction.Right; }
        }

        if (TimeIndicating < Numbers.Current.IndicatingTime)
        {
            if (VehicleGameObjectMap[v].transform.childCount < 1)
            {
                TurnOnIndicator(v, Turning);
            }
        }
        else
        {
            if (VehicleGameObjectMap[v].transform.childCount > 0)
            {
                foreach (Transform t in VehicleGameObjectMap[v].transform)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }
        }
        TimeIndicating += Time.deltaTime;
        if (TimeIndicating >= Numbers.Current.IndicatingTime * 2) { TimeIndicating = 0F; }
       
    }

    void TurnOnIndicator(Vehicle v, Direction Turning)
    {
        if (VehicleGameObjectMap.ContainsKey(v) == false)
        {
            return;
        }
        if (v.Direction == Direction.Down)
        {
            GameObject vehGo = VehicleGameObjectMap[v];

            GameObject Indicator = new GameObject();
            Indicator.transform.SetParent(vehGo.transform, true);
            Indicator.transform.position = new Vector3(v.X, v.Y);
            Indicator.AddComponent<SpriteRenderer>();
            Indicator.GetComponent<SpriteRenderer>().sortingLayerName = Words.Current.FixtureLayer;
            
            Indicator.GetComponent<SpriteRenderer>().sprite = VehicleSpriteMap[Words.Current.IndicatorDownUp];

            if (Turning == Direction.Left)
            {
                Indicator.transform.Rotate(new Vector3(0, 180, 0));
            }
            return;
        }

        return;

        //This is the code to paint the pixels...not working...come back to it
        SpriteRenderer sr = VehicleGameObjectMap[v].GetComponent<SpriteRenderer>();
       // sr.sprite = VehicleSpriteMap[Words.Current.Car + " " + v.Direction];
        Sprite s = sr.sprite;
        Texture2D carTexture2D = CopyTexture2D(VehicleSpriteMap[Words.Current.Car + " " + v.Direction].texture, v.Direction, Turning);
        carTexture2D.Apply();
        Sprite newSprite = Sprite.Create(carTexture2D,s.rect, new Vector2(0, 1));
        //sr.sprite = newSprite;
        sr.material.mainTexture = carTexture2D;
        sr.material.SetTexture("_MainTex", carTexture2D);
    }

    Texture2D CopyTexture2D(Texture2D copiedTexture, Direction vehicleDirection, Direction Turning)
    {
        copiedTexture.wrapMode = TextureWrapMode.Clamp;
        if (vehicleDirection == Direction.Down)
        {
            if (Turning == Direction.Right)
            {
                //17 x
                //10 y
                for (int x = 17; x <= 22; x++)
                {
                    for (int y = 10; y <= 11; y++)
                    {
                        copiedTexture.SetPixel(x, y, new Color(218, 204, 44, 255));
                    }
                }
            }
        }
        copiedTexture.Apply();
        return copiedTexture;
    }

    void OnVehicleRemoved(Vehicle v)
    {
        if (VehicleGameObjectMap.ContainsKey(v) == true)
        {
            GameObject vehGO = VehicleGameObjectMap[v];
            Destroy(vehGO);
            VehicleGameObjectMap.Remove(v);
        }
    }

}
