using System.Collections.Generic;
using UnityEngine;

public class CombatBootstrap : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerSpawn;
    public Transform enemySpawnParent;
    public int enemyCount = 6;

    private void Start()
    {
        if (FindObjectOfType<CombatArenaBuilder>() == null)
        {
            var arenaBuilder = new GameObject("CombatArenaBuilder").AddComponent<CombatArenaBuilder>();
            arenaBuilder.enemyCount = enemyCount;
            arenaBuilder.BuildArena();
        }

        if (FindObjectOfType<PlayerController>() == null)
            SpawnPlayer();

        if (FindObjectOfType<EnemyCombatant>() == null)
            SpawnEnemies();

        if (FindObjectOfType<GameHUD>() == null)
            CreateHUD();

        SetupCamera();
    }

    private void SpawnPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1.08f, 6f);

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 1f, 0f);

        var animator = player.AddComponent<Animator>();
        var playerScript = player.AddComponent<PlayerController>();
        playerScript.characterController = controller;
        playerScript.animator = animator;

        GameObject saber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        saber.name = "Lightsaber";
        saber.transform.SetParent(player.transform);
        saber.transform.localPosition = new Vector3(0.65f, 1.3f, 0.25f);
        saber.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        saber.transform.localScale = new Vector3(0.12f, 1.35f, 0.12f);
        Destroy(saber.GetComponent<CapsuleCollider>());

        var blade = saber.GetComponent<MeshRenderer>();
        blade.material = new Material(Shader.Find("Standard"));
        blade.material.EnableKeyword("_EMISSION");
        blade.material.SetColor("_Color", new Color(0.2f, 0.9f, 1f, 1f));
        blade.material.SetColor("_EmissionColor", new Color(0.2f, 0.9f, 1f, 1f) * 4f);

        var weapon = saber.AddComponent<LightsaberWeapon>();
        weapon.bladeRenderer = blade;
        weapon.bladeColor = new Color(0.2f, 0.9f, 1f, 1f);
        weapon.SetPowered(true);

        var trail = saber.AddComponent<TrailRenderer>();
        trail.time = 0.14f;
        trail.startWidth = 0.08f;
        trail.endWidth = 0.01f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.material.color = new Color(0.3f, 1f, 1f, 1f);
        weapon.trail = trail;

        playerScript.slashOrigin = saber.transform;
        playerScript.lightsaber = weapon;

        if (player.TryGetComponent(out Rigidbody rb))
            Destroy(rb);
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Enemy_" + i;
            enemy.transform.position = new Vector3(Random.Range(-10f, 10f), 1.08f, Random.Range(-10f, 10f));

            var controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);

            var animator = enemy.AddComponent<Animator>();
            var enemyScript = enemy.AddComponent<EnemyCombatant>();
            enemyScript.characterController = controller;
            enemyScript.animator = animator;
            enemyScript.player = FindObjectOfType<PlayerController>().transform;
            enemyScript.enemyType = i % 3 == 0 ? EnemyCombatant.EnemyType.Brute : (i % 2 == 0 ? EnemyCombatant.EnemyType.Assassin : EnemyCombatant.EnemyType.Grunt);

            if (enemy.TryGetComponent(out Rigidbody rb))
                Destroy(rb);
        }
    }

    private void CreateHUD()
    {
        GameObject hudGO = new GameObject("GameHUD");
        var canvas = hudGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudGO.AddComponent<CanvasScaler>();
        hudGO.AddComponent<GraphicRaycaster>();

        var hud = hudGO.AddComponent<GameHUD>();
        hud.player = FindObjectOfType<PlayerController>();
    }

    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCamera = camGO.AddComponent<Camera>();
            mainCamera.nearClipPlane = 0.1f;
        }

        mainCamera.transform.position = new Vector3(0f, 7f, -11f);
        mainCamera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        if (mainCamera.GetComponent<CameraFollow>() == null)
            mainCamera.gameObject.AddComponent<CameraFollow>();

        var follow = mainCamera.GetComponent<CameraFollow>();
        follow.target = FindObjectOfType<PlayerController>().transform;
    }
}
