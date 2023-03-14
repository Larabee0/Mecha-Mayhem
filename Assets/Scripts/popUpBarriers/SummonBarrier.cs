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

    [Space]
    [Header("Likelihood of Barriers - Where 0 is 0%, 1 is 50%...")]
    [SerializeField] int upperBound;
    [SerializeField] bool guaranteeBarriers;

    [Space]
    [Header("Number of Barriers")]
    
    [SerializeField] int numberOfBarriers;

    float xStart;
    float zStart;
    float xEnd;
    float zEnd;

    Rect barrierSpawn;
    Rect barrierLimit;
    Rect barrierCoords;
    Bounds floorSize;


    GameObject[] barriers;

    GameObject[] mechs;

    private void Start()
    {

        floorSize = floor.GetComponent<MeshCollider>().bounds;
        

        xStart = floorSize.min.x;
        xEnd = floorSize.max.x;
        zStart = floorSize.min.z;
        zEnd = floorSize.min.z;


        barriers = new GameObject[] { barrierL1, barrierL2, barrierI, barrierX };

        
        

        StartCoroutine("GetMechs");

    }

    IEnumerator GetMechs()
    {
        yield return new WaitForEndOfFrame();
        mechs = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(mechs.Length);
        StartCoroutine("SpawnBarrier");
    }
    
    IEnumerator SpawnBarrier()
    {   
        int timeBetweenBarriers = Random.Range(minTimeBetweenBarriers, maxTimeBetweenBarriers);
        
        int rotationOfBarrier = Random.Range(0, 3);
        rotationOfBarrier *= 90;
       
        for (int i = 1; i < numberOfBarriers; i++)
        {
            //List<Vector3> avoidVectors = new List<Vector3>();
            Vector3 spawnBarrierRange = new Vector3(Random.Range(floorSize.min.x + margin, floorSize.max.x - margin), floor.transform.position.y, Random.Range(floorSize.min.z + margin, floorSize.max.z - margin));
            GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], spawnBarrierRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
            barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
            //if (avoidVectors != null)
            //{
            //    foreach (Vector3 avoidVector in avoidVectors)
            //    {
            //        Vector3 newRect = avoidVector - new Vector3(rangePlayer, 0, rangePlayer);
            //        barrierLimit = new Rect(newRect.x, newRect.z, rangePlayer * 2, rangePlayer * 2);
            //        if (barrierLimit.Contains(spawnBarrierRange))
            //        {
            //            break;
            //        }
            //        else
            //        {
            //            GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], spawnBarrierRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
            //            barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
            //        }
            //    }
            //}
            //else
            //{
            //    GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], spawnBarrierRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
            //    barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
            //}
            //avoidVectors.Add(spawnBarrierRange);
        }

        //    foreach (GameObject mech in mechs)
        //    {

        //        barrierCoords = new Rect(mech.transform.position.x, mech.transform.position.z, rangePlayer, rangePlayer);
        //        Debug.Log(barrierCoords);
        //        Vector3 barrierSpawnRange = new Vector3(Random.Range(barrierCoords.xMin, barrierCoords.xMax), floor.transform.position.y, Random.Range(barrierCoords.yMin, barrierCoords.yMax));
        //        Debug.Log(barrierSpawnRange);
        //        barrierLimit = new Rect(floor.transform.position.x, floor.transform.position.z, floor.GetComponent<MeshCollider>().bounds.extents.x - margin, floor.GetComponent<MeshCollider>().bounds.extents.z - margin);
        //        Debug.Log(barrierLimit);


        //        if (floorSize.Contains(barrierSpawnRange))
        //        {
        //            Debug.Log("Got To Point A");
        //            if (!guaranteeBarriers)
        //            {
        //                int likelyhoodOfBarrier = Random.Range(0, upperBound);
        //                if (likelyhoodOfBarrier != 0)
        //                {
        //                    GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], barrierSpawnRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
        //                    barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
        //                }
        //            }
        //            else
        //            {
        //                GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], barrierSpawnRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
        //                barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
        //            }
        //        }

        //    }

        yield return new WaitForSeconds(timeBetweenBarriers);

        StartCoroutine("SpawnBarrier");
    }
}
//GameObject barrierToSpawn = Instantiate(barriers[barrierIndex], Vector3.zero, Quaternion.identity);
//barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);