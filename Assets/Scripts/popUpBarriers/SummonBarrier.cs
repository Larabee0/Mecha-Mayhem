using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SummonBarrier : MonoBehaviour
{
    [Header("Floor")]
    [SerializeField] GameObject floor;


    [Space]
    [Header("Barrier Prefabs")]
    [SerializeField] GameObject barrierL1;
    [SerializeField] GameObject barrierL2;
    [SerializeField] GameObject barrierI;
    [SerializeField] GameObject barrierX;


    [Space]
    [Header("Height of barrier")]
    [SerializeField] float barrierHeight;


    [Space]
    [Header("Margin For Barrier Spawn Range")]
    [SerializeField] float margin;
    [SerializeField] float marginPlayer;
    [SerializeField] float rangePlayer;


    [Space]
    [Header("Range of time between barriers")]
    [SerializeField] int minTimeBetweenBarriers;
    [SerializeField] int maxTimeBetweenBarriers;


    float xStart;
    float zStart;
    float xEnd;
    float zEnd;

    Rect barrierSpawn;
    Rect barrierLimit;
    Rect barrierCoords;



    System.Random rand;

    GameObject[] barriers;

    GameObject[] mechs;

    private void Start()
    {

        Bounds floorSize = gameObject.GetComponent<MeshRenderer>().bounds;
        

        xStart = floorSize.min.x;
        xEnd = floorSize.max.x;
        zStart = floorSize.min.z;
        zEnd = floorSize.min.z;


        barriers = new GameObject[] { barrierL1, barrierL2, barrierI, barrierX };

        mechs = GameObject.FindGameObjectsWithTag("Player");
        

        StartCoroutine("SpawnBarrier");

    }



    IEnumerator SpawnBarrier()
    {
        rand = new System.Random();

        int timeBetweenBarriers = rand.Next(minTimeBetweenBarriers, maxTimeBetweenBarriers);

        //int barrierIndex = rand.Next(0, barriers.Length);

        int rotationOfBarrier = rand.Next(0, 3);
        rotationOfBarrier *= 90;


        foreach (GameObject mech in mechs)
        {

            barrierCoords = new Rect(mech.transform.position.x, mech.transform.position.z, rangePlayer, rangePlayer);
            Vector3 barrierSpawnRange = new Vector3(Random.Range(barrierCoords.yMin, barrierCoords.xMax), -barrierHeight, Random.Range(barrierCoords.yMin, barrierCoords.yMax));
            barrierLimit = new Rect(floor.transform.position.x, floor.transform.position.z, floor.GetComponent<MeshRenderer>().bounds.extents.x - margin, floor.GetComponent<MeshRenderer>().bounds.extents.z - margin);
            barrierSpawn = new Rect(mech.transform.position.x, mech.transform.position.z, marginPlayer, marginPlayer);

            if (barrierLimit.Contains(barrierSpawnRange) && !barrierSpawn.Contains(barrierSpawnRange))
            {
                GameObject barrierToSpawn = Instantiate(barriers[rand.Next(0, barriers.Length)], barrierSpawnRange, Quaternion.identity);
                barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
            }
            
        }

        yield return new WaitForSeconds(timeBetweenBarriers);

        StartCoroutine("SpawnBarrier");
    }
}
//GameObject barrierToSpawn = Instantiate(barriers[barrierIndex], Vector3.zero, Quaternion.identity);
//barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);