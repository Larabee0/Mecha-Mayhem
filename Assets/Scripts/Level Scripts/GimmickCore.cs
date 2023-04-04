using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class GimmickCore : MonoBehaviour
    {
        [Header("Timings")]
        [SerializeField] private float MaxTimeInterval;
        [SerializeField] private float MinTimeInterval;
        [SerializeField] private float SpacebarCooldown;
        [SerializeField] private float MaxDuration;
        [SerializeField] private float MinDuration;

        [Space]
        [Header("Level Specific Parameters")]
        public int VolcanoLikeleyhood;
        public int VolcanoAmount;
        public int TestGroundLikelyhood;
        public int TestGroundAmount;

        private float Interval;
        private float Duration;
        private float TimeFlag;
        [HideInInspector] public bool IsGimmickOccuring = false;


        // Start is called before the first frame update
        void Start()
        {
            TimeFlag = Time.time;
            Interval = Random.Range(MinTimeInterval, MaxTimeInterval);
            Duration = Random.Range(MinDuration, MaxDuration);
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > TimeFlag + Interval && Time.time < TimeFlag + Interval + Duration)
            {
                IsGimmickOccuring = true;
            }
            else if (Time.time > TimeFlag + Interval + Duration)
            {
                IsGimmickOccuring = false;
                Interval = Random.Range(MinTimeInterval, MaxTimeInterval);
                Duration = Random.Range(MinDuration, MaxDuration);
                TimeFlag = Time.time;
            }
        }
    }
}
