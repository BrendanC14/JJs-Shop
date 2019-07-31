using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class Path_ASTar  {

    Queue<Tile> path;

    public Path_ASTar(World world, Tile tileStart, Tile tileEnd, bool Vehicle, bool cust, Direction d)
    {
        Dictionary<Tile, Path_Nodes<Tile>> nodes;
        if (Vehicle)
        {
            if (World.Current.DrivingGraph == null) { World.Current.DrivingGraph = new Path_DrivingGraph(World.Current, d); }
            nodes = World.Current.DrivingGraph.nodes;
        }
        else
        {
            if (World.Current.WalkingGraph == null) { World.Current.WalkingGraph = new Path_WalkingGraph(World.Current); }
            nodes = World.Current.WalkingGraph.nodes;
        }

        if (nodes.ContainsKey(tileStart) == false)
        {
            Debug.LogError("Starting Tile isn't in list of nodes");
            return;
        }
        if (nodes.ContainsKey(tileEnd) == false)
        {
            Debug.LogError("Starting Tile isn't in list of nodes");
            return;
        }

        Path_Nodes<Tile> start = nodes[tileStart];
        Path_Nodes<Tile> goal = nodes[tileEnd];

        List<Path_Nodes<Tile>> ClosedSet = new List<Path_Nodes<Tile>>();
        SimplePriorityQueue<Path_Nodes<Tile>> OpenSet = new SimplePriorityQueue<Path_Nodes<Tile>>();
        OpenSet.Enqueue(start, 0);

        Dictionary<Path_Nodes<Tile>, Path_Nodes<Tile>> Came_From = new Dictionary<Path_Nodes<Tile>, Path_Nodes<Tile>>();
        Dictionary<Path_Nodes<Tile>, float> g_score = new Dictionary<Path_Nodes<Tile>, float>();
        foreach (Path_Nodes<Tile> n in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }
        g_score[nodes[tileStart]] = 0;

        Dictionary<Path_Nodes<Tile>, float> f_score = new Dictionary<Path_Nodes<Tile>, float>();
        foreach (Path_Nodes<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }

        f_score[start] = heuristic_cost_estimate(start, goal);


        while (OpenSet.Count > 0)
        {
            Path_Nodes<Tile> current = OpenSet.Dequeue();

            if (current == goal)
            {
                //TODO : return reconstruct path
                reconstruct_path(Came_From, current);
                return;
            }

            ClosedSet.Add(current);
            foreach (Path_Edges<Tile> edge_neighbour in current.edges)
            {
                Path_Nodes<Tile> neighbour = edge_neighbour.nodes;
                if (ClosedSet.Contains(neighbour))
                {
                    continue;
                }
                float movementCostToNeighbour;
                if (Vehicle) { movementCostToNeighbour = neighbour.data.VehicleMovementCost * dist_between(current, neighbour); }
                else { movementCostToNeighbour = neighbour.data.PersonMovementCost * dist_between(current, neighbour); }
                
                 float tentative_g_score = g_score[current] + movementCostToNeighbour;

                if (OpenSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                {
                    continue;
                }
                Came_From[neighbour] = current;
                g_score[neighbour] = tentative_g_score;
                f_score[neighbour] = g_score[neighbour] + heuristic_cost_estimate(neighbour, goal);
                if (OpenSet.Contains(neighbour) == false)
                {
                    //If they're a customer, don't let them go through a stock door
                    if (!(cust && neighbour.data.Fixture != null &&
                    neighbour.data.Fixture.FixtureType == Words.Current.Door &&
                    !World.Current.CustomerDoors.Contains(neighbour.data.Fixture)))
                    {
                        OpenSet.Enqueue(neighbour, f_score[neighbour]);
                    }
                }
                else
                {
                    OpenSet.UpdatePriority(neighbour, f_score[neighbour]);
                }
            }//foreach neighbour
        }//while

    }
    float heuristic_cost_estimate(Path_Nodes<Tile> a, Path_Nodes<Tile> b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2)
            );
    }

    float dist_between(Path_Nodes<Tile> a, Path_Nodes<Tile> b)
    {
        //Hori/Vert neighbours have distance of 1
        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1f;
        }

        //Diag neighbours has distance of 1.41421356237
        if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1.41421356237f;
        }

        //Otherwise do the Math
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2)
            );
    }
    void reconstruct_path(Dictionary<Path_Nodes<Tile>, Path_Nodes<Tile>> Came_From, Path_Nodes<Tile> current)
    {
        //At this point, current IS the goal so we want to walk backwards through the Came_From map
        //until we reach the end which will be our starting node
        Queue<Tile> total_Path = new Queue<Tile>();
        total_Path.Enqueue(current.data);
        while (Came_From.ContainsKey(current))
        {
            //Came_From is a map where the key => calue relation is really saying some_node =>
            //we_got_there_from_this_node
            current = Came_From[current];
            total_Path.Enqueue(current.data);
        }

        path = new Queue<Tile>(total_Path.Reverse());
    }

    public Tile Dequeue()
    {
        if (Length() == 0)
        {
            return null;
        }
        return path.Dequeue();
    }

    public int Length()
    {
        if (path == null) { return 0; }
        return path.Count();
    }
}
