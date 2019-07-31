using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Words {
    //Folder names
    public string FixtureSpriteFolder = "Images/Fixtures";
    public string ItemSpriteFolder = "Images/Items";
    public string PrefabFolder = "Images/Prefabs";
    public string VillageMapSpriteFolder = "Images/TileMaps/City V2";

    //Layer Names
    public string TileLayer = "Tile";
    public string CharacterLayer = "Character";
    public string VehicleLayer = "Vehicle";
    public string FixtureLayer = "Fixture";
    public string UILayer = "UI";

    //Character Names
    public string CharacterName = "Manager";
    public List<string> FirstNameList;

    //FixtureNames
    public string Lampost = "Lampost";
    public string Phonebox = "Phonebox";
    public string ParkTree = "Park Tree";
    public string Bin = "Bin";
    public string BusStop = "Bus Stop";
    public string StreetTree = "Street Tree";
    public string ParkBush = "Park Bush";
    public string Door = "ClosedDoor";
    public string DoorOpening = "OpeningDoor";
    public string DoorAlmost = "AlmostOpenDoor";
    public string DoorOpen = "OpenDoor";
    public string Postbox = "Postbox";
    public string Bench = "Bench";
    public string Checkout = "Checkout";
    public string FridgeDown = "Fridge Down";
    public string FridgeCorner = "Fridge Corner";
    public string FridgeRight = "Fridge Right";
    public string ShelfCorner = "Shelf Corner";
    public string ShelfDown = "Shelf Down";
    public string ShelfLeft = "Shelf Left";
    public string ShelfUp = "Shelf Up";
    public string ShelfWall = "Shelf Wall"; 
    public string Car = "Car";
    public string Bus = "Bus";
    public string IndicatorDownUp = "Indicator Down";
    public string IndicatorLeftRight = "Indicator Left";

    //Item Names
    public string Milk = "Milk"; //
    public string Butter = "Butter";//
    public string Yhogurt = "Yoghurt";//
    public string Meat = "Meat";//
    public string Cheese = "Cheese";//
    public string Bread = "Bread";//
    public string Newspaper = "Newspaper";//
    public string Teabag = "Teabags";//
    public string Sugar = "Sugar";//
    public string Coffee = "Coffee";//
    public string Biscuits = "Biscuits";//
    public string CookingSauce = "Cooking Sauces";//
    public string Pasta = "Pasta";//
    public string ChocolateBar = "Chocolate Bar";//
    public string BottledDrink = "Bottled Drink";//
    public string Crisps = "Crisps";//
    public string Fruit = "Fruit";
    public string Vegetable = "Vegetable";//
    public string Sweets = "Sweets";//

    //Purchase Words
    public string CustomerPurchase = "Customer Purchase";
    public string ShopOrder = "Shop Order";

    //Activity Names
    public string ShopActivity = "Shop";
    public string WalkToFountain = "Fountain";
    public string PostLetter = "Post";
    public string WaitForBus = "Bus";
    public string PostboxActivity = "Posting";
    public string BinActivity = "Bin";
    public string BenchActivity = "Bench";
    public string PayActivity = "Pay";
    public string CarActivity = "Walk To Car";
    public string WalkActivity = "Walk Home";
    public Direction PostboxDirection = Direction.Up;
    public Direction BinDirection = Direction.Up;
    public Direction BenchDirection = Direction.Down;

    //Queue Names
    public string ConstructionQueue = "Construction Queue";
    public string CheckoutQueue = "Checkouts";
    public string StockQueue = "Restock";
    public string LoiterQueue = "Loiter Queue";
    public string CostChangeQueue = "Price Change";

    public string ManEmptyCheckout = "Man Empty Checkout";
    public string WalkingToWork = "Walk To Work";
    public string WalkingHome = "Walking Home";

    //Colours
    public string Red = "Red";
    public string Blue = "Blue";
    public string Orange = "Orange";

    public Words()
    {
        Current = this;
        FirstNameList = new List<string>(PopulateListOfNames());
    }

    public string GetColour()
    {
        int ColourPicker = UnityEngine.Random.Range(1, 4);
        if (ColourPicker == 1)
        {
            return Red;
        }
        else if (ColourPicker == 2)
        {
            return Blue;
        }
        else { return Orange; }
    }

    public List<string> GetFullPriorityList()
    {
        List<string> list = new List<string>();
        list.Add(CheckoutQueue);
        list.Add(StockQueue);
        list.Add(CostChangeQueue);
        return list;
    }
   

    public string GetRandomCharacterSpriteName()
    {

        int rand = UnityEngine.Random.Range(1, 5);
        return "Person" + rand;
    }

    public string GetRandomName()
    {
        int rand = UnityEngine.Random.Range(0, FirstNameList.Count);
        return FirstNameList[rand];
    }
    public string GetEmployeeCurrJobDisplay(string currQueue)
    {
        if (currQueue == CheckoutQueue)
        {
            return "Serving a customer on Checkout";
        }
        else if (currQueue == StockQueue)
        {
            return "Restocking a shelf";
        }
        else if (currQueue == CostChangeQueue)
        {
            return "Updating prices";
        }
        else if (currQueue == ManEmptyCheckout)
        {
            return "Waiting for customer on Checkout";
        }
        else if (currQueue == WalkingToWork)
        {
            return "Walking to work";
        }
        else if (currQueue == WalkingHome)
        {
            return "Walking home";
        }
        return "Waiting for a Job";

    }

    public static Words Current { get; protected set; }
    List<string> PopulateListOfNames()
    {
        List<string> names = new List<string>();

        names.Add("Josh");
        names.Add("Jess");
        names.Add("Olly");
        names.Add("Brendan");
        names.Add("Helen");
        names.Add("Rob");
        names.Add("Martin");
        names.Add("Gerard");
        names.Add("James");
        names.Add("Matt");
        names.Add("Mike");
        names.Add("Ryan");
        names.Add("Saurabh");
        names.Add("Kam");
        names.Add("Pete");
        names.Add("Yan");
        names.Add("Shivan");
        names.Add("Gemma");
        names.Add("Lewis");
        names.Add("Adam");
        names.Add("Dave");
        names.Add("Becky");
        names.Add("Lee");
        names.Add("Luke");
        names.Add("Adam");
        names.Add("Lizzie");
        names.Add("Alex");
        names.Add("Kay");
        names.Add("Erik");
        names.Add("Saj");
        names.Add("Dan");
        names.Add("Tyler");
        names.Add("Emma");
        names.Add("Scott");
        names.Add("Tom");



        return names;
    }
}
