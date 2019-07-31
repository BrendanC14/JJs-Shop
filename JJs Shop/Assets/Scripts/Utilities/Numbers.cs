using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Numbers  {

    //Activity Numbers
    public float ShopTime = 1f;
    public float PostBoxTime = 1f;
    public float BenchTime = 30f;
    public float BinTime = 0.5f;
    public float GetInCarTime = 0.5f;
    public float WalkOfMappTime = 0f;
    public int ChanceOfShopping = 100;
    public int ChanceOfPost = 30;
    public int ChanceOFBench = 40;
    public int ChanceOfBin = 60;

    //Character Numbers
    public float TimeOnEachSprite = 10;
    public float ManagerSpeed = 1f;
    public float BusPersonPercChanceOfShopping = 100f;
    public float TimeBetweenPedestrians = 30f;
    public int PercChangeOfWantingItem = 25;
    public float TimeWillWaitForCashier = 100f;
    public int PersonID = 0;
    public int MaxNumberCustomers;

    //A random number will be generated between min and max,
    //Each customer will then have their own acceptance %s
    public float TooExpensivePercentageMin = 1.2f;
    public float TooExpensivePercentageMax = 1.4f;
    public float BuyTwoPercentageMin = 0.5f;
    public float BuyTwoPercentageMax = 0.6f;
    public float ExpectedPriceModifierMin = 0.9f;
    public float ExpectedPriceModifierMax = 1.1f;

    //City Numbers
    public int VillageWidth = 24;
    public int VillageHeight = 24;

    //Employee Numbers
    public int MaxCanCarry = 50;
    public float TimeToScanItem = 2f;
    public int MaxNumOfHours = 8;
    public int NextEmployeeID = 0;
    public Dictionary<int, int> ReliabilityPercentageMap;

    //Fixture Numbers
    public float DoorOpenSpeed = 4;
    public int FridgeMaxItems = 24;
    public int ShelfMaxItems = 24;
    public int StockFridgeMaxItems = 48;
    public int StockShelfMaxItems = 48;
    public float ShowOpinionTime = 4f;

    //Item Numbers

    public float DeliverTime = 30f;
    public int MilkSpace = 1;
    public int ButterSpace = 1;
    public int YhogurtSpace = 1;
    public int MeatSpace = 1;
    public int CheeseSpace = 1;
    public int BreadSpace = 1;
    public int NewspaperSpace = 1;
    public int TeabagSpace = 1;
    public int SugarSpace = 1;
    public int CoffeeSpace = 1;
    public int BiscuitsSpace = 1;
    public int CookingSauceSpace = 1;
    public int PastaSpace = 1;
    public int ChocolateBarSpace = 1;
    public int BottledDrinkSpace = 1;
    public int CrispsSpace = 1;
    public int FruitSpace = 1;
    public int VegetableSpace = 1;
    public int SweetsSpace = 1;
    public int NextItemID = 0;


    //These are default starting costs, will be overwritten throughout gameplay
    public double MilkCost = 0.70;
    public double ButterCost = 2.00;
    public double YhogurtCost = 0.50;
    public double MeatCost = 6.00;
    public double CheeseCost = 2.50;
    public double BreadCost = 1.00;
    public double NewspaperCost = 0.60;
    public double TeabagCost = 3.00;
    public double SugarCost = 1.00;
    public double CoffeeCost = 3.00;
    public double BiscuitsCost = 0.50;
    public double CookingSauceCost = 1.00;
    public double PastaCost = 2.00;
    public double ChocolateBarCost = 0.70;
    public double BottledDrinkCost = 1.50;
    public double CrispsCost = 0.80;
    public double FruitCost = 0.25;
    public double VegetableCost = 0.15;
    public double SweetsCost = 0.40;

    public double MilkOrderCost = 0.64;
    public double ButterOrderCost = 1.80;
    public double YhogurtOrderCost = 0.45;
    public double MeatOrderCost = 5.40;
    public double CheeseOrderCost = 2.25;
    public double BreadOrderCost = 0.90;
    public double NewspaperOrderCost = 0.54;
    public double TeabagOrderCost = 2.70;
    public double SugarOrderCost = 0.90;
    public double CoffeeOrderCost = 2.70;
    public double BiscuitsOrderCost = 0.45;
    public double CookingSaucOrdereCost = 0.90;
    public double PastaOrderCost = 1.80;
    public double ChocolateBarOrderCost = 0.63;
    public double BottledDrinkOrderCost = 1.10;
    public double CrispsOrderCost = 0.72;
    public double FruitOrderCost = 0.22;
    public double VegetableOrderCost = 0.13;
    public double SweetsOrderCost = 0.36;

    public double ExpectedMilkCost = 1.00;
    public double ExpectedButterCost = 1.90;
    public double ExpectedYhogurtCost = 0.50;
    public double ExpectedMeatCost = 5.50;
    public double ExpectedCheeseCost = 2.20;
    public double ExpectedBreadCost = 1.00;
    public double ExpectedNewspaperCost = 0.60;
    public double ExpectedTeabagCost = 3.00;
    public double ExpectedSugarCost = 1.00;
    public double ExpectedCoffeeCost = 3.00;
    public double ExpectedBiscuitsCost = 0.60;
    public double ExpectedCookingSauceCost = 1.00;
    public double ExpectedPastaCost = 2.50;
    public double ExpectedChocolateBarCost = 0.70;
    public double ExpectedBottledDrinkCost = 1.20;
    public double ExpectedCrispsCost = 0.75;
    public double ExpectedFruitCost = 0.25;
    public double ExpectedVegetableCost = 0.15;
    public double ExpectedSweetsCost = 0.90;

    //Job Numbers
    public float WaitAtCheckoutTime = 10f;
    public float RestockAtPercentage = 0.8f;
    public float RestockTime = 0.5f;
    public float LoiterJobTime = 0.5f;
    public float CostChangeTime = 1f;

    //Vehicle Numbers
    public float CarSpeedMax = 4f;
    public float CarSpeedMin = 1f;
    public float TimeBetweenVehicles = 2f;
    public float ZebraCrossingWaitTime = 0.5f;
    public float StopLineWaitTime = 1f;
    public float StartTileWaitTime = 2f;
    public float BusStopWaitTime = 10f;
    public float ShoppingTime = 30f;
    public float IndicatingTime = 1.5f;
    public int PercChanceOfSHopping = 5;
    public float TimeBetweenBuses = 30f;
    Dictionary<Tile, float> TileWaitTime;


    //UI Numbers
    public float ZoomSpeed = 0.01f;
    public float ZoomMin = 2.5f;
    public float ZoomMax = 5f;
    public float CameraXPositionMax = 19f;
    public float CameraXPositionMin = 4f;
    public float CameraYPositionMax = 21.5f;
    public float CameraYPositionMin = 1.5f;
    public float TransactionScreenTime = 3f;

    //World Numbers
    public float TimeBetweenMinutes = 3f;
    //public DateTime OpeningTime;
    //public DateTime ClosingTime;
    public int OpeningHour = 9;
    public int OpeningMinute = 0;
    public int ClosingHour = 17;
    public int ClosingMinute = 00;

    public int GetNextItemID()
    {
        NextItemID++;
        return NextItemID;
    }
    public int GetNextEmplyeeID()
    {
        NextEmployeeID++;
        return NextEmployeeID;
    }
    public int GetNextPersonID()
    {
        PersonID++;
        return PersonID;
    }
    public DateTime GetRandomDOB()
    {
        DateTime today = WorldTime.Current.Date;
        int randYear = UnityEngine.Random.Range(today.Year - 30, today.Year - 15);
        int randMonth = UnityEngine.Random.Range(1, 13);
        int randDay = UnityEngine.Random.Range(1, 29);
        return new DateTime(randYear, randMonth, randDay);
    }
    public double GetExpectedWage()
    {
        return 7.00;
    }
    public int GetPreferredHours(DateTime DOB)
    {
        int rand = UnityEngine.Random.Range(1, 100);
        //70% chance they want to work 8 hours
        //20% chance they want to work 6 hours
        //10% chance they want to work 4 hours
        if (rand <= 70)
        {
            return 8;
        }
        else if (rand <= 90)
        {
            return 6;
        }
        return 4;
    }
    public int GetEmployeeExperience(int Age)
    {
        if (Age - 15 >= 1)
        {
            return UnityEngine.Random.Range(0, Age - 15);
        }
        return 0;
    }
    public int GetEmployeeFlexibility()
    {
        return UnityEngine.Random.Range(1, 21);
    }
    public int GetEmployeeReliability()
    {
        return UnityEngine.Random.Range(1, 21);
    }
    public int GetEmployeeCheckoutAbilitiy()
    {
        return UnityEngine.Random.Range(1, 10);
    }
    public int GetEmployeeRestockAbility()
    {
        return UnityEngine.Random.Range(1, 10);
    }
    public int GetEmployeeCleanAbility()
    {
        return UnityEngine.Random.Range(1, 10);
    }
    

    public float TileStopWaitTime(Tile t)
    {
        if (TileWaitTime.ContainsKey(t))
        {
            return TileWaitTime[t];
        }
        return 0f;
    }

    public float GetPedestrianSpeed()
    {
        return UnityEngine.Random.Range(0.8f, 1.5f);
    }

    public float ModifierIncrease(float speed)
    {
        return speed * 0.015f;
    }
    public float GetRandomTileModerator()
    {
        return UnityEngine.Random.Range(-0.2f, 0.2F);
    }

    public int RandomNumberOfPeople(string VehicleType)
    {
        if (VehicleType == Words.Current.Car)
        {
            return UnityEngine.Random.Range(1, 5);
        }
        else
        {
            return UnityEngine.Random.Range(1, 8);
        }
    }

    public float GetRandomTooExpensivePercentage()
    {
        return UnityEngine.Random.Range(TooExpensivePercentageMin, TooExpensivePercentageMax);
    }
    public float GetRandomBuyTwoPercentage()
    {
        return UnityEngine.Random.Range(BuyTwoPercentageMin, BuyTwoPercentageMax);
    }
    public float GetRandomExpectedPriceModifier()
    {
        return UnityEngine.Random.Range(ExpectedPriceModifierMin, ExpectedPriceModifierMax);
    }

    //public void UpdateOpeningTime(int year, int month, int day)
    //{
    //    OpeningTime = new DateTime(year, month, day, OpeningHour, OpeningMinute, 0);
    //}

    //public void UpdateClosingTime(int year, int month, int day)
    //{
    //    ClosingTime = new DateTime(year, month, day, ClosingHour, ClosingMinute, 0);

    //}

    public Numbers()
    {
        Current = this;
        TileWaitTime = new Dictionary<Tile, float>();
        ReliabilityPercentageMap = new Dictionary<int, int>(PopulateReliabilityPercentageMap());
    }

    Dictionary<int, int> PopulateReliabilityPercentageMap()
    {
        Dictionary<int, int> dictionary = new Dictionary<int, int>();
        dictionary.Add(20, 101);
        dictionary.Add(19, 99);
        dictionary.Add(18, 97);
        dictionary.Add(17, 95);
        dictionary.Add(16, 93);
        dictionary.Add(15, 91);
        dictionary.Add(14, 89);
        dictionary.Add(13, 87);
        dictionary.Add(12, 85);
        dictionary.Add(11, 83);
        dictionary.Add(10, 81);
        dictionary.Add(9, 75);
        dictionary.Add(8, 69);
        dictionary.Add(7, 63);
        dictionary.Add(6, 57);
        dictionary.Add(5, 51);
        dictionary.Add(4, 41);
        dictionary.Add(3, 31);
        dictionary.Add(2, 21);
        dictionary.Add(1, 11);

        return dictionary;
    }

    public void AddTileWaitTimes(Tile t, float n)
    {
        TileWaitTime.Add(t, n);
    }
    public static Numbers Current { get; protected set; }
}
