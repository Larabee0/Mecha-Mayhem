using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class RisingLava : MonoBehaviour
    {
        private float Interval;
        private float Duration;
        private int Damage;

        public Vector3 LavaSpeed;
        private float Lavatick;
        public float timer = 0f;
        public Vector3 destination;
        public Vector3 origin;
        private bool IsGimmickOccuring;
        private bool LocalGim = false;

        // Start is called before the first frame update
        void Start()
        {
            timer = Time.time;
            GameObject GameArb = GameObject.Find("GameArbiter");
            Damage = GameArb.GetComponent<GimmickCore>().VolcanoDamage;
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
            while (gameObject.transform.position.y <= destination.y)
            {
                yield return new WaitForSeconds(Lavatick);
                RaiseLava();
            }
            yield return new WaitForSeconds(Duration);
            while (gameObject.transform.position.y > origin.y)
            {
                yield return new WaitForSeconds(Lavatick);
                LowerLava();
            }
            LocalGim = false;
            StopCoroutine(Erupt());           
        }

        public void RaiseLava()
        {
            gameObject.transform.Translate(LavaSpeed * Time.deltaTime);
        }

             
        public void LowerLava()
        {
            gameObject.transform.Translate(-LavaSpeed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Mech.CentralMechComponent mech = collision.gameObject.GetComponentInParent<Mech.CentralMechComponent>();
                mech.UpdateHealth(Damage);
            }
        }
    }
}
