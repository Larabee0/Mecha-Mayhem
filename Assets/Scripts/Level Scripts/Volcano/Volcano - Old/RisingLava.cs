using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class RisingLava : MonoBehaviour
    {
        [Header("Lava Charecteristics: \n")]
        [SerializeField] private int EruptionInterval = 60;
        [SerializeField] private int Duration = 10;
        [SerializeField] public int damage = 110;

        public Vector3 LavaSpeed;
        public float timer = 0f;
        public Vector3 destination;
        public Vector3 origin;
        // Start is called before the first frame update
        void Start()
        {
            timer = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= timer + EruptionInterval)
            {
                Erupt();
            }
        }

        public void Erupt()
        {
            if (Time.time < timer + Duration + EruptionInterval && gameObject.transform.position.y <= destination.y)
            {
                RaiseLava();
            }
            else if (Time.time > timer + Duration + EruptionInterval && gameObject.transform.position.y > origin.y)
            {
                LowerLava();
            }
            else if (Time.time > timer + Duration + EruptionInterval && gameObject.transform.position.y <= origin.y)
            {
                timer = Time.time;
            }
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
                mech.UpdateHealth(damage);
            }
        }
    }
}
