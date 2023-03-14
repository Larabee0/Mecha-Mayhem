using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class RiverFlooding : MonoBehaviour
    {
        [Header("Flood Charecteristics: \n")]
        public int FloodInterval;
        public int Duration;
        public float FloodSpeed = 5f;
        public float timer = 0f;
        public Vector3 destination;
        public Vector3 origin;
        private Vector3 FloodSpeedV3;
        public Transform RiverTransform;
        public float RiverFinalWidth;
        // Start is called before the first frame update
        void Start()
        {
            timer = Time.time;
            FloodSpeedV3.Set(0, FloodSpeed, 0);
            GameObject FloodCylinder = GameObject.Find("FloodCylinder");
            RiverTransform = FloodCylinder.GetComponentInChildren<Transform>();
            RiverFinalWidth = RiverTransform.localScale.x;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= timer + FloodInterval)
            {
                Flood();
            }
        }

        public void Flood()
        {
            if (Time.time < timer + Duration + FloodInterval && gameObject.transform.position.y <= destination.y)
            {
                RaiseRiver();
            }
            if (Time.time > timer + Duration + FloodInterval && gameObject.transform.position.y > origin.y)
            {
                LowerRiver();
            }
            else if (Time.time > timer + Duration + FloodInterval && gameObject.transform.position.y <= origin.y)
            {
                timer = Time.time;
            }
        }

        public void RaiseRiver()
        {
            gameObject.transform.Translate(FloodSpeedV3 * Time.deltaTime, Space.World);
        }


        public void LowerRiver()
        {
            gameObject.transform.Translate(-FloodSpeedV3 * Time.deltaTime, Space.World);
        }
    }
}
