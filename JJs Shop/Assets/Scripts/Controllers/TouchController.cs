using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TouchController : MonoBehaviour {

    Vector3 BeginPosition;
    Vector3 EndPosition;
    Vector3 lastFramePosition;
    Vector3 currFramePosition;

    public GameObject WorldTimeDisplay;

    public GameObject Canvas;
    public GameObject ItemMenu;
    public GameObject ItemName;
    public GameObject ItemPicture;
    public GameObject OnShelfNumber;
    public GameObject InStockNumber;
    public GameObject OrderMoreButton;
    public GameObject IncreasePriceButton;
    public GameObject DecreasePriceButton;
    public GameObject Price;
    public GameObject PriceAdvice;
    public GameObject ConfirmPriceButton;

    public GameObject FeedbackPrice;
    public GameObject ItemBoughtTwo;
    public GameObject ItemHappy;
    public GameObject ItemNeutral;
    public GameObject ItemTooExpensive;
    public GameObject ItemWouldaBoughtTwo;
    public GameObject ItemOuttaStock;
    public GameObject FeedbackPrevPrice;
    public GameObject PrevItemBoughtTwo;
    public GameObject PrevItemHappy;
    public GameObject PrevItemNeutral;
    public GameObject PrevItemTooExpensive;
    public GameObject PrevItemWouldaBoughtTwo;
    public GameObject PrevItemOuttaStock;

    public GameObject OrderConf;
    public GameObject ItemNameConf;
    public GameObject ItemPicConf;
    public GameObject OrderConfText;
    public GameObject OrderConfButton;

    public GameObject InfoMenu;
    public GameObject InfoItemName;
    public GameObject InfoItemPic;
    public GameObject InfoText;

    public GameObject ShopMenu;
    public GameObject OpenTime;
    public GameObject CloseTime;

    public GameObject TransactionMenu;
    public RectTransform ContentPanel;
    public GameObject TransactionDisplayPrefab;
    TransactionDisplayController controller;
    GameObject newTransaction;

    public GameObject OpeningHoursMenu;
    public GameObject OpeningHoursCloeButton;
    public GameObject OpeningTimeOpenTime;
    public GameObject OpeningTimeCloseTime;
    public GameObject ShopBar;
    public GameObject OpeningHoursEmployeeMenu;
    int TempOpeningTime;
    int TempClosingTime;
    public GameObject OpeningHoursContentPanel;
    public GameObject EmployeeHoursPrefab;
    public GameObject RedHourPrefab;
    EmployeeHoursDisplayController employeeController;
    Dictionary<int, GameObject> EmployeeOpeningHoursGOMap;
    List<GameObject> RedHours;
    public GameObject OpeningHoursConfirm;

    public GameObject ApplicantsMenu;
    public GameObject ApplicantSmallViewPanel;
    public GameObject ApplicantSmallViewPrefab;
    ApplicantSmallViewDisplayController applicantController;
    public GameObject ApplicantName;
    public GameObject ApplicantDOB;
    public GameObject ApplicantPrefHours;
    public GameObject ApplicantYearsExperience;
    public GameObject ApplicantFlexibility;
    public GameObject ApplicantReliability;
    public GameObject ApplicantCheckoutAbility;
    public GameObject ApplicantRestockAbility;
    public GameObject ApplicantHireButton;
    Employee applicantSelect;

    public GameObject EODMenu;
    public GameObject EODStats;

    public GameObject EmployeeLateMenu;
    public GameObject EmployeeLateText;

    public GameObject EmployeeMenu;
    public GameObject EmployeeMenuContentPanel;
    public GameObject EmployeeSettingsPrefab;
    EmployeeSettingsDisplayController employeeSettingsController;

    public GameObject EmployeeResponsibiliiesMenu;
    public GameObject ResponsibilitiesHeader;
    public GameObject ResponsibilitesContentPanel;
    JobQueueDisplayController jobQueueController;
    public GameObject JobQueueNotIncludedContentPanel;
    JobQueueNotIncludedDisplayController jobQueueNotIncludedController;
    public GameObject JobQueuePrefab;
    public GameObject JobQueueNotIncludedPrefab;
    public GameObject ResponsibilitiesWage;
    public GameObject ResponsibilityStart;
    public GameObject ResponsibilityEnd;
    public GameObject ResponsibilityFlex;
    public GameObject ResponsibilityReliability;
    public GameObject ResponsibilityCheckout;
    public GameObject ResponsibilityRestock;
    public GameObject CloseResponsibilityButton;
    public GameObject ConfirmResponsibilityButton;

    public GameObject SingleEmployeeMenu;

    public GameObject SettingsMenu;

    public GameObject Paused;

    public GameObject SpeedButton;
    public GameObject Balance;
    public GameObject BalanceDiff;
    Dictionary<string, Sprite> SpriteMap;
    float TimeToWait = 0f;
    bool Ordering = false;
    bool ItemMenuing = false;
    bool ReviewingPurchases = false;
    bool ShopSettings = false;
    bool EmployeeMenuActive = false;
    int OrderCapacity;
    double OrderCost;
    double CurrentItemCost;
    double NewItemCost;


    

    List<GameObject> PreviewGameObjects;

    BuildModeController bmc;
    FixtureSpriteController fsc;
    Tile SelectedTile;
    Person SelectedPerson;
    Fixture SelectedFixture;
    enum MouseMode
    {
        SELECT,
        ITEM
    }
    MouseMode currentMode = MouseMode.SELECT;

	// Use this for initialization
	void Start () {
        bmc = GameObject.FindObjectOfType<BuildModeController>();
        fsc = GameObject.FindObjectOfType<FixtureSpriteController>();
        RedHours = new List<GameObject>();
        EmployeeOpeningHoursGOMap = new Dictionary<int, GameObject>();
        ItemMenu.SetActive(false);
        OrderConf.SetActive(false);
        BalanceDiff.SetActive(false);
        InfoMenu.SetActive(false);
        TransactionMenu.SetActive(false);
        ShopMenu.SetActive(false);
        ApplicantsMenu.SetActive(false);
        EODMenu.SetActive(false);
        EmployeeLateMenu.SetActive(false);
        EmployeeMenu.SetActive(false);
        PreviewGameObjects = new List<GameObject>();
        TempOpeningTime = Numbers.Current.OpeningHour;
        TempClosingTime = Numbers.Current.ClosingHour;
        OpeningHoursConfirm.GetComponent<Button>().onClick.AddListener(() => { ConfirmOpeningHoursFirstTime(); });

        Balance.GetComponentInChildren<Text>().text = "£" +  World.Current.Balance.ToString("N");
        LoadSprites();
        Paused.SetActive(false);
        World.Current.AddBalanceUpdatingCallback(ShowBlanceUpdating);
        World.Current.AddEODCallback(OpenEODMenu);
        OpenEmployeeHours();
        OpeningHoursCloeButton.SetActive(false);

    }
    // Update is called once per frame
    void Update()
    {

        WorldTimeDisplay.GetComponentInChildren<Text>().text = WorldTime.Current.Date.ToString("dd/MM/yyyy") + "\n" + WorldTime.Current.Date.ToShortTimeString();

        if (Input.touchCount == 1)
        {
            if (!Ordering && !ItemMenu)
            {
                currFramePosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            if (!Ordering && !ItemMenu)
            {
                currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SeeIfMenuNeeded();
            }
        }
        if (TimeToWait > 0f)
        {
            TimeToWait -= Time.deltaTime;
            if (TimeToWait < 0f)
            {
                TimeToWait = 0f;
            }
            return;
        }

        if (!ItemMenuing && !Ordering && !ReviewingPurchases && !ShopSettings)
        {
            if (Input.touchCount == 1)
            {

                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    lastFramePosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                    lastFramePosition.z = 0;
                    BeginPosition = Camera.main.ViewportToScreenPoint(Input.GetTouch(0).position);
                    BeginPosition.z = 0;

                }
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    currFramePosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);


                    currFramePosition.z = 0;
                    if (currentMode == MouseMode.SELECT)
                    {
                        UpdateCameraMovement();
                    }
                }
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    EndPosition = Camera.main.ViewportToScreenPoint(Input.GetTouch(0).position);
                    EndPosition.z = 0;
                    if (currentMode == MouseMode.SELECT &&
                        ((EndPosition.x - BeginPosition.x > -3000 &&
                        EndPosition.x - BeginPosition.x < 3000) ||
                        (EndPosition.y - BeginPosition.y > -3000 &&
                        EndPosition.y - BeginPosition.y < 3000)))
                    {
                        currFramePosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);

                        SeeIfMenuNeeded();
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                if (currentMode == MouseMode.SELECT)
                {
                    UpdateCameraZoom();
                }
            }
            else if (Input.GetMouseButton(1))
            {
                currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SeeIfMenuNeeded();
            }
            else if (ItemMenuing)
            {
                Tile T = GetTileUnderTouch();
                Fixture F = T.Fixture;
                if (F != null)
                {
                    InStockNumber.GetComponent<Text>().text = F.ItemOnShelf.StockShelf.GetItemsOnShelf().Count.ToString();
                    OnShelfNumber.GetComponent<Text>().text = F.ItemOnShelf.ShopShelf.GetItemsOnShelf().Count.ToString();


                    if (F.ItemOnShelf.PrevPrice > 0)
                    {
                        FeedbackPrevPrice.GetComponent<Text>().text = "£" + F.ItemOnShelf.PrevPrice.ToString("N");
                        PrevItemBoughtTwo.GetComponent<Text>().text = F.ItemOnShelf.PrevBoughtTwo.ToString();
                        PrevItemHappy.GetComponent<Text>().text = F.ItemOnShelf.PrevHappy.ToString();
                        PrevItemNeutral.GetComponent<Text>().text = F.ItemOnShelf.PrevNeutral.ToString();
                        PrevItemTooExpensive.GetComponent<Text>().text = F.ItemOnShelf.PrevNotBuying.ToString();
                        PrevItemWouldaBoughtTwo.GetComponent<Text>().text = F.ItemOnShelf.PrevWouldaBoughtTwo.ToString();
                        PrevItemOuttaStock.GetComponent<Text>().text = F.ItemOnShelf.PrevOuttaStock.ToString();
                    }
                    else
                    {
                        FeedbackPrevPrice.GetComponent<Text>().text = "N/A";
                        PrevItemBoughtTwo.GetComponent<Text>().text = "N/A";
                        PrevItemHappy.GetComponent<Text>().text = "N/A";
                        PrevItemNeutral.GetComponent<Text>().text = "N/A";
                        PrevItemTooExpensive.GetComponent<Text>().text = "N/A";
                        PrevItemWouldaBoughtTwo.GetComponent<Text>().text = "N/A";
                        PrevItemOuttaStock.GetComponent<Text>().text = "N/A";

                    }
                    FeedbackPrice.GetComponent<Text>().text = "£" + F.ItemOnShelf.Price.ToString("N");
                    ItemBoughtTwo.GetComponent<Text>().text = F.ItemOnShelf.BoughtTwo.ToString();
                    ItemHappy.GetComponent<Text>().text = F.ItemOnShelf.Happy.ToString();
                    ItemNeutral.GetComponent<Text>().text = F.ItemOnShelf.Neutral.ToString(); ;
                    ItemTooExpensive.GetComponent<Text>().text = F.ItemOnShelf.NotBuying.ToString();
                    ItemWouldaBoughtTwo.GetComponent<Text>().text = F.ItemOnShelf.WouldaBoughtTwo.ToString();
                    ItemOuttaStock.GetComponent<Text>().text = F.ItemOnShelf.OuttaStock.ToString();
                }

            }
            if (Ordering)
            {
                Tile t = GetTileUnderTouch();
                Fixture f = t.Fixture;
                if (f != null)
                {
                    UpdateOrderConfText(t, f);
                }
            }
            if (EmployeeMenuActive)
            {
                employeeSettingsController = SingleEmployeeMenu.GetComponent<EmployeeSettingsDisplayController>();
                Employee e = null;
                foreach (Employee emp in World.Current.Employees)
                {
                    if (emp.Name == employeeSettingsController.EmployeeName.text)
                    {
                        e = emp;
                    }
                }
                SingleEmployeeMenu.transform.SetParent(Canvas.transform, false);
                if (e.currJob != null)
                {
                    employeeSettingsController.CurrentJob.text = Words.Current.GetEmployeeCurrJobDisplay(e.currJob.jobQueue);
                }
                else
                {
                    if (!e.OnShift)
                    {
                        employeeSettingsController.CurrentJob.text = "Not working";

                    }
                    else
                    {
                        employeeSettingsController.CurrentJob.text = Words.Current.GetEmployeeCurrJobDisplay("");
                    }
                }
            }
            else
            {
                SingleEmployeeMenu.SetActive(false);
            }
            // if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            //   {   // Right or Middle Mouse Button
            //      currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //       currFramePosition.z = 0;
            //         UpdateCameraMovement();
            //
            //      lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //       lastFramePosition.z = 0;
            //
            //      Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
            //     Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, Numbers.Current.ZoomMin, Numbers.Current.ZoomMax);

            //  }


        }
    }
    
    void SeeIfMenuNeeded()
    {
        Tile t = GetTileUnderTouch();

        if (t != null)
        {
            if (t.Employees.Count > 0)
            {
                EmployeeSelected(t.Employees[0]);
            }
            else
            {
                EmployeeMenuActive = false;
            }
            if (t.Fixture != null)
            {
                if (t.Fixture.MaxShelfSpace > 0)
                {
                    OpenItemMenu();
                }
            }
        }
    }

    public Tile GetTileUnderTouch()
    {
        return World.Current.GetTileAtWorldCoord(currFramePosition);
    }

    void UpdateCameraMovement()
    {
        
        Vector3 diff = lastFramePosition - currFramePosition;
        diff = RestrictScreen(diff);
        Camera.main.transform.Translate(diff);

        
    }
    Vector3 RestrictScreen(Vector3 diff)
    {
        float MaxX = Numbers.Current.CameraXPositionMax - Camera.main.orthographicSize;
        float MinX = Numbers.Current.CameraXPositionMin + Camera.main.orthographicSize;

        if (Camera.main.transform.position.x + diff.x > MaxX)
        {
            diff.x = Camera.main.transform.position.x - MaxX;
            if (diff.x < 0) { diff.x = 0; }
        }
        else if (Camera.main.transform.position.x + diff.x < MinX)
        {
            diff.x = MinX - Camera.main.transform.position.x;
            if (diff.x > 0) { diff.x = 0; }
        }

        float MaxY = Numbers.Current.CameraYPositionMax - Camera.main.orthographicSize;
        float MinY = Numbers.Current.CameraYPositionMin + Camera.main.orthographicSize;

        if (Camera.main.transform.position.y + diff.y > MaxY)
        {
            diff.y = Camera.main.transform.position.y - MaxY;
            if (diff.y < 0) { diff.y = 0; }
        }
        else if (Camera.main.transform.position.y + diff.y < MinY)
        {
            diff.y = MinY - Camera.main.transform.position.y;
            if (diff.y > 0) { diff.y = 0; }
        }
        return diff;
    }
    void UpdateCameraZoom()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        Camera.main.orthographicSize += deltaMagnitudeDiff * Numbers.Current.ZoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, Numbers.Current.ZoomMin, Numbers.Current.ZoomMax);
    }

    void RemoveAllPreviews()
    {
        foreach (GameObject go in PreviewGameObjects)
        {
            Destroy(go);
        }
        PreviewGameObjects.Clear();
    }
    void ShowFixtureSpriteAtTile(string fixtureType, Tile t)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(this.transform, true);
        PreviewGameObjects.Add(go);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Fixture";
        sr.sprite = fsc.GetSpriteForFixture(fixtureType);

        if (World.Current.IsFixturePlacementValid(fixtureType, t))
        {
            sr.color = new Color(0f, 0.4f, 0f, 1f);
        }
        else
        {
            sr.color = new Color(0.6f, 0.1f, 0f, 1f);
        }

        Fixture proto = World.Current.FixturePrototypes[fixtureType];
        go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), t.Y + ((proto.Height - 1) / 2f), 0);
    }
    
    void ShowBlanceUpdating(double BalanceToDisplay)
    {
        if (World.Current.Balance != BalanceToDisplay)
        {
            BalanceDiff.SetActive(true);
            Balance.GetComponentInChildren<Text>().text = "£" +  BalanceToDisplay.ToString("N");
            double diff = (World.Current.Balance - BalanceToDisplay);

            BalanceDiff.GetComponent<Text>().text = diff.ToString("N");

            if (diff > 0)
            {
                BalanceDiff.GetComponent<Text>().text = " +£" + diff.ToString("N");
                BalanceDiff.GetComponent<Text>().color = new Color(0f, 0.4f, 0f, 1f);
            }
            else
            {
                BalanceDiff.GetComponent<Text>().text = " +£" + diff.ToString("N");
                BalanceDiff.GetComponent<Text>().color = new Color(0.6f, 0.1f, 0f, 1f);

            }
        }
        else
        {
            BalanceDiff.SetActive(false);
            Balance.GetComponentInChildren<Text>().text = "£" + World.Current.Balance.ToString("N");
        }

    }

    public void OnPauseButton()
    {
        World.Current.PauseUnpause();
        if (World.Current.paused)
        {
            Paused.SetActive(true);
        }
        else
        {
            Paused.SetActive(false);
        }
    }
    public void OnSpeedButton()
    {
        if (World.Current.WorldSpeed == 1)
        {
            World.Current.WorldSpeed = 2;
            SpeedButton.GetComponentInChildren<Text>().text = "2x";
        }
        else if (World.Current.WorldSpeed == 2)
        {
            World.Current.WorldSpeed = 3;
            SpeedButton.GetComponentInChildren<Text>().text = "3x";
        }
        else
        {
            World.Current.WorldSpeed = 1;
            SpeedButton.GetComponentInChildren<Text>().text = "1x";
        }
    }
    void OpenItemMenu()
    {
        if (!Ordering && !ReviewingPurchases && !ShopSettings)
        {
            ItemMenuing = true;
            Tile t = GetTileUnderTouch();
            Fixture f = t.Fixture;

            if (!World.Current.paused)
            {
                OnPauseButton();
            }

            CurrentItemCost = f.ItemOnShelf.Price;
            ConfirmPriceButton.SetActive(false);
            ItemMenu.SetActive(true);
            currentMode = MouseMode.ITEM;
            ItemName.GetComponent<Text>().text = f.ItemOnShelf.Name;
            ItemPicture.GetComponent<Image>().sprite = SpriteMap[f.ItemOnShelf.Name];
            OnShelfNumber.GetComponent<Text>().text = f.ItemOnShelf.ShopShelf.GetItemsOnShelf().Count.ToString();
            InStockNumber.GetComponent<Text>().text = f.ItemOnShelf.StockShelf.GetItemsOnShelf().Count.ToString();
            Price.GetComponent<Text>().text = "£" + f.ItemOnShelf.Price.ToString("N");
            PriceAdvice.GetComponent<Text>().text = "Current Order price for these is: £" +
                f.ItemOnShelf.OrderPrice.ToString("N") + ".";

            if (f.ItemOnShelf.NewPrice != 0)
            {
                IncreasePriceButton.GetComponent<Button>().interactable = false;
                DecreasePriceButton.GetComponent<Button>().interactable = false;
                string s = Price.GetComponent<Text>().text;
                s += "*";
                Price.GetComponent<Text>().text = s;
            }
            else
            {

                IncreasePriceButton.GetComponent<Button>().interactable = true;
                DecreasePriceButton.GetComponent<Button>().interactable = true;
            }

            if (f.ItemOnShelf.DeliveryPending)
            {
                OrderMoreButton.GetComponent<Button>().interactable = false;
                OrderMoreButton.GetComponentInChildren<Text>().text = "Items ordered";
            }
            else
            {
                OrderMoreButton.GetComponent<Button>().interactable = true;
                OrderMoreButton.GetComponentInChildren<Text>().text = "Order more";

            }
            

            if (f.ItemOnShelf.PrevPrice > 0)
            {
                FeedbackPrevPrice.GetComponent<Text>().text = "£" + f.ItemOnShelf.PrevPrice.ToString("N");
                PrevItemBoughtTwo.GetComponent<Text>().text = f.ItemOnShelf.PrevBoughtTwo.ToString();
                PrevItemHappy.GetComponent<Text>().text = f.ItemOnShelf.PrevHappy.ToString();
                PrevItemNeutral.GetComponent<Text>().text = f.ItemOnShelf.PrevNeutral.ToString();
                PrevItemTooExpensive.GetComponent<Text>().text = f.ItemOnShelf.PrevNotBuying.ToString();
                PrevItemWouldaBoughtTwo.GetComponent<Text>().text = f.ItemOnShelf.PrevWouldaBoughtTwo.ToString();
                PrevItemOuttaStock.GetComponent<Text>().text = f.ItemOnShelf.PrevOuttaStock.ToString();
            }
            else
            {

                FeedbackPrevPrice.GetComponent<Text>().text = "N/A";
                PrevItemBoughtTwo.GetComponent<Text>().text = "N/A";
                PrevItemHappy.GetComponent<Text>().text = "N/A";
                PrevItemNeutral.GetComponent<Text>().text = "N/A";
                PrevItemTooExpensive.GetComponent<Text>().text = "N/A";
                PrevItemWouldaBoughtTwo.GetComponent<Text>().text = "N/A";
                PrevItemOuttaStock.GetComponent<Text>().text = "N/A";
            }
            FeedbackPrice.GetComponent<Text>().text = "£" + f.ItemOnShelf.Price.ToString("N");
            ItemBoughtTwo.GetComponent<Text>().text = f.ItemOnShelf.BoughtTwo.ToString();
            ItemHappy.GetComponent<Text>().text = f.ItemOnShelf.Happy.ToString();
            ItemNeutral.GetComponent<Text>().text = f.ItemOnShelf.Neutral.ToString(); ;
            ItemTooExpensive.GetComponent<Text>().text = f.ItemOnShelf.NotBuying.ToString();
            ItemWouldaBoughtTwo.GetComponent<Text>().text = f.ItemOnShelf.WouldaBoughtTwo.ToString();
            ItemOuttaStock.GetComponent<Text>().text = f.ItemOnShelf.OuttaStock.ToString();

        }
    }

    public void OnCloseItemMenu()
    {
        if (!Ordering)
        {
            ItemMenuing = false;
            Ordering = false;
            currentMode = MouseMode.SELECT;
            ItemMenu.SetActive(false);
            TimeToWait = 0.1f;
            if (World.Current.paused)
            {
                OnPauseButton();
            }
        }
    }
    public void OnCloseConfOrderMenu()
    {
        OrderConf.SetActive(false);
        ItemMenuing = true;
        Ordering = false;
    }


    //Used when user presses Order More from Item Menu
    public void OnOrderMore()
    {
        ItemMenuing = false;
        Ordering = true;
        Tile t = GetTileUnderTouch();
        Fixture f = t.Fixture;
        OrderConf.SetActive(true);
        ItemNameConf.GetComponent<Text>().text = f.ItemOnShelf.Name;
        ItemPicConf.GetComponent<Image>().sprite = SpriteMap[f.ItemOnShelf.Name];
        UpdateOrderConfText(t, f);

    }

    void UpdateOrderConfText(Tile t, Fixture f)
    {

        OrderCapacity = f.ItemOnShelf.StockShelf.MaxShelfSpace - f.ItemOnShelf.StockShelf.GetItemsOnShelf().Count;
        OrderCost = f.ItemOnShelf.OrderPrice * OrderCapacity;
        if (OrderCapacity > 0)
        {
            OrderConfButton.GetComponent<Button>().interactable = true;
            OrderConfText.GetComponent<Text>().text = "I can restock you for £" + (OrderCost).ToString("N") + "." +
                "\nThat's " + OrderCapacity + " items at £" + f.ItemOnShelf.OrderPrice.ToString("N") + " each.";
        }
        else
        {
            OrderConfButton.GetComponent<Button>().interactable = false;
            OrderConfText.GetComponent<Text>().text = "You've got no space in the Stock Room for this";
        }
        if (OrderCost > World.Current.Balance)
        {
            OrderConfButton.GetComponent<Button>().interactable = false;
            OrderConfButton.GetComponent<Text>().text = "Too expensive";

        }
        if (World.Current.DeliveryTime > 0)
        {
            OrderConfButton.GetComponent<Button>().interactable = false;
            OrderConfText.GetComponent<Text>().text = "I'm still delivering your last request!";
        }
    }
    //Used when user presses Confirm Order from Confirm Menu
    public void OnConfirmOrder()
    {
        GetTileUnderTouch().Fixture.ItemOnShelf.OrderMore(OrderCapacity, OrderCost, OnItemDelivered);
        OrderMoreButton.GetComponent<Button>().interactable = false;
        OrderMoreButton.GetComponentInChildren<Text>().text = "Items ordered";
        OnCloseConfOrderMenu();
        

    }

    void OnItemDelivered(Item i)
    {
        Ordering = true;
        InfoMenu.SetActive(true);
        OpenInfoBox(i.Name, SpriteMap[i.Name], "Your item has been delivered to its shelf in the Stock Room!");

    }

    void OpenInfoBox(string TitleText, Sprite TitleImage, string infText)
    {
        Ordering = true;
        InfoMenu.SetActive(true);

        InfoItemName.GetComponent<Text>().text = TitleText;
        InfoItemPic.GetComponent<Image>().sprite = TitleImage;
        InfoText.GetComponent<Text>().text = infText;
    }
    public void CloseInfoBox()
    {
        Ordering = false;
        InfoMenu.SetActive(false);
    }

    public void OpenShopMenu()
    {
        ShopSettings = true;
        ShopMenu.SetActive(true);
        OpenTime.GetComponent<Text>().text = World.Current.Today.OpeningTime.ToShortTimeString();
        CloseTime.GetComponent<Text>().text = World.Current.Today.ClosingTime.ToShortTimeString();
    }

    public void CloseShopMenu()
    {
        ShopSettings = false;
        ShopMenu.SetActive(false);
    }
    
    public void OnIncreasePrice()
    {
        if (!Ordering)
        {
            string s = Price.GetComponent<Text>().text;
            s = s.Substring(1); 
            NewItemCost = double.Parse(s);
            NewItemCost += 0.05;
            Price.GetComponent<Text>().text = "£" + NewItemCost.ToString("N");
            if (NewItemCost != CurrentItemCost)
            {
                ConfirmPriceButton.SetActive(true);

            }
            else
            {
                ConfirmPriceButton.SetActive(false);
            }

        }
    }

    public void OnDecreasePrice()
    {
        if (!Ordering)
        {
            string s = Price.GetComponent<Text>().text;
            s = s.Substring(1);
            NewItemCost = double.Parse(s);
            if (NewItemCost > 0.05)
            {
                DecreasePriceButton.GetComponent<Button>().interactable = true;
                NewItemCost -= 0.05;

                Price.GetComponent<Text>().text = "£" + NewItemCost.ToString("N");
                if (NewItemCost != CurrentItemCost)
                {
                    ConfirmPriceButton.SetActive(true);

                }
                else
                {
                    ConfirmPriceButton.SetActive(false);
                }
            }
            else
            {
                DecreasePriceButton.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void ConfirmPriceChange()
    {
        if (!Ordering)
        {
            Tile t = GetTileUnderTouch();
            Fixture f = t.Fixture;
            f.ItemOnShelf.NewPrice = NewItemCost;
            World.Current.CostChangeQueue.Enqueue(new Job(
                t,
                Words.Current.CostChangeQueue,
                Words.Current.CostChangeQueue,
                null,
                Numbers.Current.CostChangeTime));
            Price.GetComponent<Text>().text = "£" + NewItemCost.ToString("N") + "*";
            IncreasePriceButton.GetComponent<Button>().interactable = false;
            DecreasePriceButton.GetComponent<Button>().interactable = false;
            ConfirmPriceButton.SetActive(false);
        }
    }

    void LoadSprites()
    {

        SpriteMap = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>(Words.Current.ItemSpriteFolder);

        foreach (Sprite s in sprites)
        {
            SpriteMap[s.name] = s;
        }
        
    }
    public void CloseTransactionsMenu()
    {

        foreach (Transform child in ContentPanel)
        {
            Destroy(child.gameObject);
        }
        ReviewingPurchases = false;
        TransactionMenu.SetActive(false);
    }
    public void OpenTransactions()
    {
        ReviewingPurchases = true;
        TransactionMenu.SetActive(true);
        for (int i = (World.Current.Transactions.Count - 1); i >= 0; i--)
        {
            Transaction t = World.Current.Transactions[i];
            newTransaction = Instantiate(TransactionDisplayPrefab) as GameObject;
            newTransaction.name = "Transaction_" + t.DateTimePurchase;
            controller = newTransaction.GetComponent<TransactionDisplayController>();
            controller.Date.text = t.DateTimePurchase.ToShortDateString() + "\n" + t.DateTimePurchase.ToShortTimeString();
            controller.Cost.text = "£" + t.Cost.ToString("N");
            if (t.Type == Words.Current.CustomerPurchase)
            {
                string transDetails = "";
                string costDetails = "";

                foreach (Purchase p in t.ItemsBought)
                {
                    controller.Cost.color = new Color(0f, 0.4f, 0f, 1f);
                    transDetails += p.i.Name + "\n";
                    costDetails += "£" + p.Cost.ToString("N") + "\n";
                }
                controller.TransactionDetails.text = transDetails;
                controller.CostDetails.text = costDetails;
            }
            else if (t.Type == Words.Current.ShopOrder)
            {
                controller.Cost.color = new Color(0.6f, 0.1f, 0f, 1f);
                controller.TransactionDetails.text = "Order made for "
                    + t.ItemsBought.Count + " x " + t.ItemsBought[0].i.Name;
                controller.CostDetails.text = "£" + t.Cost.ToString("N");
            }
            newTransaction.transform.SetParent(ContentPanel.transform, false);
            //newTransaction.transform.localScale = Vector3.one;
        }
    }

    public void OpenEmployeeHours()
    {
        ShopSettings = true;
        OpeningHoursCloeButton.SetActive(true);
        if (!World.Current.paused)
        {
            OnPauseButton();
        }
        OpeningHoursMenu.SetActive(true);
        OpeningHoursConfirm.GetComponent<Button>().interactable = false;
        DateTime tomorrow = WorldTime.Current.Date.AddDays(1);
        OpeningTimeOpenTime.GetComponent<Text>().text = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, TempOpeningTime, 0, 0).ToShortTimeString();
        OpeningTimeCloseTime.GetComponent<Text>().text = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, TempClosingTime, 0, 0).ToShortTimeString();
        foreach (Employee e in World.Current.Employees)
        {
            if (!EmployeeOpeningHoursGOMap.ContainsKey(e.EmployeeID))
            {
                GameObject newEmployee = Instantiate(EmployeeHoursPrefab) as GameObject;
                newEmployee.name = "Employee_" + e.Name;
                employeeController = newEmployee.GetComponent<EmployeeHoursDisplayController>();
                employeeController.EmployeeName.text = e.Name;
                employeeController.StartTime.text = e.GetTodaysStartTime(World.Current.Today).ToShortTimeString();
                employeeController.EndTime.text = e.GetTodaysEndTime(World.Current.Today).ToShortTimeString();
                employeeController.Wage.text = "£" + e.Wage.ToString("N");
                employeeController.PrefHours.text = e.PrefHours.ToString();
                employeeController.DecreaseStartTime.onClick.AddListener(() => { DecreaseEmployeeStartTime(e); });
                employeeController.IncreaseStartTime.onClick.AddListener(() => { IncreaseEmployeeStartTime(e); });
                employeeController.DecreaseEndTime.onClick.AddListener(() => { DecreaseEmployeeEndTime(e); });
                employeeController.IncreaseEndTime.onClick.AddListener(() => { IncreaseEmployeeEndTime(e); });
                newEmployee.transform.SetParent(OpeningHoursContentPanel.transform, false);
                employeeController.WorkHoursBar.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)e.StartingHour * 100f, 0f, 0f);
                employeeController.WorkHoursBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (e.EndHour - e.StartingHour) * 100);
                ChangeEmployeeWorkingBarColor(employeeController, e);
                EmployeeOpeningHoursGOMap.Add(e.EmployeeID, newEmployee);
            }
        }
        CheckRedHours();
    }
    public void OpenEmployeeMenu()
    {
        ItemMenuing = true;
        if (!World.Current.paused)
        {
            OnPauseButton();
        }
        EmployeeMenu.SetActive(true);
        foreach (Transform child in EmployeeMenuContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Employee e in World.Current.Employees)
        {
            GameObject newEmployee = Instantiate(EmployeeSettingsPrefab) as GameObject;
            newEmployee.name = "Employee_" + e.Name;
            employeeSettingsController = newEmployee.GetComponent<EmployeeSettingsDisplayController>();
            employeeSettingsController.EmployeeName.text = e.Name;
            employeeSettingsController.WorkingHours.text = "Works " +
                World.Current.Today.EmployeesStartingTimes[e].ToShortTimeString() +
                " til " +
                World.Current.Today.EmployeeEndTimes[e].ToShortTimeString();
            employeeSettingsController.Wage.text = "£" + e.Wage.ToString("N") + " p/h";

            employeeSettingsController.Responsibilities.onClick.AddListener(() => { OpenEmployeeResponsibilitesMenu(e); });
            newEmployee.transform.SetParent(EmployeeMenuContentPanel.transform, false);
            if (e.currJob != null)
            {
                employeeSettingsController.CurrentJob.text = Words.Current.GetEmployeeCurrJobDisplay(e.currJob.jobQueue);
            }
            else
            {
                if (!e.OnShift)
                {
                    employeeSettingsController.CurrentJob.text = "Not working";

                }
                else
                {
                    employeeSettingsController.CurrentJob.text = Words.Current.GetEmployeeCurrJobDisplay("");
                }
            }
        }
    }

    public void OpenEmployeeResponsibilitesMenu(Employee e)
    {
        EmployeeMenuActive = false;
        foreach (Transform child in ResponsibilitesContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in JobQueueNotIncludedContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        EmployeeResponsibiliiesMenu.SetActive(true);
        ResponsibilitiesHeader.GetComponent<Text>().text = e.Name + "'s Responsibilities";
        ResponsibilitiesWage.GetComponent<Text>().text = e.Wage.ToString("N");
        ResponsibilityStart.GetComponent<Text>().text = World.Current.Today.EmployeesStartingTimes[e].ToShortTimeString();
        ResponsibilityEnd.GetComponent<Text>().text = World.Current.Today.EmployeeEndTimes[e].ToShortTimeString();
        ResponsibilityFlex.GetComponent<Text>().text = e.Flexibility.ToString();
        ResponsibilityReliability.GetComponent<Text>().text = e.Reliabilitiy.ToString();
        ResponsibilityCheckout.GetComponent<Text>().text = e.CheckoutAbility.ToString();
        ResponsibilityRestock.GetComponent<Text>().text = e.RestockAbility.ToString();
        CloseResponsibilityButton.GetComponent<Button>().onClick.AddListener(() => { CloseEmployeeResponsibilitesMenu(e); });
        ConfirmResponsibilityButton.GetComponent<Button>().onClick.AddListener(() => { ConfirmEmployeeResponsiiblities(e); });

        for (int i = 0; i < e.TempPriorityList.Count; i++)
        {
            String s = e.TempPriorityList[i];
            
            GameObject newJobQueue = Instantiate(JobQueuePrefab) as GameObject;
            newJobQueue.name = "JobQueue_" + s;
            jobQueueController = newJobQueue.GetComponent<JobQueueDisplayController>();
            jobQueueController.JobQueueName.text = s;
            jobQueueController.RemoveResponsibility.onClick.AddListener(() => { RemoveResponsibility(s,e); });
            jobQueueController.IncreasePriority.onClick.AddListener(() => { IncreaseResponsibilityPriority(s,e); });
            jobQueueController.DecreasePriority.onClick.AddListener(() => { DecreaseResponsibilityPriority(s,e); });
            newJobQueue.transform.SetParent(ResponsibilitesContentPanel.transform, false);

            if (i == 0)
            {
                //Top of the last, can't increase priority
                jobQueueController.IncreasePriority.interactable = false;
            }
            if (i == e.TempPriorityList.Count - 1)
            {
                //At the end of the list, can't reduce priority
                jobQueueController.DecreasePriority.interactable = false;
            }
            if (e.TempPriorityList.Count == 1)
            {
                //If only 1, you can't remove it as can't be left with no jobs
                jobQueueController.RemoveResponsibility.interactable = false;
            }

        }

        foreach (String s in Words.Current.GetFullPriorityList())
        {
            if (!e.TempPriorityList.Contains(s))
            {
                GameObject newJobQueueNotIncluded = Instantiate(JobQueueNotIncludedPrefab) as GameObject;
                newJobQueueNotIncluded.name = "JobQueue_" + s;
                jobQueueNotIncludedController = newJobQueueNotIncluded.GetComponent<JobQueueNotIncludedDisplayController>();
                jobQueueNotIncludedController.JobQueueName.text = s;
                jobQueueNotIncludedController.Add.onClick.AddListener(() => { AddResponsibility(s, e); });
                newJobQueueNotIncluded.transform.SetParent(JobQueueNotIncludedContentPanel.transform, false);
            }
        }
    }
    public void AddResponsibility(string jobToAdd, Employee e)
    {
        e.TempPriorityList.Add(jobToAdd);
        OpenEmployeeResponsibilitesMenu(e);
    }
    public void RemoveResponsibility(String jobToRemove, Employee e)
    {
        if (e.TempPriorityList.Contains(jobToRemove))
        {
            e.TempPriorityList.Remove(jobToRemove);
            OpenEmployeeResponsibilitesMenu(e);
        }
    }
    public void IncreaseResponsibilityPriority(String s, Employee e)
    {
        int currPosition = e.TempPriorityList.IndexOf(s);
        int newPosition = currPosition - 1;
        string queueReplacing = e.TempPriorityList[newPosition];

        e.TempPriorityList.RemoveAt(currPosition);
        e.TempPriorityList.RemoveAt(newPosition);

        e.TempPriorityList.Insert(newPosition, s);
        e.TempPriorityList.Insert(currPosition, queueReplacing);
        OpenEmployeeResponsibilitesMenu(e);
    }
    public void DecreaseResponsibilityPriority(String s, Employee e)
    {
        int currPosition = e.TempPriorityList.IndexOf(s);
        int newPosition = currPosition + 1;
        string queueReplacing = e.TempPriorityList[newPosition];

        e.TempPriorityList.RemoveAt(currPosition);
        e.TempPriorityList.RemoveAt(newPosition);

        e.TempPriorityList.Insert(newPosition, s);
        e.TempPriorityList.Insert(currPosition, queueReplacing);
        OpenEmployeeResponsibilitesMenu(e);

    }
    
    public void CloseEmployeeResponsibilitesMenu(Employee e)
    {
        e.TempPriorityList = e.PriorityList;
        EmployeeResponsibiliiesMenu.SetActive(false);
    }
    public void ConfirmEmployeeResponsiiblities(Employee e)
    {
        e.PriorityList = new List<string>(e.TempPriorityList);
        CloseEmployeeResponsibilitesMenu(e);
    }
    public void CloseEmployeeMenu()
    {

        if (World.Current.paused)
        {
            OnPauseButton();
        }
        ItemMenuing = false;
        EmployeeMenu.SetActive(false);
    }
    public void CloseEmployeeHours()
    {
        ShopSettings = false;
        OpeningHoursMenu.SetActive(false);
        TempOpeningTime = Numbers.Current.OpeningHour;
        TempClosingTime = Numbers.Current.ClosingHour;
        if (World.Current.paused)
        {
            OnPauseButton();
        }

    }
    public void IncreaseOpeningTime()
    {
        if (TempClosingTime - TempOpeningTime <= 1)
        {
            //If we increase the opening time another hour shop will never be open
        }
        else
        {
            TempOpeningTime += 1;
            DateTime today = WorldTime.Current.Date;
            OpeningTimeOpenTime.GetComponent<Text>().text = new DateTime(today.Year,
                today.Month,
                today.Day,
                TempOpeningTime,
                0,
                0).ToShortTimeString();
            SetShopBarSizeAndPosition();
        }

        CheckRedHours();
    }
    public void DecreaseOpeningTime()
    {
        if (TempOpeningTime == 0)
        {

        }
        else
        {
            TempOpeningTime -= 1;
            DateTime today = WorldTime.Current.Date;
            OpeningTimeOpenTime.GetComponent<Text>().text = new DateTime(today.Year,
                today.Month,
                today.Day,
                TempOpeningTime,
                0,
                0).ToShortTimeString();
            SetShopBarSizeAndPosition();
        }
        CheckRedHours();
    }
    public void IncreaseClosingTime()
    {
        if (TempClosingTime == 23)
        {
            //Increasing 1 more hour will open until midnight, will display 23:59 for ease
            TempClosingTime += 1;
            DateTime today = WorldTime.Current.Date;
            OpeningTimeCloseTime.GetComponent<Text>().text = new DateTime(today.Year,
                today.Month,
                today.Day,
                23,
                59,
                0).ToShortTimeString();
            SetShopBarSizeAndPosition();
        }
        else if (TempClosingTime == 24)
        {

        }
        else
        {
            TempClosingTime += 1;
            DateTime today = WorldTime.Current.Date;
            OpeningTimeCloseTime.GetComponent<Text>().text = new DateTime(today.Year,
                today.Month,
                today.Day,
                TempClosingTime,
                0,
                0).ToShortTimeString();
            SetShopBarSizeAndPosition();
        }
        CheckRedHours();
    }
    public void DecreaseClosingTime()
    {
        if (TempClosingTime - TempOpeningTime <= 1)
        {
            //If we decrease the closing time another hour shop will never be open
        }
        else
        {
            TempClosingTime -= 1;
            DateTime today = WorldTime.Current.Date;
            OpeningTimeCloseTime.GetComponent<Text>().text = new DateTime(today.Year,
                today.Month,
                today.Day,
                TempClosingTime,
                0,
                0).ToShortTimeString();
            SetShopBarSizeAndPosition();
        }
        CheckRedHours();
    }
    void SetShopBarSizeAndPosition()
    {
        ShopBar.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)TempOpeningTime * 100f, -100f, 0f);
        ShopBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (TempClosingTime - TempOpeningTime) * 100);
    }

    public void EmployeeSelected(Employee e)
    {

        EmployeeMenuActive = true;
        SingleEmployeeMenu.SetActive(true);
        SingleEmployeeMenu.name = "Employee_" + e.Name;
        employeeSettingsController = SingleEmployeeMenu.GetComponent<EmployeeSettingsDisplayController>();
        employeeSettingsController.EmployeeName.text = e.Name;
        employeeSettingsController.WorkingHours.text = "Works " +
            World.Current.Today.EmployeesStartingTimes[e].ToShortTimeString() +
            " til " +
            World.Current.Today.EmployeeEndTimes[e].ToShortTimeString();
        employeeSettingsController.Wage.text = "£" + e.Wage.ToString("N") + " p/h";

        String resp = "";
        foreach (String s in e.PriorityList)
        {
            resp += s + ", ";
        }
        employeeSettingsController.ResponsibilitieText.text = resp;
        employeeSettingsController.Responsibilities.onClick.AddListener(() => { OpenEmployeeResponsibilitesMenu(e); });
        SingleEmployeeMenu.transform.SetParent(Canvas.transform, false);
        if (e.currJob != null)
        {
            employeeSettingsController.CurrentJob.text = Words.Current.GetEmployeeCurrJobDisplay(e.currJob.jobQueue);
        }
        else
        {
            if (!e.OnShift)
            {
                employeeSettingsController.CurrentJob.text = "Not working";

            }
            else
            {
                employeeSettingsController.CurrentJob.text = Words.Current.GetEmployeeCurrJobDisplay("");
            }
        }
    }
    public void CloseEmployeeSelected()
    {
        EmployeeMenuActive = false;
        SingleEmployeeMenu.SetActive(false);
    }

    void DecreaseEmployeeStartTime(Employee e)
    {
        if (e.TempStartHour == 0)
        {

        }
        else
        { 
            GameObject employeeGO;
            if (!EmployeeOpeningHoursGOMap.ContainsKey(e.EmployeeID))
            {
                return;
            }
            employeeGO = EmployeeOpeningHoursGOMap[e.EmployeeID];
            employeeController = employeeGO.GetComponent<EmployeeHoursDisplayController>();
            e.TempStartHour -= 1;
            DateTime today = WorldTime.Current.Date;
            employeeController.StartTime.text = new DateTime(today.Year,
                today.Month,
                today.Day,
                e.TempStartHour,
                0,
                0).ToShortTimeString();
            employeeController.WorkHoursBar.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)e.TempStartHour * 100f, 0f, 0f);

            if (e.TempEndHour - e.TempStartHour > Numbers.Current.MaxNumOfHours)
            {
                //Employee can't work more than set number of hours.
                e.TempEndHour -= 1;
                employeeController.EndTime.text = new DateTime(today.Year,
                    today.Month,
                    today.Day,
                    e.TempEndHour,
                    0,
                    0).ToShortTimeString();
            }


            employeeController.WorkHoursBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (e.TempEndHour - e.TempStartHour) * 100);
            ChangeEmployeeWorkingBarColor(employeeController, e);


            CheckRedHours();
        }
    }
    void IncreaseEmployeeStartTime(Employee e)
    {
        if (e.TempEndHour - e.TempStartHour <= 1)
        {
            //If we get any closer to employee's end time they won't be working
        }
        else
        {
            GameObject employeeGO;
            if (!EmployeeOpeningHoursGOMap.ContainsKey(e.EmployeeID))
            {
                return;
            }
            employeeGO = EmployeeOpeningHoursGOMap[e.EmployeeID];
            employeeController = employeeGO.GetComponent<EmployeeHoursDisplayController>();
            e.TempStartHour += 1;
            DateTime today = WorldTime.Current.Date;
            employeeController.StartTime.text = new DateTime(today.Year,
                today.Month,
                today.Day,
                e.TempStartHour,
                0,
                0).ToShortTimeString();
            employeeController.WorkHoursBar.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)e.TempStartHour * 100f, 0f, 0f);
            employeeController.WorkHoursBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (e.TempEndHour - e.TempStartHour) * 100);
            ChangeEmployeeWorkingBarColor(employeeController, e);

            CheckRedHours();
        }
    }
    void DecreaseEmployeeEndTime(Employee e)
    {

        if (e.TempEndHour - e.TempStartHour <= 1)
        {
            //If we get any closer to employee's end time they won't be working
        }
        else
        {

            GameObject employeeGO;
            if (!EmployeeOpeningHoursGOMap.ContainsKey(e.EmployeeID))
            {
                return;
            }
            employeeGO = EmployeeOpeningHoursGOMap[e.EmployeeID];
            employeeController = employeeGO.GetComponent<EmployeeHoursDisplayController>();
            e.TempEndHour -= 1;
            DateTime today = WorldTime.Current.Date;
            employeeController.EndTime.text = new DateTime(today.Year,
                today.Month,
                today.Day,
                e.TempEndHour,
                0,
                0).ToShortTimeString();
            employeeController.WorkHoursBar.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)e.TempStartHour * 100f, 0f, 0f);
            employeeController.WorkHoursBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (e.TempEndHour - e.TempStartHour) * 100);
            ChangeEmployeeWorkingBarColor(employeeController, e);

            CheckRedHours();
        }
    }
    void IncreaseEmployeeEndTime(Employee e)
    {

        if (e.TempEndHour == 24) 
        {

        }
        else
        {
            GameObject employeeGO;
            if (!EmployeeOpeningHoursGOMap.ContainsKey(e.EmployeeID))
            {
                return;
            }
            employeeGO = EmployeeOpeningHoursGOMap[e.EmployeeID];
            employeeController = employeeGO.GetComponent<EmployeeHoursDisplayController>();
            DateTime today = WorldTime.Current.Date;
            if (e.TempEndHour == 23)
            {
                //Increasing 1 more hour will open until midnight, will display 23:59 for ease
                e.TempEndHour += 1;
                employeeController.EndTime.text = new DateTime(today.Year,
                    today.Month,
                    today.Day,
                    23,
                    59,
                    0).ToShortTimeString();
            }
            else
            {
                e.TempEndHour += 1;
                employeeController.EndTime.text = new DateTime(today.Year,
                    today.Month,
                    today.Day,
                    e.TempEndHour,
                    0,
                    0).ToShortTimeString();
            }

            if (e.TempEndHour - e.TempStartHour > Numbers.Current.MaxNumOfHours)
            {
                //Employee can't work more than set number of hours.
                e.TempStartHour += 1;
                employeeController.StartTime.text = new DateTime(today.Year,
                    today.Month,
                    today.Day,
                    e.TempStartHour,
                    0,
                    0).ToShortTimeString();
            }

            employeeController.WorkHoursBar.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)e.TempStartHour * 100f, 0f, 0f);
            employeeController.WorkHoursBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (e.TempEndHour - e.TempStartHour) * 100);
            ChangeEmployeeWorkingBarColor(employeeController, e);
            CheckRedHours();
        }

    }
    void ChangeEmployeeWorkingBarColor(EmployeeHoursDisplayController controller, Employee e)
    {
        if (e.TempEndHour - e.TempStartHour != e.PrefHours)
        {
            controller.WorkHoursBar.GetComponent<Image>().color = new Color(0.8f, 0.5f, 0.1f, 1f);
            controller.PrefHours.GetComponent<Text>().text = e.PrefHours.ToString() + "!";
        }
        else
        {
            controller.WorkHoursBar.GetComponent<Image>().color = new Color(0.5f, 0.9f, 0.2f, 1f);
            controller.PrefHours.GetComponent<Text>().text = e.PrefHours.ToString();
        }

    }

    void CheckRedHours()
    {
        foreach (GameObject go in RedHours)
        {
            Destroy(go);
        }
        //A Red Hour is either:
        //An hour that the shop is open but nobody is employed OR
        //An hour that an employee is working but the shop is not open
        List<int> ShopHoursNotCovered = new List<int>();
        for (int i = TempOpeningTime; i < TempClosingTime; i++)
        {
            ShopHoursNotCovered.Add(i);
        }

        foreach (Employee e in World.Current.Employees)
        {
            for (int i = e.TempStartHour; i < e.TempEndHour; i++)
            {
                if (ShopHoursNotCovered.Contains(i))
                {
                    ShopHoursNotCovered.Remove(i);
                }
            }
        }


        foreach (int i in ShopHoursNotCovered)
        {
            GameObject newRedHour = Instantiate(RedHourPrefab) as GameObject;
            newRedHour.name = "RedHour_" + i;
            newRedHour.transform.SetParent(OpeningHoursEmployeeMenu.transform);
            newRedHour.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)i * 100f, 0f, 0f);
            newRedHour.transform.localScale = new Vector3(1, 1, 1);
            RedHours.Add(newRedHour);
        }

        if (ShopHoursNotCovered.Count == 0)
        {
            OpeningHoursConfirm.GetComponent<Button>().interactable = true;
        }
        else
        {
            OpeningHoursConfirm.GetComponent<Button>().interactable = false;
        }

    }
    void ConfirmOpeningHours()
    {
        
        Numbers.Current.OpeningHour = TempOpeningTime;
        Numbers.Current.ClosingHour = TempClosingTime;
        //Numbers.Current.UpdateOpeningTime(tomorrow.Year, tomorrow.Month, tomorrow.Day);
        //Numbers.Current.UpdateClosingTime(tomorrow.Year, tomorrow.Month, tomorrow.Day);

        foreach (WorkingDay day in World.Current.Next30Days)
        {
            day.OpeningTime = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, TempOpeningTime, 0, 0);
            day.ClosingTime = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, TempClosingTime, 0, 0);

            foreach (Employee e in World.Current.Employees)
            {
                day.EmployeesStartingTimes[e] = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, e.TempStartHour, 0, 0);
                day.EmployeeEndTimes[e] = new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, e.TempEndHour, 0, 0);
            }
        }

        foreach (Employee e in World.Current.Employees)
        {
            e.UpdateWorkingHours();

        }
        CloseEmployeeHours();
    }

    public void OpeningHoursHireButton()
    {
        OpenApplicantsMenu();
    }
    void OpenApplicantsMenu()
    {
        foreach (Transform child in ApplicantSmallViewPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        ApplicantsMenu.SetActive(true);
        if (World.Current.Applicants.Count < 5)
        {
            World.Current.AddApplicants();
        }

        foreach (Employee a in World.Current.Applicants)
        {
            GameObject newApplicant = Instantiate(ApplicantSmallViewPrefab) as GameObject;
            newApplicant.name = "Applicant_" + a.Name;
            applicantController = newApplicant.GetComponent<ApplicantSmallViewDisplayController>();
            applicantController.Name.text = a.Name;
            applicantController.Age.text = a.Age.ToString();
            applicantController.PrefHours.text = "Wants to work " + a.PrefHours + " hours a day";
            newApplicant.transform.SetParent(ApplicantSmallViewPanel.transform, false);
            newApplicant.GetComponent<Button>().onClick.AddListener(() => { UpdateApplicantBigDetails(a); });
        }
        UpdateApplicantBigDetails(World.Current.Applicants[0]);

    }

    void UpdateApplicantBigDetails(Employee e)
    {
        ApplicantName.GetComponent<Text>().text = e.Name;
        ApplicantDOB.GetComponent<Text>().text = e.DOB.ToString("dd/MM/yyyy") + " (" + e.Age.ToString() + ")";
        ApplicantPrefHours.GetComponent<Text>().text = e.PrefHours.ToString() + " hours a day";
        ApplicantYearsExperience.GetComponent<Text>().text = e.Experience.ToString();
        ApplicantFlexibility.GetComponent<Text>().text = e.Flexibility.ToString();
        ApplicantReliability.GetComponent<Text>().text = e.Reliabilitiy.ToString();
        ApplicantCheckoutAbility.GetComponent<Text>().text = e.CheckoutAbility.ToString();
        ApplicantRestockAbility.GetComponent<Text>().text = e.RestockAbility.ToString();
        applicantSelect = e;
    }

    public void CloseApplicantDetails()
    {
        ApplicantsMenu.SetActive(false);
    }

    public void ConfirmHireApplicant()
    {

        World.Current.Applicants.Remove(applicantSelect);
        applicantSelect.StartingHour = TempOpeningTime;
        applicantSelect.EndHour = TempOpeningTime + applicantSelect.PrefHours;
        applicantSelect.TempStartHour = applicantSelect.StartingHour;
        applicantSelect.TempEndHour = applicantSelect.EndHour;
        //applicantSelect.SetHours();
        applicantSelect.AddEmployeeCalledInLateCallback(OpenEmployeeLateMenu);
        applicantSelect.AddEmployeeSelectedCallback(EmployeeSelected);
        World.Current.Employees.Add(applicantSelect);
        DateTime today = World.Current.Today.Date;
        World.Current.Today.EmployeesStartingTimes.Add(applicantSelect, new DateTime(today.Year, today.Month, today.Day, applicantSelect.TempStartHour, 0, 0));
        World.Current.Today.EmployeeEndTimes.Add(applicantSelect, new DateTime(today.Year, today.Month, today.Day, applicantSelect.TempEndHour, 0, 0));

        CloseApplicantDetails();
        OpenEmployeeHours();
    }
    void ConfirmOpeningHoursFirstTime()
    {
        Numbers.Current.OpeningHour = TempOpeningTime;
        Numbers.Current.ClosingHour = TempClosingTime;
        OpeningHoursConfirm.GetComponent<Button>().onClick.RemoveAllListeners();
        OpeningHoursConfirm.GetComponent<Button>().onClick.AddListener(() => { ConfirmOpeningHours(); });
        //ConfirmOpeningHours();
        for (int i = 0; i < 30; i++)
        {
            DateTime Date = WorldTime.Current.Date.AddDays(i);
            Dictionary<Employee, DateTime> startTimes = new Dictionary<Employee, DateTime>();
            Dictionary<Employee, DateTime> endTimes = new Dictionary<Employee, DateTime>();

            foreach (Employee e in World.Current.Employees)
            {
                startTimes.Add(e, new DateTime(Date.Year, Date.Month, Date.Day, e.TempStartHour, 0, 0));
                endTimes.Add(e, new DateTime(Date.Year, Date.Month, Date.Day, e.TempEndHour, 0, 0));
            }
            DateTime openTime = new DateTime(Date.Year, Date.Month, Date.Day, Numbers.Current.OpeningHour, 0, 0);
            DateTime closeTime = new DateTime(Date.Year, Date.Month, Date.Day, Numbers.Current.ClosingHour, 0, 0);
            WorkingDay day = new WorkingDay(Date, openTime, closeTime, startTimes, endTimes);
            World.Current.Next30Days.Add(day);
        }
        World.Current.StartNextDay();
        CloseEmployeeHours();
    }

    public void OpenEODMenu()
    {
        EODMenu.SetActive(true);
        if (!World.Current.paused)
        {
            OnPauseButton();
        }
        ItemMenuing = true;

        String stats = "";
        int CustServed = 0;
        double CustMoneyMade = 0; 
        int OrdersMade = 0; 
        double OrdersSpent = 0; 
        int TotBought2 = 0; 
        int TotHappy = 0; 
        int TotNeutral = 0; 
        int TotExpensive = 0; 
        int TotWouldaBought2 = 0; 
        int TotWouldaBought1 = 0; 
        foreach (Transaction t in World.Current.Transactions)
        {
            if (t.Type == Words.Current.CustomerPurchase)
            {
                CustServed++;
                CustMoneyMade += t.Cost;
            }
            else
            {
                OrdersMade++;
                OrdersSpent += t.Cost;
            }
        }

        foreach (Item i in World.Current.ItemsOnShelves)
        {
            TotBought2 += i.BoughtTwo;
            TotHappy += i.Happy;
            TotNeutral += i.Neutral;
            TotExpensive += i.NotBuying;
            TotWouldaBought2 += i.WouldaBoughtTwo;
            TotWouldaBought1 += i.OuttaStock;
        }

        stats = "You served " + CustServed + " customers today.\n" +
            "You took in £" + CustMoneyMade.ToString("N") + " from customer purchases.\n" +
            "You made " + OrdersMade + " orders today.\n" +
            "You spent £" + OrdersSpent.ToString("N") + " on orders today.\n" +
            "You had " + TotBought2 + " customers so happy with the price of an item they bought two.\n" +
            "You had " + TotHappy + " customers happy with the price of an item.\n" +
            "You had " + TotNeutral + " customers who thought an item was expensive but still bought it.\n" +
            "You had " + TotExpensive + " customers who didn't buy an item because it was too expensive.\n" +
            "You had " + TotWouldaBought2 + " customers who would've bought two but the shelf was empty.\n" +
            "You had " + TotWouldaBought1 + " customers who would've bought an item but the shelf was empty.\n";
        EODStats.GetComponent<Text>().text = stats;
    }
    public void NextDay()
    {

        EODMenu.SetActive(false);
        if (World.Current.paused)
        {
            OnPauseButton();
        }
        ItemMenuing = false;
        World.Current.StartNextDay();
    }

    void OpenEmployeeLateMenu(Employee e, int MinutesLate)
    {
        if (!World.Current.paused)
        {
            OnPauseButton();
        }
        ItemMenuing = true;
        EmployeeLateMenu.SetActive(true);
        EmployeeLateText.GetComponent<Text>().text = "'Hey boss, it's " +
            e.Name + ". Sorry to be a pain, I'm running late. I'll be about " +
            MinutesLate + " minutes late. Sorry!'";
    }

    public void CloseEmployeeLateMenu()
    {
        if (World.Current.paused)
        {
            OnPauseButton();
        }
        ItemMenuing = false;
        EmployeeLateMenu.SetActive(false);
    }

    public void OpenSettings()
    {
        ItemMenuing = true;
        SettingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {

        ItemMenuing = false;
        SettingsMenu.SetActive(false);
    }
}
