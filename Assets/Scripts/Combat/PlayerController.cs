using System.Collections;
using UnityEngine;

public enum CombatState
{
    Idle,
    Moving,
    Attacking,
    Blocking,
    Dodging,
    Parrying,
    Staggered,
    Dead
}

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Animator animator;
    public Transform slashOrigin;
    public LightsaberWeapon lightsaber;
    public GameHUD hud;

    [Header("Movement")]
    public float moveSpeed = 6.25f;
    public float rotationSpeed = 12f;
    public float jumpHeight = 1.8f;
    public float gravity = -28f;

    [Header("Combat")]
    public float attackRange = 2.8f;
    public float heavyRange = 3.6f;
    public float attackRadius = 1.4f;
    public float heavyRadius = 2.0f;
    public float stamina = 100f;
    public float maxStamina = 100f;
    public int maxHealth = 120;
    public int currentHealth;
    public int comboStep;
    public bool isBlocking;
    public bool isDead;

    private Vector3 moveDirection;
    private Vector3 velocity;
    private bool canAttack = true;
    private float nextComboReset;
    private CombatState state = CombatState.Idle;

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (lightsaber == null)
        {
            var saber = transform.Find("Lightsaber");
            if (saber != null)
                lightsaber = saber.GetComponent<LightsaberWeapon>();
        }

        currentHealth = maxHealth;
        if (hud == null)
            hud = FindObjectOfType<GameHUD>();
    }

    private void Update()
    {
        if (isDead)
            return;

        HandleMovement();
        HandleInput();
        UpdateCombatState();
        RegenerateStamina();

        if (hud != null)
            hud.UpdateHUD(this);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            moveDirection = camForward * input.z + camRight * input.x;
        }
        else
        {
            moveDirection = new Vector3(input.x, 0f, input.z);
        }

        if (characterController.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (moveDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Vector3 movement = moveDirection.normalized * moveSpeed;
        characterController.Move(movement * Time.deltaTime + new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;

        if (animator != null)
        {
            animator.SetFloat("Movement", moveDirection.magnitude);
            animator.SetBool("Grounded", characterController.isGrounded);
        }
    }

    private void HandleInput()
    {
        if (isDead)
            return;

        if (Input.GetMouseButtonDown(0) && !isBlocking)
            TryAttack(false);

        if (Input.GetMouseButtonDown(1) && !isBlocking)
            TryAttack(true);

        if (Input.GetKeyDown(KeyCode.Q))
            ToggleBlock();

        if (Input.GetKeyDown(KeyCode.E))
            TriggerParry();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            Dodge();
    }

    private void UpdateCombatState()
    {
        if (isDead)
        {
            state = CombatState.Dead;
            return;
        }

        if (isBlocking)
        {
            state = CombatState.Blocking;
            return;
        }

        if (moveDirection.magnitude > 0.01f && !isBlocking)
        {
            state = CombatState.Moving;
            return;
        }

        if (state == CombatState.Attacking)
            return;

        state = CombatState.Idle;
    }

    private void TryAttack(bool heavy)
    {
        if (!canAttack || isDead)
            return;

        if (heavy && stamina < 25f)
            return;

        canAttack = false;
        comboStep = heavy ? 0 : comboStep + 1;
        state = CombatState.Attacking;

        if (animator != null)
            animator.SetTrigger(heavy ? "HeavyAttack" : "LightAttack");

        if (lightsaber != null)
            lightsaber.SwingPulse();

        float damage = heavy ? 36f : 18f + comboStep * 6f;
        float range = heavy ? heavyRange : attackRange;
        float radius = heavy ? heavyRadius : attackRadius;

        StartCoroutine(ResolveAttack(damage, range, radius, heavy));
    }

    private IEnumerator ResolveAttack(float damage, float range, float radius, bool heavy)
    {
        yield return new WaitForSeconds(heavy ? 0.16f : 0.1f);

        if (heavy)
            stamina = Mathf.Max(0f, stamina - 25f);
        else
            stamina = Mathf.Max(0f, stamina - 8f);

        Vector3 center = slashOrigin ? slashOrigin.position : transform.position + transform.forward * 1.5f;
        Collider[] hits = Physics.OverlapSphere(center, radius);

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.TryGetComponent(out EnemyCombatant enemy))
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist <= range)
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

        yield return new WaitForSeconds(heavy ? 0.55f : 0.35f);
        canAttack = true;
        nextComboReset = Time.time + 1.6f;

        if (!heavy && comboStep >= 4)
            comboStep = 0;

        state = isBlocking ? CombatState.Blocking : CombatState.Idle;
    }

    private void ToggleBlock()
    {
        if (isDead)
            return;

        isBlocking = !isBlocking;
        if (animator != null)
            animator.SetBool("Blocking", isBlocking);
    }

    private void TriggerParry()
    {
        if (isDead || isBlocking || !canAttack)
            return;

        state = CombatState.Parrying;
        if (animator != null)
            animator.SetTrigger("Parry");

        StartCoroutine(ParryWindow());
    }

    private IEnumerator ParryWindow()
    {
        yield return new WaitForSeconds(0.16f);
        state = CombatState.Idle;
    }

    private void Dodge()
    {
        if (isDead || !canAttack)
            return;

        if (stamina < 15f)
            return;

        stamina -= 15f;
        state = CombatState.Dodging;

        if (animator != null)
            animator.SetTrigger("Dodge");

        Vector3 dodgeDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
        characterController.Move(dodgeDirection * 2.4f);

        StartCoroutine(ResetDodgeState());
    }

    private IEnumerator ResetDodgeState()
    {
        yield return new WaitForSeconds(0.35f);
        state = isBlocking ? CombatState.Blocking : CombatState.Idle;
    }

    public void TakeDamage(int damage, Vector3 source)
    {
        if (isDead)
            return;

        if (isBlocking)
        {
            damage = Mathf.RoundToInt(damage * 0.18f);
            if (animator != null)
                animator.SetTrigger("BlockHit");
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

    private void RegenerateStamina()
    {
        if (stamina < maxStamina)
            stamina += Time.deltaTime * 18f;
    }

    private void Die()
    {
        isDead = true;
        state = CombatState.Dead;
        if (animator != null)
            animator.SetTrigger("Death");

        if (hud != null)
            hud.ShowStatus("You were defeated");

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}

public struct DamageInfo
{
    public float amount;
    public Vector3 source;
    public bool isHeavy;
    public object attacker;
}

public class LightsaberWeapon : MonoBehaviour
{
    public MeshRenderer bladeRenderer;
    public TrailRenderer trail;
    public ParticleSystem glowBurst;
    public AudioSource humSource;
    public Color bladeColor = new Color(0.2f, 0.9f, 1f, 1f);
    public bool powered = true;

    private void Start()
    {
        if (bladeRenderer == null)
            bladeRenderer = GetComponentInChildren<MeshRenderer>();

        if (trail == null)
            trail = GetComponent<TrailRenderer>();

        ApplyColor();
    }

    public void SwingPulse()
    {
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
            StartCoroutine(DisableTrail());
        }

        if (glowBurst != null)
            glowBurst.Play();

        if (humSource != null)
        {
            humSource.pitch = Random.Range(0.95f, 1.35f);
            if (!humSource.isPlaying)
                humSource.Play();
        }
    }

    private IEnumerator DisableTrail()
    {
        yield return new WaitForSeconds(0.18f);
        if (trail != null)
            trail.emitting = false;
    }

    public void SetPowered(bool enabled)
    {
        powered = enabled;
        ApplyColor();
    }

    private void ApplyColor()
    {
        if (bladeRenderer == null || bladeRenderer.material == null)
            return;

        Color c = powered ? bladeColor : Color.gray;
        bladeRenderer.material.SetColor("_Color", c);
        bladeRenderer.material.SetColor("_EmissionColor", c * 3f);
    }
}
