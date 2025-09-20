using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitGA : MonoBehaviour
{
    public float speed;
    public float awareness;
    public float evasionRange;

    public float survivalTime = 0f;
    public float totalFitness = 0f;
    public bool isSafe = false;
    public float caveBonus = 1000f;

    private Rigidbody rb;
    private Transform caveTarget;
    private PathfindingGrid grid;
    private List<Node> path;
    private int targetPathIndex;
    private Transform predatorTarget;
    private IEnumerator rabbitBehaviorCoroutine;

    private EcosystemManager manager;

    void Awake()
    {
        manager = FindObjectOfType<EcosystemManager>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid = FindObjectOfType<PathfindingGrid>();

        if (speed == 0f) speed = Random.Range(1f, 5f);
        if (awareness == 0f) awareness = Random.Range(2f, 10f);

        if (grid != null)
        {
            FindCave();
            rabbitBehaviorCoroutine = RabbitBehaviorRoutine();
            StartCoroutine(rabbitBehaviorCoroutine);
        }
        else
        {
            Debug.LogError("PathfindingGrid no encontrado");
        }
    }

    void Update()
    {
        if (!isSafe)
            survivalTime += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isSafe) return;
        if (path == null || targetPathIndex >= path.Count) return;

        Vector3 targetPos = path[targetPathIndex].worldPosition;
        Vector3 dir = (targetPos - transform.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
            targetPathIndex++;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cave") && !isSafe)
        {
            isSafe = true;
            manager?.RabbitReachedCave(this);
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wolf") && !isSafe)
        {
            isSafe = true;
            manager?.RabbitCaught(this);
            gameObject.SetActive(false);
        }
    }

    private void FindCave()
    {
        GameObject caveObject = GameObject.FindGameObjectWithTag("Cave");
        if (caveObject != null)
        {
            caveTarget = caveObject.transform;
            RecalculatePath();
        }
    }

    private void RecalculatePath()
    {
        if (grid == null || caveTarget == null) return;

        foreach (Node n in grid.grid)
            n.movementPenalty = 0;

        Collider[] wolves = Physics.OverlapSphere(transform.position, awareness, LayerMask.GetMask("Wolf"));
        foreach (Collider w in wolves)
        {
            Node centerNode = grid.NodeFromWorldPoint(w.transform.position);
            if (centerNode == null) continue;

            int radius = 2;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    int nx = centerNode.gridX + x;
                    int ny = centerNode.gridY + y;
                    if (nx >= 0 && nx < grid.gridCountX && ny >= 0 && ny < grid.gridCountY)
                        grid.grid[nx, ny].movementPenalty = 1000;
                }
            }
        }

        Node startNode = grid.NodeFromWorldPoint(transform.position);
        Node targetNode = grid.NodeFromWorldPoint(caveTarget.position);
        path = Pathfinder.FindPath(startNode, targetNode, grid);
        targetPathIndex = 0;
    }

    IEnumerator RabbitBehaviorRoutine()
    {
        while (!isSafe)
        {
            predatorTarget = FindPredator();
            RecalculatePath();
            yield return new WaitForSeconds(0.3f);
        }
    }

    private Transform FindPredator()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, awareness, LayerMask.GetMask("Wolf"));
        if (nearby.Length == 0) return null;

        Transform closest = null;
        float bestDist = float.MaxValue;
        foreach (Collider col in nearby)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = col.transform;
            }
        }
        return closest;
    }

    public void ResetAgent()
    {
        isSafe = false;
        totalFitness = 0f;
        survivalTime = 0f;
        gameObject.SetActive(true);

        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        if (grid != null)
        {
            Node random = grid.GetRandomWalkableNode();
            transform.position = random.worldPosition + Vector3.up * 0.5f;
        }

        if (rabbitBehaviorCoroutine != null)
            StopCoroutine(rabbitBehaviorCoroutine);

        rabbitBehaviorCoroutine = RabbitBehaviorRoutine();
        StartCoroutine(rabbitBehaviorCoroutine);
    }
}
