using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EcosystemManager : MonoBehaviour
{
    public GameObject rabbitPrefab;
    public GameObject wolfPrefab;

    public int initialRabbitPopulation = 50;
    public int initialWolfPopulation = 10;

    public float generationTime = 20f;
    private float generationTimer;
    private int currentGeneration = 1;

    private List<RabbitGA> rabbits;
    private List<WolfGA> wolves;
    private List<RabbitGA> breedingRabbits;

    public float mutationStrength = 1.0f;

    public float fixedWolfSpeed = 5.0f;
    public float fixedWolfAwareness = 10.0f;

    public Transform[] rabbitSpawnPoints;
    public Transform[] wolfSpawnPoints;

    public TextMeshProUGUI statsText;

    public float maxArrivalBonus = 1000f;
    public float arrivalPenaltyPerRabbit = 50f;
    public float speedBonusFactor = 5000f;

    private int rabbitsThatReachedCave = 0;

    public float speedTime = 1f;

    public float maxSpeedForColor = 30f;
    public float maxAwarenessForColor = 30f;

    void Start()
    {
        Time.timeScale = speedTime;

        rabbits = new List<RabbitGA>();
        wolves = new List<WolfGA>();
        breedingRabbits = new List<RabbitGA>();

        CreateInitialGeneration();
        generationTimer = generationTime;
        UpdateUIAndLogData();
    }

    void Update()
    {
        generationTimer -= Time.deltaTime;

        if (generationTimer <= 0)
        {
            StartNewGeneration();
        }
    }

    public void RabbitReachedCave(RabbitGA rabbit)
    {
        rabbitsThatReachedCave++;

        float arrivalBonus = Mathf.Max(0, maxArrivalBonus - (rabbitsThatReachedCave - 1) * arrivalPenaltyPerRabbit);

        /*float timeEfficiencyBonus = 0;
        if (rabbit.survivalTime > 0)
        {
            timeEfficiencyBonus = speedBonusFactor / rabbit.survivalTime;
        }*/

        rabbit.totalFitness = rabbit.survivalTime + arrivalBonus;// + timeEfficiencyBonus;
    }

    public void RabbitCaught(RabbitGA rabbit)
    {
        rabbit.totalFitness = rabbit.survivalTime;
    }

    void CreateInitialGeneration()
    {
        for (int i = 0; i < initialRabbitPopulation; i++)
        {
            Vector3 spawnPos = rabbitSpawnPoints[i % rabbitSpawnPoints.Length].position;
            GameObject newRabbitGO = Instantiate(rabbitPrefab, spawnPos, Quaternion.identity);
            RabbitGA newRabbit = newRabbitGO.GetComponent<RabbitGA>();

            newRabbit.speed = Random.Range(1f, 8f);
            newRabbit.awareness = Random.Range(2f, 10f);
            newRabbit.evasionRange = newRabbit.awareness / 2f;
            newRabbit.UpdateColor(maxSpeedForColor, maxAwarenessForColor);

            rabbits.Add(newRabbit);
        }

        for (int i = 0; i < initialWolfPopulation; i++)
        {
            Vector3 spawnPos = wolfSpawnPoints[i % wolfSpawnPoints.Length].position;
            GameObject newWolfGO = Instantiate(wolfPrefab, spawnPos, Quaternion.identity);
            WolfGA newWolf = newWolfGO.GetComponent<WolfGA>();

            newWolf.speed = fixedWolfSpeed;
            newWolf.awareness = fixedWolfAwareness;

            wolves.Add(newWolf);
        }
    }

    /*private void SelectFittestAgents()
    {
        rabbits.Sort((a, b) => b.totalFitness.CompareTo(a.totalFitness));
        breedingRabbits.Clear();
        int breedingCount = Mathf.CeilToInt(rabbits.Count * 0.5f);
        for (int i = 0; i < breedingCount; i++)
        {
            breedingRabbits.Add(rabbits[i]);
        }
    }*/

    private void SelectFittestAgents()
    {
        breedingRabbits.Clear();
        List<RabbitGA> dominatedRabbits = new List<RabbitGA>();

        foreach (RabbitGA rabbitA in rabbits)
        {
            bool isDominated = false;
            foreach (RabbitGA rabbitB in rabbits)
            {
                if (rabbitA == rabbitB) continue;

                if (rabbitB.speed >= rabbitA.speed && rabbitB.awareness >= rabbitA.awareness)
                {
                    if (rabbitB.speed > rabbitA.speed || rabbitB.awareness > rabbitA.awareness)
                    {
                        isDominated = true;
                        break;
                    }
                }
            }

            if (!isDominated)
            {
                breedingRabbits.Add(rabbitA);
            }
            else
            {
                dominatedRabbits.Add(rabbitA);
            }
        }
    }

    private void CreateNewGenerationFromBreeding(List<RabbitGA> newRabbits, List<WolfGA> newWolves)
    {
        for (int i = 0; i < initialRabbitPopulation; i++)
        {
            RabbitGA parent1 = breedingRabbits[Random.Range(0, breedingRabbits.Count)];
            RabbitGA parent2 = breedingRabbits[Random.Range(0, breedingRabbits.Count)];

            float newSpeed = (parent1.speed + parent2.speed) / 2f;
            float newAwareness = (parent1.awareness + parent2.awareness) / 2f;

            newSpeed += Random.Range(-mutationStrength, mutationStrength);
            newAwareness += Random.Range(-mutationStrength, mutationStrength);

            Vector3 spawnPos = rabbitSpawnPoints[i % rabbitSpawnPoints.Length].position;
            GameObject newRabbitGO = Instantiate(rabbitPrefab, spawnPos, Quaternion.identity);
            RabbitGA newRabbit = newRabbitGO.GetComponent<RabbitGA>();

            newRabbit.speed = newSpeed;
            newRabbit.awareness = newAwareness;
            newRabbit.evasionRange = newAwareness / 2f;

            newRabbit.UpdateColor(maxSpeedForColor, maxAwarenessForColor);

            newRabbits.Add(newRabbit);
        }

        for (int i = 0; i < initialWolfPopulation; i++)
        {
            Vector3 spawnPos = wolfSpawnPoints[i % wolfSpawnPoints.Length].position;
            GameObject newWolfGO = Instantiate(wolfPrefab, spawnPos, Quaternion.identity);
            WolfGA newWolf = newWolfGO.GetComponent<WolfGA>();

            newWolf.speed = fixedWolfSpeed;
            newWolf.awareness = fixedWolfAwareness;

            newWolves.Add(newWolf);
        }
    }

    void StartNewGeneration()
    {
        UpdateUIAndLogData();
        SelectFittestAgents();

        List<RabbitGA> tempRabbits = new List<RabbitGA>();
        List<WolfGA> tempWolves = new List<WolfGA>();
        CreateNewGenerationFromBreeding(tempRabbits, tempWolves);

        foreach (RabbitGA rabbit in rabbits)
        {
            if (rabbit != null) Destroy(rabbit.gameObject);
        }
        foreach (WolfGA wolf in wolves)
        {
            if (wolf != null) Destroy(wolf.gameObject);
        }

        rabbits.Clear();
        wolves.Clear();
        rabbits = tempRabbits;
        wolves = tempWolves;

        currentGeneration++;
        generationTimer = generationTime;
        rabbitsThatReachedCave = 0;
    }

    private void UpdateUIAndLogData()
    {
        float totalSpeed = 0f;
        float totalAwareness = 0f;
        float highestFitness = 0f;

        foreach (RabbitGA rabbit in rabbits)
        {
            totalSpeed += rabbit.speed;
            totalAwareness += rabbit.awareness;

            if (rabbit.totalFitness > highestFitness)
            {
                highestFitness = rabbit.totalFitness;
            }
        }

        int survivors = rabbitsThatReachedCave;

        float avgSpeed = totalSpeed / rabbits.Count;
        float avgAwareness = totalAwareness / rabbits.Count;

        string stats = $"Generación: {currentGeneration}\n" +
                       $"Velocidad promedio: {avgSpeed:F2}\n" +
                       $"Conciencia promedio: {avgAwareness:F2}\n" +
                       $"Sobrevivientes: {survivors}/{initialRabbitPopulation}\n" +
                       $"Fitness más alto: {highestFitness:F2}";

        if (statsText != null)
        {
            statsText.text = stats;
        }

        Debug.Log(stats);
    }
}