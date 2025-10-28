using UnityEngine;

public class MosquitoDebugVisualizer : MonoBehaviour
{
    public Transform playerTarget;
    public bool showDebugInfo = true;
    public bool drawGizmos = true;

    void Update()
    {
        if (!showDebugInfo) return;

        // Press L to list all mosquitoes and their status
        if (Input.GetKeyDown(KeyCode.L))
        {
            ListAllMosquitoes();
        }

        // Press B to show bone positions
        if (Input.GetKeyDown(KeyCode.B))
        {
            ShowBonePositions();
        }

        // Press K to kill all mosquitoes (for testing)
        if (Input.GetKeyDown(KeyCode.K))
        {
            KillAllMosquitoes();
        }
    }

    void ListAllMosquitoes()
    {
        GameObject[] mosquitoes = GameObject.FindGameObjectsWithTag("Mosquito");

        Debug.Log($"========================================");
        Debug.Log($"TOTAL MOSQUITOES: {mosquitoes.Length}");
        Debug.Log($"========================================");

        if (mosquitoes.Length == 0)
        {
            Debug.LogWarning("⚠️ NO MOSQUITOES IN SCENE!");
            return;
        }

        for (int i = 0; i < mosquitoes.Length; i++)
        {
            GameObject mosq = mosquitoes[i];

            Debug.Log($"\n--- MOSQUITO {i + 1} ---");
            Debug.Log($"  Name: {mosq.name}");
            Debug.Log($"  Position: {mosq.transform.position}");
            Debug.Log($"  Scale: {mosq.transform.localScale.x}");
            Debug.Log($"  Active: {mosq.activeSelf}");

            if (Camera.main != null)
            {
                float distFromCam = Vector3.Distance(Camera.main.transform.position, mosq.transform.position);
                Debug.Log($"  Distance from camera: {distFromCam:F2}m");

                // Check if in front of camera
                Vector3 viewPos = Camera.main.WorldToViewportPoint(mosq.transform.position);
                bool inView = viewPos.z > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;
                Debug.Log($"  In camera view: {inView}");
                Debug.Log($"  Viewport pos: {viewPos}");
            }

            // Check script
            Component script = mosq.GetComponent("MosquitoLanding");
            if (script == null) script = mosq.GetComponent("Mosquito");

            if (script != null)
            {
                Debug.Log($"  Has script: {script.GetType().Name}");

                // Try to get landed state
                var isLandedField = script.GetType().GetField("isLanded",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (isLandedField != null)
                {
                    bool landed = (bool)isLandedField.GetValue(script);
                    Debug.Log($"  🦟 LANDED STATUS: {landed}");
                }

                // Try to get target bone
                var bodyTargetField = script.GetType().GetField("bodyTarget",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (bodyTargetField != null)
                {
                    Transform target = (Transform)bodyTargetField.GetValue(script);
                    if (target != null)
                    {
                        Debug.Log($"  Target bone: {target.name} at {target.position}");
                    }
                    else
                    {
                        Debug.LogWarning($"  ⚠️ bodyTarget is NULL!");
                    }
                }
            }
            else
            {
                Debug.LogError($"  ❌ NO MOSQUITO SCRIPT!");
            }

            // Check collider
            Collider col = mosq.GetComponent<Collider>();
            if (col != null)
            {
                Debug.Log($"  Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ NO COLLIDER!");
            }

            // Check renderer
            Renderer rend = mosq.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Debug.Log($"  Renderer: {rend.enabled}, Material: {rend.material.name}");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ NO RENDERER - INVISIBLE!");
            }
        }

        Debug.Log($"========================================\n");
    }

    void ShowBonePositions()
    {
        if (playerTarget == null)
        {
            Debug.LogError("❌ Player Target not assigned!");
            return;
        }

        Animator anim = playerTarget.GetComponent<Animator>();
        if (anim == null || !anim.isHuman)
        {
            Debug.LogError("❌ No Humanoid Animator on player!");
            return;
        }

        Debug.Log($"========================================");
        Debug.Log($"PLAYER BONES:");
        Debug.Log($"========================================");

        HumanBodyBones[] bones = new HumanBodyBones[]
        {
            HumanBodyBones.Head,
            HumanBodyBones.Neck,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.RightShoulder
        };

        foreach (var bone in bones)
        {
            Transform t = anim.GetBoneTransform(bone);
            if (t != null)
            {
                Debug.Log($"  {bone}: {t.name} at {t.position}");
            }
            else
            {
                Debug.LogWarning($"  {bone}: NOT FOUND");
            }
        }
        Debug.Log($"========================================\n");
    }

    void KillAllMosquitoes()
    {
        GameObject[] mosquitoes = GameObject.FindGameObjectsWithTag("Mosquito");
        Debug.Log($"💀 Killing {mosquitoes.Length} mosquitoes...");

        foreach (var mosq in mosquitoes)
        {
            Destroy(mosq);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // Background
        GUI.Box(new Rect(10, 10, 300, 120), "");

        // Instructions
        GUI.Label(new Rect(20, 20, 280, 30), "MOSQUITO DEBUG CONTROLS:", style);

        style.fontSize = 14;
        style.fontStyle = FontStyle.Normal;
        GUI.Label(new Rect(20, 50, 280, 20), "L - List all mosquitoes", style);
        GUI.Label(new Rect(20, 70, 280, 20), "B - Show bone positions", style);
        GUI.Label(new Rect(20, 90, 280, 20), "K - Kill all mosquitoes", style);
        GUI.Label(new Rect(20, 110, 280, 20), "M - Show mosquito count", style);

        // Mosquito count
        int count = GameObject.FindGameObjectsWithTag("Mosquito").Length;
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(20, 140, 280, 30), $"Mosquitoes: {count}", style);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Draw all mosquitoes
        GameObject[] mosquitoes = GameObject.FindGameObjectsWithTag("Mosquito");

        foreach (var mosq in mosquitoes)
        {
            if (mosq == null) continue;

            // Get landed state
            bool isLanded = false;
            Component script = mosq.GetComponent("MosquitoLanding");
            if (script != null)
            {
                var field = script.GetType().GetField("isLanded",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    isLanded = (bool)field.GetValue(script);
                }
            }

            // Draw sphere - RED if landed, YELLOW if flying
            Gizmos.color = isLanded ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(mosq.transform.position, 0.5f);

            // Draw line from camera to mosquito
            if (Camera.main != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(Camera.main.transform.position, mosq.transform.position);
            }

            // Draw target bone connection if available
            if (script != null)
            {
                var targetField = script.GetType().GetField("bodyTarget",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (targetField != null)
                {
                    Transform target = (Transform)targetField.GetValue(script);
                    if (target != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(mosq.transform.position, target.position);
                        Gizmos.DrawWireSphere(target.position, 0.3f);
                    }
                }
            }
        }

        // Draw player bones
        if (playerTarget != null)
        {
            Animator anim = playerTarget.GetComponent<Animator>();
            if (anim != null && anim.isHuman)
            {
                Gizmos.color = Color.magenta;

                Transform head = anim.GetBoneTransform(HumanBodyBones.Head);
                if (head != null) Gizmos.DrawWireSphere(head.position, 0.2f);

                Transform neck = anim.GetBoneTransform(HumanBodyBones.Neck);
                if (neck != null) Gizmos.DrawWireSphere(neck.position, 0.2f);
            }
        }
    }
}