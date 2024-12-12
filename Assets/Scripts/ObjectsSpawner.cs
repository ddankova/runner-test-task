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

    float minObstacleSpawnDistance = 10;
    float maxObstacleSpawnDistance = 25;

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

        int pointIndex = 0;
        // stores the points within that the obstacle will be placed
        List<int> indexesOfCurrentobstacle = new List<int>();

        while (pointIndex < pointsCount - 1)
        {
            // clear previous points
            indexesOfCurrentobstacle.Clear();

            // check if the index is acceptable
            if (CheckPointIndex(pointIndex))
            {
                indexesOfCurrentobstacle.Add(pointIndex);
                int previousPointIndex = pointIndex;
                Vector3 previousPointPos = rootPoints.GetChild(previousPointIndex).transform.position;

                int nextPointIndex = pointIndex + 1;
                float distanceToNextPoint = (rootPoints.GetChild(nextPointIndex).transform.position - previousPointPos).magnitude;

                while (distanceToNextPoint < minObstacleSpawnDistance)
                {
                    previousPointIndex = nextPointIndex;
                    indexesOfCurrentobstacle.Add(previousPointIndex);
                    previousPointPos = rootPoints.GetChild(previousPointIndex).transform.position;
                    nextPointIndex++;

                    if (CheckPointIndex(nextPointIndex))
                    {
                        distanceToNextPoint += (rootPoints.GetChild(nextPointIndex).transform.position - previousPointPos).magnitude;
                    }
                    else
                    {
                        break;
                    }
                }

                Debug.Log("----------------------indexes of current obstacles------------------------");
                indexesOfCurrentobstacle.ForEach(i => Debug.Log(i));
            }

            pointIndex++;
        }

        GameObject nextObstacleToSpawn = ChooseObstacleToSpawn();
        if (nextObstacleToSpawn != null)
        {
            
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
        return (index >= 6 && index <= 41) || (index >= 104 && index <= 144);
    }
}
