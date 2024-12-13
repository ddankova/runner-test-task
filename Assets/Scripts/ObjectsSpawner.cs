using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject barrel;
    [SerializeField] GameObject bush;
    [SerializeField] GameObject cart;
    [SerializeField] GameObject crates;

    [SerializeField] GameObject blueBonus;
    [SerializeField] GameObject greenBonus;
    [SerializeField] GameObject yellowBonus;

    [SerializeField] Transform obstaclesParent;
    [SerializeField] Transform bonusParent;

    [SerializeField] Transform rootPoints;

    float currentDeviation = 2.5f;

    float regularJumpHeight = 3f;
    float minHeightOfBonusPlacing = 0.5f;

    float minObstacleSpawnDistance = 8;
    float maxObstacleSpawnDistance = 15;

    float bonusesRegularSpawnDistance = 8;

    Dictionary<GameObject, float> posiibilitiesOfSpawningObstacles = new Dictionary<GameObject, float>();
    Dictionary<GameObject, float> posiibilitiesOfSpawningBonuses = new Dictionary<GameObject, float>();

    // Start is called before the first frame update
    void Start()
    {
        if (posiibilitiesOfSpawningObstacles.Count == 0) 
        {
            posiibilitiesOfSpawningObstacles.Add(barrel, 0.35f);
            posiibilitiesOfSpawningObstacles.Add(crates, 0.35f);
            posiibilitiesOfSpawningObstacles.Add(bush, 0.2f);
            posiibilitiesOfSpawningObstacles.Add(cart, 0.1f);
        }

        if (posiibilitiesOfSpawningBonuses.Count == 0)
        {
            posiibilitiesOfSpawningBonuses.Add(blueBonus, 0.5f);
            posiibilitiesOfSpawningBonuses.Add(greenBonus, 0.4f);
            posiibilitiesOfSpawningBonuses.Add(yellowBonus, 0.1f);
        }
    }

    public void ResetSpawnObjects()
    {
        ResetObstacles();
        ResetBonuses();
    }

    public void DestroyAllSpawnedObjects()
    {
        int obstaclesCount = obstaclesParent.childCount;

        if (obstaclesCount > 0)
        {
            for (int i = 0; i < obstaclesCount; i++) Destroy(obstaclesParent.GetChild(i).gameObject);
        }

        int bonusesCount = bonusParent.childCount;

        if (bonusesCount > 0)
        {
            for (int i = 0; i < bonusesCount; i++) Destroy(bonusParent.GetChild(i).gameObject);
        }
    }

    void ResetObstacles()
    {
        int obstaclesCount = obstaclesParent.childCount;

        if (obstaclesCount > 0)
        {
            for (int i = 0; i < obstaclesCount; i++) Destroy(obstaclesParent.GetChild(i).gameObject);
        }

        // iterating through points to set obstacles
        int pointsCount = rootPoints.childCount;

        List<int> acceptablePointIndices = new List<int>();

        // creating the list of acceptable indices
        for (int i = 0; i < pointsCount; i++) 
        {
            if (CheckPointIndexForObstacles(i)) acceptablePointIndices.Add(i);
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
                        float distanceOffset = futureDistanceFromObstacle - randomSpawnDistance;
                        // find the new obstacle point on the line between two points
                        Vector3 direction = (nextPointPos - pointPos).normalized;
                        // adding the deviation
                        Vector3 deviationVector = GetDeviationVector(direction);

                        // placing the object with its position and deviation from the center of the road
                        lastObstaclePosition = pointPos + direction * distanceOffset + deviationVector;
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

    void ResetBonuses()
    {
        int bonusesCount = bonusParent.childCount;

        if (bonusesCount > 0)
        {
            for (int i = 0; i < bonusesCount; i++) Destroy(bonusParent.GetChild(i).gameObject);
        }

        // iterating through points to set obstacles
        int pointsCount = rootPoints.childCount;

        List<int> acceptablePointIndices = new List<int>();

        // creating the list of acceptable indices
        for (int i = 0; i < pointsCount; i++)
        {
            if (CheckPointIndexForBonuses(i)) acceptablePointIndices.Add(i);
        }

        // iterate through the list of acceptable indices
        foreach (int pointIndex in acceptablePointIndices)
        {
            // current index of the point
            int index = acceptablePointIndices.IndexOf(pointIndex);

            if (index + 1 < acceptablePointIndices.Count)
            {
                int nextPointIndex = acceptablePointIndices[index + 1];
                Vector3 nextPointPosition = rootPoints.GetChild(nextPointIndex).position;

                // setting the bonus in the root point
                Vector3 pointPosition = rootPoints.GetChild(pointIndex).position;
                Vector3 lastBonusPosition = pointPosition;

                Vector3 direction = (nextPointPosition - pointPosition).normalized;

                if (index == 0) SetBonus(pointPosition, direction, CheckPointOnWater(pointIndex));
                else
                {
                    if (bonusParent.childCount > 0)
                    {
                        Transform lastBonusSpawned = bonusParent.GetChild(bonusParent.childCount - 1);
                        if ((pointPosition - lastBonusSpawned.position).magnitude > bonusesRegularSpawnDistance) SetBonus(pointPosition, direction, CheckPointOnWater(pointIndex));
                    }
                }

                if (nextPointIndex - pointIndex > 1) { continue; }
                else
                {
                    float distanceBetweenPoints = (nextPointPosition - pointPosition).magnitude;
                    if (distanceBetweenPoints > (bonusesRegularSpawnDistance * 2)) 
                    { 
                        float bonusesCountInInterval = distanceBetweenPoints / bonusesRegularSpawnDistance - 1;
                        float bonusesPosOffset = distanceBetweenPoints / (bonusesCountInInterval + 1);

                        for (int i = 0; i <= bonusesCountInInterval; i++)
                        {
                            lastBonusPosition += direction * bonusesPosOffset;
                            SetBonus(lastBonusPosition, direction, CheckPointOnWater(pointIndex));
                        }
                    }
                }
            }
        }

        for (int i = 0; i < bonusParent.childCount; i++) 
        {
            Transform bonus = bonusParent.GetChild(i);
            SphereCollider collider = bonus.GetComponent<SphereCollider>();
            collider.enabled = true;
        }
    }

    /// <summary>
    /// Sets the obstacle
    /// </summary>
    /// <param name="position"></param>
    void SetObstacle(Vector3 position)
    {
        GameObject obstaclePrefab = ChooseObjectToSpawn(posiibilitiesOfSpawningObstacles);
        if (obstaclePrefab == crates || obstaclePrefab == cart) position += Vector3.up * 0.2f;

        if (obstaclePrefab != null)
        {
            Instantiate(obstaclePrefab, position, Quaternion.identity, obstaclesParent);
        }
    }

    /// <summary>
    /// Sets the bonus, adds random deviation and height
    /// </summary>
    /// <param name="position"></param>
    void SetBonus(Vector3 position, Vector3 direction, bool onWater)
    {
        GameObject bonusPrefab = ChooseObjectToSpawn(posiibilitiesOfSpawningBonuses);
        float randomHeight = onWater ? minHeightOfBonusPlacing : Random.Range(minHeightOfBonusPlacing, regularJumpHeight);
        Vector3 deviationVector = GetDeviationVector(direction);
        Vector3 finalBonusPosition = position + deviationVector + Vector3.up * randomHeight;

        if (bonusPrefab != null)
        {
            Instantiate(bonusPrefab, finalBonusPosition, Quaternion.identity, bonusParent);
        }
    }

    Vector3 GetDeviationVector(Vector3 direction)
    {
        // adding the deviation
        float deviationForUsing = currentDeviation - 0.2f;
        float randomDeviation = Random.Range(-deviationForUsing, deviationForUsing);
        Vector3 sideDirection = RotateVector(direction, 90f * Mathf.Sign(randomDeviation), Vector3.up);
        Vector3 deviationVector = sideDirection * Mathf.Abs(randomDeviation);

        return deviationVector;
    }

    GameObject ChooseObjectToSpawn(Dictionary<GameObject, float> possibilitiesDict)
    {
        float randomValue = UnityEngine.Random.Range(0f, 1);

        float cumulativeWeight = 0f;
        foreach (var entry in possibilitiesDict)
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
    bool CheckPointIndexForObstacles(int index)
    {
        return (index >= 2 && index <= 41) || (index >= 104 && index < 141);
    }

    /// <summary>
    /// Checks if the bonuses can be placed
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool CheckPointIndexForBonuses(int index)
    {
        return (index > 0 && index != 86 && index != 87 && index < 141);
    }

    bool CheckPointOnWater(int index)
    {
        string pointName = rootPoints.GetChild(index).name;
        List<string> pointNameList = DivideString(pointName);

        bool result = false;

        if (pointNameList != null)
        {
            string nameMarker = pointNameList[1];
            if (nameMarker == "river" || nameMarker == "boatPoint" || nameMarker == "exitRiverPoint") { result = true; }
        }

        return result;
    }

    List<string> DivideString(string input)
    {
        if (string.IsNullOrEmpty(input) || !input.Contains("_"))
        {
            return null;
        }

        string[] parts = input.Split('_');
        return new List<string>(parts);
    }

    Vector3 RotateVector(Vector3 vector, float angleInDegrees, Vector3 axis)
    {
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, axis); // Create rotation
        return rotation * vector; // Apply rotation to the vector
    }
}
