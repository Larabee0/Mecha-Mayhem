using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BarrierMoves : MonoBehaviour
{
    [SerializeField] GameObject thingToDestroy;

    [SerializeField] int timeUp;

    [SerializeField] float barrierHeight;

    [SerializeField] float animWaitTime;

    
    IEnumerator MoveDown()
    {
        
        for (float i = barrierHeight; i > 0; i -= 0.1f)
        {
            gameObject.transform.position = gameObject.transform.position - new Vector3(0, 0.1f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(thingToDestroy);
        
    }
    
}
