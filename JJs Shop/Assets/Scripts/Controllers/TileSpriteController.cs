using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpriteController : MonoBehaviour {

    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<string, Sprite> VillageSprites;

    World World { get { return World.Current; } }
	// Use this for initialization
	void Start () {
        int Count = 0;
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        LoadSprites();
        if (World == null) { Debug.Log("Hmm"); }
        for (int y = World.Height-1; y >= 0; y--)
        {
            for (int x = 0; x < World.Width; x++)
            {
                Tile tileData = World.GetTileAt(x, y);
                GameObject tileGo = new GameObject();

                tileGameObjectMap.Add(tileData, tileGo);
                tileGo.name = "Tile_" + x + "," + y;
                if (!(tileData.X == 0 || tileData.X == World.Width -1 || tileData.Y == 0 || tileData.Y == World.Height -1
                    || tileData.X == 1 || tileData.X == World.Width - 2 || tileData.Y  == 1 || tileData.Y == World.Height - 2))
                {
                    tileGo.transform.position = new Vector3(tileData.X, tileData.Y, 0);

                }
                tileGo.transform.SetParent(this.transform, true);

                tileGo.AddComponent<SpriteRenderer>().sprite = VillageSprites["City V2_" + Count];
                Count++;

            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadSprites()
    {
        VillageSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>(Words.Current.VillageMapSpriteFolder);

        foreach(Sprite s in sprites)
        {
            VillageSprites[s.name] = s;
        }

    }

}
