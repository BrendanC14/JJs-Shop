using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Linq;


public class World : IXmlSerializable {

    Tile[,] tiles;
    TileType[] TypeMap;
    int width;
    public int Width { get { return width; } }
    int height;
    public int Height { get { return height; } }
    List<Tile> VehicleStartingTiles;
    public List<Tile> VehicleEndingTiles;
    public List<Tile> ParkingTiles;
    public List<Tile> RoadTiles;
    public List<Tile> ShopTiles;
    public List<Tile> StockRoomTiles;
    public List<Tile> CrossingTiles;
    public List<Tile> PedestrianExitTiles;
    public List<Fixture> Checkouts;
    public List<Item> ItemsOnShelves;
    public List<Item> ItemsInStockRoom;

    public List<Fixture> Fixtures;
    public List<Vehicle> Vehicles;
    public List<Person> People;
    public List<Employee> Employees;
    public List<Transaction> Transactions;
    public List<Employee> Applicants;
    public List<WorkingDay> Next30Days;
    public List<WorkingDay> Last30Days;

    public WorkingDay Today;

    public Action<Vehicle> cbVehicleCreated;
    public Action<Vehicle> cbVehicleRemoved;
    public Action<Person> cbPersonCreated;
    public Action<Person> cbPersonRemoved;
    public Action<Fixture> cbFixtureCreated;
    public Action<Employee> cbEmployeeCreated;
    public Action<Employee> cbEmployeeRemoved;
    public Action<double> cbBalanceUpdating;
    public Action cbEndOfDay;

    public Path_DrivingGraph DrivingGraph;
    public Path_WalkingGraph WalkingGraph;
    public Dictionary<string, Fixture> FixturePrototypes;
    public Dictionary<string, Activity> ActivityPrototypes;
    public Dictionary<string, Job> FixtureJobPrototypes;
    public Dictionary<string, Item> ItemPrototypes;
    public List<Fixture> CustomerDoors;
    public Tile CheckoutTile;
    public float DeliveryTime = 0f;
    public Item ItemToDeliver;
    public int QuantityToDeliver;

    public JobQueue JobQueue;
    public ConstructionQueue ConstructionQueue;
    public CheckoutQueue CheckoutQueue;
    public StockQueue StockQueue;
    public CostChangeQueue CostChangeQueue;
    float timeLastAddedVehicle;
    float timeLastAddedBus;
    float timeLastAddedPerson;
    float timeCount;
    float timeSinceLastMinute;
    public double Balance = 0;
    public double BalanceToDisplay;
    public double BalanceDiff = 0;
    public int checkoutLength;
    public int PeopleInQueue
    {
        get { return checkoutLength; }
        set
        {
            checkoutLength = value;
            if (Checkouts[0].cbOnChanged != null)
            {
                Checkouts[0].cbOnChanged(Checkouts[0]);
            }
        }
    }

    public bool paused;
    public bool ShopOpen;
    public int WorldSpeed = 1;

    public static World Current { get; protected set; }

    public void PauseUnpause()
    {
        if (paused)
        {
            paused = false;
        }
        else
        {
            paused = true;
        }
    }

    public void Update(float deltaTime)
    {
        deltaTime = deltaTime * WorldSpeed;
        if (Balance != BalanceToDisplay)
        {
            if (Balance - BalanceToDisplay < 1 && Balance - BalanceToDisplay > -1)
            {
                BalanceToDisplay = Balance;
                BalanceDiff = 0;
            }
            else
            {
                BalanceToDisplay += (Balance - BalanceToDisplay) / 30;

            }

            if (cbBalanceUpdating != null)
            {
                cbBalanceUpdating(BalanceToDisplay);
            }
        }
        if (!paused)
        {
            timeSinceLastMinute += deltaTime;
            if (timeSinceLastMinute > Numbers.Current.TimeBetweenMinutes)
            {
                WorldTime.Current.AddMinute();
                if (ShopOpen)
                {
                    if (WorldTime.Current.Date.TimeOfDay == Today.ClosingTime.TimeOfDay)
                    {
                        DateTime today = WorldTime.Current.Date;
                        ShopOpen = false;
                       // Numbers.Current.UpdateClosingTime(today.Year, today.Month, today.Day + 1);
                    }
                }
                else if (!ShopOpen)
                {
                    if (WorldTime.Current.Date.TimeOfDay == Today.OpeningTime.TimeOfDay)
                    {
                        DateTime today = WorldTime.Current.Date;
                        ShopOpen = true;
                      //  Numbers.Current.UpdateOpeningTime(today.Year, today.Month, today.Day + 1);
                    }
                }
                timeSinceLastMinute = 0f;
            }
            if (VehicleStartingTiles.Count > 0) { AddMoreCars(deltaTime); AddMoreBuses(deltaTime); }
            if (PedestrianExitTiles.Count > 0) { AddMoreShoopers(deltaTime); }
            if (Vehicles.Count > 0 || People.Count > 0)
            {
                List<Vehicle> vehs = new List<Vehicle>(Vehicles);
                foreach (Vehicle v in vehs)
                {
                    v.Update(deltaTime);
                }
                List<Person> peeps = new List<Person>(People);
                foreach (Person p in peeps)
                {
                    p.Update(deltaTime);
                }
                List<Fixture> fixts = new List<Fixture>(Fixtures);
                foreach (Fixture f in fixts)
                {
                    f.Update(deltaTime);
                }
            }

            int CountNotWorking = 0;
            foreach (Employee e in Employees)
            {
                if (!e.OnShift)
                {
                    if (e.GetTodaysStartTime(Today).AddMinutes(-10) == WorldTime.Current.Date &&
                        e.ReliabilityPassedCanStart())
                    {
                        e.StartShift();
                        if (cbEmployeeCreated != null) { cbEmployeeCreated(e); }
                    }
                    else
                    {
                        CountNotWorking++;
                    }
                }
                else
                {
                    if (e.GetTodaysEndTime(Today) <= WorldTime.Current.Date)
                    {
                        //I should've finished, let me make sure it's not closing time too
                        if (ShopOpen)
                        {
                            e.EndShift();
                        }
                        else
                        {
                            bool Shoppers = false;
                            //Shop is closed, but let me make sure everyone has left
                            foreach (Tile t in ShopTiles)
                            {
                                if (t.People.Count > 0)
                                {
                                    Shoppers = true;
                                }
                            }
                            if (!Shoppers)
                            {
                                e.EndShift();
                            }

                        }
                    }
                    e.Update(deltaTime);
                }
            }


            //if (Employees.Count > 0)
            //{
            //    foreach (Employee e in Employees)
            //    {
            //        e.Update(deltaTime);
            //    }
            //}
            //else
            //{
            //    Employee e2 = new Employee(GetTileAt(13, 17),
            //        Numbers.Current.GetPedestrianSpeed(),
            //        Words.Current.GetRestockPriorityList(),
            //        1);
            //    Employees.Add(e2);
            //    if (cbEmployeeCreated != null) { cbEmployeeCreated(e2); }
            //    Employee e = new Employee(GetTileAt(10, 11),
            //        Numbers.Current.GetPedestrianSpeed(),
            //        Words.Current.GetDefaultPriorityList(),
            //        2);
            //    Employees.Add(e);
            //    if (cbEmployeeCreated != null) { cbEmployeeCreated(e); }

            //}


            if (DeliveryTime > 0f)
            {
                DeliveryTime -= deltaTime;
                if (DeliveryTime < 0f)
                {
                    DeliveryTime = 0f;
                    if (ItemToDeliver != null)
                    {
                        ItemToDeliver.ItemDelivered(QuantityToDeliver);
                        ItemToDeliver = null;
                        QuantityToDeliver = 0;
                    }
                }
                return;
            }
            if (CountNotWorking == Employees.Count && !ShopOpen && WorldTime.Current.Date > Today.ClosingTime)
            {
                //All Employees aren't working and shop closed 
                if (cbEndOfDay != null)
                {
                    cbEndOfDay();
                }
            }
        }

    }

    public World(int width, int height, bool load)
    {

        SetupWorld(width, height, load);
    }

