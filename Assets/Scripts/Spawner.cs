using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    #region Public Member Variables 
    public NavMeshSurface surface;
    public GameObject playerPrefab;
    public GameObject dualStairsPrefab;
    public GameObject pillarPrefab;
    public GameObject moreStairsPrefab;
    public GameObject wallPrefab;
    public GameObject domePrefab;
    public MoveCamera moveCam;
    public PlayerCam playerCam;
    public MazeGenerator maze;
    public GameObject enemyPrefab;

    public Transform spawnPoint;
    // public FollowCamera followCam;     // Main Camera
    public float respawnDelay = 1.5f;
    #endregion
    #region Private Member Variables
    // float rangeX = 50f; // Range for random X position
    //float rangeZ = 50f; // Range for random Z position


    GameObject player;
    GameObject enemy;
    bool respawning;
    #endregion

    #region Singleton
    public static Spawner Instance { get; internal set; }
    #endregion

    void Start()
    {
        SpawnSceneObjects();
        Vector3 start = maze.Generate(spawnPoint.position);
        spawnPoint.position = start + Vector3.up * 1f;   // player spawns in the first cell
        StartCoroutine(BuildNavmeshAndSpawnEnemy());
    }
    System.Collections.IEnumerator BuildNavmeshAndSpawnEnemy()
    {
        // Wait one frame and then build the navigation mesh for the surface before spawning enemy
        yield return null;
        surface.BuildNavMesh();

        yield return null; // Wait one frame for walls to show up before Instantiating enemy to chase it 
                           // Pick a spot far from the player. Here it's a random point on the NavMesh.
        Vector3 p = spawnPoint.position + new Vector3(40f, 0, 40f);
        if (UnityEngine.AI.NavMesh.SamplePosition(p, out var hit, 30f, UnityEngine.AI.NavMesh.AllAreas))
            p = hit.position;

        enemy = Instantiate(enemyPrefab, p, Quaternion.identity);
        enemy.GetComponent<Seeker>().target = player.transform;
    }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnSceneObjects()
    {
        // Instantiate the player
        player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        moveCam.cameraPosition = player.transform.Find("CameraPos");
        playerCam.orientation = player.transform.Find("Orientation");

        respawning = false;

        // Instantiate dome prefab at random positions around the spawn point
        /* for (int i = 0; i < 10; i++)
        {
            GameObject dome = Instantiate(domePrefab, spawnPoint.position + new Vector3(Random.Range(-rangeX, rangeX), 0, Random.Range(-rangeZ, rangeZ)), spawnPoint.rotation);
            dome.transform.localScale = domePrefab.transform.localScale * Random.Range(0.4f, 0.8f); // Randomly scale the dome
        }

        // Instantiate the wall prefab
        for (int i = 0; i < 120; i++)
        {
            GameObject wall = Instantiate(wallPrefab, spawnPoint.position + new Vector3(Random.Range(-rangeX, rangeX), 0, Random.Range(-rangeZ, rangeZ)), spawnPoint.rotation);
            wall.transform.localScale = wallPrefab.transform.localScale * Random.Range(1.5f, 1.5f); // Randomly scale the wall
        }

        //  Instantiate the stairs prefab. 
        for (int i = 0; i < 70; i++)
        {
            GameObject stairs = Instantiate(dualStairsPrefab, spawnPoint.position + new Vector3(Random.Range(-rangeX, rangeX), 0, Random.Range(-rangeZ, rangeZ)), spawnPoint.rotation);
            stairs.transform.localScale = dualStairsPrefab.transform.localScale * Random.Range(6.0f, 6.0f); // Randomly scale the stairs
        } */

        // reconnect the scene references a prefab can't store
        /* followCam.target = player.transform;
         player.GetComponent<PlayerController>().cam = followCam.transform;
         player.GetComponent<RagdollToggle>().followCam = followCam; */

        respawning = false;
    }

    public void Kill()
    {
        if (respawning) return;   // many colliders can trigger at once
        respawning = true;

        // Destroy the current player, respawn new player 
        Destroy(player);
        Invoke(nameof(SpawnSceneObjects), respawnDelay);
    }
    #region Coroutines
    /*System.Collections.IEnumerator SpawnFlowers()
    {
        // Instantiate flower prefab at random positions around the spawn point
        while (true)
        {
            GameObject flower = null;
            if (Flower.flowersAvailableForPlanting.TryDequeue(out flower))
            {
                flower.transform.localScale = flowerPrefab.transform.localScale * Random.Range(0.5f, 1.5f); // Randomly scale the flower
                flower.transform.position = spawnPoint.position + new Vector3(Random.Range(-rangeX, rangeX), 0, Random.Range(-rangeZ, rangeZ));
                flower.transform.rotation = spawnPoint.rotation; // Set the rotation of the flower
                flower.SetActive(true); // Activate the flower
                Flower.flowersPlanted.Add(flower);
            }
            yield return new WaitForSeconds(Random.Range(0.2f, 0.7f)); // Wait for a short duration before instantiating the next flower prefab
        }
    }
    #endregion
    #region Queue Management
    public void ReturnFlowerToQueue(GameObject flower)
    {
        Flower.flowersPlanted.Remove(flower); // Remove the flower from the planted set
        flower.SetActive(false);
        Flower.flowersAvailableForPlanting.Enqueue(flower);
    } */
    #endregion

}