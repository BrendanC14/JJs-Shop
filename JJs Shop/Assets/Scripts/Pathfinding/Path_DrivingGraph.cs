using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_DrivingGraph
{

    //Each tile is a node, each walkable neighbour from a tile is linked via and Edge

    public Dictionary<Tile, Path_Nodes<Tile>> nodes;

    public Path_DrivingGraph(World world, Direction direction)
    {
        nodes = new Dictionary<Tile, Path_Nodes<Tile>>();

        for (int x = 0; x < World.Current.Width; x++)
        {
            for (int y = 0; y < World.Current.Height; y++)
            {
                Tile t = World.Current.GetTileAt(x, y);
                Path_Nodes<Tile> n = new Path_Nodes<Tile>();
                n.data = t;
                nodes.Add(t, n);
            }
        }
        int edgeCount = 0;
        foreach (Tile t in nodes.Keys)
        {
            Path_Nodes<Tile> n = nodes[t];
            List<Path_Edges<Tile>> edges = new List<Path_Edges<Tile>>();
            Tile[] neighbours = t.GetNeighbours(false);

            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null && neighbours[i].GetVehicleDirectionMovementCost(t) > 0)
                {
                    if (isClippingCorner(t, neighbours[i]))
                    {
                        continue;
                    }
                    Path_Edges<Tile> e = new Path_Edges<Tile>();
                    e.cost = neighbours[i].VehicleMovementCost;
                    e.nodes = nodes[neighbours[i]];
                    edges.Add(e);
                    edgeCount++;
                }
            }
            n.edges = edges.ToArray();
        }

    }

    bool isClippingCorner(Tile currTile, Tile neighbour)
    {
        int dX = currTile.X - neighbour.X;
        int dY = currTile.Y - neighbour.Y;

        //If the movement from curr to neigh is diagonal
        if (Mathf.Abs(dX) + Mathf.Abs(dY) == 2)
        {
            //We are diagonal
            if (World.Current.GetTileAt(currTile.X - dX, currTile.Y).VehicleMovementCost == 0)
            {
                //East or West is unwalkable, would be clipped
                return true;
            }
            if (World.Current.GetTileAt(currTile.X, currTile.Y - dY).VehicleMovementCost == 0)
            {
                //North or South is unwalkable, would be clipped
                return true;
            }
        }

        return false;
    }
}