    void SetupWorld(int w, int h, bool loading)
    {
        width = w;
        height = h;
        Current = this;
        tiles = new Tile[width + 1, height + 1];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
            }
        }
        Fixtures = new List<Fixture>();
        Vehicles = new List<Vehicle>();
        People = new List<Person>();
        Employees = new List<Employee>();
        Applicants = new List<Employee>();
        Transactions = new List<Transaction>();
        VehicleStartingTiles = new List<Tile>();
        VehicleEndingTiles = new List<Tile>();
        ParkingTiles = new List<Tile>();
        RoadTiles = new List<Tile>();
        ShopTiles = new List<Tile>();
        StockRoomTiles = new List<Tile>();
        CrossingTiles = new List<Tile>();
        PedestrianExitTiles = new List<Tile>();
        Last30Days = new List<WorkingDay>();
        Next30Days = new List<WorkingDay>();
        Today = new WorkingDay(WorldTime.Current.Date, WorldTime.Current.Date, WorldTime.Current.Date, new Dictionary<Employee, DateTime>(), new Dictionary<Employee, DateTime>());
        ConstructionQueue = new ConstructionQueue();
        CheckoutQueue = new CheckoutQueue();
        StockQueue = new StockQueue();
        Checkouts = new List<Fixture>();
        ItemsOnShelves = new List<Item>();
        ItemsInStockRoom = new List<Item>();
        JobQueue = new JobQueue();
        CostChangeQueue = new CostChangeQueue();
        CustomerDoors = new List<Fixture>();
        CreateFixturePrototypes();
        HardcodeVillageTileTypes();
        SetVillageVehicleStartingTiles();
        SortWhereVehiclesComeFrom();
        SetShopTiles();
        SetStockRoomTiles();
        SetParkingExitTiles();
        SetPedestrianExitTiles();
        AddCrossingTiles();
        PutOutVillageFixtures();
        SetEmployerTiles();
        CreateItemPrototypes();
        SetInteractorTiles();
        if (!loading)
        {


            StockShelves();
            FillStockRoom();

            BalanceToDisplay = Balance;
            PeopleInQueue = 0;
            timeSinceLastMinute = Time.deltaTime;
        }
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x >= width || x < 0 || y >= height || y < 0)
        {
            return null;
        }
        return tiles[x, y];
    }

    public void StartNextDay()
    {
        Transaction purchases = new Transaction(Words.Current.CustomerPurchase, new List<Item>());
        Transaction orders = new Transaction(Words.Current.ShopOrder, new List<Item>());
        foreach (Transaction t in Today.transactions)
        {
            //To make it easier to save/load, only today's transactions will break into individual 
            //receipts. The previous days will just have totals
            if (t.Type == Words.Current.CustomerPurchase)
            {
                purchases.Cost += t.Cost;
            }
            else if (t.Type == Words.Current.ShopOrder)
            {
                orders.Cost += t.Cost;
            }
        }
        Transactions = new List<Transaction>();
        Transactions.Add(purchases);
        Transactions.Add(orders);
        Today.transactions = Transactions;

        Transactions = new List<Transaction>();
        Last30Days.Add(Today);
        Today = Next30Days[0];
        Next30Days.RemoveAt(0);
        Next30Days.Add(CreateAWorkingDay());
        
        WorldTime.Current.Date = new DateTime(Today.Date.Year, Today.Date.Month, Today.Date.Day, Numbers.Current.OpeningHour - 1, 45, 0);
    }
    WorkingDay CreateAWorkingDay()
    {
        DateTime TodayPlus30 = new DateTime();
        TodayPlus30 = WorldTime.Current.Date.AddDays(30);
        Dictionary<Employee, DateTime> startTimes = new Dictionary<Employee, DateTime>();
        Dictionary<Employee, DateTime> endTimes = new Dictionary<Employee, DateTime>();

        foreach (Employee e in World.Current.Employees)
        {
            startTimes.Add(e, new DateTime(TodayPlus30.Year, TodayPlus30.Month, TodayPlus30.Day, e.StartingHour, 0, 0));
            endTimes.Add(e, new DateTime(TodayPlus30.Year, TodayPlus30.Month, TodayPlus30.Day, e.EndHour, 0, 0));
        }
        DateTime OpenTime = new DateTime(TodayPlus30.Year, TodayPlus30.Month, TodayPlus30.Day, Numbers.Current.OpeningHour, 0, 0);
        DateTime CloseTime = new DateTime(TodayPlus30.Year, TodayPlus30.Month, TodayPlus30.Day, Numbers.Current.ClosingHour, 0, 0);
        return new WorkingDay(TodayPlus30, OpenTime, CloseTime, startTimes, endTimes);


    }
    public void PlaceFixture(string fixtureType, Tile t)
    {
        if (!FixturePrototypes.ContainsKey(fixtureType))
        {
            Debug.Log("Why isn't this a Fixture Prototype?");
        }
        Fixture fixt = Fixture.PlaceInstance(FixturePrototypes[fixtureType], t);
        Fixtures.Add(fixt);
        if (fixtureType == Words.Current.Checkout)
        {
            Checkouts.Add(fixt);
        }
        
        if (cbFixtureCreated != null)
        {
            cbFixtureCreated(fixt);
            if (fixt.PersonMovementCost != 1)
            {
                InvalidateTileGraphs();
            }
        }

    }

    void AddMoreBuses(float deltaTime)
    {
        if (timeCount - timeLastAddedBus > Numbers.Current.TimeBetweenBuses)
        {
            bool WantsToShop = false;
            Tile startingTile = GetRandomStartingTile();
            Tile endTile = GetRandomEndTile();
            if (startingTile.IsNeighbour(endTile, true) || startingTile.Vehicle != null)
            {
                return;
            }

            List<Tile> TilesToStop = GetBusTilesToStop();
            Vehicle v = new Vehicle(Words.Current.Bus, startingTile, endTile, TilesToStop, WantsToShop, 2);
            Vehicles.Add(v);
            if (cbVehicleCreated != null) { cbVehicleCreated(v); }
            timeLastAddedBus = timeCount;
        }
    }
    void AddMoreCars(float deltaTime)
    {
        timeCount += deltaTime;
        if ((timeCount - timeLastAddedVehicle > Numbers.Current.TimeBetweenVehicles))
        {
            bool WantsToShop = false;
            if (UnityEngine.Random.Range(1, 101) < Numbers.Current.PercChanceOfSHopping)
            {
                TimeSpan waitingTime = Today.OpeningTime - WorldTime.Current.Date;
                if (ShopOpen || (!ShopOpen && waitingTime.Minutes < 15 && waitingTime.Hours == 0 && waitingTime.Days == 0))
                {
                    WantsToShop = true;
                }
            }

            Tile startingTile = GetRandomStartingTile();
            Tile endTile;
            if (!WantsToShop)
            {
                endTile = GetRandomEndTile();
            }
            else
            {
                if (startingTile.Y > Current.Height / 2)
                {
                    endTile = World.Current.GetTileAt(3, 15);
                }
                else { endTile = World.Current.GetTileAt(2, 13); }
            }
            

            if (startingTile.IsNeighbour(endTile, true) || startingTile.Vehicle != null)
            {
                return;
            }


            List<Tile> TilesToStop = GetTilesToStop();

            Vehicle v = new Vehicle(Words.Current.Car, startingTile, endTile, TilesToStop, WantsToShop, 1);
            Vehicles.Add(v);
            if (cbVehicleCreated != null) { cbVehicleCreated(v); }
            timeLastAddedVehicle = timeCount;
        }
    }
    void AddMoreShoopers(float deltaTime)
    {
        if (timeCount - timeLastAddedPerson > Numbers.Current.TimeBetweenPedestrians)
        {

            TimeSpan waitingTime = Today.OpeningTime - WorldTime.Current.Date;
            if (ShopOpen || (!ShopOpen && waitingTime.Minutes < 15 && waitingTime.Hours == 0 && waitingTime.Days == 0))
            {
                float Speed = Numbers.Current.GetPedestrianSpeed();
                int rand = UnityEngine.Random.Range(0, PedestrianExitTiles.Count - 1);

                int WantsToShop = UnityEngine.Random.Range(1, 10);
                bool shopping;
                if (WantsToShop <= Numbers.Current.ChanceOfShopping)
                {
                    shopping = true;
                }
                else { shopping = false; }
                Person p = new Person(PedestrianExitTiles[rand], shopping, Speed, false, null);
                if (cbPersonCreated != null) { cbPersonCreated(p); }
                People.Add(p);
                timeLastAddedPerson = timeCount;
            }
        }
    }

    public Tile GetRandomStartingTile()
    {
        int randomStartPosition = UnityEngine.Random.Range(0, VehicleStartingTiles.Count);
        return VehicleStartingTiles[randomStartPosition];

    }
    public Tile GetRandomEndTile()
    {

        int randomEndPosition = UnityEngine.Random.Range(0, VehicleEndingTiles.Count);
        return VehicleEndingTiles[randomEndPosition];
    }

    public void RemoveVehicle(Vehicle v)
    {
        Vehicles.Remove(v);
        if (cbVehicleRemoved != null) { cbVehicleRemoved(v); }
    }
    public void RemovePerson(Person p)
    {
        People.Remove(p);
        if (cbPersonRemoved != null) { cbPersonRemoved(p); }
    }

    public List<Tile> GetTilesToStop()
    {
        List<Tile> TTS = new List<Tile>();
        TTS.Add(GetTileAt(3, 4));
        TTS.Add(GetTileAt(2, 19));
        TTS.Add(GetTileAt(2, 0));
        TTS.Add(GetTileAt(0, 3));
        TTS.Add(GetTileAt(0, 21));
        TTS.Add(GetTileAt(3, 22));
        TTS.Add(GetTileAt(4, 13));



        return TTS;
    }

    List<Tile> GetBusTilesToStop()
    {
        List<Tile> TTS = new List<Tile>();
        TTS.Add(GetTileAt(3, 4));
        TTS.Add(GetTileAt(2, 18));
        TTS.Add(GetTileAt(2, 0));
        TTS.Add(GetTileAt(0, 3));
        TTS.Add(GetTileAt(0, 21));
        TTS.Add(GetTileAt(1, 22));
        TTS.Add(GetTileAt(3, 14));




        return TTS;
    }
    
    void SetParkingExitTiles()
    {
        GetTileAt(7, 12).InteracterTile = GetTileAt(7, 11);
        GetTileAt(8, 12).InteracterTile = GetTileAt(8, 11);
        GetTileAt(7, 15).InteracterTile = GetTileAt(6, 15);
        GetTileAt(8, 15).InteracterTile = GetTileAt(8, 16);
        GetTileAt(3, 16).InteracterTile = GetTileAt(4, 16);

    }
    void SetPedestrianExitTiles()
    {
        PedestrianExitTiles.Add(GetTileAt(21, 19));
        PedestrianExitTiles.Add(GetTileAt(21, 4));
        PedestrianExitTiles.Add(GetTileAt(21, 5));
    }
    void CreateFixturePrototypes()
    {
        FixturePrototypes = new Dictionary<string, Fixture>();
        FixtureJobPrototypes = new Dictionary<string, Job>();
        FixturePrototypes.Add(Words.Current.Lampost, new Fixture(
            Words.Current.Lampost,
            1, //Walking Movement Cost
            0, //Driving Movement Cost
            1, //Width
            2)); //Height

        FixturePrototypes.Add(Words.Current.Phonebox, new Fixture(
            Words.Current.Phonebox,
            0, //Walking Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.ParkTree, new Fixture(
            Words.Current.ParkTree,
            1, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            2)); //Height

        FixturePrototypes.Add(Words.Current.Bin, new Fixture(
            Words.Current.Bin,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.BusStop, new Fixture(
            Words.Current.BusStop,
            1, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            2)); //Height

        FixturePrototypes.Add(Words.Current.StreetTree, new Fixture(
            Words.Current.StreetTree,
            3, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            2)); //Height

        FixturePrototypes.Add(Words.Current.ParkBush, new Fixture(
            Words.Current.ParkBush,
            3, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.Door, new Fixture(
            Words.Current.Door,
            1, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height
        FixturePrototypes[Words.Current.Door].SetParameter("openness", 0);
        FixturePrototypes[Words.Current.Door].SetParameter("is_opening", 0);
        FixturePrototypes[Words.Current.Door].RegisterUpdateAction(FixtureActions.Door_UpdateAction);
        FixturePrototypes[Words.Current.Door].IsEnterable = FixtureActions.Door_IsEnterable;


        FixturePrototypes.Add(Words.Current.Postbox, new Fixture(
            Words.Current.Postbox,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.Bench, new Fixture(
            Words.Current.Bench,
            1.5f, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.Checkout, new Fixture(
            Words.Current.Checkout,
            2f, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height
        FixtureJobPrototypes.Add(Words.Current.Checkout,
            new Job(null,
            Words.Current.CheckoutQueue,
            Words.Current.Checkout,
            FixtureActions.JobComplete_FurnitureBuilding,
            0f));

        FixturePrototypes.Add(Words.Current.FridgeDown, new Fixture(
            Words.Current.FridgeDown,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.FridgeCorner, new Fixture(
            Words.Current.FridgeCorner,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.FridgeRight, new Fixture(
            Words.Current.FridgeRight,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.ShelfCorner, new Fixture(
            Words.Current.ShelfCorner,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.ShelfDown, new Fixture(
            Words.Current.ShelfDown,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.ShelfLeft, new Fixture(
            Words.Current.ShelfLeft,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.ShelfUp, new Fixture(
            Words.Current.ShelfUp,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height

        FixturePrototypes.Add(Words.Current.ShelfWall, new Fixture(
            Words.Current.ShelfWall,
            0, //Movement Cost
            0, //Driving Movement Cost
            1, //Width
            1)); //Height
        
    }
    void CreateItemPrototypes()
    {

        ItemPrototypes = new Dictionary<string, Item>();
        ItemPrototypes.Add(Words.Current.Milk, new Item(
            Words.Current.Milk,
            Numbers.Current.MilkSpace,
            Numbers.Current.MilkCost,
            Numbers.Current.MilkOrderCost,
            Numbers.Current.ExpectedMilkCost,
            true));

        ItemPrototypes.Add(Words.Current.Butter, new Item(
            Words.Current.Butter,
            Numbers.Current.ButterSpace,
            Numbers.Current.ButterCost,
            Numbers.Current.ButterOrderCost,
            Numbers.Current.ExpectedButterCost,
            true));

        ItemPrototypes.Add(Words.Current.Yhogurt, new Item(
            Words.Current.Yhogurt,
            Numbers.Current.YhogurtSpace,
            Numbers.Current.YhogurtCost,
            Numbers.Current.YhogurtOrderCost,
            Numbers.Current.ExpectedYhogurtCost,
            true));

        ItemPrototypes.Add(Words.Current.Meat, new Item(
            Words.Current.Meat,
            Numbers.Current.MeatSpace,
            Numbers.Current.MeatCost,
            Numbers.Current.MeatOrderCost,
            Numbers.Current.ExpectedMeatCost,
            true));

        ItemPrototypes.Add(Words.Current.Cheese, new Item(
            Words.Current.Cheese,
            Numbers.Current.CheeseSpace,
            Numbers.Current.CheeseCost,
            Numbers.Current.CheeseOrderCost,
            Numbers.Current.ExpectedCheeseCost,
            true));

        ItemPrototypes.Add(Words.Current.Bread, new Item(
            Words.Current.Bread,
            Numbers.Current.BreadSpace,
            Numbers.Current.BreadCost,
            Numbers.Current.BreadOrderCost,
            Numbers.Current.ExpectedBreadCost,
            false));

        ItemPrototypes.Add(Words.Current.Newspaper, new Item(
            Words.Current.Newspaper,
            Numbers.Current.NewspaperSpace,
            Numbers.Current.NewspaperCost,
            Numbers.Current.NewspaperOrderCost,
            Numbers.Current.ExpectedNewspaperCost,
            false));

        ItemPrototypes.Add(Words.Current.Teabag, new Item(
            Words.Current.Teabag,
            Numbers.Current.TeabagSpace,
            Numbers.Current.TeabagCost,
            Numbers.Current.TeabagOrderCost,
            Numbers.Current.ExpectedTeabagCost,
            false));

        ItemPrototypes.Add(Words.Current.Sugar, new Item(
            Words.Current.Sugar,
            Numbers.Current.SugarSpace,
            Numbers.Current.SugarCost,
            Numbers.Current.SugarOrderCost,
            Numbers.Current.ExpectedSugarCost,
            false));

        ItemPrototypes.Add(Words.Current.Coffee, new Item(
            Words.Current.Coffee,
            Numbers.Current.CoffeeSpace,
            Numbers.Current.CoffeeCost,
            Numbers.Current.CoffeeOrderCost,
            Numbers.Current.ExpectedCoffeeCost,
            false));

        ItemPrototypes.Add(Words.Current.Biscuits, new Item(
            Words.Current.Biscuits,
            Numbers.Current.BiscuitsSpace,
            Numbers.Current.BiscuitsCost,
            Numbers.Current.BiscuitsOrderCost,
            Numbers.Current.ExpectedBiscuitsCost,
            false));

        ItemPrototypes.Add(Words.Current.CookingSauce, new Item(
            Words.Current.CookingSauce,
            Numbers.Current.CookingSauceSpace,
            Numbers.Current.CookingSauceCost,
            Numbers.Current.CookingSaucOrdereCost,
            Numbers.Current.ExpectedCookingSauceCost,
            false));

        ItemPrototypes.Add(Words.Current.Pasta, new Item(
            Words.Current.Pasta,
            Numbers.Current.PastaSpace,
            Numbers.Current.PastaCost,
            Numbers.Current.PastaOrderCost,
            Numbers.Current.ExpectedPastaCost,
            false));

        ItemPrototypes.Add(Words.Current.ChocolateBar, new Item(
            Words.Current.ChocolateBar,
            Numbers.Current.ChocolateBarSpace,
            Numbers.Current.ChocolateBarCost,
            Numbers.Current.ChocolateBarOrderCost,
            Numbers.Current.ExpectedChocolateBarCost,
            false));

        ItemPrototypes.Add(Words.Current.BottledDrink, new Item(
            Words.Current.BottledDrink,
            Numbers.Current.BottledDrinkSpace,
            Numbers.Current.BottledDrinkCost,
            Numbers.Current.BottledDrinkOrderCost,
            Numbers.Current.ExpectedBottledDrinkCost,
            false));

        ItemPrototypes.Add(Words.Current.Crisps, new Item(
            Words.Current.Crisps,
            Numbers.Current.CrispsSpace,
            Numbers.Current.CrispsCost,
            Numbers.Current.CrispsOrderCost,
            Numbers.Current.ExpectedCrispsCost,
            false));

        ItemPrototypes.Add(Words.Current.Fruit, new Item(
            Words.Current.Fruit,
            Numbers.Current.FruitSpace,
            Numbers.Current.FruitCost,
            Numbers.Current.FruitOrderCost,
            Numbers.Current.ExpectedFruitCost,
            false));

        ItemPrototypes.Add(Words.Current.Vegetable, new Item(
            Words.Current.Vegetable,
            Numbers.Current.VegetableSpace,
            Numbers.Current.VegetableCost,
            Numbers.Current.VegetableOrderCost,
            Numbers.Current.ExpectedVegetableCost,
            false));

        ItemPrototypes.Add(Words.Current.Sweets, new Item(
            Words.Current.Sweets,
            Numbers.Current.SweetsSpace,
            Numbers.Current.SweetsCost,
            Numbers.Current.SweetsOrderCost,
            Numbers.Current.ExpectedSweetsCost,
            false));

    }

    public void InvalidateTileGraphs()
    {
        DrivingGraph = null;
        WalkingGraph = null;
    }
    void PutOutVillageFixtures()
    {
        Words w = Words.Current;
        PlaceFixture(w.ParkBush, GetTileAt(21, 14));
        PlaceFixture(w.ParkBush, GetTileAt(21, 13));
        PlaceFixture(w.ParkBush, GetTileAt(21, 12));
        PlaceFixture(w.ParkBush, GetTileAt(21, 11));
        PlaceFixture(w.ParkBush, GetTileAt(21, 10));
        PlaceFixture(w.Bench, GetTileAt(20, 6));
        PlaceFixture(w.Lampost, GetTileAt(19, 19));
        PlaceFixture(w.ParkTree, GetTileAt(19, 18));
        PlaceFixture(w.ParkTree, GetTileAt(19, 17));
        PlaceFixture(w.ParkTree, GetTileAt(19, 16));
        PlaceFixture(w.ParkTree, GetTileAt(19, 15));
        PlaceFixture(w.ParkTree, GetTileAt(19, 8));
        PlaceFixture(w.ParkTree, GetTileAt(19, 7));
        PlaceFixture(w.ParkTree, GetTileAt(19, 6));
        PlaceFixture(w.Lampost, GetTileAt(18, 4));
        PlaceFixture(w.Lampost, GetTileAt(17, 19));
        PlaceFixture(w.ParkTree, GetTileAt(17, 18));
        PlaceFixture(w.ParkTree, GetTileAt(17, 17));
        PlaceFixture(w.ParkTree, GetTileAt(17, 16));
        PlaceFixture(w.ParkTree, GetTileAt(17, 15));
        PlaceFixture(w.ParkTree, GetTileAt(17, 8));
        PlaceFixture(w.Checkout, GetTileAt(10, 11));
        CheckoutTile = GetTileAt(10, 11);

        PlaceFixture(w.ParkTree, GetTileAt(17, 7));
        PlaceFixture(w.ParkTree, GetTileAt(17, 6));
        PlaceFixture(w.Bench, GetTileAt(16, 6));
        PlaceFixture(w.ParkBush, GetTileAt(15, 14));
        PlaceFixture(w.ParkBush, GetTileAt(15, 13));
        PlaceFixture(w.ParkBush, GetTileAt(15, 12));
        PlaceFixture(w.ParkBush, GetTileAt(15, 11));
        PlaceFixture(w.ParkBush, GetTileAt(15, 10));
        PlaceFixture(w.Door, GetTileAt(13, 12));
        PlaceFixture(w.Door, GetTileAt(8, 10));
        PlaceFixture(w.Bin, GetTileAt(12, 5));
        PlaceFixture(w.Door, GetTileAt(11, 6));
        PlaceFixture(w.Lampost, GetTileAt(11, 4));
        PlaceFixture(w.Lampost, GetTileAt(10, 19));
        PlaceFixture(w.Door, GetTileAt(10, 17));
        PlaceFixture(w.Postbox, GetTileAt(10, 6));
        PlaceFixture(w.Phonebox, GetTileAt(7, 18));
        PlaceFixture(w.StreetTree, GetTileAt(5, 10));
        PlaceFixture(w.StreetTree, GetTileAt(5, 9));
        PlaceFixture(w.StreetTree, GetTileAt(5, 8));
        PlaceFixture(w.StreetTree, GetTileAt(5, 7));
        PlaceFixture(w.StreetTree, GetTileAt(5, 6));
        PlaceFixture(w.Lampost, GetTileAt(4, 18));
        PlaceFixture(w.BusStop, GetTileAt(4, 15));
        PlaceFixture(w.Lampost, GetTileAt(4, 4));

        //Shop Shelves
        PlaceFixture(w.FridgeRight, GetTileAt(6, 7));
        PlaceFixture(w.FridgeRight, GetTileAt(6, 8));
        PlaceFixture(w.FridgeRight, GetTileAt(6, 9));
        PlaceFixture(w.FridgeCorner, GetTileAt(6, 10));
        PlaceFixture(w.FridgeDown, GetTileAt(7, 10));

        PlaceFixture(w.ShelfDown, GetTileAt(8, 8));
        PlaceFixture(w.ShelfDown, GetTileAt(9, 8));
        PlaceFixture(w.ShelfDown, GetTileAt(10, 8));
        PlaceFixture(w.ShelfDown, GetTileAt(11, 8));
        PlaceFixture(w.ShelfDown, GetTileAt(12, 8));
        PlaceFixture(w.ShelfDown, GetTileAt(12, 10));

        PlaceFixture(w.ShelfWall, GetTileAt(9, 10));
        PlaceFixture(w.ShelfWall, GetTileAt(12, 12));
        PlaceFixture(w.ShelfWall, GetTileAt(11, 12));
        PlaceFixture(w.ShelfWall, GetTileAt(10, 12));


        PlaceFixture(w.ShelfLeft, GetTileAt(14, 7));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 8));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 9));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 10));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 11));

        //Stockroom Shelves
        PlaceFixture(w.FridgeRight, GetTileAt(9, 13));
        PlaceFixture(w.FridgeRight, GetTileAt(9, 14));
        PlaceFixture(w.FridgeRight, GetTileAt(9, 15));
        PlaceFixture(w.FridgeCorner, GetTileAt(9, 16));
        PlaceFixture(w.FridgeDown, GetTileAt(10, 16));

        PlaceFixture(w.ShelfUp, GetTileAt(10, 13));
        PlaceFixture(w.ShelfUp, GetTileAt(11, 13));
        PlaceFixture(w.ShelfUp, GetTileAt(12, 13));

        PlaceFixture(w.ShelfLeft, GetTileAt(14, 13));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 14));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 15));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 16));
        PlaceFixture(w.ShelfLeft, GetTileAt(14, 17));
        PlaceFixture(w.ShelfCorner, GetTileAt(14, 18));

        PlaceFixture(w.ShelfWall, GetTileAt(13, 18));
        PlaceFixture(w.ShelfWall, GetTileAt(12, 18));
        PlaceFixture(w.ShelfWall, GetTileAt(11, 18));

        PlaceFixture(w.ShelfUp, GetTileAt(12, 16));
        PlaceFixture(w.ShelfDown, GetTileAt(12, 15));


        CustomerDoors.Add(GetTileAt(8, 10).Fixture);
        CustomerDoors.Add(GetTileAt(11, 6).Fixture);
    }

    void StockShelves()
    {
        GetTileAt(7, 10).Fixture.MaxShelfSpace = Numbers.Current.FridgeMaxItems;
        GetTileAt(7, 10).Fixture.Fridge = true;
        while (GetTileAt(7, 10).Fixture.MaxShelfSpace > GetTileAt(7, 10).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(7, 10).Fixture.PlaceItem(ItemPrototypes[Words.Current.Milk]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Milk], GetTileAt(7, 10).Fixture);

        GetTileAt(6, 10).Fixture.MaxShelfSpace = Numbers.Current.FridgeMaxItems;
        GetTileAt(6, 10).Fixture.Fridge = true;
        while (GetTileAt(6, 10).Fixture.MaxShelfSpace > GetTileAt(6, 10).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(6, 10).Fixture.PlaceItem(ItemPrototypes[Words.Current.Cheese]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Cheese], GetTileAt(6, 10).Fixture);



        GetTileAt(6, 9).Fixture.MaxShelfSpace = Numbers.Current.FridgeMaxItems;
        GetTileAt(6, 9).Fixture.Fridge = true;
        while (GetTileAt(6, 9).Fixture.MaxShelfSpace > GetTileAt(6, 9).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(6, 9).Fixture.PlaceItem(ItemPrototypes[Words.Current.Yhogurt]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Yhogurt], GetTileAt(6, 9).Fixture);
        
        GetTileAt(6, 8).Fixture.MaxShelfSpace = Numbers.Current.FridgeMaxItems;
        GetTileAt(6, 8).Fixture.Fridge = true;
        while (GetTileAt(6, 8).Fixture.MaxShelfSpace > GetTileAt(6, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(6, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Butter]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Butter], GetTileAt(6, 8).Fixture);
        
        GetTileAt(6, 7).Fixture.MaxShelfSpace = Numbers.Current.FridgeMaxItems;
        GetTileAt(6, 7).Fixture.Fridge = true;
        while (GetTileAt(6, 7).Fixture.MaxShelfSpace > GetTileAt(6, 7).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(6, 7).Fixture.PlaceItem(ItemPrototypes[Words.Current.Meat]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Meat], GetTileAt(6, 7).Fixture);



        GetTileAt(12, 8).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(12, 8).Fixture.MaxShelfSpace > GetTileAt(12, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Bread]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Bread], GetTileAt(12, 8).Fixture);

        GetTileAt(11, 8).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(11, 8).Fixture.MaxShelfSpace > GetTileAt(11, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(11, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Teabag]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Teabag], GetTileAt(11, 8).Fixture);
        
        GetTileAt(10, 8).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(10, 8).Fixture.MaxShelfSpace > GetTileAt(10, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(10, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Sugar]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Sugar], GetTileAt(10, 8).Fixture);
        
        GetTileAt(9, 8).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(9, 8).Fixture.MaxShelfSpace > GetTileAt(9, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(9, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Coffee]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Coffee], GetTileAt(9, 8).Fixture);

        GetTileAt(8, 8).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(8, 8).Fixture.MaxShelfSpace > GetTileAt(8, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(8, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Biscuits]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Biscuits], GetTileAt(8, 8).Fixture);



        GetTileAt(9, 10).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(9, 10).Fixture.MaxShelfSpace > GetTileAt(9, 10).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(9, 10).Fixture.PlaceItem(ItemPrototypes[Words.Current.Sweets]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Sweets], GetTileAt(9, 10).Fixture);


        
        GetTileAt(11, 12).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(11, 12).Fixture.MaxShelfSpace > GetTileAt(11, 12).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(11, 12).Fixture.PlaceItem(ItemPrototypes[Words.Current.ChocolateBar]);

        }
        AddToItemList(ItemPrototypes[Words.Current.ChocolateBar], GetTileAt(11, 12).Fixture);
        
        GetTileAt(12, 12).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(12, 12).Fixture.MaxShelfSpace > GetTileAt(12, 12).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 12).Fixture.PlaceItem(ItemPrototypes[Words.Current.Crisps]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Crisps], GetTileAt(12, 12).Fixture);
        
        GetTileAt(14, 7).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(14, 7).Fixture.MaxShelfSpace > GetTileAt(14, 7).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 7).Fixture.PlaceItem(ItemPrototypes[Words.Current.Fruit]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Fruit], GetTileAt(14, 7).Fixture);
        
        GetTileAt(14, 8).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(14, 8).Fixture.MaxShelfSpace > GetTileAt(14, 8).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 8).Fixture.PlaceItem(ItemPrototypes[Words.Current.Vegetable]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Vegetable], GetTileAt(14, 8).Fixture);
        
        GetTileAt(14, 9).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(14, 9).Fixture.MaxShelfSpace > GetTileAt(14, 9).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 9).Fixture.PlaceItem(ItemPrototypes[Words.Current.BottledDrink]);

        }
        AddToItemList(ItemPrototypes[Words.Current.BottledDrink], GetTileAt(14, 9).Fixture);
        
        GetTileAt(14, 10).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(14, 10).Fixture.MaxShelfSpace > GetTileAt(14, 10).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 10).Fixture.PlaceItem(ItemPrototypes[Words.Current.Newspaper]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Newspaper], GetTileAt(14, 10).Fixture);

        GetTileAt(14, 11).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(14, 11).Fixture.MaxShelfSpace > GetTileAt(14, 11).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 11).Fixture.PlaceItem(ItemPrototypes[Words.Current.CookingSauce]);

        }
        AddToItemList(ItemPrototypes[Words.Current.CookingSauce], GetTileAt(14, 11).Fixture);

        GetTileAt(12, 10).Fixture.MaxShelfSpace = Numbers.Current.ShelfMaxItems;
        while (GetTileAt(12, 10).Fixture.MaxShelfSpace > GetTileAt(12, 10).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 10).Fixture.PlaceItem(ItemPrototypes[Words.Current.Pasta]);

        }
        AddToItemList(ItemPrototypes[Words.Current.Pasta], GetTileAt(12, 10).Fixture);

    }
    void AddToItemList(Item i, Fixture f)
    {
        if (f != null)
        {
            i.ShopShelf = f;
        }
        ItemsOnShelves.Add(i);
    }
    void FillStockRoom()
    {

        GetTileAt(9, 13).Fixture.MaxShelfSpace = Numbers.Current.StockFridgeMaxItems;
        GetTileAt(9, 13).Fixture.Fridge = true;
        while (GetTileAt(9, 13).Fixture.MaxShelfSpace > GetTileAt(9, 13).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(9, 13).Fixture.PlaceItem(ItemPrototypes[Words.Current.Milk]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Milk], GetTileAt(9, 13).Fixture);

        GetTileAt(9, 14).Fixture.MaxShelfSpace = Numbers.Current.StockFridgeMaxItems;
        GetTileAt(9, 14).Fixture.Fridge = true;
        while (GetTileAt(9, 14).Fixture.MaxShelfSpace > GetTileAt(9, 14).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(9, 14).Fixture.PlaceItem(ItemPrototypes[Words.Current.Cheese]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Cheese], GetTileAt(9, 14).Fixture);



        GetTileAt(9, 15).Fixture.MaxShelfSpace = Numbers.Current.StockFridgeMaxItems;
        GetTileAt(9, 15).Fixture.Fridge = true;
        while (GetTileAt(9, 15).Fixture.MaxShelfSpace > GetTileAt(9, 15).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(9, 15).Fixture.PlaceItem(ItemPrototypes[Words.Current.Yhogurt]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Yhogurt], GetTileAt(9, 15).Fixture);

        GetTileAt(9, 16).Fixture.MaxShelfSpace = Numbers.Current.StockFridgeMaxItems;
        GetTileAt(9, 16).Fixture.Fridge = true;
        while (GetTileAt(9, 16).Fixture.MaxShelfSpace > GetTileAt(9, 16).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(9, 16).Fixture.PlaceItem(ItemPrototypes[Words.Current.Butter]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Butter], GetTileAt(9, 16).Fixture);

        GetTileAt(10, 16).Fixture.MaxShelfSpace = Numbers.Current.StockFridgeMaxItems;
        GetTileAt(10, 16).Fixture.Fridge = true;
        while (GetTileAt(10, 16).Fixture.MaxShelfSpace > GetTileAt(10, 16).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(10, 16).Fixture.PlaceItem(ItemPrototypes[Words.Current.Meat]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Meat], GetTileAt(10, 16).Fixture);



        GetTileAt(10, 13).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(10, 13).Fixture.MaxShelfSpace > GetTileAt(10, 13).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(10, 13).Fixture.PlaceItem(ItemPrototypes[Words.Current.Bread]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Bread], GetTileAt(10, 13).Fixture);

        GetTileAt(11, 13).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(11, 13).Fixture.MaxShelfSpace > GetTileAt(11, 13).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(11, 13).Fixture.PlaceItem(ItemPrototypes[Words.Current.Teabag]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Teabag], GetTileAt(11, 13).Fixture);

        GetTileAt(12, 13).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(12, 13).Fixture.MaxShelfSpace > GetTileAt(12, 13).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 13).Fixture.PlaceItem(ItemPrototypes[Words.Current.Sugar]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Sugar], GetTileAt(12, 13).Fixture);

        GetTileAt(14, 13).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(14, 13).Fixture.MaxShelfSpace > GetTileAt(14, 13).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 13).Fixture.PlaceItem(ItemPrototypes[Words.Current.Coffee]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Coffee], GetTileAt(14, 13).Fixture);

        GetTileAt(14, 14).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(14, 14).Fixture.MaxShelfSpace > GetTileAt(14, 14).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 14).Fixture.PlaceItem(ItemPrototypes[Words.Current.Biscuits]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Biscuits], GetTileAt(14, 14).Fixture);



        GetTileAt(14, 15).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(14, 15).Fixture.MaxShelfSpace > GetTileAt(14, 15).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 15).Fixture.PlaceItem(ItemPrototypes[Words.Current.Sweets]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Sweets], GetTileAt(14, 15).Fixture);



        GetTileAt(14, 16).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(14, 16).Fixture.MaxShelfSpace > GetTileAt(14, 16).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 16).Fixture.PlaceItem(ItemPrototypes[Words.Current.ChocolateBar]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.ChocolateBar], GetTileAt(14, 16).Fixture);

        GetTileAt(14, 17).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(14, 17).Fixture.MaxShelfSpace > GetTileAt(14, 17).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 17).Fixture.PlaceItem(ItemPrototypes[Words.Current.Crisps]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Crisps], GetTileAt(14, 17).Fixture);

        GetTileAt(14, 18).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(14, 18).Fixture.MaxShelfSpace > GetTileAt(14, 18).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(14, 18).Fixture.PlaceItem(ItemPrototypes[Words.Current.Fruit]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Fruit], GetTileAt(14, 18).Fixture);

        GetTileAt(13, 18).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(13, 18).Fixture.MaxShelfSpace > GetTileAt(13, 18).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(13, 18).Fixture.PlaceItem(ItemPrototypes[Words.Current.Vegetable]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Vegetable], GetTileAt(13, 18).Fixture);

        GetTileAt(12, 18).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(12, 18).Fixture.MaxShelfSpace > GetTileAt(12, 18).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 18).Fixture.PlaceItem(ItemPrototypes[Words.Current.BottledDrink]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.BottledDrink], GetTileAt(12, 18).Fixture);

        GetTileAt(11, 18).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(11, 18).Fixture.MaxShelfSpace > GetTileAt(11, 18).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(11, 18).Fixture.PlaceItem(ItemPrototypes[Words.Current.Newspaper]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Newspaper], GetTileAt(11, 18).Fixture);

        GetTileAt(12, 15).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(12, 15).Fixture.MaxShelfSpace > GetTileAt(12, 15).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 15).Fixture.PlaceItem(ItemPrototypes[Words.Current.CookingSauce]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.CookingSauce], GetTileAt(12, 15).Fixture);

        GetTileAt(12, 16).Fixture.MaxShelfSpace = Numbers.Current.StockShelfMaxItems;
        while (GetTileAt(12, 16).Fixture.MaxShelfSpace > GetTileAt(12, 16).Fixture.GetItemsOnShelf().Count)
        {
            GetTileAt(12, 16).Fixture.PlaceItem(ItemPrototypes[Words.Current.Pasta]);

        }
        AddToStockRoomList(ItemPrototypes[Words.Current.Pasta], GetTileAt(12, 16).Fixture);

    }
    void AddToStockRoomList(Item i, Fixture f)
    {
        if (f!= null)
        {
            i.StockShelf = f;
        }
        ItemsInStockRoom.Add(i);
    }

    void SetShopTiles()
    {
        for (int x = 7; x < 14; x++)
        {
            for (int y = 7; y < 10; y++)
            {
                ShopTiles.Add(GetTileAt(x, y));
            }
        }
        for (int x = 10; x < 14; x++)
        {
            for (int y = 10; y < 12; y++)
            {
                ShopTiles.Add(GetTileAt(x, y));
            }
        }

        ShopTiles.Remove(GetTileAt(8, 8));
        ShopTiles.Remove(GetTileAt(9, 8));
        ShopTiles.Remove(GetTileAt(10, 8));
        ShopTiles.Remove(GetTileAt(11, 8));
        ShopTiles.Remove(GetTileAt(12, 8));
        ShopTiles.Remove(GetTileAt(12, 10));
        ShopTiles.Remove(GetTileAt(10, 11));
        
    }
    void SetStockRoomTiles()
    {
        StockRoomTiles.Add(GetTileAt(10, 14));
        StockRoomTiles.Add(GetTileAt(11, 14));
        StockRoomTiles.Add(GetTileAt(12, 14));
        StockRoomTiles.Add(GetTileAt(13, 14));

        StockRoomTiles.Add(GetTileAt(10, 15));
        StockRoomTiles.Add(GetTileAt(11, 15));
        StockRoomTiles.Add(GetTileAt(13, 15));

        StockRoomTiles.Add(GetTileAt(11, 16));
        StockRoomTiles.Add(GetTileAt(13, 16));

        StockRoomTiles.Add(GetTileAt(11, 17));
        StockRoomTiles.Add(GetTileAt(12, 17));
        StockRoomTiles.Add(GetTileAt(13, 17));
    }
    void SetEmployerTiles()
    {
        GetTileAt(7, 10).Fixture.EmployeeTile = GetTileAt(7, 9);
        GetTileAt(7, 10).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(7, 10).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(6, 10).Fixture.EmployeeTile = GetTileAt(7, 9);
        GetTileAt(6, 10).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(6, 10).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(6, 10).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(6, 9).Fixture.EmployeeTile = GetTileAt(7, 9);
        GetTileAt(6, 9).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(6, 9).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(6, 8).Fixture.EmployeeTile = GetTileAt(7, 8);
        GetTileAt(6, 8).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(6, 8).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(6, 7).Fixture.EmployeeTile = GetTileAt(7, 7);
        GetTileAt(6, 7).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(6, 7).Fixture.EmployeeDirection = Direction.Left;


        GetTileAt(8, 8).Fixture.EmployeeTile = GetTileAt(8, 7);
        GetTileAt(8, 8).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(8, 8).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(9, 8).Fixture.EmployeeTile = GetTileAt(9, 7);
        GetTileAt(9, 8).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(9, 8).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(10, 8).Fixture.EmployeeTile = GetTileAt(10, 7);
        GetTileAt(10, 8).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(10, 8).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(11, 8).Fixture.EmployeeTile = GetTileAt(11, 7);
        GetTileAt(11, 8).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(11, 8).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(12, 8).Fixture.EmployeeTile = GetTileAt(12, 7);
        GetTileAt(12, 8).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(12, 8).Fixture.EmployeeDirection = Direction.Up;


        GetTileAt(9, 10).Fixture.EmployeeTile = GetTileAt(9, 9);
        GetTileAt(9, 10).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(9, 10).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(11, 12).Fixture.EmployeeTile = GetTileAt(11, 11);
        GetTileAt(11, 12).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(11, 12).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(12, 12).Fixture.EmployeeTile = GetTileAt(12, 11);
        GetTileAt(12, 12).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(12, 12).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(12, 10).Fixture.EmployeeTile = GetTileAt(12, 9);
        GetTileAt(12, 10).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(12, 10).Fixture.EmployeeDirection = Direction.Up;


        GetTileAt(14, 11).Fixture.EmployeeTile = GetTileAt(13, 11);
        GetTileAt(14, 11).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 11).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 10).Fixture.EmployeeTile = GetTileAt(13, 10);
        GetTileAt(14, 10).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 10).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 9).Fixture.EmployeeTile = GetTileAt(13, 9);
        GetTileAt(14, 9).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 9).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 8).Fixture.EmployeeTile = GetTileAt(13, 8);
        GetTileAt(14, 8).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 8).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 7).Fixture.EmployeeTile = GetTileAt(13, 7);
        GetTileAt(14, 7).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 7).Fixture.EmployeeDirection = Direction.Right;
        //Checkout
        GetTileAt(10, 11).Fixture.EmployeeTile = GetTileAt(10, 11);
        GetTileAt(10, 11).Fixture.EntranceTile = GetTileAt(11, 11);
        GetTileAt(10, 11).Fixture.EmployeeYModifier = 0.2f;
        GetTileAt(10, 11).Fixture.EmployeeDirection = Direction.Down;


        GetTileAt(12, 13).Fixture.EmployeeTile = GetTileAt(13, 13);
        GetTileAt(12, 13).Fixture.EmployeeXModifier = -1f;
        GetTileAt(12, 13).Fixture.EmployeeYModifier = -0.25f;
        GetTileAt(12, 13).Fixture.EmployeeDirection = Direction.Down;

        GetTileAt(11, 13).Fixture.EmployeeTile = GetTileAt(11, 14);
        GetTileAt(11, 13).Fixture.EmployeeYModifier = -1.25f;
        GetTileAt(11, 13).Fixture.EmployeeDirection = Direction.Down;

        GetTileAt(10, 13).Fixture.EmployeeTile = GetTileAt(11, 14);
        GetTileAt(10, 13).Fixture.EmployeeXModifier = -1f;
        GetTileAt(10, 13).Fixture.EmployeeYModifier = -1.25f;
        GetTileAt(10, 13).Fixture.EmployeeDirection = Direction.Down;


        GetTileAt(9, 13).Fixture.EmployeeTile = GetTileAt(10, 14);
        GetTileAt(9, 13).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(9, 13).Fixture.EmployeeYModifier = -1f;
        GetTileAt(9, 13).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(9, 14).Fixture.EmployeeTile = GetTileAt(10, 14);
        GetTileAt(9, 14).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(9, 14).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(9, 15).Fixture.EmployeeTile = GetTileAt(10, 15);
        GetTileAt(9, 15).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(9, 15).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(9, 16).Fixture.EmployeeTile = GetTileAt(10, 15);
        GetTileAt(9, 16).Fixture.EmployeeXModifier = -0.25f;
        GetTileAt(9, 16).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(9, 16).Fixture.EmployeeDirection = Direction.Left;

        GetTileAt(10, 16).Fixture.EmployeeTile = GetTileAt(10, 15);
        GetTileAt(10, 16).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(10, 16).Fixture.EmployeeDirection = Direction.Up;


        GetTileAt(14, 13).Fixture.EmployeeTile = GetTileAt(13, 13);
        GetTileAt(14, 13).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 13).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 14).Fixture.EmployeeTile = GetTileAt(13, 14);
        GetTileAt(14, 14).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 14).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 15).Fixture.EmployeeTile = GetTileAt(13, 15);
        GetTileAt(14, 15).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 15).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 16).Fixture.EmployeeTile = GetTileAt(13, 16);
        GetTileAt(14, 16).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 16).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 17).Fixture.EmployeeTile = GetTileAt(13, 17);
        GetTileAt(14, 17).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 17).Fixture.EmployeeDirection = Direction.Right;

        GetTileAt(14, 18).Fixture.EmployeeTile = GetTileAt(13, 17);
        GetTileAt(14, 18).Fixture.EmployeeXModifier = 0.25f;
        GetTileAt(14, 18).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(14, 18).Fixture.EmployeeDirection = Direction.Right;


        GetTileAt(13, 18).Fixture.EmployeeTile = GetTileAt(13, 17);
        GetTileAt(13, 18).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(13, 18).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(12, 18).Fixture.EmployeeTile = GetTileAt(12, 17);
        GetTileAt(12, 18).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(12, 18).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(11, 18).Fixture.EmployeeTile = GetTileAt(11, 17);
        GetTileAt(11, 18).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(11, 18).Fixture.EmployeeDirection = Direction.Up;


        GetTileAt(12, 15).Fixture.EmployeeTile = GetTileAt(12, 14);
        GetTileAt(12, 15).Fixture.EmployeeYModifier = 0.25f;
        GetTileAt(12, 15).Fixture.EmployeeDirection = Direction.Up;

        GetTileAt(12, 16).Fixture.EmployeeTile = GetTileAt(12, 17);
        GetTileAt(12, 16).Fixture.EmployeeYModifier = -1.3f;
        GetTileAt(12, 16).Fixture.EmployeeDirection = Direction.Down;

    }
    void SetInteractorTiles()
    {
        GetTileAt(7, 10).Fixture.CustomerTile = GetTileAt(7, 9);
        GetTileAt(7, 10).Fixture.InteractYModifier = 0.25f;
        GetTileAt(7, 10).Fixture.InteractDirection = Direction.Up;

        GetTileAt(6, 10).Fixture.CustomerTile = GetTileAt(7, 9);
        GetTileAt(6, 10).Fixture.InteractXModifier = -0.25f;
        GetTileAt(6, 10).Fixture.InteractYModifier = 0.25f;
        GetTileAt(6, 10).Fixture.InteractDirection = Direction.Left;

        GetTileAt(6, 9).Fixture.CustomerTile = GetTileAt(7, 9);
        GetTileAt(6, 9).Fixture.InteractXModifier = -0.25f;
        GetTileAt(6, 9).Fixture.InteractDirection = Direction.Left;

        GetTileAt(6, 8).Fixture.CustomerTile = GetTileAt(7, 8);
        GetTileAt(6, 8).Fixture.InteractXModifier = -0.25f;
        GetTileAt(6, 8).Fixture.InteractDirection = Direction.Left;

        GetTileAt(6, 7).Fixture.CustomerTile = GetTileAt(7, 7);
        GetTileAt(6, 7).Fixture.InteractXModifier = -0.25f;
        GetTileAt(6, 7).Fixture.InteractDirection = Direction.Left;


        GetTileAt(8, 8).Fixture.CustomerTile = GetTileAt(8, 7);
        GetTileAt(8, 8).Fixture.InteractYModifier = 0.25f;
        GetTileAt(8, 8).Fixture.InteractDirection = Direction.Up;

        GetTileAt(9, 8).Fixture.CustomerTile = GetTileAt(9, 7);
        GetTileAt(9, 8).Fixture.InteractYModifier = 0.25f;
        GetTileAt(9, 8).Fixture.InteractDirection = Direction.Up;

        GetTileAt(10, 8).Fixture.CustomerTile = GetTileAt(10, 7);
        GetTileAt(10, 8).Fixture.InteractYModifier = 0.25f;
        GetTileAt(10, 8).Fixture.InteractDirection = Direction.Up;

        GetTileAt(11, 8).Fixture.CustomerTile = GetTileAt(11, 7);
        GetTileAt(11, 8).Fixture.InteractYModifier = 0.25f;
        GetTileAt(11, 8).Fixture.InteractDirection = Direction.Up;

        GetTileAt(12, 8).Fixture.CustomerTile = GetTileAt(12, 7);
        GetTileAt(12, 8).Fixture.InteractYModifier = 0.25f;
        GetTileAt(12, 8).Fixture.InteractDirection = Direction.Up;


        GetTileAt(9, 10).Fixture.CustomerTile = GetTileAt(9, 9);
        GetTileAt(9, 10).Fixture.InteractYModifier = 0.25f;
        GetTileAt(9, 10).Fixture.InteractDirection = Direction.Up;

        GetTileAt(11, 12).Fixture.CustomerTile = GetTileAt(11, 11);
        GetTileAt(11, 12).Fixture.InteractYModifier = 0.25f;
        GetTileAt(11, 12).Fixture.InteractDirection = Direction.Up;

        GetTileAt(12, 12).Fixture.CustomerTile = GetTileAt(12, 11);
        GetTileAt(12, 12).Fixture.InteractYModifier = 0.25f;
        GetTileAt(12, 12).Fixture.InteractDirection = Direction.Up;

        GetTileAt(12, 10).Fixture.CustomerTile = GetTileAt(12, 9);
        GetTileAt(12, 10).Fixture.InteractYModifier = 0.25f;
        GetTileAt(12, 10).Fixture.InteractDirection = Direction.Up;


        GetTileAt(14, 11).Fixture.CustomerTile = GetTileAt(13, 11);
        GetTileAt(14, 11).Fixture.InteractXModifier = 0.25f;
        GetTileAt(14, 11).Fixture.InteractDirection = Direction.Right;

        GetTileAt(14, 10).Fixture.CustomerTile = GetTileAt(13, 10);
        GetTileAt(14, 10).Fixture.InteractXModifier = 0.25f;
        GetTileAt(14, 10).Fixture.InteractDirection = Direction.Right;

        GetTileAt(14, 9).Fixture.CustomerTile = GetTileAt(13, 9);
        GetTileAt(14, 9).Fixture.InteractXModifier = 0.25f;
        GetTileAt(14, 9).Fixture.InteractDirection = Direction.Right;

        GetTileAt(14, 8).Fixture.CustomerTile = GetTileAt(13, 8);
        GetTileAt(14, 8).Fixture.InteractXModifier = 0.25f;
        GetTileAt(14, 8).Fixture.InteractDirection = Direction.Right;

        GetTileAt(14, 7).Fixture.CustomerTile = GetTileAt(13, 7);
        GetTileAt(14, 7).Fixture.InteractXModifier = 0.25f;
        GetTileAt(14, 7).Fixture.InteractDirection = Direction.Right;

        //Checkout
        GetTileAt(10, 11).Fixture.CustomerTile = GetTileAt(10, 10);
        GetTileAt(10, 11).Fixture.InteractYModifier = 0.25f;
        GetTileAt(10, 11).Fixture.InteractDirection = Direction.Up;


    }

    void CreateTileTypeMap()
    {
        TileType E = TileType.Empty;
        TileType R = TileType.Road;
        TileType P = TileType.Pavement;
        TileType W = TileType.Wall;
        TileType G = TileType.Grass;
        TileType F = TileType.Floor;
        TileType C = TileType.Crossing;
        TileType H = TileType.Water;
        TileType K = TileType.Parking;


        TypeMap = new TileType[576]
       {
           E, E, R, R, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,  
           E, E, R, R, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
           R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R,
           R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R,
           E, E, R, R, P, P, P, P, P, C, P, P, P, P, P, P, P, P, P, P, P, P, E, E,
           E, E, R, R, P, P, P, P, W, C, W, W, W, W, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, P, P, W, C, F, F, F, F, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, P, P, P, W, W, F, F, F, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, P, K, K, W, F, F, F, F, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, R, C, R, R, R, W, F, F, F, F, W, G, P, P, P, P, P, G, E, E,
           E, E, R, R, R, C, R, R, R, W, F, F, F, F, W, G, P, H, H, H, P, G, E, E,
           E, E, R, R, P, P, P, K, K, W, W, W, W, F, W, G, P, H, H, H, P, G, E, E,
           E, E, R, R, P, P, P, P, P, W, F, F, F, F, W, G, P, H, H, H, P, G, E, E,
           E, E, R, R, P, P, W, W, F, W, F, F, F, F, W, G, P, P, P, P, P, G, E, E,
           E, E, R, R, P, P, W, F, F, F, F, F, F, F, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, W, F, F, F, F, F, F, F, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, W, F, F, F, F, F, F, F, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, W, W, W, W, W, F, W, W, W, G, G, G, P, G, G, G, E, E,
           E, E, R, R, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, E, E,
           E, E, R, R, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, P, E, E,
           R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R,
           R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R,
           E, E, R, R, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
           E, E, R, R, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
       };
    }
    void HardcodeVillageTileTypes()
    {
        CreateTileTypeMap();
        int i = 0;
        for (int y = World.Current.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < World.Current.Width; x++)
            {
                GetTileAt(x, y).Type = TypeMap[i];
                if (TypeMap[i] == TileType.Road)
                {
                    RoadTiles.Add(GetTileAt(x, y));
                }
                i++;
            }
        }
        

        for (int x = 0; x < 23; x++)
        {
            int y = 2;
            GetTileAt(x, y).AllowedDirections.Add(Direction.Left);

            y = 3;
            GetTileAt(x, y).AllowedDirections.Add(Direction.Right);

            y = 10;
            GetTileAt(x, y).AllowedDirections.Add(Direction.Left);

            y = 21;
            GetTileAt(x, y).AllowedDirections.Add(Direction.Right);
        }

        for (int y = 0; y < 23; y++)
        {
            int x = 2;
            GetTileAt(x, y).AllowedDirections.Add(Direction.Up);

            x = 3;
            GetTileAt(x, y).AllowedDirections.Add(Direction.Down);
        }

        GetTileAt(2, 2).AllowedDirections.Add(Direction.Left);
        GetTileAt(2, 2).AllowedDirections.Add(Direction.Up);
        GetTileAt(3, 2).AllowedDirections.Add(Direction.Left);
        GetTileAt(3, 2).AllowedDirections.Add(Direction.Down);
        GetTileAt(2, 3).AllowedDirections.Add(Direction.Up);
        GetTileAt(2, 3).AllowedDirections.Add(Direction.Right);
        GetTileAt(3, 3).AllowedDirections.Add(Direction.Left);
        GetTileAt(3, 3).AllowedDirections.Add(Direction.Down);
        GetTileAt(3, 3).AllowedDirections.Add(Direction.Right);

        GetTileAt(2, 20).AllowedDirections.Add(Direction.Up);
        GetTileAt(2, 20).AllowedDirections.Add(Direction.Left);
        GetTileAt(3, 20).AllowedDirections.Add(Direction.Left);
        GetTileAt(3, 20).AllowedDirections.Add(Direction.Down);
        GetTileAt(2, 21).AllowedDirections.Add(Direction.Up);
        GetTileAt(2, 21).AllowedDirections.Add(Direction.Right);
        GetTileAt(3, 21).AllowedDirections.Add(Direction.Down);
        GetTileAt(3, 21).AllowedDirections.Add(Direction.Right);
        GetTileAt(3, 21).AllowedDirections.Add(Direction.Left);

        GetTileAt(7, 15).AllowedDirections.Add(Direction.Up);
        GetTileAt(8, 15).AllowedDirections.Add(Direction.Up);
        GetTileAt(7, 12).AllowedDirections.Add(Direction.Down);
        GetTileAt(8, 12).AllowedDirections.Add(Direction.Down);
        GetTileAt(3, 14).AllowedDirections.Add(Direction.Right);
        GetTileAt(4, 14).AllowedDirections.Add(Direction.Right);
        GetTileAt(5, 14).AllowedDirections.Add(Direction.Right);
        GetTileAt(6, 14).AllowedDirections.Add(Direction.Right);
        GetTileAt(7, 14).AllowedDirections.Add(Direction.Right);
        GetTileAt(8, 14).AllowedDirections.Add(Direction.Right);
        GetTileAt(7, 14).AllowedDirections.Add(Direction.Down);
        GetTileAt(8, 14).AllowedDirections.Add(Direction.Down);

        GetTileAt(2, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(3, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(4, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(5, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(6, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(7, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(8, 13).AllowedDirections.Add(Direction.Left);
        GetTileAt(7, 13).AllowedDirections.Add(Direction.Down);
        GetTileAt(8, 13).AllowedDirections.Add(Direction.Down);
        GetTileAt(7, 13).AllowedDirections.Add(Direction.Up);
        GetTileAt(8, 13).AllowedDirections.Add(Direction.Up);


        Numbers.Current.AddTileWaitTimes(GetTileAt(3, 4), Numbers.Current.StopLineWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(2, 19), Numbers.Current.StopLineWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(3, 5), Numbers.Current.StopLineWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(2, 18), Numbers.Current.StopLineWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(2, 1), Numbers.Current.StopLineWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(1, 3), Numbers.Current.StartTileWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(1, 21), Numbers.Current.StartTileWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(3, 22), Numbers.Current.StopLineWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(3, 16), Numbers.Current.BusStopWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(4, 14), Numbers.Current.ZebraCrossingWaitTime);
        Numbers.Current.AddTileWaitTimes(GetTileAt(4, 13), Numbers.Current.ZebraCrossingWaitTime);

    }
    void SetVillageVehicleStartingTiles()
    {
        VehicleStartingTiles.Add(World.Current.GetTileAt(23, 2));
        VehicleStartingTiles.Add(World.Current.GetTileAt(0, 3));
        VehicleStartingTiles.Add(World.Current.GetTileAt(0, 21));
        VehicleStartingTiles.Add(World.Current.GetTileAt(23, 20));

        VehicleEndingTiles.Add(World.Current.GetTileAt(23, 3));
        VehicleEndingTiles.Add(World.Current.GetTileAt(0, 2));
        VehicleEndingTiles.Add(World.Current.GetTileAt(0, 20));
        VehicleEndingTiles.Add(World.Current.GetTileAt(23, 21));



    }
    void SortWhereVehiclesComeFrom()
    {
        foreach(Tile t in RoadTiles)
        { 
            if (t.AllowedDirections.Count > 0)
            {
                foreach (Direction d in t.AllowedDirections)
                {
                    if (d == Direction.Right)
                    {
                        if (GetTileAt(t.X + 1, t.Y) != null)
                        {
                            GetTileAt(t.X + 1, t.Y).VehiclesComeFrom.Add(t);
                        }
                    }
                    if (d == Direction.Left)
                    {
                        if (GetTileAt(t.X - 1, t.Y) != null)
                        {
                            GetTileAt(t.X - 1, t.Y).VehiclesComeFrom.Add(t);
                        }
                    }
                    if (d == Direction.Up)
                    {
                        if (GetTileAt(t.X, t.Y + 1) != null)
                        {
                            GetTileAt(t.X, t.Y + 1).VehiclesComeFrom.Add(t);
                        }
                    }
                    if (d == Direction.Down)
                    {
                        if (GetTileAt(t.X, t.Y - 1) != null)
                        {
                            GetTileAt(t.X, t.Y - 1).VehiclesComeFrom.Add(t);
                        }
                    }
                }
            }
        }
    }

    public void AddApplicants()
    {
        for (int i = Applicants.Count; i < 5; i++)
        {
            Applicants.Add(new Employee(
                Numbers.Current.GetPedestrianSpeed(),
                Words.Current.GetFullPriorityList()));
        }
    }

    void AddCrossingTiles()
    {
        CrossingTiles.Add(GetTileAt(5, 15));
        CrossingTiles.Add(GetTileAt(5, 14));
        CrossingTiles.Add(GetTileAt(5, 13));
        CrossingTiles.Add(GetTileAt(5, 12));
    }
    void SetTileType(int x, int y, TileType type)
    {
        World.Current.GetTileAt(x, y).Type = type;
    }
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return WorldController.Instance.World.GetTileAt(x, y);
    }
    public bool IsFixturePlacementValid(string furnitureType, Tile t)
    {
        return FixturePrototypes[furnitureType].IsValidPosition(t);
    }

    public Item GetItemFromShopShelf(string name)
    {
        foreach (Item i in ItemsOnShelves)
        {
            if (i.Name == name);
            return i;
        }
        return null;
    }
    public Item GetItemFromStockShelf(string name)
    {
        foreach (Item i in ItemsInStockRoom)
        {
            if (i.Name == name) ;
            return i;
        }
        return null;

    }
    public Employee GetEmployeeWithID(int ID)
    {
        foreach (Employee e in Employees)
        {
            if (ID == e.EmployeeID)
            {
                return e;
            }
        }
        return null;
    }


    public void AddVehicleCreatedCallback(Action<Vehicle> callback) { cbVehicleCreated += callback; }
    public void RemoveVehicleCreatedCallback(Action<Vehicle> callback) { cbVehicleCreated -= callback; }
    public void AddVehicleRemovedCallback(Action<Vehicle> callback) { cbVehicleRemoved += callback; }
    public void RemoveVehicleRemovedCallback(Action<Vehicle> callback) { cbVehicleRemoved -= callback; }
    public void AddPersonCreatedCallback(Action<Person> callback) { cbPersonCreated += callback; }
    public void RemovePersonCreatedCallback(Action<Person> callback) { cbPersonCreated -= callback; }
    public void AddPersonRemovedCallback(Action<Person> callback) { cbPersonRemoved += callback; }
    public void RemovePersonRemovedCallback(Action<Person> callback) { cbPersonRemoved -= callback; }
    public void AddFixtureCreatedCallback(Action<Fixture> callback) { cbFixtureCreated += callback; }
    public void RemoveFixtureCreatedCallback(Action<Fixture> callback) { cbFixtureCreated -= callback; }
    public void AddEmployeeCreatedCallback(Action<Employee> callback) { cbEmployeeCreated += callback; }
    public void AddEmployeeRemovedCallback(Action<Employee> callback ) { cbEmployeeRemoved += callback; }
    public void AddBalanceUpdatingCallback(Action<double> callback) { cbBalanceUpdating += callback; }
    public void AddEODCallback(Action callback) { cbEndOfDay += callback; }
    public void RemoveEODCallback(Action callback) { cbEndOfDay -= callback; }











    //////////////////////////////////////////////////////////////////////////////////////
    /////               
    /////                           SAVING & LOADING
    /////       
    ///////////////////////////////////////////////////////////////////////////////////////
    
    public void SaveGame()
    {
        foreach (Employee e in Employees)
        {
            EmployeeSaveFile save = new EmployeeSaveFile(e);
        }
        foreach (Item i in ItemsOnShelves)
        {
            ItemSaveFile save = new ItemSaveFile(i, "ItemOnShelf");
        }
        foreach (Item i in ItemsInStockRoom)
        {
            ItemSaveFile save = new ItemSaveFile(i, "ItemInStock");
        }

        WorldSaveFile saveWorld = new WorldSaveFile(this);

        foreach (Fixture f in Fixtures)
        {
            if (f.NeedsRestock || f.ItemsOnShelf.Count > 0)
            {
                //Only want to save it if the above are true, otherwise the prototype load is fine
                FixtureSaveFile save = new FixtureSaveFile(f);
            }
        }

        Numbers.Current.PersonID = 0;
        foreach (Person p in People)
        {
            PersonSaveFile save = new PersonSaveFile(p);
        }

        WorkingDaySaveFile saveToday = new WorkingDaySaveFile(Today, 0);

        int count = 1;
        foreach (WorkingDay wd in Next30Days)
        {
            WorkingDaySaveFile save = new WorkingDaySaveFile(wd, count);
            count++;
        }
        foreach (WorkingDay wd in Last30Days)
        {
            WorkingDaySaveFile save = new WorkingDaySaveFile(wd, count);
            count++;
        }

        count = 0;
        List<Job> constructionQueue = new List<Job>(ConstructionQueue.constructionQueue.ToList());
        foreach (Job j in constructionQueue)
        {
            JobSaveFile save = new JobSaveFile(j, count);
            count++;
        }
        List<Job> checkoutQueue = new List<Job>(CheckoutQueue.checkoutQueue.ToList());
        foreach (Job j in checkoutQueue)
        {
            JobSaveFile save = new JobSaveFile(j, count);
            count++;
        }
        List<Job> stockQueue = new List<Job>(StockQueue.stockQueue.ToList());
        foreach (Job j in stockQueue)
        {
            JobSaveFile save = new JobSaveFile(j, count);
            count++;
        }
        List<Job> costChangeQueue = new List<Job>(CostChangeQueue.costChangeQueue.ToList());
        foreach (Job j in costChangeQueue)
        {
            JobSaveFile save = new JobSaveFile(j, count);
            count++;
        }
    }
    public void LoadGame()
    {
        int i = 1;
        do
        {
            string json = PlayerPrefs.GetString("Employee" + i);
            Employee e = new Employee(JsonUtility.FromJson<EmployeeSaveFile>(json));
            Employees.Add(e);
            i++;
        } while (PlayerPrefs.HasKey("Employee" + i));

        i = 1;
        do
        {
            string json = PlayerPrefs.GetString("ItemOnShelf" + i);
            Item it = new Item(JsonUtility.FromJson<ItemSaveFile>(json));
            AddToItemList(it, it.ShopShelf);
            i++;
        } while (PlayerPrefs.HasKey("ItemOnShelf" + i));

        i = 1;
        do
        {
            string json = PlayerPrefs.GetString("ItemInStock" + i);
            Item it = new Item(JsonUtility.FromJson<ItemSaveFile>(json));
            AddToStockRoomList(it, it.StockShelf);
            i++;
        } while (PlayerPrefs.HasKey("ItemInStock" + i));

        if (PlayerPrefs.HasKey("World"))
        {
            string json = PlayerPrefs.GetString("World");
            WorldSaveFile savedWorld = JsonUtility.FromJson<WorldSaveFile>(json);
            Balance = savedWorld.Balance;
            DeliveryTime = savedWorld.DeliveryTime;
            if (DeliveryTime > 0)
            {
                ItemToDeliver = GetItemFromStockShelf(savedWorld.ItemToDeliver);
                QuantityToDeliver = savedWorld.QuantityToDeliver;
            }
        }

        foreach (Fixture f in Fixtures)
        {
            if (PlayerPrefs.HasKey("Fixture" + f.Tile.X.ToString() + f.Tile.Y.ToString()))
            {
                string json = PlayerPrefs.GetString("Fixture" + f.Tile.X.ToString() + f.Tile.Y.ToString());
                FixtureSaveFile savedFixture = JsonUtility.FromJson<FixtureSaveFile>(json);
                f.NeedsRestock = savedFixture.NeedsRestock;

                if (savedFixture.ItemCount > 0)
                {
                    for (int a = 0; a < savedFixture.ItemCount; a++)
                    {
                        f.ItemsOnShelf.Add(GetItemFromShopShelf(savedFixture.ItemOnShelf));
                    }
                }
            }
        }

        i = 1;
        do
        {
            string json = PlayerPrefs.GetString("Person" + i);
            Person p = new Person(JsonUtility.FromJson<PersonSaveFile>(json));
            People.Add(p);
            i++;
        } while (PlayerPrefs.HasKey("Person" + i));

        i = 0;

        do
        {
            string json = PlayerPrefs.GetString("WorkingDay" + i);
            if (i == 0)
            {
                WorkingDay day = new WorkingDay(JsonUtility.FromJson<WorkingDaySaveFile>(json), true);
                Today = day;
            }
            else if (i >= 1 || i <= 30)
            {
                WorkingDay day = new WorkingDay(JsonUtility.FromJson<WorkingDaySaveFile>(json), false);
                Next30Days.Add(day);
            }
            else if (i > 30)
            {
                WorkingDay day = new WorkingDay(JsonUtility.FromJson<WorkingDaySaveFile>(json), false);
                Last30Days.Add(day);
            }

            i++;
        } while (PlayerPrefs.HasKey("WorkingDay" + i));

        i = 0;

        do
        {
            string json = PlayerPrefs.GetString("Job" + i);
            Job j = new Job(JsonUtility.FromJson<JobSaveFile>(json));
            if (j.jobQueue == Words.Current.CheckoutQueue)
            {
                CheckoutQueue.Enqueue(j);
            }
            else if (j.jobQueue == Words.Current.ConstructionQueue)
            {
                ConstructionQueue.Enqueue(j);
            }
            else if (j.jobQueue == Words.Current.StockQueue)
            {
                StockQueue.Enqueue(j);
            }
            else if (j.jobQueue == Words.Current.CostChangeQueue)
            {
                CostChangeQueue.Enqueue(j);
            }

            i++;
        } while (PlayerPrefs.HasKey("Job" + i));

    }
    public World()
    {

    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {

        writer.WriteStartElement("ItemsOnShelves");
        foreach (Item i in ItemsOnShelves)
        {
            writer.WriteStartElement("Item");
            i.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("ItemsInStockRoom");
        foreach (Item i in ItemsInStockRoom)
        {
            writer.WriteStartElement("Item");
            i.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("World");
        writer.WriteAttributeString("DeliveryTime", DeliveryTime.ToString());
        writer.WriteAttributeString("Balance", Balance.ToString());
        if (DeliveryTime > 0)
        {
            writer.WriteAttributeString("ItemDeliver", ItemToDeliver.Name);
            writer.WriteAttributeString("QuantityDeliver", QuantityToDeliver.ToString());
        }
        else
        {
            writer.WriteAttributeString("ItemDeliver", "");
            writer.WriteAttributeString("QuantityDeliver", "");
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Fixtures");
        foreach (Fixture f in Fixtures)
        {
            writer.WriteStartElement("Fixture");
            f.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        

        writer.WriteStartElement("Employees");
        foreach (Employee e in Employees)
        {
            writer.WriteStartElement("Employee");
            e.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("People");
        foreach (Person p in People)
        {
            writer.WriteStartElement("Person");
            p.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Today");
        Today.WriteXml(writer);
        writer.WriteEndElement();

        writer.WriteStartElement("Next30Days");
        foreach (WorkingDay w in Next30Days)
        {
            writer.WriteStartElement("WorkingDay");
            w.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Last30Days");
        foreach (WorkingDay w in Last30Days)
        {
            writer.WriteStartElement("WorkingDay");
            w.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("CheckoutJobs");
        List<Job> checkQueue = new List<Job>(CheckoutQueue.checkoutQueue.ToList());
        foreach (Job j in checkQueue)
        {
            writer.WriteStartElement("Job");
            j.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("RestockJobs");
        List<Job> restockQueue = new List<Job>(StockQueue.stockQueue.ToList());
        foreach (Job j in restockQueue)
        {
            writer.WriteStartElement("Job");
            j.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("ConstructionJobs");
        List<Job> constructionQueue = new List<Job>(ConstructionQueue.constructionQueue.ToList());
        foreach (Job j in constructionQueue)
        {
            writer.WriteStartElement("Job");
            j.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("CostChangeJobs");
        List<Job> costChangeQueue = new List<Job>(CostChangeQueue.costChangeQueue.ToList());
        foreach (Job j in costChangeQueue)
        {
            writer.WriteStartElement("Job");
            j.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

    }

    public void ReadXml(XmlReader reader)
    {
        width = int.Parse(reader.GetAttribute("Width"));
        height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(width, height, true);

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "ItemsOnShelves":
                    ReadXml_ItemsShelves(reader);
                    break;

                case "ItemsInStockRoom":
                    ReadXml_ItemsStock(reader);
                    break;

                case "World":
                    ReadXml_World(reader);
                    break;

                case "Fixtures":
                    ReadXml_Fixtures(reader);
                    break;

                case "Employees":
                    ReadXml_Employees(reader);
                    break;

                case "People":
                    ReadXml_People(reader);
                    break;

                case "Today":
                    ReadXml_Today(reader);
                    break;

                case "Next30Days":
                    ReadXml_Next30Days(reader);
                    break;

                case "Last30Days":
                    ReadXml_Last30Days(reader);
                    break;

                case "CheckoutJobs":
                    ReadXml_CheckoutJobs(reader);
                    break;

                case "RestockJobs":
                    ReadXml_RestockJobs(reader);
                    break;

                case "ConstructionJobs":
                    ReadXml_ConstructionJobs(reader);
                    break;

                case "CostChangeJobs":
                    ReadXml_CostChangeJobs(reader);
                    break;
            }

        }
    }

    void ReadXml_World(XmlReader reader)
    {
        if (reader.ReadToDescendant("World"))
        {
            Balance = double.Parse(reader.GetAttribute("Balance"));
            DeliveryTime = int.Parse(reader.GetAttribute("DeliveryTime"));
            if (DeliveryTime > 0)
            {
                ItemToDeliver = GetItemFromStockShelf(reader.GetAttribute("ItemDeliver"));
                QuantityToDeliver = int.Parse(reader.GetAttribute("QuantityDeliver"));
            }
        }

    }
    void ReadXml_ItemsShelves(XmlReader reader)
    {
        if (reader.ReadToDescendant("Item"))
        {
            do
            {
                string name = reader.GetAttribute("Name");

                Item i = ItemPrototypes[name];
                ItemsOnShelves.Add(i);
                i.ReadXml(reader);
            } while (reader.ReadToNextSibling("Item"));
        }
    }
    void ReadXml_ItemsStock(XmlReader reader)
    {
        if (reader.ReadToDescendant("Item"))
        {
            do
            {
                string name = reader.GetAttribute("Name");

                Item i = ItemPrototypes[name];
                ItemsOnShelves.Add(i);
                i.ReadXml(reader);
            } while (reader.ReadToNextSibling("Item"));

        }
    }
    void ReadXml_Fixtures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Fixture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                Fixture f = GetTileAt(x, y).Fixture;
                f.ReadXml(reader);
            } while (reader.ReadToNextSibling("Fixture"));

        }

    }
    void ReadXml_Employees(XmlReader reader)
    {

        if (reader.ReadToDescendant("Employee"))
        {
            do
            {
                float speed = float.Parse(reader.GetAttribute("Speed"));
                List<string> PriList = new List<string>();

                if (reader.ReadToDescendant("Queue"))
                {
                    do
                    {
                        PriList.Add(reader.GetAttribute("Name"));
                    } while (reader.ReadToNextSibling("Queue"));
                }

                Employee e = new Employee(speed, PriList);
                e.ReadXml(reader);
                Employees.Add(e);
            } while (reader.ReadToNextSibling("Employee"));

        }
    }
    void ReadXml_People(XmlReader reader)
    {
        if (reader.ReadToDescendant("Person"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                bool wantshop = false;
                if (reader.GetAttribute("WantsShop") == "True")
                {
                    wantshop = true;
                }
                float speed = float.Parse(reader.GetAttribute("Speed"));


                Person p = new Person(
                    GetTileAt(x, y),
                    wantshop,
                    speed,
                    true);
                p.ReadXml(reader);
                People.Add(p);
            } while (reader.ReadToNextSibling("Person"));

        }

    }
    void ReadXml_Today(XmlReader reader)
    {

    }
    void ReadXml_Next30Days(XmlReader reader)
    {

    }
    void ReadXml_Last30Days(XmlReader reader)
    {

    }
    void ReadXml_CheckoutJobs(XmlReader reader)
    {

    }
    void ReadXml_RestockJobs(XmlReader reader)
    {

    }
    void ReadXml_ConstructionJobs(XmlReader reader)
    {

    }
    void ReadXml_CostChangeJobs(XmlReader reader)
    {

    }
}
