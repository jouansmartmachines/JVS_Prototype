using UnityEngine;

namespace Monstres
{
    public class Monstres_AccueilTarget : MonoBehaviour
    {
        private Monstres_AccueilPath pathToFollow;
        private int pathPointIdx = 0;
        private float speed = 0f;

        // Update is called once per frame
        void Update()
        {
            if (transform.position != pathToFollow.pathPoints[pathPointIdx].transform.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, pathToFollow.pathPoints[pathPointIdx].position, speed * Time.deltaTime);
            }
            else if (transform.position == pathToFollow.pathPoints[pathPointIdx].transform.position && pathPointIdx < pathToFollow.pathPoints.Count - 1)
            {
                NextPathPoint();
            }
            else if (transform.position == pathToFollow.pathPoints[pathPointIdx].transform.position && pathPointIdx >= pathToFollow.pathPoints.Count - 1)
            {
                Destroy(gameObject);
            }
        }

        public void SetupBaseVariables(Monstres_AccueilPath newPathToFollow, float newSpeed, int newLayer)
        {
            pathToFollow = newPathToFollow;
            speed = newSpeed;

            foreach (Transform child in transform)
            {
                if (child.GetComponent<SpriteRenderer>())
                {
                    child.GetComponent<SpriteRenderer>().sortingOrder = newLayer;
                }
            }
        }

        void NextPathPoint()
        {
            pathPointIdx++;
        }
    }
}
