using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BarrierMoves : MonoBehaviour
{
    [SerializeField] int timeUp;

    [SerializeField] float barrierHeight;

    [SerializeField] float animWaitTime;

    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(Move());
    }
    private void Update()
    {
        
    }
    IEnumerator Move()
    {
        yield return new WaitForSeconds(animWaitTime);
        for (float i = 0; i < barrierHeight; i += 0.1f)
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, 0.1f, 0);
            yield return new WaitForSeconds(0.01f);

        }
        yield return new WaitForSeconds(timeUp);
        for (float i = barrierHeight; i > 0; i -= 0.1f)
        {
            gameObject.transform.position = gameObject.transform.position - new Vector3(0, 0.1f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(gameObject);
        
    }
    
}
