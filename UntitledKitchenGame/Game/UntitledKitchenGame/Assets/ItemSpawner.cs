using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public OrderGenerator orderGenerator;

    public Transform upperSpawnLimit;
    public Transform lowerSpawnLimit;

    public float spawnRate;
    float spawnDelay;
    Vector3 spawnPos;
    private void Update()
    {
        spawnConditionals();
    }
    protected virtual void spawnConditionals()
    {
        spawnDelay++;
        //print(spawnDelay * Time.deltaTime);
        if (spawnDelay * Time.deltaTime >= spawnRate)
        {
            spawnObjects();
            spawnDelay = 0;
        }
    }

    protected virtual void spawnObjects()
    {
        float scalar = Random.Range(0.1f, 0.9f);
        print(scalar);
        //they need to spawn within a set height of the conveyor belt.\
        float distX = upperSpawnLimit.position.x - lowerSpawnLimit.position.x;
        float distY = upperSpawnLimit.position.y - lowerSpawnLimit.position.y;

        float modX = (distX * scalar) + lowerSpawnLimit.position.x;
        float modY = (distY * scalar) + lowerSpawnLimit.position.y;

        spawnPos = new Vector3(modX, modY, 0);

        FoodItem spawnedItem = orderGenerator.SelectRandomItem();

        Instantiate(spawnedItem, spawnPos, Quaternion.identity);
        Debug.Log(spawnedItem.displayName);
    }
}
