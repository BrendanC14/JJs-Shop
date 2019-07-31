using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixtureSpriteController : MonoBehaviour {

    Dictionary<Fixture, GameObject> FixtureGameObjectMap;
    Dictionary<string, GameObject> ItemGameObjectMap;
    Dictionary<string, Sprite> FixtureSprites;
    public GameObject InventoryStackSizePrefab;

    // Use this for initialization
    void Start () {
        LoadSprites();

        FixtureGameObjectMap = new Dictionary<Fixture, GameObject>();
        World.Current.AddFixtureCreatedCallback(OnFixtureCreated);

        foreach (Fixture fixt in World.Current.Fixtures )
        {
            OnFixtureCreated(fixt);
        }

        
    }

    public void OnFixtureCreated(Fixture fixt)
    {
        GameObject fixtGo = new GameObject();
        FixtureGameObjectMap.Add(fixt, fixtGo);

        fixtGo.name = fixt.FixtureType + "_" + fixt.Tile.X + "," + fixt.Tile.Y;
        fixtGo.transform.position = new Vector3(fixt.Tile.X + ((fixt.Width - 1) / 2f), fixt.Tile.Y + ((fixt.Height - 1) / 2f), 0);
        fixtGo.transform.SetParent(this.transform, true);

        fixtGo.AddComponent<SpriteRenderer>().sprite = GetSpriteForFixture(fixt);
        if (fixt.FixtureType == Words.Current.Bench)
        {
            fixtGo.GetComponent<SpriteRenderer>().sortingLayerName = Words.Current.TileLayer;

        }
        else
        {
            fixtGo.GetComponent<SpriteRenderer>().sortingLayerName = Words.Current.FixtureLayer;
        }
            fixt.AddAFixtureChangedCallback(OnFixtureChanged);
        fixt.AddCustomerOpinionCallback(OnCustomerOpinionChanged);

        if (fixt.MaxShelfSpace > 0)
        {
            GameObject ui_go = Instantiate(InventoryStackSizePrefab);
            ui_go.transform.SetParent(fixtGo.transform);
            ui_go.name = "StockLevel";
            if (fixt.EmployeeDirection == Direction.Up )
            {
                ui_go.transform.localPosition = new Vector3(0.5F, 0.35F, 0);
            }
            else if (fixt.EmployeeDirection == Direction.Down)
            {
                ui_go.transform.localPosition = new Vector3(0.5F, -0.35F, 0);
            }
            else if (fixt.EmployeeDirection == Direction.Left)
            {
                ui_go.transform.localPosition = new Vector3(0.75F, 0, 0);
            }
            else
            {
                ui_go.transform.localPosition = new Vector3(0.2F, 0, 0);
            }

            if (fixt.GetItemsOnShelf().Count == 0)
            {
                ui_go.GetComponentInChildren<Text>().text = "!";
            }
            else
            {
                ui_go.GetComponentInChildren<Text>().text = fixt.GetItemsOnShelf().Count.ToString();
            }
            CheckTextColour(ui_go.GetComponentInChildren<Text>(), fixt.GetPercentageFull());

        }
        if (fixt.GetItemsOnShelf().Count > 0 && (fixt.FixtureType == Words.Current.ShelfDown ||
            fixt.FixtureType == Words.Current.FridgeDown || fixt.FixtureType == Words.Current.ShelfWall))
        {
            float x = -0.36f;
            float y = -0.375f;
            foreach(Item i in fixt.GetItemsOnShelf())
            {
                if (ItemGameObjectMap.ContainsKey(i.Name))
                {
                    GameObject item_go = Instantiate(ItemGameObjectMap[i.Name]);
                    item_go.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
                    item_go.transform.SetParent(fixtGo.transform);
                    item_go.transform.localPosition = new Vector3(x, y, 0);
                    x += 0.135f;
                    if (x >= 0.4f)
                    {
                        x = -0.36f;
                        y += 0.175f;
                    }
                    if (y > 0.15f)
                    {
                        y = -0.375f;
                    }
                }
            }
        }
        else if (fixt.FixtureType == Words.Current.Checkout)
        {
            GameObject ui_go = Instantiate(InventoryStackSizePrefab);
            ui_go.transform.SetParent(fixtGo.transform);
            ui_go.transform.localPosition = new Vector3(0.8F, -0.5F, 0);
            ui_go.GetComponentInChildren<Text>().text = World.Current.PeopleInQueue.ToString();
            ui_go.GetComponentInChildren<Text>().color = new Color(0f, 0f, 0f, 1f);

        }

    }

    void OnFixtureChanged(Fixture f)
    {
        GameObject fixGO = FixtureGameObjectMap[f];
        SpriteRenderer sr = fixGO.GetComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFixture(f);
        if (f.UserSelected)
        {
            sr.color = new Color(0.5f, 1f, 0.5f, 1f);
        }
        else { sr.color = new Color(1f, 1f, 1f, 1f); }

        if (f.MaxShelfSpace > 0)
        {
            Text t = fixGO.transform.Find("StockLevel").GetComponentInChildren<Text>();
            if (t != null)
            {
                if (f.GetItemsOnShelf().Count == 0)
                {
                    t.text = "!";
                }
                else
                {
                    t.text = f.GetItemsOnShelf().Count.ToString();
                }
                    CheckTextColour(t, f.GetPercentageFull());
            }
        }
        foreach (Transform child in fixGO.transform)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                Destroy(child.gameObject);
            }
        }
        if (f.GetItemsOnShelf().Count > 0 && (f.FixtureType == Words.Current.ShelfDown ||
            f.FixtureType == Words.Current.FridgeDown || f.FixtureType == Words.Current.ShelfWall))
        {
            float x = -0.36f;
            float y = -0.375f;
            foreach (Item i in f.GetItemsOnShelf())
            {
                if (ItemGameObjectMap.ContainsKey(i.Name))
                {
                    GameObject item_go = Instantiate(ItemGameObjectMap[i.Name]);
                    item_go.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
                    item_go.transform.SetParent(fixGO.transform);
                    item_go.transform.localPosition = new Vector3(x, y, 0);
                    x += 0.135f;
                    if (x >= 0.4f)
                    {
                        x = -0.36f;
                        y += 0.175f;
                    }
                    if ( y > 0.15f)
                    {
                        y = -0.375f;
                    }
                }
            }
        }
        if (f.FixtureType == Words.Current.Checkout)
        {
            Text t = fixGO.GetComponentInChildren<Text>();
            t.text = World.Current.PeopleInQueue.ToString();

        }
    }

    void CheckTextColour(Text t, float PercentageFull)
    {
        if (PercentageFull >= 0.75f)
        {
            t.color = new Color(0f, 0.4f, 0f, 1f);
        }
        else if (PercentageFull >= 0.5f)
        {
            t.color = new Color(0.2f, 0.3f, 0f, 1f);
        }
        else if (PercentageFull >= 0.25f)
        {
            t.color = new Color(0.4f, 0.2f, 0f, 1f);
        }
        else
        {
            t.color = new Color(0.6f, 0.1f, 0f, 1f);
        }

    }
    
    void OnCustomerOpinionChanged(Fixture f)
    {
        GameObject fixGO = FixtureGameObjectMap[f];
        if (f.PriceOpinion == 0)
        {
            foreach (Transform child in fixGO.transform)
            {
                if (child.name == "Opinion")
                {
                    Destroy(child.gameObject);
                }
            }
        }
        else 
        {

            GameObject opiGO = Instantiate(InventoryStackSizePrefab);
            opiGO.transform.SetParent(fixGO.transform);
            opiGO.name = "Opinion";
            if (f.EmployeeDirection == Direction.Up)
            {
                opiGO.transform.localPosition = new Vector3(0.15F, 0.35F, 0);
            }
            else if (f.EmployeeDirection == Direction.Down)
            {
                opiGO.transform.localPosition = new Vector3(-0.15F, -0.35F, 0);
            }
            else if (f.EmployeeDirection == Direction.Left)
            {
                opiGO.transform.localPosition = new Vector3(0.75F, 0.35f, 0);
            }
            else
            {
                opiGO.transform.localPosition = new Vector3(0.25F, 0.35f, 0);
            }

            if (f.PriceOpinion == 1)
            {

                opiGO.GetComponentInChildren<Text>().color = new Color(0f, 0.4f, 0f, 1f);
            }
            else { opiGO.GetComponentInChildren<Text>().color = new Color(0.6f, 0.1f, 0f, 1f); }

            opiGO.GetComponentInChildren<Text>().text = "£";
        }
    }
       

	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadSprites()
    {
        FixtureSprites = new Dictionary<string, Sprite>();
        ItemGameObjectMap = new Dictionary<string, GameObject>();
        Sprite[] sprites = Resources.LoadAll<Sprite>(Words.Current.FixtureSpriteFolder);

        foreach (Sprite s in sprites)
        {
            FixtureSprites[s.name] = s;
        }

        GameObject[] GOs = Resources.LoadAll<GameObject>(Words.Current.PrefabFolder);

        foreach (GameObject g in GOs)
        {
            ItemGameObjectMap.Add(g.name, g);
        }
    }

    public Sprite GetSpriteForFixture(Fixture f)
    {
        string spriteName;
        if (f.FixtureType == Words.Current.Door)
        {
            if (f.GetParameter("openness") < 0.1f) { spriteName = Words.Current.Door; }
            else if (f.GetParameter("openness") < 0.5f) { spriteName = Words.Current.DoorOpening; }
            else if (f.GetParameter("openness") < 0.9f) { spriteName = Words.Current.DoorAlmost; }
            else { spriteName = Words.Current.DoorOpen; }
            return FixtureSprites[spriteName];
        }
        if (FixtureSprites.ContainsKey(f.FixtureType))
        {
            return FixtureSprites[f.FixtureType];
        }
        return null;
    }

    public Sprite GetSpriteForFixture(string objectType)
    {
        if (FixtureSprites.ContainsKey(objectType))
        {
            return FixtureSprites[objectType];
        }

        return null;
    }
}
