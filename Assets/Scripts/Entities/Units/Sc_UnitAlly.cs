﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sc_UnitAlly : Sc_Unit
{
    [Header("Unit ally")]
    [SerializeField] protected Material SelectedMat;
    [SerializeField] GameObject selectedVFX;
    public bool selected;

    public bool CorrectPath(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(target, path);
    }

    public void MoveTo(Vector3 pos, out bool valid)
    {
        NavMeshPath path = new NavMeshPath();
        valid = agent.CalculatePath(pos, path);
        if (valid)
        {
            agent.isStopped = false;
            currentState = UnitState.IsMoving;
            agent.SetDestination(pos);
        }
    }

    public void Select(bool select)
    {
        selected = select;
    }

    public override void Update()
    {
        selectedVFX.SetActive(selected);
        if (HighlightMat != null && SelectedMat != null)
            mr.material = selected ? HighlightMat : baseMat;

        base.Update();
    }
}