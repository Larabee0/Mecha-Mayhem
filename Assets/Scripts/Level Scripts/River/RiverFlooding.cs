using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedButton.GamePlay
{
    public class RiverFlooding : MonoBehaviour
    {
        [Header("Pushers")]
        [SerializeField] private Rigidbody pusherRB;
        [SerializeField] private Transform pusherParent;
        [SerializeField] private Vector2 pusherMinMaxHeight;
        [SerializeField] private float raiseLowerTime;

        [Header("Flood Charecteristics: \n")]
        public float Interval;
        public float Duration;
        public float FloodSpeed = 5f;
        public float timer = 0f;
        private float Floodtick;
        public Vector3 destination;
        public Vector3 origin;
        private Vector3 FloodSpeedV3;
        public Transform RiverTransform;
        public float RiverFinalWidth;
        private bool IsGimmickOccuring;
        private bool LocalGim = false;
        // Start is called before the first frame update
        void Start()
        {
            timer = Time.time;
            FloodSpeedV3.Set(0, FloodSpeed, 0);
            GameObject GameArb = GameObject.Find("GameArbiter");
            Interval = GameArb.GetComponent<GimmickCore>().Interval;
            Duration = GameArb.GetComponent<GimmickCore>().Duration;
        }

        // Update is called once per frame
        void Update()
        {
            GameObject GameArb = GameObject.Find("GameArbiter");
            IsGimmickOccuring = GameArb.GetComponent<GimmickCore>().IsGimmickOccuring;
            if (IsGimmickOccuring == true)
            {
                if (LocalGim == false)
                {
                    LocalGim = true;
                    Interval = GameArb.GetComponent<GimmickCore>().Interval;
                    Duration = GameArb.GetComponent<GimmickCore>().Duration;
                    StartCoroutine(Erupt());
                }

            }
        }

        IEnumerator Erupt()
        {
            for (float i = 0; i < raiseLowerTime; i += Time.deltaTime)
            {
                float time = Mathf.InverseLerp(0, raiseLowerTime, i);
                Vector3 waterPos = transform.position;
                waterPos.y = Mathf.Lerp(origin.y, destination.y, time);
                transform.position = waterPos;

                Vector3 pusherPos = pusherParent.position;
                pusherPos.y = Mathf.Lerp(pusherMinMaxHeight.x,pusherMinMaxHeight.y, time);
                //pusherParent.transform.localPosition = pusherPos;
                pusherRB.MovePosition(pusherPos);
                yield return null;
            }
            // while (gameObject.transform.position.y <= destination.y)
            // {
            //     yield return new WaitForSeconds(Floodtick);
            //     RaiseRiver();
            // }
            yield return new WaitForSeconds(Duration);



            for (float i = 0; i < raiseLowerTime; i += Time.deltaTime)
            {
                float time = Mathf.InverseLerp( 0, raiseLowerTime, i);
                Vector3 waterPos = transform.position;
                waterPos.y = Mathf.Lerp(destination.y, origin.y, time);
                transform.position = waterPos;

                Vector3 pusherPos = pusherParent.position;
                pusherPos.y = Mathf.Lerp(pusherMinMaxHeight.y, pusherMinMaxHeight.x, time);
                //pusherParent.transform.localPosition = pusherPos;
                pusherRB.MovePosition(pusherPos);
                yield return null;
            }

            // while (gameObject.transform.position.y > origin.y)
            // {
            //     yield return new WaitForSeconds(Floodtick);
            //     LowerRiver();
            // }
            LocalGim = false;
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
