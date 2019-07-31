using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FixtureSaveFile
{
    public int X;
    public int Y;
    public bool NeedsRestock;
    public string ItemOnShelf;
    public int ItemCount = 0;

    public FixtureSaveFile(Fixture f)
    {
        X = f.Tile.X;
        Y = f.Tile.Y;
        NeedsRestock = f.NeedsRestock;
        if (f.ItemsOnShelf.Count > 0)
        {
            ItemOnShelf = f.ItemsOnShelf[0].Name;
            ItemCount = f.ItemsOnShelf.Count;
        }

        string JSON = JsonUtility.ToJson(this);

        PlayerPrefs.SetString("Fixture" + X.ToString() + Y.ToString(), JSON);
    }
}
