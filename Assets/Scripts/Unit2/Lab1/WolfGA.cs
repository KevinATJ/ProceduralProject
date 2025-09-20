using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfGA : MonoBehaviour
{
    public float speed;
    public float awareness;
    public int rabbitsCaught = 0;

    private Rigidbody rb;
    private PathfindingGrid grid;
    private List<Node> path;
    private int targetPathIndex;
    private GameObject rabbitTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid = FindObjectOfType<PathfindingGrid>();

        if (speed == 0) speed = Random.Range(2f, 7f);
        if (awareness == 0) awareness = Random.Range(5f, 15f);

        StartCoroutine(FindTargetRoutine());
    }

    void FixedUpdate()
    {
        if (path != null && targetPathIndex < path.Count)
        {
            Vector3 targetPosition = path[targetPathIndex].worldPosition;
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
            {
                targetPathIndex++;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rabbit"))
        {
            rabbitsCaught++;
            other.gameObject.SetActive(false);
        }
    }

    IEnumerator FindTargetRoutine()
    {
        while (true)
        {
            FindRabbitTarget();

            if (rabbitTarget != null)
            {
                Node start = grid.NodeFromWorldPoint(transform.position);
                Node target = grid.NodeFromWorldPoint(rabbitTarget.transform.position);
                path = Pathfinder.FindPath(start, target, grid);
                targetPathIndex = 0;
            }
            else
            {
                Node randomNode = grid.GetRandomWalkableNode();
                Node start = grid.NodeFromWorldPoint(transform.position);
                path = Pathfinder.FindPath(start, randomNode, grid);
                targetPathIndex = 0;
                yield return new WaitForSeconds(2f);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void FindRabbitTarget()
    {
        Collider[] nearbyRabbits = Physics.OverlapSphere(transform.position, awareness);
        float closestDist = float.MaxValue;
        rabbitTarget = null;

        foreach (Collider rabbit in nearbyRabbits)
        {
            if (rabbit.CompareTag("Rabbit"))
            {
                float dist = Vector3.Distance(transform.position, rabbit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    rabbitTarget = rabbit.gameObject;
                }
            }
        }
    }

    public void ResetAgent()
    {
        rb.velocity = Vector3.zero;
        Node node = grid.GetRandomWalkableNode();
        transform.position = node.worldPosition + Vector3.up * 0.5f;
        rabbitsCaught = 0;
        path = null;
        targetPathIndex = 0;
    }
}
