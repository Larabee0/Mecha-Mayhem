using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarriersGoDown : MonoBehaviour
{
    [Space]
    [Header("Height of barrier")]
    [SerializeField] float barrierHeight;

    [Space]
    [Header("Range of time between barriers")]
    [SerializeField] int minTimeBetweenBarriers;
    [SerializeField] int maxTimeBetweenBarriers;

    [Space]
    [Header("Number of Barriers going down at one time")]
    [SerializeField] int numberOfBarriers;

    List<GameObject> barriers;

    bool spacebarReady = true;
    
    // Start is called before the first frame update
    void Start()
    {
        barriers.AddRange(GameObject.FindGameObjectsWithTag("Barrier"));
        StartCoroutine(BarriersMovingDown());
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
            SendMessage("MoveDown", barrierToMove);

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
            int spacebarBarrierNumber = barriers.Count / 2;
            for (int i = 0; i < spacebarBarrierNumber; i++)
            {

                int barrierNumber = Random.Range(0, barriers.Count - 1);
                GameObject barrierToMove = barriers[barrierNumber];
                barriers.RemoveAt(barrierNumber);
                SendMessage("MoveDown", barrierToMove);

            }
        }
    }
}
