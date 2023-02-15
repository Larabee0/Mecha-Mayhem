using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTimeScript : MonoBehaviour
{
    [SerializeField]
    private float time;

    void Start()
    {
        Destroy(gameObject, time);
    }
}
