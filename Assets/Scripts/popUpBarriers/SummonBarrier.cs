using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SummonBarrier : MonoBehaviour
{
    //[Header("Floor")]
    //[SerializeField] GameObject floor;

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

    [Space]
    [Header("Map Range Values")]
    [SerializeField] float xStart;
    [SerializeField] float zStart;
    [SerializeField] float xEnd;
    [SerializeField] float zEnd;

    [Space]
    [Header("Range of time between barriers")]
    [SerializeField] int minTimeBetweenBarriers;
    [SerializeField] int maxTimeBetweenBarriers;

    [Space]
    [Header("Symmetry mode")]
    public bool symetryMode;

    float barrierxPopUpStart;
    float barrierzPopUpStart;
    float barrierxPopUpRange;
    float barrierzPopUpRange;


    System.Random rand;

    GameObject[] barriers;

    private void Start()
    {
        
        barriers = new GameObject[] { barrierL1, barrierL2, barrierI, barrierX };

        barrierxPopUpStart = xStart + margin;
        barrierzPopUpStart = zStart + margin;
        barrierxPopUpRange = xEnd - margin;
        barrierzPopUpRange = zEnd - margin;

        StartCoroutine("SpawnBarrier");

    }


    IEnumerator SpawnBarrier()
    {
        rand = new System.Random();

        int timeBetweenBarriers = rand.Next(minTimeBetweenBarriers, maxTimeBetweenBarriers);

        int barrierIndex = rand.Next(0,barriers.Length);

        //int rotationOfBarrier = rand.Next(0, 3);
        //if (rotationOfBarrier == 0) { rotationOfBarrier = 0; }
        //else if (rotationOfBarrier == 1) { rotationOfBarrier = 90; }
        //else if (rotationOfBarrier == 2) { rotationOfBarrier = 180; }
        //else if (rotationOfBarrier == 3) { rotationOfBarrier = 270; }

        yield return new WaitForSeconds(timeBetweenBarriers);

        if (!symetryMode)
        {
            Instantiate(barriers[barrierIndex], new Vector3(Random.Range(barrierxPopUpStart, barrierxPopUpRange), -barrierHeight, Random.Range(barrierzPopUpStart, barrierzPopUpRange)), Quaternion.identity);
                //Quaternion.Euler(0, rotationOfBarrier, 0));
        }
        
        StartCoroutine("SpawnBarrier");
    }
}
