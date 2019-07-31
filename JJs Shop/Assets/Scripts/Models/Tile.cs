using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum TileType { Empty, Road, Pavement, Wall, Grass, Floor, Crossing, Water, Parking }
public enum Enterability { Yes, Never, Soon };

public class Tile : IXmlSerializable {

    int x;
    public int X { get { return x; } }
    int y;
    public int Y { get { return y; } }

    float personMovementCost;
    public float PersonMovementCost
    {
        get
        {
            if (Fixture != null)
            {
                return personMovementCost * Fixture.PersonMovementCost;
            }
            return personMovementCost;
        }
        set
        {
            personMovementCost = value;
        }
    }
    public float VehicleMovementCost;
    public List<Direction> AllowedDirections;
    public List<Tile> VehiclesComeFrom;
    public List<Person> People;
    public List<Employee> Employees;
    public Vehicle Vehicle;
    public Tile InteracterTile;
    public bool UserSelected;

    public Job pendingFurnitureJob = null;

    TileType type;
    public TileType Type
    {
        get { return type; }    
        set
        {
            type = value;
            if (Type == TileType.Road) { PersonMovementCost = 0; }
            else if (Type == TileType.Pavement) { PersonMovementCost = 1; }
            else if (Type == TileType.Wall) { PersonMovementCost = 0; }
            else if (Type == TileType.Grass) { PersonMovementCost = 3; }
            else if (Type == TileType.Floor) { PersonMovementCost = 1; }
            else if (Type == TileType.Crossing) { PersonMovementCost = 1; }
            else if (Type == TileType.Water) { PersonMovementCost = 4; }
            else if (Type == TileType.Parking) { PersonMovementCost = 0; World.Current.ParkingTiles.Add(this); }
            
            if (Type == TileType.Road) { VehicleMovementCost = 1; }
            else if (Type == TileType.Pavement) { VehicleMovementCost = 0; }
            else if (Type == TileType.Wall) { VehicleMovementCost = 0; }
            else if (Type == TileType.Grass) { VehicleMovementCost = 0; }
            else if (Type == TileType.Floor) { VehicleMovementCost = 0; }
            else if (Type == TileType.Crossing) { VehicleMovementCost = 1; }
            else if (Type == TileType.Water) { VehicleMovementCost = 0; }
            else if (Type == TileType.Parking) { VehicleMovementCost = 1; }

        }
    }
    Action<Tile> cbTileChanged;

    public Fixture Fixture { get; protected set; }

    public Tile(World world, int x, int y)
    {
        this.x = x;
        this.y = y;
        AllowedDirections = new List<Direction>();
        VehiclesComeFrom = new List<Tile>();
        People = new List<Person>();
        Employees = new List<Employee>();

    }

    public void PlaceFixture(Fixture fixtureInstance)
    {
        for (int x_off = X; x_off < (X + fixtureInstance.Width); x_off++)
        {
            for (int y_off = Y; y_off < (Y + fixtureInstance.Height); y_off++)
            {
                Fixture = fixtureInstance;
                Tile t = World.Current.GetTileAt(x_off, y_off);
                t.Fixture = fixtureInstance; 

            }
        }
    }

    public void AddATileChangedCallback(Action<Tile> callback) { cbTileChanged += callback; }
    public void RemoveATileChangedCallback(Action<Tile> callback) { cbTileChanged -= callback; }

    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;
        if (diagOkay) { ns = new Tile[8]; }
        else ns = new Tile[4];

        Tile n;
        n = World.Current.GetTileAt(X, Y + 1);
        ns[0] = n;
        n = World.Current.GetTileAt(X + 1, Y);
        ns[1] = n;
        n = World.Current.GetTileAt(X, Y - 1);
        ns[2] = n;
        n = World.Current.GetTileAt(X - 1, Y);
        ns[3] = n;

        if (diagOkay)
        {
            n = World.Current.GetTileAt(X + 1, Y + 1);
            ns[4] = n;
            n = World.Current.GetTileAt(X + 1, Y - 1);
            ns[5] = n;
            n = World.Current.GetTileAt(X - 1, Y - 1);
            ns[6] = n;
            n = World.Current.GetTileAt(X - 1, Y + 1);
            ns[7] = n;
        }
        return ns;
    }

    public float GetVehicleDirectionMovementCost(Tile previousTile)
    {
        if (AllowedDirections == null || AllowedDirections.Count == 0)
        {
            return VehicleMovementCost;
        }
        Direction vehicleDirection;

        if (previousTile.X + 1 == X) { vehicleDirection = Direction.Right; }
        else if (previousTile.X - 1 == X) { vehicleDirection = Direction.Left; }
        else if (previousTile.Y + 1 == Y) { vehicleDirection = Direction.Up; }
        else { vehicleDirection = Direction.Down; }

        bool directionAllowed = false;
        foreach (Direction d in AllowedDirections)
        {
            if (d == vehicleDirection)
            {
                directionAllowed = true;
            }
        }

        if (directionAllowed) { return VehicleMovementCost; }
        else { return 0; }
    }


    public Enterability IsEnterable(Tile tileComingFrom)
    {
        if (PersonMovementCost == 0)
        {
            return Enterability.Never;
        }
        if (Fixture != null && Fixture.IsEnterable != null)
        {
            
            return Fixture.IsEnterable(Fixture, tileComingFrom);
        }

        return Enterability.Yes;
    }

    public bool IsNeighbour (Tile tile, bool diagOkay = false)
    {
        if (this.X == tile.X && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1)) { return true; }
        if (this.Y == tile.Y && (this.X == tile.X + 1 || this.X == tile.X - 1)) { return true; }

        if (diagOkay)
        {
            if (this.X == tile.X + 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1)) { return true; }
            if (this.X == tile.X - 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1)) { return true; }

        }

        return false;

    }






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
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }
}
