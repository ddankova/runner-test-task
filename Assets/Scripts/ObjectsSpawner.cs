using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject barrel;
    [SerializeField] GameObject bush;
    [SerializeField] GameObject cart;
    [SerializeField] GameObject crates;

    [SerializeField] Transform obstaclesParent;

    [SerializeField] Transform rootPoints;

    public float currentDeviation = 0;

    float minObstacleSpawnDistance = 5;
    float maxObstacleSpawnDistance = 15;

    float distanceFromTheLastObstacle = 0;

    Dictionary<GameObject, float> posiibilitiesOfSpawning = new Dictionary<GameObject, float>();

    // Start is called before the first frame update
    void Start()
    {
        if (posiibilitiesOfSpawning.Count == 0) 
        {
            posiibilitiesOfSpawning.Add(barrel, 0.35f);
            posiibilitiesOfSpawning.Add(crates, 0.35f);
            posiibilitiesOfSpawning.Add(bush, 0.2f);
            posiibilitiesOfSpawning.Add(cart, 0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetObstacles()
    {
        int obstaclesCount = obstaclesParent.childCount;

        if (obstaclesCount > 0)
        {
            for (int i = 0; i < obstaclesCount; i++) Destroy(obstaclesParent.GetChild(i));
        }

        // iterating through points to set obstacles
        int pointsCount = rootPoints.childCount;

        List<int> acceptablePointIndices = new List<int>();

        // creating the list of acceptable indices
        for (int i = 0; i < pointsCount; i++) 
        {
            if (CheckPointIndex(i)) acceptablePointIndices.Add(i);
        }

        Vector3 lastObstaclePosition = Vector3.zero;
        float distanceFromObstacle = 0;
        int previousPointIndex = -1;
        bool obstacleJustSet = false;

        // random distance between obstacles
        float randomSpawnDistance = Random.Range(minObstacleSpawnDistance, maxObstacleSpawnDistance);

        // iterate through the list of acceptable indices
        foreach (int pointIndex in acceptablePointIndices)
        {
            if (acceptablePointIndices.IndexOf(pointIndex) == 0)
            {
                // setting the first obstacle in the first acceptable point
                lastObstaclePosition = rootPoints.GetChild(pointIndex).position;
                SetObstacle(lastObstaclePosition);
                obstacleJustSet = true;
            }
            // if indices are too far, skip thye point
            else if (pointIndex - previousPointIndex > 1) { previousPointIndex = pointIndex; continue; }
            else
            {
                Vector3 pointPos = rootPoints.GetChild(pointIndex).position;

                if (obstacleJustSet)
                {
                    // if obstacle just set reset distance
                    distanceFromObstacle = (pointPos - lastObstaclePosition).magnitude;
                }
                else
                {
                    // add the distance between points to distances scope
                    Vector3 previousPointPos = rootPoints.GetChild(previousPointIndex).position;
                    distanceFromObstacle += (pointPos - previousPointPos).magnitude;
                }

                // current index of the point
                int index = acceptablePointIndices.IndexOf(pointIndex);

                if (index + 1 < acceptablePointIndices.Count)
                {
                    int nextPointIndex = acceptablePointIndices[index + 1];
                    Vector3 nextPointPos = rootPoints.GetChild(nextPointIndex).position;
                    float futureDistanceFromObstacle = distanceFromObstacle + (nextPointPos - pointPos).magnitude;

                    // check if in the next iteration distance will be needed
                    if (futureDistanceFromObstacle > randomSpawnDistance) 
                    { 
                        float distanceOffset = randomSpawnDistance - futureDistanceFromObstacle;
                        // find the new obstacle point on the line between two points
                        lastObstaclePosition = pointPos + (nextPointPos - pointPos).normalized * distanceOffset;
                        SetObstacle(lastObstaclePosition);
                        // reset random distance
                        randomSpawnDistance = Random.Range(minObstacleSpawnDistance, maxObstacleSpawnDistance);
                        obstacleJustSet = true;
                    }
                    else obstacleJustSet = false;
                }
                else break;
            }

            previousPointIndex = pointIndex;
        }
    }

    void SetObstacle(Vector3 position)
    {
        GameObject obstaclePrefab = ChooseObstacleToSpawn();

        if (obstaclePrefab != null)
        {
            Instantiate(obstaclePrefab, position, Quaternion.identity, obstaclesParent);
        }
    }

    GameObject ChooseObstacleToSpawn()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1);

        float cumulativeWeight = 0f;
        foreach (var entry in posiibilitiesOfSpawning)
        {
            cumulativeWeight += entry.Value;
            if (randomValue <= cumulativeWeight)
            {
                return entry.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if the obstacles can be placed
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool CheckPointIndex(int index)
    {
        // spawning points: 6-41 104-144
        return (index >= 2 && index <= 41) || (index >= 104 && index <= 144);
    }
}
