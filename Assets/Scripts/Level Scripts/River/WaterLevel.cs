using System.Collections;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class WaterLevel : MonoBehaviour
    {
        public float Interval;
        public float Duration;
        public float timer = 0f;
        private Vector3 RiverFinalWidth;
        private Vector3 OriginalWidth;
        private float speed = 0.7f;
        private bool IsGimmickOccuring;

        // Use this for initialization
        void Start()
        {
            
            GameObject GameArb = GameObject.Find("GameArbiter");
            Interval = GameArb.GetComponent<GimmickCore>().Interval;
            Duration = GameArb.GetComponent<GimmickCore>().Duration;
            OriginalWidth.Set(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            GameObject RiverWater = GameObject.Find("FloodWater");
            RiverFinalWidth.Set(RiverWater.GetComponent<RiverFlooding>().RiverFinalWidth, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }

        void Update()
        {
            GameObject GameArb = GameObject.Find("GameArbiter");
            IsGimmickOccuring = GameArb.GetComponent<GimmickCore>().IsGimmickOccuring;
            if (IsGimmickOccuring == true)
            {
                Flood();
            }
        }

        public void Flood()
        {
            if (Time.time < timer + Duration + Interval && gameObject.transform.localScale.x <= RiverFinalWidth.x)
            {
                RaiseRiver();
            }
            if (Time.time > timer + Duration + Interval && gameObject.transform.localScale.x > OriginalWidth.x)
            {
                LowerRiver();
            }
            else if (Time.time > timer + Duration + Interval && gameObject.transform.localScale.x <= OriginalWidth.x)
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