using System.Collections;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class WaterLevel : MonoBehaviour
    {
        private int FloodInterval;
        private int Duration;
        private float FloodSpeed;
        public float timer = 0f;
        public Vector3 destination;
        public Vector3 origin;
        private Vector3 FloodSpeedV3;
        // Use this for initialization
        void Start()
        {
            GameObject RiverWater = GameObject.Find("RiverWater");
            FloodSpeed = RiverWater.GetComponent<RiverFlooding>().FloodSpeed;
            destination = RiverWater.GetComponent<RiverFlooding>().destination;
            Duration = RiverWater.GetComponent<RiverFlooding>().Duration;
            FloodInterval = RiverWater.GetComponent<RiverFlooding>().FloodInterval;
            FloodSpeedV3.Set(0, FloodSpeed, 0);
            destination.y = destination.y / 5;
            FloodSpeedV3.y = FloodSpeedV3.y / 5;
        }

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