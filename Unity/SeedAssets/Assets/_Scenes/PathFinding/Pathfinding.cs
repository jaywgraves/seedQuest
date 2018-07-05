﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour {

    public Transform seeker, target;
    Grid grid;

    PathManager requestManager;

    private void Awake() {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathManager>();
    }

    private void Update() {
        if( Input.GetButtonDown("Jump"))
            FindPath(seeker.position, target.position);
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPathCoroutine(startPos, targetPos));
    }

    IEnumerator FindPathCoroutine(Vector3 startPos, Vector3 targetPos) {
        Vector3[] waypoints = FindPath(startPos, targetPos);
        bool pathSucess = false;
        if (waypoints.Length > 0)
            pathSucess = true;
        requestManager.FinishedProcessingPath(waypoints, pathSucess);

        yield return null;
    }

    Vector3[] FindPath(Vector3 startPos, Vector3 targetPos) {

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSucess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        //Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0) {
            //Node node = openSet.RemoveFirst();


            Node node = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost) {
                    if (openSet[i].hCost < node.hCost)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);


            closedSet.Add(node);

            if (node == targetNode) {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + " ms");
                pathSucess = true;
                break;
            }

            foreach (Node neighbor in grid.GetNeighbors(node)) {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) {
                    continue;
                }

                int costToNeighbor = node.gCost + GetDistance(node, neighbor);
                if (costToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                    neighbor.gCost = costToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = node;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);                    
                }

            }
        }

        if(pathSucess) {
            waypoints = RetracePath(startNode, targetNode);
        }

        return waypoints;
    }

    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path = path;

        Vector3[] waypoints = SimplifyPath(path);
        return waypoints;
    } 

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
                waypoints.Add(path[i].worldPosition);
            directionOld = directionNew;
        }

        Node targetNode = path[path.Count - 1];
        waypoints.Add( targetNode.worldPosition );

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB) {
        int x = (nodeA.gridX - nodeB.gridX);
        int y = (nodeA.gridY - nodeB.gridY);

        return x * x + y * y;
    }
}
