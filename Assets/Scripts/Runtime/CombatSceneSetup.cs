using UnityEngine;

public class CombatSceneSetup : MonoBehaviour
{
    [ContextMenu("Create Auto Combat Scene")]
    public void CreateScene()
    {
        if (FindObjectOfType<CombatBootstrap>() == null)
        {
            GameObject bootstrap = new GameObject("CombatBootstrap");
            bootstrap.AddComponent<CombatBootstrap>();
        }

        if (FindObjectOfType<PlayerController>() == null)
            FindObjectOfType<CombatBootstrap>().Start();
    }
}
