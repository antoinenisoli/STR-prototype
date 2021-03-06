﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEngine : Building
{
    public override BuildingType buildingType => BuildingType.Engine;

    [Header("Engine")]
    [SerializeField] int resourceAmount = 5;
    [SerializeField] float f_waitingTime = 15;
    float timer;

    public override void UseBuilding()
    {
        timer += Time.deltaTime;
        if (timer > f_waitingTime)
        {
            resourceManager.ModifyValue(resourceAmount, ResourceType.Oil);
            resourceManager.ModifyValue(resourceAmount, ResourceType.Steel);
            timer = 0;
        }
    }
}
