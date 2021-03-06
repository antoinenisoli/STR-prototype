﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using DG.Tweening;

enum Detectables
{
    Ground,
    HoverUnit,
    Buildings,
    None,
}

public class SelectionManager : MonoBehaviour
{
    Camera mainCam;
    bool stopGame;
    [SerializeField] List<UnitAlly> allPlayerUnits = new List<UnitAlly>();
    [SerializeField] List<UnitEnemy> allEnemyUnits = new List<UnitEnemy>();
    [SerializeField] List<Building> allBuildings = new List<Building>();

    [Header("Select")]
    [SerializeField] MouseState mouseState;
    [SerializeField] LayerMask unitLayer;
    [SerializeField] LayerMask buildingsLayer;
    [SerializeField] Detectables isDetecting;
    public Building selectedBuilding;
    public RaycastHit hit;
    bool invalid, attackUnit;

    [Header("Rectangle selection")]
    [SerializeField] Color textureColor = Color.white;
    public List<UnitAlly> selectedUnits = new List<UnitAlly>();
    Rect selectRect;
    Vector3 mousePos;

    [Header("Move units")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] FormationType formationType;
    UnitsFormation formation = new UnitsFormation();
    public GameObject moveMark;
    public float dummyAnimDuration = 3;

    private void Awake()
    {
        mainCam = Camera.main;
        allPlayerUnits = FindObjectsOfType<UnitAlly>().ToList();
        allEnemyUnits = FindObjectsOfType<UnitEnemy>().ToList();
        allBuildings = FindObjectsOfType<Building>().ToList();
    }

    private void Start()
    {
        EventManager.Instance.onNewUnit.AddListener(NewEntity);
        EventManager.Instance.onEndGame.AddListener(StopGame);
    }

