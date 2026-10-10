using System.Collections;
using UnityEngine;

public class EnemyCombatant : MonoBehaviour, IDamageable
{
    [Header("References")]
    public CharacterController controller;
    public Animator animator;
    public Transform player;

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float attackRange = 2.1f;
    public float moveSpeed = 2.5f;
    public float attackDamage = 18f;
    public float attackCooldown = 1.1f;
    public float attackWindup = 0.25f;

    private float nextAttackTime;
    private bool isDead;

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
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.magnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
        }

        if (toPlayer.magnitude > attackRange)
        {
            Vector3 move = toPlayer.normalized * moveSpeed;
            if (controller != null)
                controller.Move(move * Time.deltaTime);
            else
                transform.position += move * Time.deltaTime;

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

    private IEnumerator AttackRoutine()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackWindup);

        if (player == null)
            yield break;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange + 0.35f)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.TakeDamage(Mathf.RoundToInt(attackDamage));
        }
    }

    private void Die()
    {
        isDead = true;
        if (animator != null)
            animator.SetTrigger("Death");

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2.4f);
        Destroy(gameObject);
    }
}
