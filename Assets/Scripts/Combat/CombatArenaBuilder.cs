using UnityEngine;

public class CombatArenaBuilder : MonoBehaviour
{
    public Vector3 arenaSize = new Vector3(30f, 1f, 30f);
    public int enemyCount = 6;

    public void BuildArena()
    {
        GameObject arena = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arena.name = "ArenaFloor";
        arena.transform.localScale = arenaSize;
        arena.transform.position = new Vector3(0f, -0.5f, 0f);
        var floorRenderer = arena.GetComponent<Renderer>();
        if (floorRenderer != null)
            floorRenderer.material.color = new Color(0.08f, 0.08f, 0.11f);

        for (int i = 0; i < 4; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "ArenaWall_" + i;
            wall.transform.localScale = new Vector3(1f, 4f, 30f);

            switch (i)
            {
                case 0:
                    wall.transform.position = new Vector3(-15f, 1.5f, 0f);
                    break;
                case 1:
                    wall.transform.position = new Vector3(15f, 1.5f, 0f);
                    break;
                case 2:
                    wall.transform.localScale = new Vector3(30f, 4f, 1f);
                    wall.transform.position = new Vector3(0f, 1.5f, -15f);
                    break;
                case 3:
                    wall.transform.localScale = new Vector3(30f, 4f, 1f);
                    wall.transform.position = new Vector3(0f, 1.5f, 15f);
                    break;
            }
        }

        GameObject lightGO = new GameObject("ArenaLight");
        Light arenaLight = lightGO.AddComponent<Light>();
        arenaLight.type = LightType.Directional;
        arenaLight.intensity = 1.15f;
        arenaLight.color = new Color(0.8f, 0.9f, 1f, 1f);
        lightGO.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
    }
}