    private void OnGUI()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, textureColor);
        texture.Apply();
        GUI.DrawTexture(selectRect, texture);
    }

    void StopGame(bool b)
    {
        stopGame = true;
    }

    public static void GenerateEntity(Entity _new)
    {
        EventManager.Instance.onNewUnit.Invoke(_new);
    }

    public void NewEntity(Entity entity)
    {
        if (entity.GetComponent<Building>())
            allBuildings.Add(entity as Building);
        else if (entity.GetComponent<UnitAlly>())
            allPlayerUnits.Add(entity as UnitAlly);
        else if (entity.GetComponent<UnitEnemy>())
            allEnemyUnits.Add(entity as UnitEnemy);
    }

    void MoveUnits()
    {
        if (hit.transform != null)
        {
            Vector3 position = hit.point;
            invalid = !NavMesh.SamplePosition(position, out _, 5f, NavMesh.AllAreas) && selectedUnits.Count > 0;
        }
        else
            invalid = false;

        if (selectedUnits.Count > 0 && isDetecting == Detectables.Ground)
        {
            Vector3 position = hit.point;           
            if (Input.GetMouseButtonDown(1) && !invalid)
                formation.DoFormation(this, position);
        }
    }

    void SelectUnits()
    {
        foreach (var ally in allPlayerUnits)
        {
            if (hit.collider != null)
            {
                UnitAlly unit = hit.collider.gameObject.GetComponentInParent<UnitAlly>();
                ally.highlighted =
                (
                hit.collider != null
                && !EventSystem.current.IsPointerOverGameObject()
                && ally.Equals(unit)
                && !ally.selected
                )
                || selectedUnits.Contains(ally)
                ;
            }
            else
                ally.highlighted = false;
        }

        foreach (var enemy in allEnemyUnits)
        {
            if (hit.collider != null)
            {
                UnitEnemy unit = hit.collider.gameObject.GetComponentInParent<UnitEnemy>();
                enemy.highlighted =
                    hit.collider != null
                    && selectedUnits.Count > 0
                    && enemy.Equals(unit)
                    && !EventSystem.current.IsPointerOverGameObject()
                    ;
            }
            else
                enemy.highlighted = false;
        }

        foreach (var building in allBuildings)
        {
            if (hit.collider != null)
            {
                Building buildScript = hit.collider.gameObject.GetComponentInParent<Building>();
                bool hover = building.Equals(buildScript) && !building.selected && !EventSystem.current.IsPointerOverGameObject();
                bool teamCondition = true;
                teamCondition = building.myTeam == Team.Player ? building.currentState == BuildingState.Builded : selectedUnits.Count > 0;
                building.highlighted = hover && teamCondition;
            }
            else
                building.highlighted = false;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            for (int i = 0; i < allPlayerUnits.Count; i++)
            {
                allPlayerUnits[i].Select(false);
                selectedUnits.Clear();
            }

            if (selectedBuilding)
            {
                selectedBuilding.SelectMe(false);
                selectedBuilding = null;
            }

            if (isDetecting == Detectables.HoverUnit) //select a unit
            {
                UnitAlly thisUnit = hit.collider.GetComponentInParent<UnitAlly>();
                if (thisUnit && !thisUnit.health.isDead && !selectedUnits.Contains(thisUnit))
                {
                    thisUnit.Select(true);
                    selectedUnits.Add(thisUnit);
                }
            }
            else if (isDetecting == Detectables.Buildings) //select a building
            {
                Building pointedBuilding = hit.collider.GetComponentInParent<Building>();
                if (pointedBuilding.currentState == BuildingState.Builded && !pointedBuilding.health.isDead)
                {
                    selectedBuilding = pointedBuilding;
                    selectedBuilding.SelectMe(true);
                }
            }
        }
    }

    void InteractUnits()
    {
        if (hit.transform != null)
        {
            Entity target = hit.collider.GetComponentInParent<Entity>();
            attackUnit = selectedUnits.Count > 0 && target && target.myTeam == Team.Enemy;
            if (attackUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    foreach (var unit in selectedUnits)
                        unit.Attack(target);

                    CreateDummy(target.transform.position, Quaternion.identity, Color.red);
                }
            }
        }
        else
            attackUnit = false;
    }

    public void CreateDummy(Vector3 position, Quaternion rotation, Color color)
    {
        GameObject dummy = Instantiate(moveMark, position + Vector3.up * 0.2f, rotation);
        dummy.GetComponent<MeshRenderer>().material.color = color;
        Vector3 currentScale = dummy.transform.localScale;
        dummy.transform.DOScale(currentScale * 1.5f, dummyAnimDuration / 3);
        dummy.transform.DOScale(Vector3.zero, dummyAnimDuration / 3).SetDelay(dummyAnimDuration / 3);
        Destroy(dummy, dummyAnimDuration);
    }

    void DrawRectangle()
    {
        selectRect = new Rect(mousePos.x, Screen.height - mousePos.y, Input.mousePosition.x - mousePos.x, -1 * (Input.mousePosition.y - mousePos.y));

        if (selectRect.width < 0)
        {
            selectRect.x += selectRect.width;
            selectRect.width = Mathf.Abs(selectRect.width);
        }
        if (selectRect.height < 0)
        {
            selectRect.y += selectRect.height;
            selectRect.height = Mathf.Abs(selectRect.height);
        }
    }

    void SelectInBox()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
                mousePos = Input.mousePosition;

            DrawRectangle();
            if (selectRect.size.y < 2 || selectRect.size.x < 2)
                return;

            foreach (var unit in allPlayerUnits)
            {
                if (unit == null)
                {
                    allPlayerUnits.Remove(unit);
                    return;
                }

                Vector3 unitPos = mainCam.WorldToScreenPoint(unit.transform.position);
                unitPos.y = Screen.height - unitPos.y;

                if (selectRect.Contains(unitPos))
                {
                    if (!selectedUnits.Contains(unit))
                        selectedUnits.Add(unit);
                }
                else
                {
                    if (selectedUnits.Contains(unit))
                        selectedUnits.Remove(unit);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectRect = new Rect();
            if (selectedUnits.Count <= 0)
                return;

            foreach (var item in selectedUnits)
                item.Select(true);
        }
    }

    void DetectItems()
    {
        Ray screenPoint = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(screenPoint, out hit, Mathf.Infinity, unitLayer))
            isDetecting = Detectables.HoverUnit;
        else if (Physics.Raycast(screenPoint, out hit, Mathf.Infinity, buildingsLayer))
            isDetecting = Detectables.Buildings;
        else if (Physics.Raycast(screenPoint, out hit, Mathf.Infinity, groundLayer))
            isDetecting = Detectables.Ground;
        else
            isDetecting = Detectables.None;
    }

    void ManageCursor()
    {
        CursorManager.instance.currentState = attackUnit ? MouseState.Attack : invalid ? MouseState.Invalid : MouseState.Valid;
    }

    private void Update()
    {
        if (stopGame)
            return;

        DetectItems();
        SelectInBox();
        SelectUnits();
        ManageCursor();
        InteractUnits();
        MoveUnits();
    }
}
