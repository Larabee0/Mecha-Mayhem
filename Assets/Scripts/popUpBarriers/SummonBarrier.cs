using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonBarrier : MonoBehaviour
{
    [Header("Barrier Prefabs")]
    [SerializeField] GameObject barrier1;
    [SerializeField] GameObject barrier2;
    [SerializeField] GameObject barrier3;
    [SerializeField] GameObject barrier4;
    [SerializeField] GameObject barrier5;
    [SerializeField] GameObject barrier6;
    [SerializeField] GameObject barrier7;

    [Header("Height of barrier")]
    [SerializeField] float barrierHeight;

    [Header("Margin For Barrier Spawn Range")]
    [SerializeField] float margin;

    float barrierxPopUpStart;
    float barrierzPopUpStart;
    float barrierxPopUpRange;
    float barrierzPopUpRange;

    float xMidPoint;
    float zMidPoint;

    System.Random rand;

    GameObject[] barriers;

    [Header("Map Range Values")]
    [SerializeField] float xStart;
    [SerializeField] float zStart;
    [SerializeField] float xEnd;
    [SerializeField] float zEnd;

    [Header("Range of time between barriers")]
    [SerializeField] int minTimeBetweenBarriers;
    [SerializeField] int maxTimeBetweenBarriers;

    [Header("Symmetry mode")]
    public bool symetryMode;

    private void Start()
    {

        barriers = new GameObject[] { barrier1, barrier2, barrier3, barrier4, barrier5, barrier6, barrier7 };

        barrierxPopUpStart = xStart + margin;
        barrierzPopUpStart = zStart + margin;
        barrierxPopUpRange = xEnd - margin;
        barrierzPopUpRange = zEnd - margin;
        xMidPoint = (xEnd - xStart)/2;
        zMidPoint = (zEnd - zStart)/2;

        StartCoroutine("SpawnBarrier");

    }


    IEnumerator SpawnBarrier()
    {
        rand = new System.Random();

        int timeBetweenBarriers = rand.Next(minTimeBetweenBarriers, maxTimeBetweenBarriers);

        int barrierIndex = rand.Next(0,barriers.Length);

        yield return new WaitForSeconds(timeBetweenBarriers);

        if (!symetryMode)
        {

            Instantiate(barriers[barrierIndex], new Vector3(Random.Range(barrierxPopUpStart, barrierxPopUpRange), -barrierHeight, Random.Range(barrierzPopUpStart, barrierzPopUpRange)), Quaternion.identity);
        }
        else if (symetryMode)
        {
            while (true)
            {
                Vector3 barrierLocation = new Vector3(Random.Range(barrierxPopUpStart, barrierxPopUpRange), -barrierHeight, Random.Range(barrierzPopUpStart, barrierzPopUpRange));
                if (barrierLocation != new Vector3(Random.Range(xMidPoint - margin, xMidPoint + margin), -barrierHeight, Random.Range(zMidPoint - margin, zMidPoint + margin)))
                {
                    Vector3 barrier2Location = new Vector3(xEnd, -barrierHeight, zEnd) - barrierLocation - new Vector3(0, barrierHeight, 0);
                    Instantiate(barriers[barrierIndex], barrierLocation, Quaternion.identity);
                    Instantiate(barriers[barrierIndex], barrier2Location, Quaternion.identity);
                    break;
                }
            }
        }
        StartCoroutine("SpawnBarrier");
    }
}
