using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour 
{
    private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    private void Start()
    {
        targeter = GetComponent<Targeter>();
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) { return; }

        if (!CanFireAtTarget()) { return; }

        Quaternion targeRotation = Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targeRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > 1/fireRate + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimPoint().position - projectileSpawnPoint.transform.position);
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.transform.position, projectileRotation);
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}
