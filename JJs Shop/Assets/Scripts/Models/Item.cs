using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Item : IXmlSerializable {

    public string Name;
    public int ID;
    public int ShelfSpace; //How much space it takes on a shelf
    public double PrevPrice;
    public double Price;
    public double OrderPrice;
    public double ExpectedPrice;
    public bool Refrigerated;
    public Fixture ShopShelf;
    public Fixture StockShelf;
    public bool DeliveryPending;
    public double NewPrice;

    //Stats
    public int PrevNotBuying;
    public int PrevNeutral;
    public int PrevHappy;
    public int PrevOuttaStock;
    public int PrevWouldaBoughtTwo;
    public int PrevBoughtTwo;

    public int NotBuying;
    public int Neutral;
    public int Happy;
    public int OuttaStock;
    public int WouldaBoughtTwo;
    public int BoughtTwo;

    public Action<Item> cbItemDelivered;

    protected Item(Item other)
    {
        this.Name = other.Name;
        this.ShelfSpace = other.ShelfSpace;
        this.Price = other.Price;
        this.Refrigerated = other.Refrigerated;
    }
    
    virtual public Item Clone()
    {
        return new Item(this);
    }

    public Item (string name, int Space, double cost, double orderCost, double expectedCost, bool refrigerate)
    {
        this.Name = name;
        this.ShelfSpace = Space;
        this.Price = cost;
        this.OrderPrice = orderCost;
        this.ExpectedPrice = expectedCost;
        this.Refrigerated = refrigerate;
        this.ID = Numbers.Current.GetNextItemID();
        ShopShelf = null;
    }
    public Item (ItemSaveFile savedItem)
    {
        Name = savedItem.Name;
        ID = savedItem.ID;
        PrevPrice = savedItem.PrevPrice;
        Price = savedItem.Price;
        OrderPrice = savedItem.OrderPrice;
        DeliveryPending = savedItem.DeliverPending;
        NewPrice = savedItem.NewPrice;
        PrevNotBuying = savedItem.PrevNotBuying;
        PrevNeutral = savedItem.PrevNeutral;
        PrevHappy = savedItem.PrevHappy;
        PrevOuttaStock = savedItem.PrevOuttaStock;
        PrevWouldaBoughtTwo = savedItem.PrevWouldaBoughtTwo;
        PrevBoughtTwo = savedItem.PrevBoughtTwo;
        NotBuying = savedItem.NotBuying;
        Neutral = savedItem.Neutral;
        Happy = savedItem.Happy;
        OuttaStock = savedItem.OuttaStock;
        WouldaBoughtTwo = savedItem.WouldaBoughtTwo;
        BoughtTwo = savedItem.BoughtTwo;
        Refrigerated = savedItem.Refrigerated;
        ShelfSpace = savedItem.ShelfSpace;
        ShopShelf = World.Current.GetTileAt(savedItem.ShopShelfX, savedItem.ShopShelfY).Fixture;
        ShopShelf = World.Current.GetTileAt(savedItem.StockShelfX, savedItem.StockShelfY).Fixture;
    }

    static public Item GetCopyOfProto(Item proto)
    {
        return proto.Clone();
        
    }


    public bool IsValidPosition(Fixture f)
    {
        if (Refrigerated && f.Fridge == false)
        {
            //This item needs to be refrigerated
            return false;
        }

        if (f.GetItemsOnShelf().Count + ShelfSpace > f.MaxShelfSpace)
        {
            //You can't overstock the shelf
            return false;
        }

        return true;
    }

    public bool TakeOffShopShelf(int Quantity)
    {
        if (ShopShelf.GetItemsOnShelf().Count - (ShelfSpace * Quantity) >= 0)
        {
            for (int i = 0; i < Quantity; i++)
            {

                ShopShelf.RemoveItemFromShelf();
                if (ShopShelf.GetPercentageFull() <= Numbers.Current.RestockAtPercentage &&
                    ShopShelf.NeedsRestock == false)
                {
                    ShopShelf.NeedsRestock = true;
                    World.Current.StockQueue.Enqueue(new Job(
                        ShopShelf.Tile,
                        Words.Current.StockQueue,
                        "",
                        null,
                        -1));
                }
                ShopShelf.cbOnChanged(ShopShelf);
            }
            return true;
        }
        return false;
    }

    public bool PutOnShopShelf(int Quantity)
    {
        if (ShopShelf.GetItemsOnShelf().Count >= ShopShelf.MaxShelfSpace)
        {
            return false;
        }

        ShopShelf.PlaceItem(this);
        return true;
    }

    public bool TakeOffStockShelf(int Quantity)
    {
        if (StockShelf.GetItemsOnShelf().Count - (ShelfSpace * Quantity) >= 0)
        {
            StockShelf.RemoveItemFromShelf();
            return true;
        }
        return false;
    }

    public bool PutOnStockShelf(int Quantity)
    {

        if (StockShelf.GetItemsOnShelf().Count >= StockShelf.MaxShelfSpace)
        {
            return false;
        }
        
        StockShelf.PlaceItem(this);
        StockShelf.cbOnChanged(ShopShelf);
        return true;
    }

    public void OrderMore(int Quantity, double Cost, Action<Item> deliveryCallback)
    {
        DeliveryPending = true;
        World.Current.DeliveryTime += Numbers.Current.DeliverTime;
        World.Current.ItemToDeliver = this;
        World.Current.QuantityToDeliver = Quantity;
        World.Current.Balance -= Cost;

        List<Item> OrderList = new List<Item>();
        for (int i = 0; i < Quantity; i++)
        {
            OrderList.Add(this);
        }
        World.Current.Transactions.Add(new Transaction(
            Words.Current.ShopOrder, OrderList));
        if (deliveryCallback != null)
        {
            AddItemDeliveredCallback(deliveryCallback);
        }
    }

    public void ItemDelivered(int Quantity)
    {
        for (int i = 0; i < Quantity; i++)
        {
            StockShelf.PlaceItem(this);
        }
        DeliveryPending = false;
        if (cbItemDelivered != null)
        {
            cbItemDelivered(this);
        }
    }

    public void UpdatePrice()
    {
        PrevPrice = Price;
        Price = NewPrice;
        NewPrice = 0;

        PrevNotBuying = NotBuying;
        PrevNeutral = Neutral;
        PrevHappy = Happy;
        PrevOuttaStock = OuttaStock;
        PrevWouldaBoughtTwo = WouldaBoughtTwo;
        PrevBoughtTwo = BoughtTwo;
        NotBuying = 0;
        Neutral = 0;
        Happy = 0;
        OuttaStock = 0;
        WouldaBoughtTwo = 0;
        BoughtTwo = 0;
    }

    public void AddItemDeliveredCallback(Action<Item> callback) { cbItemDelivered += callback; }





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
        writer.WriteAttributeString("Name", Name);
        writer.WriteAttributeString("PrevPrice", PrevPrice.ToString());
        writer.WriteAttributeString("Price", Price.ToString());
        writer.WriteAttributeString("OrderPrice", OrderPrice.ToString());
        writer.WriteAttributeString("DeliveryPending", DeliveryPending.ToString());
        writer.WriteAttributeString("NewPrice", NewPrice.ToString());
        writer.WriteAttributeString("PrevNotBuying", PrevNotBuying.ToString());
        writer.WriteAttributeString("PrevNeutral", PrevNeutral.ToString());
        writer.WriteAttributeString("PrevHappy", PrevHappy.ToString());
        writer.WriteAttributeString("PrevOuttaStock", PrevOuttaStock.ToString());
        writer.WriteAttributeString("PrevWouldaBoughtTwo", PrevWouldaBoughtTwo.ToString());
        writer.WriteAttributeString("PrevBoughtTwo", PrevBoughtTwo.ToString());
        writer.WriteAttributeString("NotBuying", NotBuying.ToString());
        writer.WriteAttributeString("Neutral", Neutral.ToString());
        writer.WriteAttributeString("Happy", Happy.ToString());
        writer.WriteAttributeString("OuttaStock", OuttaStock.ToString());
        writer.WriteAttributeString("WouldaBoughtTwo", WouldaBoughtTwo.ToString());
        writer.WriteAttributeString("BoughtTwo", BoughtTwo.ToString());


    }

    public void ReadXml(XmlReader reader)
    {
        PrevPrice = int.Parse(reader.GetAttribute("PrevPrice"));
        Price = int.Parse(reader.GetAttribute("Price"));
        OrderPrice = int.Parse(reader.GetAttribute("OrderPrice"));
        if (reader.GetAttribute("DeliveryPending") == "True")
        {
            DeliveryPending = true;
        }
        else { DeliveryPending = false; }

        NewPrice = int.Parse(reader.GetAttribute("NewPrice"));
        PrevNotBuying = int.Parse(reader.GetAttribute("PrevNotBuying"));
        PrevNeutral = int.Parse(reader.GetAttribute("PrevNeutral"));
        PrevHappy = int.Parse(reader.GetAttribute("PrevHappy"));
        PrevOuttaStock = int.Parse(reader.GetAttribute("PrevOuttaStock"));
        PrevWouldaBoughtTwo = int.Parse(reader.GetAttribute("PrevWouldaBoughtTwo"));
        PrevBoughtTwo = int.Parse(reader.GetAttribute("PrevBoughtTwo"));
        NotBuying = int.Parse(reader.GetAttribute("NotBuying"));
        Neutral = int.Parse(reader.GetAttribute("Neutral"));
        Happy = int.Parse(reader.GetAttribute("Happy"));
        OuttaStock = int.Parse(reader.GetAttribute("OuttaStock"));
        WouldaBoughtTwo = int.Parse(reader.GetAttribute("WouldaBoughtTwo"));
        BoughtTwo = int.Parse(reader.GetAttribute("BoughtTwo"));
    }

}
