using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedButton.GamePlay
{
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
        [SerializeField] GameObject barrierL1Spacebar;
        [SerializeField] GameObject barrierL2Spacebar;
        [SerializeField] GameObject barrierISpacebar;
        [SerializeField] GameObject barrierXSpacebar;


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

        [Space]
        [Header("Spacebar cool down")]
        [SerializeField] int spacebarCoolDown;

        float xStart;
        float zStart;
        float xEnd;
        float zEnd;

        Rect barrierSpawn;
        Rect barrierLimit;
        Rect barrierCoords;
        Bounds floorSize;

        bool spacebarBarriersReady = false;

        GameObject[] barriers;

        GameObject[] spaceBarBarriers;

        GameObject[] mechs;

        private void Start()
        {
            spacebarBarriersReady = true;
            floorSize = floor.GetComponent<MeshCollider>().bounds;


            xStart = floorSize.min.x;
            xEnd = floorSize.max.x;
            zStart = floorSize.min.z;
            zEnd = floorSize.min.z;


            barriers = new GameObject[] { barrierL1, barrierL2, barrierI, barrierX };

            spaceBarBarriers = new GameObject[] { barrierL1Spacebar, barrierL2Spacebar, barrierISpacebar, barrierXSpacebar };


            StartCoroutine(GetMechs());

        }

        void Awake()
        {
            GameObject GameArb = GameObject.Find("GameArbiter");
            numberOfBarriers = GameArb.GetComponent<GimmickCore>().TestGroundAmount;
        }
        private void SpaceBarBarriers()
        {
            if (spacebarBarriersReady)
            {
                //Debug.Log(spacebarBarriersReady);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    spacebarBarriersReady = false;
                    // Debug.Log(spacebarBarriersReady);
                    for (int i = 1; i < numberOfBarriers; i++)
                    {
                        int rotationOfBarrier = Random.Range(0, 3);
                        rotationOfBarrier *= 90;

                        Vector3 spawnBarrierRange = new(Random.Range(floorSize.min.x + margin, floorSize.max.x - margin), floor.transform.position.y, Random.Range(floorSize.min.z + margin, floorSize.max.z - margin));
                        GameObject barrierToSpawn = Instantiate(spaceBarBarriers[Random.Range(0, spaceBarBarriers.Length)], spawnBarrierRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
                        barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
                        StartCoroutine(SpacebarCD());
                    }
                }
            }
        }
        private void Update()
        {
            SpaceBarBarriers();
        }
        IEnumerator SpacebarCD()
        {
            yield return new WaitForSeconds(spacebarCoolDown);
            spacebarBarriersReady = true;
        }
        IEnumerator GetMechs()
        {
            yield return new WaitForEndOfFrame();

            CentralMechComponent[] mechComponents = FindObjectsOfType<CentralMechComponent>(true);
            mechs = new GameObject[mechComponents.Length];
            for (int i = 0; i < mechComponents.Length; i++)
            {
                mechs[i] = mechComponents[i].transform.root.gameObject;
            }
            //Debug.Log(mechs.Length);
            StartCoroutine(SpawnBarrier());
        }

        IEnumerator SpawnBarrier()
        {
            int timeBetweenBarriers = Random.Range(minTimeBetweenBarriers, maxTimeBetweenBarriers);

            int rotationOfBarrier = Random.Range(0, 3);
            rotationOfBarrier *= 90;



            foreach (GameObject mech in mechs)
            {

                barrierCoords = new Rect(mech.transform.position.x, mech.transform.position.z, rangePlayer, rangePlayer);
                //Debug.Log(barrierCoords);
                Vector3 barrierSpawnRange = new(Random.Range(barrierCoords.xMin, barrierCoords.xMax), floor.transform.position.y, Random.Range(barrierCoords.yMin, barrierCoords.yMax));
                //Debug.Log(barrierSpawnRange);
                barrierLimit = new Rect(floor.transform.position.x, floor.transform.position.z, floor.GetComponent<MeshCollider>().bounds.extents.x - margin, floor.GetComponent<MeshCollider>().bounds.extents.z - margin);
                //Debug.Log(barrierLimit);


                if (floorSize.Contains(barrierSpawnRange))
                {
                    //Debug.Log("Got To Point A");
                    if (!guaranteeBarriers)
                    {
                        int likelyhoodOfBarrier = Random.Range(0, upperBound);
                        if (likelyhoodOfBarrier != 0)
                        {
                            GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], barrierSpawnRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
                            barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
                        }
                    }
                    else
                    {
                        GameObject barrierToSpawn = Instantiate(barriers[Random.Range(0, barriers.Length)], barrierSpawnRange - new Vector3(0, barrierHeight, 0), Quaternion.identity);
                        barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);
                    }
                }

            }

            yield return new WaitForSeconds(timeBetweenBarriers);

            StartCoroutine(SpawnBarrier());
        }
    }
    //GameObject barrierToSpawn = Instantiate(barriers[barrierIndex], Vector3.zero, Quaternion.identity);
    //barrierToSpawn.transform.Rotate(0, rotationOfBarrier, 0);


    //Physics.SphereCast(barrierSpawnRange, 1.5f, new Vector3(1, 0, 0), out RaycastHit hitInfo, 8f);
    //Physics.SphereCast(barrierSpawnRange, 1.5f, new Vector3(-1, 0, 0), out RaycastHit hitInfo2, 8f);
    //Physics.SphereCast(barrierSpawnRange, 1.5f, new Vector3(0, 0, 1), out RaycastHit hitInfo3, 8f);
    //Physics.SphereCast(barrierSpawnRange, 1.5f, new Vector3(0, 0, 1), out RaycastHit hitInfo4, 8f);
    //if (hitInfo.collider.CompareTag("Player") || hitInfo.collider.CompareTag("Barrier") || hitInfo2.collider.CompareTag("Player") || hitInfo2.collider.CompareTag("Barrier") || hitInfo3.collider.CompareTag("Player") || hitInfo3.collider.CompareTag("Barrier") || hitInfo4.collider.CompareTag("Player") || hitInfo4.collider.CompareTag("Barrier"))
}