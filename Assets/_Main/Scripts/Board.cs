using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    public static Board instance = null;
    public List<Node> nodes = new List<Node>();
    public List<Edge> edges = new List<Edge>();

    public bool canBuild = false;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public Board()
    {
        nodes = new List<Node>();
        edges = new List<Edge>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameManager.instance.CurrentPhase == Phase.Building && canBuild)
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "City" && !EventSystem.current.IsPointerOverGameObject())
                {
                    if (!hit.collider.gameObject.GetComponent<CityHolder>().isBlocked)
                    {
                        int s = GameManager.instance.CurrentSection;
                        int cityBaseCost = 0;
                        if (s == 1)
                        {
                            cityBaseCost = 10;
                        }
                        else if (s == 2)
                        {
                            cityBaseCost = 15;

                        }
                        else
                        {
                            cityBaseCost = 20;

                        }

                        UIManager.instance.ShowCityBuildingCost(hit.collider.gameObject.GetComponent<CityHolder>().thisCityNode, CalculateShortestDistance(hit.collider.gameObject.GetComponent<CityHolder>().thisCityNode) + cityBaseCost);
                    }
                }
            }

        }
    }

    int CalculateShortestDistance(Node startNode)
    {
        if (GameManager.instance.ReturnClientPlayer().CitiesOwned.Count == 0)
        {
            return 0;
        }
        else if (GameManager.instance.ReturnClientPlayer().CitiesOwned.Contains(startNode))
        {
            return -1;
        }
        int[] distance = new int[nodes.Count];
        List<Node> unvisited = new List<Node>();
        for (int i = 0; i < distance.Length; i++)
        {

            if (nodes[i] != startNode)
            {
                distance[i] = int.MaxValue;
            }
            else
            {
                distance[i] = 0;
            }
            unvisited.Add(nodes[i]);
        }

        while (unvisited.Count > 0)
        {
            Vector2Int newStart = FindSmallestDistanceUnvisitedVertex(distance, unvisited);


            List<Edge> edges = FindAllEdgesWithNode(nodes[newStart.x]);
            foreach (Edge e in edges)
            {
                int indexOfNeighbour = 0;
                if (e.connectedNodes[0] == nodes[newStart.x])
                {
                    indexOfNeighbour = nodes.FindIndex(x => x == e.connectedNodes[1]);
                    ////index 1 is the other node

                    if (distance[indexOfNeighbour] > e.weight + newStart.y)
                    {
                        distance[indexOfNeighbour] = e.weight + newStart.y;
                    }
                }
                else
                {
                    indexOfNeighbour = nodes.FindIndex(x => x == e.connectedNodes[0]);
                    ////index 0 is the other node

                    if (distance[indexOfNeighbour] > e.weight + newStart.y)
                    {
                        distance[indexOfNeighbour] = e.weight + newStart.y;
                    }

                }
            }
            unvisited.Remove(nodes[newStart.x]);

        }

        int smallestDistance = int.MaxValue;
        foreach (Node n in GameManager.instance.ReturnClientPlayer().CitiesOwned)
        {
            int index = nodes.FindIndex(x => x == n);
            if (distance[index] < smallestDistance)
            {
                smallestDistance = distance[index];
            }
        }
        return smallestDistance;
        //if( GameManager.instance.ReturnClientPlayer().citiesOwned)

    }

    Vector2Int FindSmallestDistanceUnvisitedVertex(int[] distances, List<Node> unvisited)
    {
        int[] newDistances = new int[distances.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            newDistances[i] = distances[i];
        }
        bool foundUnvisited = false;
        int smallestDistanceIndex = -1;
        int smallestDistance = int.MaxValue;
        while (!foundUnvisited)
        {
            smallestDistance = int.MaxValue;
            smallestDistanceIndex = -1;
            for (int i = 0; i < newDistances.Length; i++)
            {
                if (newDistances[i] < smallestDistance)
                {
                    smallestDistance = newDistances[i];
                    smallestDistanceIndex = i;
                }

            }

            if (unvisited.Contains(nodes[smallestDistanceIndex]))
            {
                foundUnvisited = true;
                break;
            }
            else
            {
                newDistances[smallestDistanceIndex] = int.MaxValue;
            }
        }
        return new Vector2Int(smallestDistanceIndex, smallestDistance);
    }

    List<Edge> FindAllEdgesWithNode(Node n)
    {
        List<Edge> edgeWithNodeN = new List<Edge>();

        foreach (Edge e in edges)
        {
            if (e.connectedNodes[0] == n || e.connectedNodes[1] == n)
            {
                edgeWithNodeN.Add(e);
            }
        }
        return edgeWithNodeN;
    }

}
