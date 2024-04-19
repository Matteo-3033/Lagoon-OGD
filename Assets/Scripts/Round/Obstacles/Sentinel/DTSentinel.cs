using UnityEngine;
using System.Collections;

namespace Round.Obstacles.Sentinel
{
    public class DTSentinel : MonoBehaviour
    {

        private float range = 60f;
        public float reactionTime = 3f;
        public string targetTag = "Player";

        public Light alarmLight = null;
        public Color alarmColor = Color.red;
        private DecisionTree dt;
        private Color baseColor; // to reset the light correctly
        [SerializeField] public LayerMask obstructionMask;
        [SerializeField] public LayerMask targetMask;

        void Start()
        {

            // Define actions
            //DTAction a1 = new DTAction(Show);
            DTAction a1 = new DTAction(Alarm);
            DTAction a2 = new DTAction(NotAlarm);
            // Define decisions
            DTDecision d1 = new DTDecision(ScanField);
            DTDecision d2 = new DTDecision(VisibleEnemy);
            // Link action with decisions

            d1.AddLink(true, d2);
            d2.AddLink(true, a1);
            //d2.AddLink(false, a1);
            d1.AddLink(false, a2);

            // Setup my DecisionTree at the root node
            dt = new DecisionTree(d1);

            // Set to hidden status
            baseColor = alarmLight.color;
            // same as - a2.Action();

            // Start patroling
            StartCoroutine(Patrol());
        }

        // GIMMICS

        private void OnValidate()
        {
            Transform t = transform.Find("Range");
            if (t != null)
            {
                // we assume it is a plane 
                t.localScale = new Vector3(range / transform.localScale.x, 1f, range / transform.localScale.z) / 5f;
            }
        }

        // Take decision every interval, run forever
        public IEnumerator Patrol()
        {
            while (true)
            {
                dt.walk();
                yield return new WaitForSeconds(reactionTime);
            }
        }

        // ACTIONS


        public object Alarm(object o)
        {
            alarmLight.color = alarmColor;
            return null;
        }

        public object NotAlarm(object o)
        {
            alarmLight.color = baseColor;
            return null;
        }

        // DECISIONS

        // Check if there are enemies in range
        public object ScanField(object o)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(targetTag))
            {
                Vector3 newVector = go.transform.position - transform.position;

                if ((Vector3.Angle(newVector, transform.forward) < 45.0f) &&
                    ((go.transform.position - transform.position).magnitude <= range))
                {
                    return true;

                }
            }

            return false;
        }

        public object VisibleEnemy(object o)
        {
            Collider[] rangeChecks = Physics.OverlapSphere(transform.position, range, targetMask);

            if (rangeChecks.Length != 0)
            {
                Transform target = rangeChecks[0].transform;
                Debug.Log(target.position);
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                float distanceTotarget = Vector3.Distance(transform.position, target.position);
                if (!(Physics.Raycast(transform.position, directionToTarget, out RaycastHit hitinfo, distanceTotarget,
                        obstructionMask)))
                {


                    Debug.Log("MUROOOOO");

                    return true;
                }
            }

            return false;
        }

    }
}