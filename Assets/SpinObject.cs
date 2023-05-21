using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 1f;
    // Update is called once per frame
    void Update()
    {
        Vector3 angle = transform.localEulerAngles;
        angle.y += spinSpeed * Time.deltaTime;
        transform.localEulerAngles = angle;
    }
}
