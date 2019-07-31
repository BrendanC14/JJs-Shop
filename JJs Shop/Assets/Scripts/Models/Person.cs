using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Person : IXmlSerializable {

    public float X
    {
        get
        {
            if (nextTile == null)
            {
                return currTile.X;
            }
            return Mathf.Lerp(currTile.X, nextTile.X, MovementPercentage);
        }
    }
    public float Y
    {
        get
        {
            if (nextTile == null)
            {
                return currTile.Y;
            }
            return Mathf.Lerp(currTile.Y, nextTile.Y, MovementPercentage);
        }
    }

    public float XModifier;
    public float YModifier;

    Path_ASTar WalkingPath_AStar;
    public Tile currTile;
    public Tile CurrTile
    {
        get { return currTile; }
        set
        {
            if (currTile != null) { currTile.People.Remove(this); }
            currTile = value;
            if (currTile != null) { currTile.People.Add(this); }
        }
    }
    public Tile nextTile;
    public Tile destTile;
    public Activity currActivity;
    public List<Item> ShoppingList;
    public List<Item> ItemsInHand;
    public List<Item> UnavailableItems;
    float TimeToWait;
    public bool UserSelected;
    public bool Interacting = false;
    float TooExpensivePercentage;
    float BuyTwoPercentage;
    float ExpectedPriceModifier;
    
    //Sprite stuff
    public int NumFrames = 1;
    public int AnimationNumber = 1;
    public string SpriteName;

    public Vehicle v;

    public Action<Person> cbPersonPositionChanged;
    public Action<Person> cbPersonDirectionChanged;

    public void AddPersonPositionChangedCallback(Action<Person> callback) { cbPersonPositionChanged += callback; }
    public void AddPersonDirectionChangedCallback(Action<Person> callback) { cbPersonDirectionChanged += callback; }

    float MovementPercentage;
    public float Speed;
    public bool WantsToShop;
        
    public bool WaitingToPay;
    public bool Walking;
    public bool Paid;
    Direction direction;
    public Direction Direction
    {
        get { return direction; }
        set
        {
            if (value != direction)
            {
                direction = value;
                if (cbPersonDirectionChanged != null) { cbPersonDirectionChanged(this); }
            }
        }
    }

    public Person(Tile startTile, bool wantToShop, float speed, bool loading, Vehicle vehicle = null)
    {
        CurrTile = startTile;
        WantsToShop = wantToShop;
        Speed = speed;
        v = vehicle;

        XModifier = Numbers.Current.GetRandomTileModerator();
        YModifier = Numbers.Current.GetRandomTileModerator();
        TooExpensivePercentage = Numbers.Current.GetRandomTooExpensivePercentage();
        BuyTwoPercentage = Numbers.Current.GetRandomBuyTwoPercentage();
        ExpectedPriceModifier = Numbers.Current.GetRandomExpectedPriceModifier();
        ShoppingList = new List<Item>();
        ItemsInHand = new List<Item>();
        UnavailableItems = new List<Item>();
        
        if (WantsToShop) { CreateShoppingList(); }
        //AddRandomActions();
        SpriteName = Words.Current.GetRandomCharacterSpriteName();
        
    }
    public Person(PersonSaveFile savedPerson)
    {
        if (savedPerson.X != 0)
        {
            currTile = World.Current.GetTileAt(savedPerson.X, savedPerson.Y);
        }
        WantsToShop = savedPerson.WantsToShop;
        Speed = savedPerson.speed;
        SpriteName = savedPerson.SpriteName;

        if (savedPerson.VehicleStartX != 0)
        {
            v = new Vehicle(savedPerson);
        }

        if (savedPerson.currActivityName != "")
        {
            currActivity = new Activity(
                savedPerson.currActivityName,
                World.Current.GetTileAt(savedPerson.currActivityX, savedPerson.currActivityY),
                savedPerson.currActivityTime,
                ParseDirection(savedPerson.currActivityDirection),
                ActivityComplete,
                ActivityCanclled,
                savedPerson.currActivityXMod,
                savedPerson.currActivityYMod);
        }

        ShoppingList = new List<Item>();
        foreach (string s in savedPerson.ShoppingList)
        {
            ShoppingList.Add(World.Current.GetItemFromShopShelf(s));
        }
        ItemsInHand = new List<Item>();
        foreach (string s in savedPerson.ItemsInHand)
        {
            ItemsInHand.Add(World.Current.GetItemFromShopShelf(s));
        }
        UnavailableItems = new List<Item>();
        foreach (string s in savedPerson.UnavailableItems)
        {
            UnavailableItems.Add(World.Current.GetItemFromShopShelf(s));
        }
    }

    

     void CreateShoppingList()
    {
        foreach (Item i in World.Current.ItemsOnShelves)
        {
            int r = UnityEngine.Random.Range(1, 101);
            if (r <= Numbers.Current.PercChangeOfWantingItem)
            {
                ShoppingList.Add(i);
            }
        }
    }

    public void Update(float deltaTime)
    {
        Update_DoActivity(deltaTime);
        if (!Interacting)
        {
            Walking = true;
            Update_DoMovement(deltaTime);
        }
        else { Walking = false; }
    }

    void Update_DoActivity(float deltaTime)
    {
        if (currActivity == null)
        {
            GetNewActivity();
            if (currActivity == null)
            {
                destTile = currTile;
                return;
            }
        }

        //I got a job.
        //Am I at my tile and have my Modifiers been met?
        if (CurrTile == destTile &&
            ModifiersCorrect(currActivity.XModifier, currActivity.YModifier)) {

            Direction = currActivity.FacingDirection;
            Interacting = true;
            currActivity.DoActivity(deltaTime);



        }
    

    }
    void GetNewActivity()
    {
        //If I still have a shopping list, let's go get it!
        if (ShoppingList.Count > 0)
        {
            currActivity = new Activity(
                Words.Current.ShopActivity,
                ShoppingList[0].ShopShelf.CustomerTile,
                Numbers.Current.ShopTime,
                ShoppingList[0].ShopShelf.InteractDirection,
                ActivityComplete,
                ActivityCanclled,
                ShoppingList[0].ShopShelf.InteractXModifier,
                ShoppingList[0].ShopShelf.InteractYModifier
                );
        }
        else
        {
            //Was I able to pick anything up?
            if (ItemsInHand.Count > 0)
            {
                //I no longer have a shopping list. Have I paid?
                if (!Paid && ItemsInHand.Count > 0)
                {
                    currActivity = new Activity(
                        Words.Current.PayActivity,
                        World.Current.Checkouts[0].CustomerTile,
                        Numbers.Current.TimeWillWaitForCashier,
                        World.Current.Checkouts[0].InteractDirection,
                        ActivityComplete,
                        ActivityCanclled,
                        World.Current.Checkouts[0].InteractXModifier,
                        World.Current.Checkouts[0].InteractYModifier);
                    World.Current.PeopleInQueue++;

                    //If I'm heading to the till we better put a job in the Checkout Queue
                    World.Current.CheckoutQueue.Enqueue(new Job(
                        World.Current.Checkouts[0].Tile,
                        Words.Current.CheckoutQueue,
                        "",
                        ServedByStaff,
                        ItemsInHand.Count * Numbers.Current.TimeToScanItem));
                }
            }
            else
            {
                currActivity = null;
                LeaveShop();
            }
        }

        if (currActivity == null)
        {
            return;
        }
        destTile = currActivity.tile;

        WalkingPath_AStar = new Path_ASTar(World.Current, CurrTile, destTile, false, true, Direction);
        if (WalkingPath_AStar.Length() == 0)
        {
            Debug.LogError("No route to dest");
            destTile = currTile;
        }
    }

    void Update_DoMovement(float deltaTime)
    {
        if (currTile == destTile)
        {
            WalkingPath_AStar = null;
            return;
        }

        if (nextTile == null || nextTile == currTile)
        {

            if (WalkingPath_AStar == null || WalkingPath_AStar.Length() == 0)
            {
                WalkingPath_AStar = new Path_ASTar(World.Current, CurrTile, destTile, false, true, Direction);
                if (WalkingPath_AStar.Length() == 0)
                {
                    Debug.LogError("No path to dest");
                    return;
                }
                nextTile = WalkingPath_AStar.Dequeue();
            }
            nextTile = WalkingPath_AStar.Dequeue();
            CheckDirection();
        }


        if (nextTile != null && nextTile.IsEnterable(CurrTile) == Enterability.Never)
        {
            nextTile = null;
            WalkingPath_AStar = null;
            return;
        }
        else if (nextTile != null && nextTile.IsEnterable(CurrTile) == Enterability.Soon)
        {
            if (!World.Current.ShopOpen)
            {
                //Shop is closed, I should see how long until it's open
                TimeSpan waitingTime = World.Current.Today.OpeningTime - WorldTime.Current.Date;
                if ((waitingTime.Days > 0 || waitingTime.Hours > 0 || waitingTime.Minutes > 15) &&
                    (!World.Current.ShopTiles.Contains(CurrTile) && !World.Current.StockRoomTiles.Contains(CurrTile)))
                {
                    TimeToWait = 1f;
                    LeaveShop();
                    nextTile = null;
                    WalkingPath_AStar = null;
                    return;
                }
            }
            return;
        }
        //Total dist from A to B
        if (nextTile == null)
        {
            Debug.Log("Wtf");
        }
        float speedWalking = Speed;
        if (XModifier > 0.5f || XModifier < -0.5f || YModifier > 0.5f || YModifier < -0.5f)
        {
            if (!ModifiersCorrect(Numbers.Current.GetRandomTileModerator(), Numbers.Current.GetRandomTileModerator()))
            {
                speedWalking = speedWalking / 2;
            }
        }
        if (nextTile != null && currTile != null)
        {
            float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));
            //How much can we travel this update
            float distThisFrame = (speedWalking / nextTile.PersonMovementCost) * deltaTime;
            //How much in terms of percentage?
            float percThisFrame = distThisFrame / distToTravel;
            //Add to overral percentage travelled   
            MovementPercentage += percThisFrame;


            if (MovementPercentage >= 1)
            {
                CurrTile = nextTile;
                MovementPercentage = 0f;
            }


            if (cbPersonPositionChanged != null)
            {
                cbPersonPositionChanged(this);
            }
        }
    }

    void GetItemFromShelf()
    {
        //I need to decide if I want the item or not
        Item i = ShoppingList[0];
        double ExpectedPrice = i.ExpectedPrice * (double)ExpectedPriceModifier;
        double TooExpensivePrice = i.ExpectedPrice * (double)TooExpensivePercentage;
        double BuyTwoPrice = i.ExpectedPrice * (double)BuyTwoPercentage;

        if (i.Price < TooExpensivePrice)
        {
            // I definitely want the item
            if (i.Price < BuyTwoPrice)
            {
                //This is so cheap I want two! 
                if (i.TakeOffShopShelf(2))
                {
                    //There are 2 available
                    ItemsInHand.Add(i);
                    ItemsInHand.Add(i);
                    i.ShopShelf.PriceOpinion = 1;
                    i.BoughtTwo++;
                }
                else if (i.TakeOffShopShelf(1))
                {
                    ItemsInHand.Add(i);
                    i.WouldaBoughtTwo++;
                    i.ShopShelf.PriceOpinion = 0;
                }
            }
            else
            {
                //I'm happy with the price but not happy enough to buy two.
                if (i.TakeOffShopShelf(1))
                {
                    ItemsInHand.Add(i);
                    if (i.Price <= ExpectedPrice)
                    {
                        i.Happy++;
                    }
                    else
                    {
                        i.Neutral++;
                    }
                    i.ShopShelf.PriceOpinion = 0;
                }
                else
                {
                    //Not enough stock
                    i.OuttaStock++;
                    i.ShopShelf.PriceOpinion = 0;
                }
            }
        }
        else
        {
            //This is far too expensive for what I want to pay.
            i.NotBuying++;
            i.ShopShelf.PriceOpinion = 2;
        }
        ShoppingList.RemoveAt(0);
    }


    void LeaveShop()
    {
        if (v!= null)
        {
            currActivity = new Activity(
                Words.Current.CarActivity,
                v.currTile[0].InteracterTile,
                Numbers.Current.GetInCarTime,
                Direction.Up,
                ActivityComplete,
                ActivityCanclled);
        }
        else
        {
            currActivity = new Activity(
                Words.Current.WalkActivity,
                GetEndTile(),
                Numbers.Current.WalkOfMappTime,
                Direction.Right,
                ActivityComplete,
                ActivityCanclled);
        }
        destTile = currActivity.tile;
    }

    void ActivityComplete(Activity a)
    {
        Interacting = false;
        if (currActivity != null)
        {
            if (currActivity.ActivityName == Words.Current.ShopActivity)
            {
                GetItemFromShelf();
                currActivity = null;
                return;
            }
            else if (currActivity.ActivityName == Words.Current.PayActivity)
            {
                if (!Paid)
                {
                    //TODO: Show the customer leaving
                    //TODO: Leave the stuff behind
                    currActivity = null;
                    return;
                }
                else
                {
                    Double cost = 0;
                    foreach (Item i in ItemsInHand)
                    {
                        cost += i.Price;
                    }
                    World.Current.Balance += cost;
                    World.Current.BalanceDiff = cost;
                    currActivity = null;
                    LeaveShop();
                    return;
                }
            }
            else if (currActivity.ActivityName == Words.Current.CarActivity)
            {
                v.people.Add(this);
                currActivity = null;
                RemovePerson();
                return;
            }
            else if (currActivity.ActivityName == Words.Current.WalkActivity)
            {
                currActivity = null;
                RemovePerson();
                return;
            }
        }
    }
    void ServedByStaff(Job j)
    {
        Paid = true;
        World.Current.PeopleInQueue--;
        World.Current.Transactions.Add(new Transaction(
            Words.Current.CustomerPurchase,
            ItemsInHand));
        ActivityComplete(currActivity);
    }
    void ActivityCanclled(Activity a)
    {

    }
    
    

    bool ModifiersCorrect(float targetX, float targetY)
    {
        bool Moving = false;

        float destModifier = targetX;
        if (destModifier > XModifier)
        {
            XModifier += Numbers.Current.ModifierIncrease(Speed);
            if (XModifier > destModifier)
            {
                XModifier = destModifier;
            }
            else
            {
                Moving = true;
            }
        }
        else if (destModifier < XModifier)
        {
            XModifier -= Numbers.Current.ModifierIncrease(Speed);
            if (XModifier < destModifier)
            {
                XModifier = destModifier;
            }
            else
            {
                Moving = true;
            }
        }

        destModifier = targetY;
        if (destModifier > YModifier)
        {
            YModifier += Numbers.Current.ModifierIncrease(Speed);
            if (YModifier > destModifier)
            {
                YModifier = destModifier;
            }
            else
            {
                Moving = true;
            }
        }
        else if (destModifier < YModifier)
        {
            YModifier -= Numbers.Current.ModifierIncrease(Speed);
            if (YModifier < destModifier)
            {
                YModifier = destModifier;
            }
            else
            {
                Moving = true;
            }
        }

        if (Moving)
        {
            Walking = true;
            CheckDirection();
            if (cbPersonPositionChanged != null)
            {
                cbPersonPositionChanged(this);
            }
            return false;
        }

        return true;
    }
    

    void CheckDirection()
    {
        if (nextTile == null || currTile == nextTile)
        {
            if (currActivity != null)
            {

                if (YModifier > currActivity.YModifier)
                {
                    Direction = Direction.Down;
                }
                else if (YModifier < currActivity.YModifier)
                {
                    Direction = Direction.Up;
                }
                else
                {
                    if (XModifier > currActivity.XModifier)
                    {
                        Direction = Direction.Left;
                    }
                    else if (XModifier < currActivity.XModifier)
                    {
                        Direction = Direction.Right;
                    }
                    else
                    {
                        Direction = Direction.Down;
                    }
                }
            }
        }
        else
        {
            if (CurrTile.X > nextTile.X)
            {
                Direction = Direction.Left;
            }
            else if (CurrTile.X < nextTile.X)
            {
                Direction = Direction.Right;
            }
            else
            {
                if (CurrTile.Y > nextTile.Y)
                {
                    Direction = Direction.Down;
                }
                else { Direction = Direction.Up; }
            }
        }
    }
    //void OnPaid()
    //{

    //}
    Tile GetRandomShoppingTile()
    {
        int Random = UnityEngine.Random.Range(0, World.Current.ShopTiles.Count - 1);

        return World.Current.ShopTiles[Random];
    }
    Tile GetEndTile()
    {
        int rand = UnityEngine.Random.Range(0, World.Current.PedestrianExitTiles.Count - 1);
        return World.Current.PedestrianExitTiles[rand];
    }
    void RemovePerson()
    {
        CurrTile = null;
        World.Current.RemovePerson(this);

    }


    //////////////////////////////////////////////////////////////////////////////////////
    /////               
    /////                           SAVING & LOADING
    /////       
    ///////////////////////////////////////////////////////////////////////////////////////
    public Person()
    {

    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", currTile.X.ToString());
        writer.WriteAttributeString("Y", currTile.Y.ToString());
        writer.WriteAttributeString("WantsShop", WantsToShop.ToString());
        writer.WriteAttributeString("Speed", Speed.ToString());
        
        if (v!= null)
        {
            writer.WriteStartElement("Vehicle");
            v.WriteXml(writer);
            writer.WriteEndElement();
        }

        if (currActivity != null)
        {
            writer.WriteStartElement("CurrActivity");
            writer.WriteAttributeString("Name", currActivity.ActivityName);
            writer.WriteAttributeString("X", currActivity.tile.X.ToString());
            writer.WriteAttributeString("Y", currActivity.tile.Y.ToString());
            writer.WriteAttributeString("Time", currActivity.TimeTakes.ToString());
            writer.WriteAttributeString("Direction", currActivity.FacingDirection.ToString());
            writer.WriteAttributeString("XMod", currActivity.XModifier.ToString());
            writer.WriteAttributeString("YMod", currActivity.YModifier.ToString());
            writer.WriteEndElement();
        }

        writer.WriteStartElement("ShoppingList");
        foreach (Item i in ShoppingList)
        {
            writer.WriteAttributeString("Item", i.Name);
        }
        writer.WriteEndElement();

        writer.WriteStartElement("ItemsInHand");
        foreach (Item i in ItemsInHand)
        {
            writer.WriteAttributeString("Item", i.Name);
        }
        writer.WriteEndElement();

        writer.WriteStartElement("UnavilableItems");
        foreach (Item i in UnavailableItems)
        {
            writer.WriteAttributeString("Item", i.Name);
        }
        writer.WriteEndElement();

        writer.WriteAttributeString("TooExp", TooExpensivePercentage.ToString());
        writer.WriteAttributeString("BuyTwo", BuyTwoPercentage.ToString());
        writer.WriteAttributeString("ExpectedPriceMod", ExpectedPriceModifier.ToString());


    }

    public void ReadXml(XmlReader reader)
    {

        if (reader.ReadToDescendant("Vehicle"))
        {
            int startX = int.Parse(reader.GetAttribute("startX"));
            int startY = int.Parse(reader.GetAttribute("startY"));
            int endX = int.Parse(reader.GetAttribute("endX"));
            int endY = int.Parse(reader.GetAttribute("endY"));
            v = new Vehicle(Words.Current.Car,
                World.Current.GetTileAt(startX, startY),
                World.Current.GetTileAt(endX, endY),
                World.Current.GetTilesToStop(),
                WantsToShop, 1);
            World.Current.Vehicles.Add(v);
        }

        if (reader.ReadToDescendant("CurrActivity"))
        {
            int x = int.Parse(reader.GetAttribute("X"));
            int y = int.Parse(reader.GetAttribute("Y"));

            currActivity = new Activity(
                reader.GetAttribute("Name"),
                World.Current.GetTileAt(x, y),
                float.Parse(reader.GetAttribute("Time")),
                ParseDirection(reader.GetAttribute("Direction")),
                ActivityComplete,
                ActivityCanclled,
                float.Parse(reader.GetAttribute("XMod")),
                float.Parse(reader.GetAttribute("YMod")));
        }


        if (reader.ReadToDescendant("ShoppingList"))
        {
            do
            {
                ShoppingList.Add(World.Current.GetItemFromShopShelf(reader.GetAttribute("Name")));
            } while (reader.ReadToNextSibling("Item"));
        }
        if (reader.ReadToDescendant("ItemsInHand"))
        {
            do
            {
                ItemsInHand.Add(World.Current.GetItemFromShopShelf(reader.GetAttribute("Name")));
            } while (reader.ReadToNextSibling("Item"));
        }
        if (reader.ReadToDescendant("UnavilableItems"))
        {
            do
            {
                UnavailableItems.Add(World.Current.GetItemFromShopShelf(reader.GetAttribute("Name")));
            } while (reader.ReadToNextSibling("Item"));
        }

        TooExpensivePercentage = float.Parse(reader.GetAttribute("TooExp"));
        BuyTwoPercentage = float.Parse(reader.GetAttribute("BuyTwo"));
        ExpectedPriceModifier = float.Parse(reader.GetAttribute("ExpectedPriceMod"));

    }
    Direction ParseDirection(string direction)
    {
        if (direction == "Right")
        {
            return Direction.Right;
        }
        else if (direction == "Left")
        {
            return Direction.Left;
        }
        else if (direction == "Up")
        {
            return Direction.Up;
        }

        return Direction.Down;
    }
}
