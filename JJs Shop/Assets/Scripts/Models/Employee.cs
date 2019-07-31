using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Employee : IXmlSerializable {

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
            if (currTile != null) { currTile.Employees.Remove(this); }
            currTile = value;
            if (currTile != null) { currTile.Employees.Add(this); }
        }
    }
    public string Name;
    public int EmployeeID;
    public DateTime DOB;
    public int Age
    {
        get
        {
            TimeSpan timeSpan = WorldTime.Current.Date - DOB;
            return timeSpan.Days / 365;
        }
    }
    public double WageExpectations;
    public int PrefHours;
    public Tile nextTile;
    public Tile destTile;
    public Job currJob;
    public List<string> PriorityList;
    public List<string> TempPriorityList;
    public List<Item> ItemsInHand;
    float TimeToWait;

    public int Experience;
    public int Flexibility;
    public int Reliabilitiy;
    public int CheckoutAbility;
    public int RestockAbility;
    public int CleanAbilitiy;
    
    public int MaxCanCarry;
    public int StartingHour = 9;
    public int EndHour = 17;
    public int TempStartHour = 9;
    public int TempEndHour = 17;
    //public DateTime StartingTime;
    //public DateTime EndTime;
    public Item ItemNeeded;
    public double Wage = 6.50;
    public float NumberItemNeeded;
    public bool UserSelected;
    public bool WorkingJob;
    public bool OnShift = false;
    public bool OnTill = false;
    public bool CalledInLate = false;
    public bool TimeToLeave = false;

    public int NumFrames = 1;
    public int AnimationNumber = 1;
    public string SpriteName;

    public Action<Employee> cbEmployeePositionChanged;
    public Action<Employee> cbEmployeeDirectionChanged;
    public Action<Employee, int> cbEmployeeCalledInLate;
    public Action<Employee> cbEmployeeSelected;

    public void AddPersonPositionChangedCallback(Action<Employee> callback) { cbEmployeePositionChanged += callback; }
    public void AddPersonDirectionChangedCallback(Action<Employee> callback) { cbEmployeeDirectionChanged += callback; }
    public void AddEmployeeCalledInLateCallback(Action<Employee, int> callback) { cbEmployeeCalledInLate += callback; }
    public void AddEmployeeSelectedCallback(Action<Employee> callback) { cbEmployeeSelected += callback; }

    float MovementPercentage;
    float speed;
    public float Speed
    {
        get { return speed; }
        set
        {
            if (speed != value)
            {
                speed = value;
                SetSpeedToDisplay();
            }
        }
    }
    public int SpeedToDisplay;
    public bool Walking;
    Direction direction;
    public Direction Direction
    {
        get { return direction; }
        set
        {
            if (value != direction)
            {
                direction = value;
                if (cbEmployeePositionChanged != null) { cbEmployeeDirectionChanged(this); }
            }
        }
    }

    public Employee(float speed, List<String> priList)
    {
        EmployeeID = Numbers.Current.GetNextEmplyeeID();
        Speed = speed;
        XModifier = 0;
        YModifier = 0.2f;
        MaxCanCarry = Numbers.Current.MaxCanCarry;
        ItemsInHand = new List<Item>();
        PriorityList = new List<string>();
        PriorityList = priList;
        TempPriorityList = new List<string>();
        TempPriorityList = PriorityList;
        direction = Direction.Down;
        SpriteName = Words.Current.CharacterName;

        Name = Words.Current.GetRandomName();
        DOB = Numbers.Current.GetRandomDOB();
        PrefHours = Numbers.Current.GetPreferredHours(DOB);
        Experience = Numbers.Current.GetEmployeeExperience(Age);
        Flexibility = Numbers.Current.GetEmployeeFlexibility();
        Reliabilitiy = Numbers.Current.GetEmployeeReliability();
        CheckoutAbility = Numbers.Current.GetEmployeeCheckoutAbilitiy();
        RestockAbility = Numbers.Current.GetEmployeeRestockAbility();
        CleanAbilitiy = Numbers.Current.GetEmployeeCleanAbility();

        Wage = Numbers.Current.GetExpectedWage();
        //SetHours();


    }
    public Employee(EmployeeSaveFile savedEmployee)
    {
        XModifier = 0;
        YModifier = 0.2f;
        MaxCanCarry = Numbers.Current.MaxCanCarry;
        ItemsInHand = new List<Item>();
        PriorityList = new List<string>();
        TempPriorityList = new List<string>();
        direction = Direction.Down;
        SpriteName = Words.Current.CharacterName;


        Name = savedEmployee.Name;
        EmployeeID = savedEmployee.ID;
        DOB = savedEmployee.DOB;
        WageExpectations = savedEmployee.WageExpectations;
        PrefHours = savedEmployee.PrefHours;
        if (savedEmployee.TileX != 0 && savedEmployee.TileY != 0)
        {
            currTile = World.Current.GetTileAt(savedEmployee.TileX, savedEmployee.TileY);
        }
        if (savedEmployee.currJobQueue != "")
        {
            currJob = new Job(
                World.Current.GetTileAt(savedEmployee.currJobX, savedEmployee.currJobY),
                savedEmployee.currJobQueue,
                savedEmployee.currJobQueue,
                OnJobCompleted,
                savedEmployee.currJobTime);

            if (currJob.jobQueue == Words.Current.StockQueue)
            {
                ItemNeeded = World.Current.GetItemFromStockShelf(savedEmployee.ItemNeeded);
                NumberItemNeeded = savedEmployee.NumberItemNeeded;
            }
        }

        PriorityList = savedEmployee.PriList;
        TempPriorityList = PriorityList;

        if (savedEmployee.NumItemsInHand > 0)
        {
            Item itemInHand = World.Current.GetItemFromStockShelf(savedEmployee.ItemInHand);
            for (int i = 0; i < savedEmployee.NumItemsInHand; i++)
            {
                ItemsInHand.Add(itemInHand);
            }
        }

        Experience = savedEmployee.Experience;
        Flexibility = savedEmployee.Flexibility;
        Reliabilitiy = savedEmployee.Reliability;
        CheckoutAbility = savedEmployee.CheckoutAbility;
        RestockAbility = savedEmployee.RestockAbility;
        CleanAbilitiy = savedEmployee.CleanAbility;
        StartingHour = savedEmployee.StartingHour;
        EndHour = savedEmployee.EndHour;
        Wage = savedEmployee.Wage;
        OnShift = savedEmployee.OnShift;
        CalledInLate = savedEmployee.CalledInLate;
        TimeToLeave = savedEmployee.TimeToLeave;
        speed = savedEmployee.speed;
        
    }
    //public void SetHours()
    //{
    //    DateTime today = WorldTime.Current.Date;
    //    StartingTime = new DateTime(today.Year, today.Month, today.Day, StartingHour, 0, 0);
    //    EndTime = new DateTime(today.Year, today.Month, today.Day, EndHour, 0, 0);
    //}
    public void Update(float deltaTime)
    {
        if (OnShift)
        {
            if (TimeToWait > 0f)
            {
                TimeToWait -= deltaTime;
                if (TimeToWait < 0f)
                {
                    TimeToWait = 0f;
                }
                return;
            }

            Update_DoJob(deltaTime);
            if (!WorkingJob)
            {
                {
                    Update_DoMovement(deltaTime);
                    Walking = true;
                }
            }
            else { Walking = false; }
        }
    }

    void Update_DoJob(float deltaTime)
    {
        if (currJob == null || currJob.jobQueue == Words.Current.LoiterQueue)
        {
            GetNewJob();
            if (currJob == null)
            {
                //No jobs are available right now

                destTile = CurrTile;
                return;

            }
        }

        //Cool, we actually have a job to work
        if (currJob.jobQueue == Words.Current.LoiterQueue ||
            currJob.jobQueue == Words.Current.CostChangeQueue)
        {
            if (CurrTile == destTile)
            {
                currJob.DoWork(deltaTime);
            }
        }
        else if (currJob.jobQueue == Words.Current.CheckoutQueue)
        {
            if (CurrTile == destTile)
            {
                DoCheckoutJob(deltaTime);
            }
        }
        else if (currJob.jobQueue == Words.Current.StockQueue)
        {
            if (destTile == ItemNeeded.ShopShelf.EmployeeTile)
            {
                if (CurrTile == destTile)
                {
                    RestockCustomerShelfJob(deltaTime);
                }
            }
            else
            {//I'm off to the StockShelf
                if (CurrTile == destTile)
                {
                    PickupStockForRestock(deltaTime);
                }

            }
        }
        else if (currJob.jobQueue == Words.Current.WalkingToWork || currJob.jobQueue == Words.Current.ManEmptyCheckout ||
            currJob.jobQueue == Words.Current.WalkingHome)
        {
            if (CurrTile == destTile)
            {
                currJob.DoWork(deltaTime);
            }
        }
    }

    void Update_DoMovement(float deltaTime)
    {
        if (CurrTile == destTile)
        {
            WalkingPath_AStar = null;
            return;
        }

        if (nextTile == null || nextTile == currTile)
        {
            if (WalkingPath_AStar == null || WalkingPath_AStar.Length() == 0)
            {
                WalkingPath_AStar = new Path_ASTar(World.Current, CurrTile, destTile, false, false, Direction);
                if (WalkingPath_AStar.Length() == 0)
                {
                    Debug.LogError("No path to dest");
                    AbandonJob();
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
            return;
        }
        if (!ModifiersCorrect(0, 0.2f))
        {
            return;
        }
        //Total dist from A to B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));
        //How much can we travel this update
        float distThisFrame = (Speed / nextTile.PersonMovementCost) * deltaTime;
        //How much in terms of percentage?
        float percThisFrame = distThisFrame / distToTravel;
        //Add to overral percentage travelled   
        MovementPercentage += percThisFrame;


        CheckDirection();
        if (MovementPercentage >= 1)
        {
            CurrTile = nextTile;
            MovementPercentage = 0f;
        }

        if (cbEmployeePositionChanged != null)
        {
            cbEmployeePositionChanged(this);
        }


    }

    void GetNewJob()
    {
        if (!TimeToLeave)
        {
            Job newJob = World.Current.JobQueue.GetJob(PriorityList, this);
            if (newJob == null)
            {
                if ((currJob != null && currJob.jobQueue == Words.Current.LoiterQueue))
                {
                    return;
                }

                if (PriorityList.Contains(Words.Current.CheckoutQueue) && !SomeoneElseOnTill())
                {
                    OnTill = true;
                    currJob = new Job(World.Current.CheckoutTile,
                        Words.Current.ManEmptyCheckout,
                        Words.Current.CheckoutQueue,
                        null,
                        0f);
                }
                else
                {
                    OnTill = false;
                    int r = UnityEngine.Random.Range(0, World.Current.ShopTiles.Count);
                    currJob = new Job(World.Current.ShopTiles[r],
                        Words.Current.LoiterQueue,
                        Words.Current.LoiterQueue,
                        null,
                        Numbers.Current.LoiterJobTime);
                }
            }
            else { currJob = newJob; }

            destTile = currJob.tile;

            if (currJob.jobQueue == Words.Current.StockQueue)
            {//I've just got a restock job and need to go get the items first.
                OnTill = false;
                ItemNeeded = currJob.tile.Fixture.ItemOnShelf;
                NumberItemNeeded = currJob.jobTime;
                if (NumberItemNeeded == 0)
                {
                    currJob = null;
                    ItemNeeded = null;
                    NumberItemNeeded = 0;
                    return;
                }
                // Before I do anything I should check whether I've got enough stock in the Stock Room
                if (ItemNeeded.StockShelf.GetItemsOnShelf().Count == 0)
                {
                    //Don't have anything in the stock room to restock
                    currJob.jobTime = -1;
                    World.Current.StockQueue.Enqueue(currJob);
                    currJob = null;
                    ItemNeeded = null;
                    NumberItemNeeded = 0;
                    return;
                }
                destTile = ItemNeeded.StockShelf.EmployeeTile;
            }
            else if (currJob.jobQueue == Words.Current.CostChangeQueue)
            {
                OnTill = false;
                ItemNeeded = currJob.tile.Fixture.ItemOnShelf;
                destTile = ItemNeeded.ShopShelf.EmployeeTile;
            }


            currJob.RegisterJobCompleteCallback(OnJobCompleted);
            currJob.RegisterJobCancelCallback(OnJobCompleted);

            WalkingPath_AStar = new Path_ASTar(World.Current, CurrTile, destTile, false, false, Direction);
            if (WalkingPath_AStar.Length() == 0)
            {
                Debug.LogError("No route to dest");
                AbandonJob();
                destTile = currTile;
            }
        }
        else
        {
            LeaveWork();
        }
    }

    void OnJobCompleted(Job j)
    {
        WorkingJob = false;
        if (currJob.jobQueue == Words.Current.CheckoutQueue)
        {
            currJob = null;
            return;
        }
        if (currJob.jobQueue == Words.Current.StockQueue)
        {
            ItemNeeded.ShopShelf.NeedsRestock = false;
            ItemNeeded = null;
            NumberItemNeeded = 0;
            currJob = null;
            return;
        }
        if (currJob.jobQueue == Words.Current.LoiterQueue)
        {
            currJob = null;
            return;
        }
        if (currJob.jobQueue == Words.Current.CostChangeQueue)
        {
            ItemNeeded.UpdatePrice();
            currJob = null;
            return;
        }
        if (currJob.jobQueue == Words.Current.WalkingHome)
        {
            currJob = null;
            RemoveEmployee();
            return;
        }
        currJob = null;
        GetNewJob();
        return;
    }

    void AbandonJob()
    {
        nextTile = destTile = currTile;
        World.Current.JobQueue.AbandonJob(currJob);
        currJob = null;
    }

    void DoCheckoutJob(float deltaTime)
    {
        WorkingJob = true;
        if (ModifiersCorrect(CurrTile.Fixture.EmployeeXModifier, CurrTile.Fixture.EmployeeYModifier))
        {
            Direction = CurrTile.Fixture.EmployeeDirection;
            if (currTile.Fixture.CustomerTile.People.Count > 0 &&
                CurrTile.Fixture.CustomerTile.People[0].Paid == false)
            {
                currJob.DoWork(deltaTime);
            }


        }
        else
        {
            return;
        }
    }

    void RestockCustomerShelfJob(float deltaTime)
    {

        WorkingJob = true;
        if (ModifiersCorrect(ItemNeeded.ShopShelf.EmployeeXModifier, ItemNeeded.ShopShelf.EmployeeYModifier))
        {//I've got the stuff and I'm at the right shelf
            Direction = ItemNeeded.ShopShelf.EmployeeDirection;
            if (ItemsInHand.Count > 0)
            {//Still got stuff to put back
                if (ItemNeeded.PutOnShopShelf(1))
                {
                    ItemsInHand.RemoveAt(0);
                    TimeToWait += Numbers.Current.RestockTime;

                }
                else
                {
                    //TODO: Gotta do something if I've got too much in my hand
                    ItemsInHand = new List<Item>();
                    currJob = null;
                }
            }
            else
            {//finished restocking

                currJob.DoWork(currJob.jobTime);
                WorkingJob = false;
            }

        }
        else { return; }
    }
    void PickupStockForRestock(float deltaTime)
    {

        WorkingJob = true;
        if (ModifiersCorrect(ItemNeeded.StockShelf.EmployeeXModifier, ItemNeeded.StockShelf.EmployeeYModifier))
        {//I've reached the stock shelf to get the stuff
            Direction = ItemNeeded.StockShelf.EmployeeDirection;
            if (ItemNeeded.TakeOffStockShelf(1))
            {// Item can come off the shelf
                ItemsInHand.Add(ItemNeeded);
                if (ItemsInHand.Count == NumberItemNeeded)
                {//Got enough stuff in my hand!
                    destTile = ItemNeeded.ShopShelf.EmployeeTile;
                    WorkingJob = false;
                }
                else
                {
                    TimeToWait += Numbers.Current.RestockTime;
                }
            }
            else
            {
                //TODO: Not enough quantity on the shelf but will just take what I have
                destTile = ItemNeeded.ShopShelf.EmployeeTile;
                WorkingJob = false;
            }

        }
        else
        {
            return;
        }
    }

    void CheckDirection()
    {
        if (nextTile == null || CurrTile == destTile)
        {
            if (currJob != null)
            {
                float destXModifier;
                float destYModifier;
                if (destTile == currJob.tile.Fixture.EmployeeTile)
                {
                    destXModifier = currJob.tile.Fixture.EmployeeXModifier;
                    destYModifier = currJob.tile.Fixture.EmployeeYModifier;
                }
                else
                {
                    destXModifier = ItemNeeded.StockShelf.EmployeeXModifier;
                    destYModifier = ItemNeeded.StockShelf.EmployeeYModifier;
                }
                if (YModifier > destYModifier)
                {
                    Direction = Direction.Down;
                }
                else if (YModifier < destYModifier)
                {
                    Direction = Direction.Up;
                }
                else
                {
                    if (XModifier > destXModifier)
                    {
                        Direction = Direction.Left;
                    }
                    else if (XModifier < destXModifier)
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
    bool ReachedDestination()
    {
        return false;
        if (nextTile == destTile)
        {
            if (currJob != null && destTile == currJob.tile.Fixture.EmployeeTile)
            {
                if (!ModifiersCorrect(currJob.tile.Fixture.EmployeeXModifier, currJob.tile.Fixture.EmployeeYModifier))
                {
                    return false;
                }
            }
        }
        return false;
    }

    bool ModifiersCorrect(float targetX, float targetY)
    {

        if (XModifier == targetX && YModifier == targetY)
        {
            return true;
        }
        else
        {

            float destXModifier = targetX;
            float destYModifier = targetY;
            if (destXModifier > XModifier)
            {
                XModifier += Numbers.Current.ModifierIncrease(Speed);
                Direction = Direction.Right;
                if (XModifier > destXModifier)
                {
                    XModifier = destXModifier;
                }
            }
            else if (destXModifier < XModifier)
            {
                XModifier -= Numbers.Current.ModifierIncrease(Speed);
                Direction = Direction.Left;
                if (XModifier < destXModifier)
                {
                    XModifier = destXModifier;
                }
            }

            else if (destYModifier > YModifier)
            {
                YModifier += Numbers.Current.ModifierIncrease(Speed);
                Direction = Direction.Up;
                if (YModifier > destYModifier)
                {
                    YModifier = destYModifier;
                }
            }
            else if (destYModifier < YModifier)
            {
                YModifier -= Numbers.Current.ModifierIncrease(Speed);
                Direction = Direction.Down;
                if (YModifier < destYModifier)
                {
                    YModifier = destYModifier;
                }
            }

            Walking = true;
            if (cbEmployeeDirectionChanged != null)
            {
                cbEmployeeDirectionChanged(this);
            }
            if (cbEmployeePositionChanged != null)
            {
                cbEmployeePositionChanged(this);
            }

        }

        return false;

    }

    void SetSpeedToDisplay()
    {
        if (Speed >= 0.8f && Speed < 0.9F)
        {
            SpeedToDisplay = 1;
        }
        else if (Speed >= 0.9f && Speed < 1F)
        {
            SpeedToDisplay = 2;
        }
        else if (Speed >= 1f && Speed < 1.1F)
        {
            SpeedToDisplay = 3;
        }
        else if (Speed >= 1.1f && Speed < 1.2F)
        {
            SpeedToDisplay = 4;
        }
        else if (Speed >= 1.2f && Speed < 1.3F)
        {
            SpeedToDisplay = 5;
        }
        else if (Speed >= 1.3f && Speed < 1.4F)
        {
            SpeedToDisplay = 6;
        }
        else 
        {
            SpeedToDisplay = 7;
        }

    }

    public DateTime GetTodaysStartTime(WorkingDay Today)
    {
        if (!Today.EmployeesStartingTimes.ContainsKey(this))
        {
            //TODO: This guy isn't working today, do something
            return new DateTime(1, 1, 1901);
        }

        return Today.EmployeesStartingTimes[this];
    }
    public DateTime GetTodaysEndTime(WorkingDay Today)
    {
        if (!Today.EmployeeEndTimes.ContainsKey(this))
        {
            //TODO: This guy isn't working today, do something
            return new DateTime(0, 0, 0);
        }

        return Today.EmployeeEndTimes[this];
    }
    public void UpdateWorkingHours()
    {
        DateTime tomorrow = WorldTime.Current.Date.AddDays(1);
        StartingHour = TempStartHour;
        EndHour = TempEndHour;
        //StartingTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, StartingHour, 0, 0);
        //EndTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, EndHour, 0, 0);
    }
    public bool ReliabilityPassedCanStart()
    {
        if (!Numbers.Current.ReliabilityPercentageMap.ContainsKey(Reliabilitiy))
        {
            Debug.Log("Why do I not have a % for this reliability?");
            //returning true to keep game running
            return true;
        }
        if (CalledInLate)
        {
            //Already called in late and updated start time so will make it in now
            return true;
        }

        int StartOnTimePerc = Numbers.Current.ReliabilityPercentageMap[Reliabilitiy];

        int r = UnityEngine.Random.Range(1, 100);

        if (r <= StartOnTimePerc)
        {
            return true;
        }
        CalledInLate = true;
        DateTime newStartingTime = World.Current.Today.EmployeesStartingTimes[this].AddMinutes(r - StartOnTimePerc);
        World.Current.Today.EmployeesStartingTimes[this] = newStartingTime;
        if (cbEmployeeCalledInLate != null)
        {
            cbEmployeeCalledInLate(this, r - StartOnTimePerc);
        }
        return false;

    }
    public void StartShift()
    {
        OnShift = true;
        CurrTile = World.Current.PedestrianExitTiles[0];

        int r = UnityEngine.Random.Range(0, World.Current.StockRoomTiles.Count);
        currJob = new Job(World.Current.StockRoomTiles[r],
            Words.Current.WalkingToWork,
            Words.Current.WalkingToWork,
            null,
            Numbers.Current.LoiterJobTime);
        destTile = currJob.tile;
        currJob.RegisterJobCompleteCallback(OnJobCompleted);
        currJob.RegisterJobCancelCallback(OnJobCompleted);

        WalkingPath_AStar = new Path_ASTar(World.Current, CurrTile, destTile, false, false, Direction);
        if (WalkingPath_AStar.Length() == 0)
        {
            Debug.LogError("No route to dest");
            AbandonJob();
            destTile = currTile;
        }
    }
    public void EndShift()
    {
        TimeToLeave = true;
    }
    void LeaveWork()
    {
        currJob = new Job(World.Current.PedestrianExitTiles[0],
               Words.Current.WalkingHome,
               Words.Current.WalkingHome,
               null,
               0f);
        OnTill = false;
        destTile = currJob.tile;
        currJob.RegisterJobCompleteCallback(OnJobCompleted);
        currJob.RegisterJobCancelCallback(OnJobCompleted);
        WalkingPath_AStar = new Path_ASTar(World.Current, CurrTile, destTile, false, false, Direction);
        if (WalkingPath_AStar.Length() == 0)
        {
            Debug.LogError("No route to dest");
            AbandonJob();
            destTile = currTile;
        }
    }
    void RemoveEmployee()
    {
        DateTime tomorrow = WorldTime.Current.Date.AddDays(1);
        //StartingTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, StartingHour, 0, 0);
        //EndTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, EndHour, 0, 0);
        CurrTile = null;
        TimeToLeave = false;
        OnShift = false;
        CalledInLate = false;
        World.Current.cbEmployeeRemoved(this);
    }
    bool SomeoneElseOnTill()
    {
        foreach (Employee e in World.Current.Employees)
        {
            if (e.EmployeeID != EmployeeID && e.OnShift && e.OnTill)
            {
                return true;
            }
        }
        return false;
    }

    //////////////////////////////////////////////////////////////////////////////////////
    /////               
    /////                           SAVING & LOADING
    /////       
    ///////////////////////////////////////////////////////////////////////////////////////



    public Employee()
    {

    }

    public XmlSchema GetSchema()
    {
        return null;
    }
    public void WriteXml(XmlWriter writer)
    {
        foreach (String s in PriorityList)
        {
            writer.WriteStartElement("Queue");
            writer.WriteAttributeString("Name", s);
            writer.WriteEndElement();
        }

        writer.WriteAttributeString("Speed", speed.ToString());
        if (CurrTile != null)
        {
            writer.WriteAttributeString("X", CurrTile.X.ToString());
            writer.WriteAttributeString("Y", CurrTile.Y.ToString());
        }
        writer.WriteAttributeString("Name", Name);
        writer.WriteAttributeString("EmployeeID", EmployeeID.ToString());
        writer.WriteAttributeString("DOB", DOB.ToString());
        writer.WriteAttributeString("WageExpectations", WageExpectations.ToString());
        writer.WriteAttributeString("PrefHours", PrefHours.ToString());
        writer.WriteAttributeString("Experience", Experience.ToString());
        writer.WriteAttributeString("Flexibility", Flexibility.ToString());
        writer.WriteAttributeString("Reliability", Reliabilitiy.ToString());
        writer.WriteAttributeString("CheckoutAbility", CheckoutAbility.ToString());
        writer.WriteAttributeString("RestockAbility", RestockAbility.ToString());
        writer.WriteAttributeString("CleanAbility", CleanAbilitiy.ToString());
        writer.WriteAttributeString("StartingHour", StartingHour.ToString());
        writer.WriteAttributeString("EndHour", EndHour.ToString());
        writer.WriteAttributeString("Wage", Wage.ToString());
        writer.WriteAttributeString("OnShift", OnShift.ToString());
        writer.WriteAttributeString("CalledInLate", CalledInLate.ToString());
        writer.WriteAttributeString("TimeToLeave", TimeToLeave.ToString());
        if (currJob != null)
        {
            writer.WriteStartElement("currJob");
            writer.WriteAttributeString("X", currJob.tile.X.ToString());
            writer.WriteAttributeString("Y", currJob.tile.Y.ToString());
            writer.WriteAttributeString("jobQueue", currJob.jobQueue);
            writer.WriteAttributeString("jobTime", currJob.jobTime.ToString());
            if (currJob.jobQueue == Words.Current.StockQueue)
            {
                writer.WriteAttributeString("ItemNeeded", ItemNeeded.Name);
                writer.WriteAttributeString("NumberItemNeeded", NumberItemNeeded.ToString());
            }

            writer.WriteEndElement();
        }

        writer.WriteAttributeString("ItemHand", ItemsInHand[0].Name);
        writer.WriteAttributeString("ItemHandCount", ItemsInHand.Count.ToString());

    }

    public void ReadXml(XmlReader reader)
    {
        int x = int.Parse(reader.GetAttribute("X"));
        if (x!= 0)
        {
            CurrTile = World.Current.GetTileAt(x, int.Parse(reader.GetAttribute("Y")));
        }
        Name = reader.GetAttribute("Name");
        EmployeeID = int.Parse(reader.GetAttribute("EmployeeID"));
        DOB = DateTime.Parse(reader.GetAttribute("DOB"));
        WageExpectations = double.Parse(reader.GetAttribute("WageExpectations"));
        PrefHours = int.Parse(reader.GetAttribute("PrefHours"));
        Experience = int.Parse(reader.GetAttribute("Experience"));
        Flexibility = int.Parse(reader.GetAttribute("Flexibility"));
        Reliabilitiy = int.Parse(reader.GetAttribute("Reliability"));
        CheckoutAbility = int.Parse(reader.GetAttribute("CheckoutAbility"));
        RestockAbility = int.Parse(reader.GetAttribute("RestockAbility"));
        CleanAbilitiy = int.Parse(reader.GetAttribute("CleanAbility"));
        StartingHour = int.Parse(reader.GetAttribute("StartingHour"));
        EndHour = int.Parse(reader.GetAttribute("EndHour"));
        Wage = double.Parse(reader.GetAttribute("Wage"));
        if (reader.GetAttribute("OnShift") == "True")
        {
            OnShift = true;
        }
        else { OnShift = false; }
        if (reader.GetAttribute("CalledInLate") == "True")
        {
            CalledInLate = true;
        }
        else { CalledInLate = false; }
        if (reader.GetAttribute("TimeToLeave") == "True")
        {
            TimeToLeave = true;
        }
        else { TimeToLeave = false; }


        if (reader.ReadToDescendant("currJob"))
        {
            currJob = new Job(
                World.Current.GetTileAt(int.Parse(reader.GetAttribute("X")), int.Parse(reader.GetAttribute("Y"))),
                reader.GetAttribute("jobQueue"),
                reader.GetAttribute("jobQueue"),
                OnJobCompleted,
                float.Parse(reader.GetAttribute("jobTime")));

            if (currJob.jobQueue == Words.Current.StockQueue)
            {
                ItemNeeded = World.Current.GetItemFromShopShelf(reader.GetAttribute("ItemNeeded"));
                NumberItemNeeded = int.Parse(reader.GetAttribute("NumberItemNeeded"));
            }

        }

        Item itemNeeded = World.Current.GetItemFromShopShelf(reader.GetAttribute("ItemNeeded"));
        for (int i = 0; i < int.Parse(reader.GetAttribute("ItemHandCount")); i++)
        {
            ItemsInHand.Add(itemNeeded);
        }

    }
}
