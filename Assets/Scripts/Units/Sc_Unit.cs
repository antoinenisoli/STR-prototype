﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum UnitState
{
    IsUnactive,
    IsMoving,
    IsAttacking,
    IsFighting,
}

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Sc_Unit : MonoBehaviour
{
    protected NavMeshAgent agent => GetComponent<NavMeshAgent>();
    protected MeshRenderer mr => GetComponentInChildren<MeshRenderer>();
    [SerializeField] TrailRenderer trail;

    [Header("Selection")]
    [SerializeField] protected Sc_HealthSystem health;
    public bool highlighted;
    [SerializeField] protected Material HighlightMat;
    protected Material baseMat;
    protected Sc_Unit lastTarget;

    [Header("Caracteristics")]
    [SerializeField] protected float firePower = 3;
    [SerializeField] protected UnitState currentState = UnitState.IsUnactive;
    [SerializeField] protected GameObject bloodFx;

    [Header("Fight")]
    [SerializeField] protected float shootRange = 5;
    [SerializeField] protected float shootSpeed = 1.5f;
    protected float shootTimer;
    public LayerMask ground = 1 >> 2;

    private void Awake()
    {
        baseMat = mr.material;
        health.Initialize(this);
    }

    [ContextMenu("Place unit")]
    void Place()
    {
        bool closestGround = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10, ground);
        if (closestGround)
            transform.position = hit.point + Vector3.up * transform.lossyScale.y;
    }

    public virtual void Death()
    {
        Destroy(gameObject);
    }

    public void TakeDamages(float amount)
    {
        health.TakeDamages(amount);
        Instantiate(bloodFx, transform.position, Quaternion.identity);
    }

    public void Attack(Sc_Unit target)
    {
        lastTarget = target;
        currentState = UnitState.IsAttacking;
    }

    public virtual void Shoot()
    {
        shootTimer = 0;
        lastTarget.TakeDamages(firePower);
        StartCoroutine(ShowRay());
    }

    IEnumerator ShowRay()
    {
        trail.gameObject.SetActive(true);
        trail.transform.position = lastTarget.transform.position;
        yield return new WaitForSeconds(1f);
        trail.transform.position = transform.position;
        yield return new WaitForSeconds(0.25f);
        trail.gameObject.SetActive(false);
    }

    public virtual void Behavior()
    {
        switch (currentState)
        {
            case UnitState.IsMoving:
                if (Vector3.Distance(transform.position, agent.destination) < 3.5f)
                {
                    agent.isStopped = true;
                    currentState = UnitState.IsUnactive;
                }
                break;

            case UnitState.IsAttacking:
                if (Vector3.Distance(transform.position, lastTarget.transform.position) < shootRange)
                {
                    currentState = UnitState.IsFighting;
                    agent.isStopped = true;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(lastTarget.transform.position);
                }
                break;

            case UnitState.IsFighting:
                if (lastTarget == null)
                {
                    currentState = UnitState.IsUnactive;
                    return;
                }

                if (Vector3.Distance(transform.position, lastTarget.transform.position) < shootRange)
                {
                    shootTimer += Time.deltaTime;
                    agent.isStopped = true;
                    if (shootTimer > shootSpeed)
                    {
                        Shoot();
                    }
                }
                else
                {
                    shootTimer = 0;
                    Attack(lastTarget);
                }
                break;

            default:
                shootTimer = 0;
                break;
        }
    }

    public virtual void Update()
    {
        Behavior();
    }
}
