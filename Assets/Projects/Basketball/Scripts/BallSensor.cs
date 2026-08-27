using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basket
{
    public class BallSensor : MonoBehaviour
    {
        public ThrowSensor sensor { get; set; }
        [SerializeField] AudioSource _source;

        public IEnumerator Start()
        {
            yield return new WaitForSeconds(3.5f);
            Destroy(this.gameObject);
        }

        bool alreadyTouchGround = false;
        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Hoop"))
            {
                _source.clip = Basket_SoundManager.i._hits[Random.Range(0, Basket_SoundManager.i._hits.Length)];
                _source.Play();
            }

            if (collision.gameObject.CompareTag("Floor"))
            {
                //_source.clip = Basket_SoundManager.i._bounce[Random.Range(0, Basket_SoundManager.i._bounce.Length)];
                //_source.Play();

                if (alreadyTouchGround)
                    Destroy(this.gameObject);
                else
                    alreadyTouchGround = true;
            }
        }

        public void OnDestroy()
        {
            if(sensor != null && !Basket_GameManager.i.Teams[sensor.Team ? 1 : 0].Next)
                sensor.CanShot = true;
        }
    }
}