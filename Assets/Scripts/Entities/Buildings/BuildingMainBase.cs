﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingMainBase : Building
{
    public override BuildingType buildingType => BuildingType.MainBase;

    [Header("MainBase")]
    [SerializeField] float constructionRadius = 50;
    [SerializeField] Projector projectArea;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, constructionRadius);
    }

    public bool InRange(Vector3 buildingToPlace)
    {
        float dist = Vector3.Distance(transform.position, buildingToPlace);
        return dist < constructionRadius;
    }

    public void SwitchBuildingMode(bool b)
    {
        projectArea.orthographicSize = constructionRadius * 1.15f;
        projectArea.gameObject.SetActive(b);
    }

    public override void Death()
    {
        switch (myTeam)
        {
            case Team.Player:
                EventManager.Instance.onEndGame.Invoke(false);
                break;
            case Team.Enemy:
                EventManager.Instance.onEndGame.Invoke(true);
                break;
        }

        base.Death();
    }

    public override void UseBuilding()
    {
        
    }
}
