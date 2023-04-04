using System.Collections;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class WaterLevel : MonoBehaviour
    {
        public int FloodInterval;
        public int Duration;
        public float timer = 0f;
        private Vector3 RiverFinalWidth;
        private Vector3 OriginalWidth;
        private float speed = 0.7f;
        // Use this for initialization
        void Start()
        {
            GameObject RiverWater = GameObject.Find("FloodWater");
            Duration = RiverWater.GetComponent<RiverFlooding>().Duration;
            FloodInterval = RiverWater.GetComponent<RiverFlooding>().FloodInterval;
            OriginalWidth.Set(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            RiverFinalWidth.Set(RiverWater.GetComponent<RiverFlooding>().RiverFinalWidth, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
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
            if (Time.time < timer + Duration + FloodInterval && gameObject.transform.localScale.x <= RiverFinalWidth.x)
            {
                RaiseRiver();
            }
            if (Time.time > timer + Duration + FloodInterval && gameObject.transform.localScale.x > OriginalWidth.x)
            {
                LowerRiver();
            }
            else if (Time.time > timer + Duration + FloodInterval && gameObject.transform.localScale.x <= OriginalWidth.x)
            {
                timer = Time.time;
            }
        }

        public void RaiseRiver()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, RiverFinalWidth, speed * Time.deltaTime);
        }


        public void LowerRiver()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, OriginalWidth, speed * Time.deltaTime);
        }
    }
}