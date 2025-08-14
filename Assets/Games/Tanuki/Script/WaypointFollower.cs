using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [Header("Waypoint Path Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float waitTime = 1f;
    public bool loop = true;
    public bool reverseAtEnd = false;

    private int currentIndex = 0;
    private bool isWaiting = false;
    private bool isReversing = false;

    void Update()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            StartCoroutine(WaitAndMoveNext());
        }
    }

    System.Collections.IEnumerator WaitAndMoveNext()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        AdvanceToNextWaypoint();
        isWaiting = false;
    }

    void AdvanceToNextWaypoint()
    {
        if (reverseAtEnd)
        {
            if (!isReversing)
            {
                currentIndex++;
                if (currentIndex >= waypoints.Length)
                {
                    currentIndex = waypoints.Length - 2;
                    isReversing = true;
                }
            }
            else
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = 1;
                    isReversing = false;
                }
            }
        }
        else
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                if (loop)
                    currentIndex = 0;
                else
                    currentIndex = waypoints.Length - 1; // Stop at last waypoint
            }
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if (loop && !reverseAtEnd)
        {
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}
