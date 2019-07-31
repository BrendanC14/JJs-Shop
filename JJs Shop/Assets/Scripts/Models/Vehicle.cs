using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum Direction { Left, Right, Up, Down }

public class Vehicle : IXmlSerializable {

    public string VehicleType;
    public List<Person> people;

    public float X
    {
        get
        {
            if (nextTile == null)
            {
                return currTile[0].X;
            }
            return Mathf.Lerp(currTile[0].X, nextTile.X, MovementPercentage);
        }
    }
    public float Y
    {
        get
        {
            if (nextTile == null) { return currTile[0].Y; }
            return Mathf.Lerp(currTile[0].Y, nextTile.Y, MovementPercentage);
        }
    }

    Path_ASTar VehiclePath_AStar;
    List<Tile> _currTiles;
    public List<Tile> currTile
    {
        get { return _currTiles; }
        set
        {
            
        }
    }

    public void AddToCurrTiles(Tile tileToAdd)
    {
        if (!_currTiles.Contains(tileToAdd))
        {
            _currTiles.Insert(0, tileToAdd);
            tileToAdd.Vehicle = this;
        }
    }

    public void RemoveFromCurrTiles()
    {
        Tile tileToRemove = _currTiles[_currTiles.Count - 1];
        prevTile = tileToRemove;
        _currTiles.Remove(tileToRemove);
        tileToRemove.Vehicle = null;


    }
    public Tile prevTile;
    public Tile nextTile;
    public Tile secondTile;
    public Tile thirdTile;
    public Tile _destTile;
    public Tile destTile
    {
        get { return _destTile; }
        set
        {
            if (_destTile != value)
            {
                _destTile = value;
                VehiclePath_AStar = null;
            }
        }
    }
    bool Reversing;
    float MovementPercentage;
    float Speed;
    public int TileSize;
    int NumOfPeople;
    public string Colour;
    //float IdealSpeed;
    float TimeToWait;
    bool WantsToShop;
    bool Parked;
    bool indicating;
    bool Indicating
    {
        get { return indicating; }
        set
        {
            indicating = value;
            if (indicating)
            {
                if (cbVehicleIndicating != null) { cbVehicleIndicating(this); }
            }
        }
    }
    Direction direction;
    public Direction Direction
    {
        get { return direction; }
        set
        {
            if (value != direction)
            {
                direction = value;
                if (cbVehicleDirectionChanged != null) { cbVehicleDirectionChanged(this); }
            }
        }
    }

    List<Tile> TilesToStop;
    Action<Vehicle> cbVehiclePositionChanged;
    Action<Vehicle> cbVehicleDirectionChanged;
    Action<Vehicle> cbVehicleIndicating;

    public Vehicle(string Type, Tile startTile, Tile endTile, List<Tile> tilesToStop, bool Shop, int tileSize)
    {
        VehicleType = Type;
        _currTiles = new List<Tile>();
        currTile = new List<Tile>();
        AddToCurrTiles(startTile);
        destTile = endTile;
        //IdealSpeed = UnityEngine.Random.Range(Numbers.Current.CarSpeedMin, Numbers.Current.CarSpeedMax);
        Speed = Numbers.Current.CarSpeedMax;
        TileSize = tileSize;
        TilesToStop = new List<Tile>();
        TilesToStop = tilesToStop;
        WantsToShop = Shop;
        TimeToWait = Numbers.Current.TileStopWaitTime(currTile[0]);
        people = new List<Person>();

        if (VehicleType == Words.Current.Car)
        {
            Colour = Words.Current.GetColour();
            NumOfPeople = 1;
        }

    }
    public Vehicle (PersonSaveFile loadedPerson)
    {
        VehicleType = loadedPerson.VehicleType;
        _currTiles = new List<Tile>();
        currTile = new List<Tile>();
        AddToCurrTiles(World.Current.GetTileAt(loadedPerson.VehicleStartX, loadedPerson.VehicleStartY));
        destTile = World.Current.GetTileAt(loadedPerson.VehicleEndX, loadedPerson.VehicleEndY);
        Speed = Numbers.Current.CarSpeedMax;
        TileSize = loadedPerson.VehicleTileSize;
        TilesToStop = new List<Tile>();
        TilesToStop = World.Current.GetTilesToStop();
        WantsToShop = true;
        people = new List<Person>();
        Colour = loadedPerson.VehicleColour;
    }

    public void Update(float deltaTime)
    {
        if (TimeToWait > 0f)
        {
            TimeToWait -= deltaTime;
            if (TimeToWait < 0f) { TimeToWait = 0f; }
            return;
        }
        if (Parked)
        {
            if (people.Count != NumOfPeople)
            {
                return;
            }
            else { Parked = false; TimeToWait += 1f; return; }
        }


        UpdatePath();
        if (CarsTrapped()) { return; }
        if (TileBlocked()) { return; }

        if (!ReachedDestination())
        {
            Drive(deltaTime);
        }


    }

