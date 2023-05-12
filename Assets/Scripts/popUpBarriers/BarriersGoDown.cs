using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedButton.GamePlay;

public class BarriersGoDown : MonoBehaviour
{
    [Space]
    [Header("Range of time between barriers")]
    [SerializeField] int minTimeBetweenBarriers;
    [SerializeField] int maxTimeBetweenBarriers;

    [Space]
    [Header("Number of Barriers going down at one time")]
    [SerializeField] int numberOfBarriers;

    [Space]
    [Header("Barrier Level Prefabs")]
    [SerializeField] private GameObject[] barrierPrefabs;
    

    List<GameObject> barriers;
    
    bool spacebarReady = true;

    private void SummonBarriers()
    {
        int prefabToUse = Random.Range(0, barrierPrefabs.Length - 1);
        Instantiate(barrierPrefabs[prefabToUse]);
    }
    // Start is called before the first frame update
    void Start()
    {

        FindObjectOfType<GameArbiter>().OnRoundStarted += BarriersRound;

    }
    private void BarriersRound()
    {
        LookForBarriersDelete();
        SummonBarriers();
        LookForBarriers();
        ResetSpaceBar();
    }
    private void LookForBarriersDelete()
    {
        barriers = new List<GameObject>();
        barriers.AddRange(GameObject.FindGameObjectsWithTag("Barrier"));

        foreach (GameObject barrier in barriers)
        {
            Destroy(barrier);
        }
    }
    private void LookForBarriers()
    {
        barriers = new List<GameObject>();
        barriers.AddRange(GameObject.FindGameObjectsWithTag("Barrier"));

        StartCoroutine(BarriersMovingDown());
    }
    private void ResetSpaceBar()
    {
        spacebarReady = true;
    }
    // Update is called once per frame
    void Update()
    {

        SpaceBarPresed();
    }

    IEnumerator BarriersMovingDown()
    {
        int timeBetweenBarriers = Random.Range(minTimeBetweenBarriers, maxTimeBetweenBarriers);

        for (int i = 0; i < numberOfBarriers; i++)
        {
            
            int barrierNumber = Random.Range(0, barriers.Count - 1);
            GameObject barrierToMove = barriers[barrierNumber];
            barriers.RemoveAt(barrierNumber);
            barrierToMove.SendMessage("MoveDown");

        }
        yield return new WaitForSeconds(timeBetweenBarriers);
        if (barriers.Count > 0)
        {
            StartCoroutine(BarriersMovingDown());
        }
    }

    private void SpaceBarPresed()
    {
        if (spacebarReady && Input.GetKeyDown(KeyCode.Space))
        {
            spacebarReady = false;
            minTimeBetweenBarriers = minTimeBetweenBarriers / 2;
            maxTimeBetweenBarriers = maxTimeBetweenBarriers / 2;
            int spacebarBarrierNumber = barriers.Count / 2;
            for (int i = 0; i < spacebarBarrierNumber; i++)
            {

                int barrierNumber = Random.Range(0, barriers.Count - 1);
                GameObject barrierToMove = barriers[barrierNumber];
                barriers.RemoveAt(barrierNumber);
                barrierToMove.SendMessage("MoveDown");

            }
        }
    }
}
