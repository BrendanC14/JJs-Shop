using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Fixture : IXmlSerializable {

    public string FixtureType { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public Tile Tile { get; protected set; }
    public Tile CustomerTile;
    public Tile EmployeeTile;
    public Tile EntranceTile;
    public float InteractXModifier = 0f;
    public float InteractYModifier = 0f;
    public float EmployeeXModifier = 0f;
    public float EmployeeYModifier = 0f;
    public Direction InteractDirection = Direction.Down;
    public Direction EmployeeDirection = Direction.Down;
    public float PersonMovementCost { get; protected set; }
    public float VehicleMovementCost { get; protected set; }
    protected Dictionary<string, float> fixtParameters;
    protected Action<Fixture, float> updateActions;
    public Action<Fixture> cbCustomerOpinion;
    public Func<Fixture, Tile, Enterability> IsEnterable;
    public bool UserSelected;
    int Opinion; //1 = good, 2 = bad;
    public int PriceOpinion
    {
        get { return Opinion; }
        set
        {
            if (value != Opinion)
            {
                Opinion = value;
                if (cbCustomerOpinion != null)
                {
                    cbCustomerOpinion(this);
                }
            }
        }
    }
    

    public bool Fridge = false;
    public bool NeedsRestock = false;
    public int MaxShelfSpace = -1;
    public Item ItemOnShelf;
    public List<Item> ItemsOnShelf;
    public List<Item> GetItemsOnShelf()
    {
        return ItemsOnShelf;
    }
    public void RemoveItemFromShelf()
    {
        ItemsOnShelf.RemoveAt(0);
        if (cbOnChanged != null)
        {
            cbOnChanged(this);
        }
    }

    public Action<Fixture> cbOnChanged;

   
    protected Fixture (Fixture other)
    {
        this.FixtureType = other.FixtureType;
        this.PersonMovementCost = other.PersonMovementCost;
        this.VehicleMovementCost = other.VehicleMovementCost;
        this.Width = other.Width;
        this.Height = other.Height;
        if (other.updateActions != null) { this.updateActions = (Action<Fixture, float>)other.updateActions.Clone(); }
        this.IsEnterable = other.IsEnterable;
        this.fixtParameters = new Dictionary<string, float>(other.fixtParameters);
        this.CustomerTile = other.CustomerTile;
        this.EmployeeTile = other.EmployeeTile;
        this.ItemsOnShelf = new List<Item>(other.ItemsOnShelf);
    }
    virtual public Fixture Clone()
    {
        return new Fixture(this);
    }
    public Fixture(string fixtureType, float personMovementCost = 1f, float vehicleMovementCost = 0f, int width = 1, int height = 1)
    {
        this.FixtureType = fixtureType;
        this.PersonMovementCost = personMovementCost;
        this.VehicleMovementCost = vehicleMovementCost;
        this.Width = width;
        this.Height = height;
        this.fixtParameters = new Dictionary<string, float>();
        this.ItemsOnShelf = new List<Item>();

    }

    public void Update(float deltaTime)
    {
        if (updateActions != null) { updateActions(this, deltaTime); }
    }

    static public Fixture PlaceInstance(Fixture proto, Tile t)
    {
        Fixture obj = proto.Clone();
        obj.Tile = t;
        
        t.PlaceFixture(obj);
        t.VehicleMovementCost = obj.VehicleMovementCost;
        return obj;
    }

    public void PlaceItem(Item i)
    {
        ItemsOnShelf.Add(i);
        ItemOnShelf = i;
        if (cbOnChanged != null)
        {
            cbOnChanged(this);
        }
    }

    public float GetParameter(string key, float default_value = 0)
    {
        if (fixtParameters == null)
        {
            return default_value;
        }
        if (fixtParameters.ContainsKey(key) == false)
        {
            return default_value;
        }
        return fixtParameters[key];
    }

    public void SetParameter(string key, float value)
    {
        if (fixtParameters != null)
        {
            fixtParameters[key] = value;
        }
    }

    public void ChangeParameter(string key, float value)
    {
        if (fixtParameters.ContainsKey(key) == false)
        {
            fixtParameters[key] = value;
        }
        fixtParameters[key] += value;
    }

    public bool IsValidPosition(Tile t)
    {
        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + Height); y_off++)
            {
                Tile t2 = World.Current.GetTileAt(x_off, y_off);

                // Make sure tile is FLOOR
                if (t2.Type != TileType.Floor)
                {
                    return false;
                }

                // Make sure tile doesn't already have furniture
                if (t2.Fixture != null)
                {
                    return false;
                }

            }
        }


        return true;
    }

    public float GetPercentageFull()
    {
        return (float)GetItemsOnShelf().Count / (float)MaxShelfSpace;
    }
    
    public void RegisterUpdateAction(Action<Fixture, float> a) { updateActions += a; }
    public void UnrregisterUpdateAction(Action<Fixture, float> a) { updateActions -= a; }

    public void AddAFixtureChangedCallback(Action<Fixture> callback) { cbOnChanged += callback; }
    public void RemoveAFixtureChangedCallback(Action<Fixture> callback) { cbOnChanged -= callback; }
    public void AddCustomerOpinionCallback(Action<Fixture> callback) { cbCustomerOpinion += callback; }





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
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Y", Tile.Y.ToString());
        writer.WriteAttributeString("NeedsRestock", NeedsRestock.ToString());
        if (ItemsOnShelf.Count > 0)
        {
            writer.WriteAttributeString("ItemOnShelf", ItemsOnShelf[0].Name);
            writer.WriteAttributeString("ItemCount", ItemsOnShelf.Count.ToString());
        }

        foreach (string k in fixtParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", fixtParameters[k].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader)
    {
        if (reader.GetAttribute("NeedsRestock") == "True")
        {
            NeedsRestock = true;
        }
        else { NeedsRestock = false; }
        string name = reader.GetAttribute("ItemOnShelf");
        Item itemToAdd = null;
        foreach(Item i in World.Current.ItemsOnShelves)
        {
            if (i.Name == name)
            {
                itemToAdd = i;
                break;
            }
        }

        for (int i = 0; i < int.Parse(reader.GetAttribute("ItemCount")); i++)
        {
            ItemsOnShelf.Add(itemToAdd);
        }

        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("Value"));
                fixtParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }
}
