﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum ResourceType
{
    Steel,
    Oil,
}

public abstract class ResourcesManager : MonoBehaviour
{
    public Team myTeam;
    [ColorUsage(true, true)]
    public Color teamColor = Color.blue;
    public bool gameEnded;
    public Resource[] resources;
    public Dictionary<ResourceType, Resource> myResources = new Dictionary<ResourceType, Resource>();

    private void Awake()
    {
        foreach (var item in resources)
            if (!myResources.ContainsKey(item.type))
                myResources.Add(item.type, item);
    }

    public virtual void Start()
    {
        EventManager.Instance.onEndGame.AddListener(End);
    }

    void End(bool b)
    {
        gameEnded = true;
    }

    public virtual void ModifyValue(int amount, ResourceType res)
    {
        if (amount == 0)
            return;

        resources[(int)res].CurrentAmount += amount;
    }

    public bool CanPay(ResourceCost[] costs)
    {
        if (gameEnded)
            return false;

        bool canPay = false;
        foreach (ResourceCost cost in costs)
        {
            Resource resource = myResources[cost.resourceType];
            canPay = (resource.CurrentAmount + cost.value) >= 0;
        }

        return canPay;
    }
}