    void Drive(float deltaTime)
    {

        //Total dist from A to B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile[0].X - nextTile.X, 2) + Mathf.Pow(currTile[0].Y - nextTile.Y, 2));
        //How much can we travel this update
        float distThisFrame = (Speed / nextTile.VehicleMovementCost) * deltaTime;
        //How much in terms of percentage?
        float percThisFrame = distThisFrame / distToTravel;
        //Add to overral percentage travelled   
        MovementPercentage += percThisFrame;

        if (MovementPercentage >= 1)
        {
            if (currTile.Count >= TileSize)
            {
                RemoveFromCurrTiles();
            }
            
            MovementPercentage = 0f;

        }

        if (MovementPercentage == 0F)
        {

            AddToCurrTiles(nextTile);
            UpdatePath();
            if (TilesToStop.Contains(currTile[0]))
            {

                TimeToWait = Numbers.Current.TileStopWaitTime(currTile[0]);

            }
        }
        if (cbVehiclePositionChanged != null)
        {
            cbVehiclePositionChanged(this);
        }

        Indicating = CheckIndicating();

    }

    bool CarsTrapped()
    {
        if (nextTile != null && nextTile.Vehicle != null &&
            !World.Current.ParkingTiles.Contains(nextTile))
        {
            Vehicle v1 = nextTile.Vehicle;
            if (v1.nextTile != null && v1.nextTile.Vehicle != null)
            {
                Vehicle v2 = v1.nextTile.Vehicle;
                if (v2.nextTile != null && v2.nextTile.Vehicle != null)
                {

                    foreach (Tile t in World.Current.VehicleEndingTiles)
                    {
                        if (Direction == Direction.Left)
                        {
                            if (t.Y < Y)
                            {
                                destTile = t;
                            }
                        }
                    }
                    VehiclePath_AStar = null;
                    return true;
                }
            }
            
        }    
        return false;
    }
    bool ReachedDestination()
    {

        if (currTile[0] == destTile)
        {
            //Vehicle has parked and just needs to wait until they leave
            if (World.Current.ParkingTiles.Contains(currTile[0]))
            {
                currTile[0].Vehicle = this;
                CreateAShopper();
                Parked = true;
                
                destTile = World.Current.GetTileAt(0, 2);
                UpdatePath();
                return true;
            }
            //Means they've reached the entrace to car park and are waiting to park
            if (WantsToShop)
            {
                WantsToShop = false;
                TimeToWait = 1f;
                Tile ParkingSpace = EmptyParkingSpace();
                if (ParkingSpace == null)
                {
                    foreach (Tile t in World.Current.VehicleEndingTiles)
                    {
                        if (Direction == Direction.Down)
                        {
                            if (t.Y < Y)
                            {
                                destTile = t;
                            }
                        }
                        else if (Direction == Direction.Up)
                        {
                            if (t.Y > Y)
                            {
                                destTile = t;
                            }
                        }
                    }
                    UpdatePath();
                    WantsToShop = false;
                    return false;
                }
                destTile = ParkingSpace;
                ParkingSpace.Vehicle = this;
                UpdatePath();
                return true;
            }

            VehiclePath_AStar = null;
            RemoveVehicle();
            return true;
        }
        return false;
    }

    void UpdatePath()
    {
        if (nextTile == null || nextTile == currTile[0])
        {
            if (VehiclePath_AStar == null || VehiclePath_AStar.Length() == 0)
            {
                //Create a path
                VehiclePath_AStar = new Path_ASTar(World.Current, currTile[0], destTile, true, false, Direction);
                if (VehiclePath_AStar.Length() == 0)
                {
                    Debug.LogError("No route to destination");
                    VehiclePath_AStar = null;
                    return;
                }
                nextTile = VehiclePath_AStar.Dequeue();
                secondTile = VehiclePath_AStar.Dequeue();
                thirdTile = VehiclePath_AStar.Dequeue();
            }
            nextTile = secondTile;
            secondTile = thirdTile;
            thirdTile = VehiclePath_AStar.Dequeue();
            CheckDirection();


        }

    }

    void CheckDirection()
    {
        
        if (nextTile == World.Current.GetTileAt(currTile[0].X, currTile[0].Y + 1))
        {
            Direction = Direction.Up;
        }
        else if (nextTile == World.Current.GetTileAt(currTile[0].X, currTile[0].Y - 1))
        {
            Direction = Direction.Down;
        }
        else if (nextTile == World.Current.GetTileAt(currTile[0].X + 1, currTile[0].Y))
        {
            Direction = Direction.Right;
        }
        else if (nextTile == World.Current.GetTileAt(currTile[0].X - 1, currTile[0].Y))
        {
            Direction = Direction.Left;
        }
        //If it's a parking space I want them to reverse in
        if (World.Current.ParkingTiles.Contains(nextTile) || Reversing)
        {
            if (Direction == Direction.Down)
            {
                Direction = Direction.Up;
            }
            else { Direction = Direction.Down; }
        }
    }

