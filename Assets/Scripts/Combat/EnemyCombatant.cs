using System.Collections;
using UnityEngine;

public class EnemyCombatant : MonoBehaviour
{
    public enum EnemyType
    {
        Grunt,
        Brute,
        Assassin
    }

    [Header("Identity")]
    public EnemyType enemyType = EnemyType.Grunt;

    [Header("References")]
    public CharacterController characterController;
    public Animator animator;
    public Transform player;

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float moveSpeed = 2.8f;
    public float attackRange = 2.2f;
    public float attackDamage = 18f;
    public float attackWindup = 0.28f;
    public float attackCooldown = 1.25f;
    public float detectionRadius = 12f;

    private float nextAttackTime;
    private bool isDead;

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (player == null)
            player = FindObjectOfType<PlayerController>()?.transform;

        currentHealth = maxHealth;
        SetEnemyProfile();
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRadius)
        {
            if (animator != null)
                animator.SetFloat("Movement", 0f);
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer.normalized, Vector3.up), 7f * Time.deltaTime);

        if (distance > attackRange)
        {
            Vector3 step = toPlayer.normalized * moveSpeed;
            if (characterController != null)
                characterController.Move(step * Time.deltaTime);
            else
                transform.position += step * Time.deltaTime;

            if (animator != null)
                animator.SetFloat("Movement", 1f);
        }
        else
        {
            if (animator != null)
                animator.SetFloat("Movement", 0f);

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private void SetEnemyProfile()
    {
        switch (enemyType)
        {
            case EnemyType.Brute:
                maxHealth = 180;
                currentHealth = 180;
                moveSpeed = 2.1f;
                attackDamage = 28f;
                attackRange = 2.6f;
                attackCooldown = 1.5f;
                break;

            case EnemyType.Assassin:
                maxHealth = 90;
                currentHealth = 90;
                moveSpeed = 3.5f;
                attackDamage = 22f;
                attackRange = 2.0f;
                attackCooldown = 0.9f;
                break;

            default:
                maxHealth = 100;
                currentHealth = 100;
                moveSpeed = 2.8f;
                attackDamage = 18f;
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackWindup);

        if (player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.45f)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.TakeDamage(Mathf.RoundToInt(attackDamage), transform.position);
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        if (isDead)
            return;

        currentHealth -= Mathf.RoundToInt(info.amount);

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        if (animator != null)
            animator.SetTrigger("Death");

        StartCoroutine(DespawnAfterDelay());
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(2.2f);
        Destroy(gameObject);
    }
}
