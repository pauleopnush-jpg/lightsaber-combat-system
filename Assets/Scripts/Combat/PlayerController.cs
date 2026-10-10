using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Animator animator;
    public Transform cameraPivot;
    public Transform slashOrigin;
    public LightsaberWeapon lightsaber;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 14f;
    public float jumpHeight = 1.5f;
    public float gravity = -30f;

    [Header("Combat")]
    public float slashRange = 2.8f;
    public float slashRadius = 1.25f;
    public float heavyRange = 3.4f;
    public float heavyRadius = 1.6f;
    public float attackCooldown = 0.32f;
    public int comboStep = 0;
    public bool isAttacking;
    public bool isBlocking;
    public bool canParry;
    public float stamina = 100f;
    public float maxStamina = 100f;

    [Header("Stats")]
    public int maxHealth = 120;
    public int currentHealth;

    private Vector3 velocity;
    private float nextAttackTime;
    private float parryTimer;
    private Vector3 moveDirection;

    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        if (lightsaber == null)
        {
            var saber = transform.Find("Lightsaber");
            if (saber != null)
                lightsaber = saber.GetComponent<LightsaberWeapon>();
        }

        if (slashOrigin == null && transform.childCount > 0)
        {
            slashOrigin = transform.GetChild(0);
        }
    }

    private void Update()
    {
        if (controller == null)
            return;

        HandleMovement();
        HandleCombatInput();
        UpdateStamina();

        if (parryTimer > 0f)
            parryTimer -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = Camera.main.transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            moveDirection = cameraForward * input.z + cameraRight * input.x;
        }
        else
        {
            moveDirection = new Vector3(input.x, 0f, input.z);
        }

        if (moveDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        Vector3 stepMove = moveDirection.normalized * moveSpeed;
        controller.Move(stepMove * Time.deltaTime + new Vector3(0f, velocity.y, 0f) * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        if (animator != null)
        {
            animator.SetFloat("Movement", moveDirection.magnitude);
            animator.SetBool("Grounded", controller.isGrounded);
        }
    }

    private void HandleCombatInput()
    {
        if (Input.GetMouseButtonDown(0) && !isBlocking)
            BeginAttack(false);

        if (Input.GetMouseButtonDown(1) && !isBlocking)
            BeginAttack(true);

        if (Input.GetKeyDown(KeyCode.Q))
            ToggleBlock();

        if (Input.GetKeyDown(KeyCode.E))
            Parry();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            Dodge();
    }

    private void BeginAttack(bool heavy)
    {
        if (isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (heavy && stamina < 25f)
            return;

        float damage = heavy ? 32f : 18f + comboStep * 5f;
        float range = heavy ? heavyRange : slashRange;
        float radius = heavy ? heavyRadius : slashRadius;

        isAttacking = true;
        comboStep = heavy ? 0 : comboStep + 1;
        nextAttackTime = Time.time + (heavy ? 0.82f : 0.42f);

        if (animator != null)
        {
            animator.SetTrigger(heavy ? "HeavyAttack" : "LightAttack");
        }

        if (lightsaber != null)
            lightsaber.SwingPulse();

        StartCoroutine(ResolveAttack(damage, range, radius, heavy));
    }

    private IEnumerator ResolveAttack(float damage, float range, float radius, bool heavy)
    {
        yield return new WaitForSeconds(heavy ? 0.18f : 0.12f);

        if (heavy)
            stamina = Mathf.Max(0f, stamina - 25f);
        else
            stamina = Mathf.Max(0f, stamina - 6f);

        Collider[] hits = Physics.OverlapSphere(slashOrigin != null ? slashOrigin.position : transform.position + transform.forward, radius);

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.TryGetComponent(out EnemyCombatant enemy))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= range)
                {
                    enemy.TakeDamage(new DamageInfo
                    {
                        amount = damage,
                        source = transform.position,
                        isHeavy = heavy,
                        attacker = this
                    });
                }
            }
        }

        yield return new WaitForSeconds(heavy ? 0.5f : 0.28f);
        isAttacking = false;
        if (!heavy && comboStep > 3)
            comboStep = 0;
    }

    public void TakeDamage(int damage)
    {
        if (isBlocking)
        {
            damage = Mathf.RoundToInt(damage * 0.2f);
            if (animator != null) animator.SetTrigger("BlockHit");
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        if (animator != null)
            animator.SetTrigger("Hit");
    }

    private void ToggleBlock()
    {
        isBlocking = !isBlocking;
        if (animator != null)
            animator.SetBool("Blocking", isBlocking);
    }

    private void Parry()
    {
        if (isAttacking || isBlocking)
            return;

        canParry = true;
        parryTimer = 0.6f;
        if (animator != null)
            animator.SetTrigger("Parry");
    }

    private void Dodge()
    {
        if (isAttacking)
            return;

        if (stamina < 15f)
            return;

        stamina -= 15f;
        if (animator != null)
            animator.SetTrigger("Dodge");

        Vector3 dodgeDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
        controller.Move(dodgeDirection * 2.2f);
    }

    private void UpdateStamina()
    {
        if (stamina < maxStamina)
            stamina += Time.deltaTime * 18f;
    }

    private void Die()
    {
        if (animator != null)
            animator.SetTrigger("Death");

        StartCoroutine(ResetSceneAfterDeath());
    }

    private IEnumerator ResetSceneAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = slashOrigin != null ? slashOrigin.position : transform.position + transform.forward;
        Gizmos.DrawWireSphere(origin, slashRadius);
    }
}

public struct DamageInfo
{
    public float amount;
    public Vector3 source;
    public bool isHeavy;
    public object attacker;
}

public interface IDamageable
{
    void TakeDamage(DamageInfo info);
}

public class LightsaberWeapon : MonoBehaviour
{
    [Header("Weapon")]
    public MeshRenderer bladeRenderer;
    public TrailRenderer bladeTrail;
    public ParticleSystem glowBurst;
    public AudioSource humSource;
    public Color bladeColor = new Color(0.2f, 0.9f, 1f, 1f);
    public float bladeLength = 1.5f;
    public bool powered = true;

    private Vector3 defaultScale;

    private void Start()
    {
        if (bladeRenderer == null)
            bladeRenderer = GetComponentInChildren<MeshRenderer>();

        defaultScale = transform.localScale;
        ApplyColor();
    }

    public void SwingPulse()
    {
        if (bladeTrail != null)
        {
            bladeTrail.Clear();
            bladeTrail.emitting = true;
            StartCoroutine(StopTrail());
        }

        if (glowBurst != null)
            glowBurst.Play();

        if (humSource != null)
        {
            humSource.pitch = Random.Range(0.9f, 1.3f);
            if (!humSource.isPlaying)
                humSource.Play();
        }
    }

    public void SetPowered(bool enabled)
    {
        powered = enabled;
        if (bladeRenderer != null)
        {
            Color c = powered ? bladeColor : Color.gray;
            bladeRenderer.material.SetColor("_EmissionColor", c * 2f);
        }
    }

    private void ApplyColor()
    {
        if (bladeRenderer == null)
            return;

        if (bladeRenderer.material == null)
            return;

        bladeRenderer.material.SetColor("_Color", bladeColor);
        bladeRenderer.material.SetColor("_EmissionColor", bladeColor * 3f);
    }

    private IEnumerator StopTrail()
    {
        yield return new WaitForSeconds(0.18f);
        if (bladeTrail != null)
            bladeTrail.emitting = false;
    }
}