    bool CheckIndicating()
    {
        if (Direction == Direction.Left || Direction == Direction.Right)
        {
            if (thirdTile != null) {
                if (thirdTile.Y != currTile[0].Y)
                {
                    return true;
                }
            }
            
        }
        else
        {
            if (thirdTile != null)
            {
                if (thirdTile.X != currTile[0].X)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void RemoveVehicle()
    {
        foreach(Tile t in currTile)
        {
            if (t != null)
            {
                t.Vehicle = null;
            }
        }
        if (nextTile != null)
        {
            nextTile.Vehicle = null;
        }
        currTile = null;
        World.Current.RemoveVehicle(this);

    }
    

    bool TileBlocked()
    {

        if (NextTileBlocked()) { return true; }
        if (CrossingTileBlocked()) { return true; }
        //if (PullOutBlocked()) { return true; }
        //if (TurningTileBlocked()) { return true; }
        return false;
    }

    bool NextTileBlocked()
    {
        if (nextTile != null)
        {
            if (nextTile.Vehicle != null && nextTile.Vehicle != this)
            {
                return true;
            }
            foreach (Tile t in nextTile.VehiclesComeFrom)
            {
                if (t.Vehicle != null )
                {
                    if (t.Vehicle.nextTile == nextTile)
                    {
                        if (!t.Vehicle.TilesToStop.Contains(t))
                        {
                            if (t.Vehicle.MovementPercentage > MovementPercentage)
                            {
                                return true;
                            }
                        }
                    }
                    if (t.Vehicle.currTile.Contains(nextTile))
                    {
                        return true;
                    }
                }
                
            }

        }

        if (secondTile!= null)
        {
            if (TileSize > 1 && secondTile.Vehicle != null &&secondTile.Vehicle != this)
            {
                return true;
            }
            if (nextTile.VehiclesComeFrom.Count > 1)
            {
                if (secondTile.Vehicle != null && secondTile.Vehicle != this)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    bool CrossingTileBlocked()
    {
        if (nextTile != null)
        {
            if (nextTile.Type == TileType.Crossing)
            {
                if (secondTile.Vehicle != null)
                {
                    //Checking the other side of the crossing
                    return true;
                }
                foreach (Tile t in World.Current.CrossingTiles)
                {
                    if (t.People.Count > 0)
                    {
                        //Checking whether the tiles at the crossing have people on them

                        foreach (Person p in t.People)
                        {
                            if (World.Current.CrossingTiles.Contains(p.nextTile) && MovementPercentage < 0.33F)
                            {
                                //Only want to stop vehicle if they're intending to cross
                                //Not if they've just crossed
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    void CreateAShopper()
    {
        float Speed = Numbers.Current.GetPedestrianSpeed();
        Person p = new Person(currTile[0].InteracterTile, true, Speed, false, this);
        if (World.Current.cbPersonCreated != null) { World.Current.cbPersonCreated(p); }
        World.Current.People.Add(p);
        p = null;

    }
    
    Tile EmptyParkingSpace()
    {
        foreach(Tile t in World.Current.ParkingTiles)
        {
            if (t.Vehicle == null)
            {
                return t;
            }
        }
        return null;
    }

    public void AddVehicleChangedPositionCallback(Action<Vehicle> callback) { cbVehiclePositionChanged += callback; }
    public void RemoveVehicleChangedPositionCallback(Action<Vehicle> callback) { cbVehiclePositionChanged -= callback; }
    public void AddVehicleChangedDirectionCallback(Action<Vehicle> callback) { cbVehicleDirectionChanged += callback; }
    public void RemoveVehicleChangedDirectionCallback(Action<Vehicle> callback) { cbVehicleDirectionChanged -= callback; }
    public void AddVehicleIndicatingCallback(Action<Vehicle> callback) { cbVehicleIndicating += callback; }
    public void RemoveVehicleIndicatingCallback(Action<Vehicle> callback) { cbVehicleIndicating -= callback; }

    //////////////////////////////////////////////////////////////////////////////////////
    /////               
    /////                           SAVING & LOADING
    /////       
    ///////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {

        writer.WriteAttributeString("startX", currTile[0].X.ToString());
        writer.WriteAttributeString("startY", currTile[0].Y.ToString());
        writer.WriteAttributeString("endX", destTile.X.ToString());
        writer.WriteAttributeString("endY", destTile.Y.ToString());

    }

    public void ReadXml(XmlReader reader)
    {

    }
}
