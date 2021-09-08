using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 2f;
    [SerializeField] private float unitSpawnDuration = 5;

    [SyncVar(hook = nameof(ClienHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private RTSPlayer player;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    
    #region Server

    public override void OnStartServer()
    {
        if (health == null)
            print("health is null");
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0) { return; }
        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) { return; }

        Vector3 spawnPoint = new Vector3(0, 0, -2f);

        Vector3 pos = transform.position + spawnPoint;
        GameObject unitInstance = Instantiate(unitPrefab.gameObject, pos, Quaternion.identity);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPoint.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(pos + spawnOffset);

        queuedUnits--;
        unitTimer = 0;

    }

    #endregion

    #region Client

    //this method is called whenever you click the object were this script is attached
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        if (!hasAuthority) { return; }

        CmdSpawnUnit();
    }

    private void ClienHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion
}
