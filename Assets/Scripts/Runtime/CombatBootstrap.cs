using UnityEngine;

public class CombatBootstrap : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerSpawn;
    public Transform enemySpawn;

    private void Start()
    {
        if (FindObjectOfType<PlayerController>() == null)
            SpawnPlayer();

        if (FindObjectOfType<EnemyCombatant>() == null)
            SpawnEnemy();

        if (Camera.main == null)
            CreateMainCamera();
        else if (Camera.main.GetComponent<CameraFollow>() == null)
            Camera.main.gameObject.AddComponent<CameraFollow>();

        var cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null && cameraFollow.target == null)
            cameraFollow.target = FindObjectOfType<PlayerController>().transform;
    }

    private void SpawnPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = playerSpawn != null ? playerSpawn.position : new Vector3(0f, 1f, 0f);
        player.transform.localScale = new Vector3(1f, 1f, 1f);

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.42f;
        controller.center = new Vector3(0f, 1f, 0f);

        var animator = player.AddComponent<Animator>();
        var playerScript = player.AddComponent<PlayerController>();
        playerScript.controller = controller;
        playerScript.animator = animator;

        GameObject saber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        saber.name = "Lightsaber";
        saber.transform.SetParent(player.transform);
        saber.transform.localPosition = new Vector3(0.7f, 1.15f, 0.2f);
        saber.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        saber.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
        Destroy(saber.GetComponent<CapsuleCollider>());

        var saberWeapon = saber.AddComponent<LightsaberWeapon>();
        saberWeapon.bladeRenderer = saber.GetComponent<MeshRenderer>();
        saberWeapon.bladeColor = new Color(0.2f, 0.9f, 1f, 1f);
        saberWeapon.SetPowered(true);

        var trail = saber.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = 0.08f;
        trail.endWidth = 0.01f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.material.color = new Color(0.25f, 1f, 1f, 1f);
        saberWeapon.bladeTrail = trail;

        playerScript.lightsaber = saberWeapon;
        playerScript.slashOrigin = saber.transform;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);
    }

    private void SpawnEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.transform.position = enemySpawn != null ? enemySpawn.position : new Vector3(8f, 1f, 0f);

        var controller = enemy.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.42f;
        controller.center = new Vector3(0f, 1f, 0f);

        var animator = enemy.AddComponent<Animator>();
        var enemyScript = enemy.AddComponent<EnemyCombatant>();
        enemyScript.controller = controller;
        enemyScript.animator = animator;
        enemyScript.player = FindObjectOfType<PlayerController>().transform;

        var rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);
    }

    private void CreateMainCamera()
    {
        GameObject cameraGo = new GameObject("Main Camera");
        Camera cam = cameraGo.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cameraGo.tag = "MainCamera";
        cameraGo.transform.position = new Vector3(0f, 3.3f, -7f);

        var follow = cameraGo.AddComponent<CameraFollow>();
        follow.target = FindObjectOfType<PlayerController>().transform;
    }
}
