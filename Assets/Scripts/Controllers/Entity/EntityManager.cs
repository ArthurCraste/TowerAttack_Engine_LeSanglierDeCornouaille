using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : SingletonMono<EntityManager>
{
    // Ref vers la global target des entités Player
    public GameObject towerIA;
    // Ref vers la global target des entités IA
    public GameObject towerPlayer;

    public GameObject outpostIA1, outpostIA2;
    public GameObject outpostPlayer1, outpostPlayer2;

    public Action<Alignment> OnTowerDestroy;

    public void PopElementFromData(EntityData entityData, Vector3 position)
    {
        GameObject newInstantiate = PoolManager.Instance.GetElement(entityData);
        if (newInstantiate != null)
        {
            
            SetPopElement(newInstantiate, position);
        }
        else
        {
            Debug.LogError("NO POOLED DATA PREFAB : " + entityData.name);
        }
    }

    public void PopElementFromPrefab(GameObject prefabToPop, Vector3 position)
    {
        GameObject newInstantiate = PoolManager.Instance.GetElement(prefabToPop);
        if (newInstantiate != null)
        {
            SetPopElement(newInstantiate, position);
        }
        else
        {
            Debug.LogError("NO POOLED PREFAB : " + prefabToPop.name);
        }
    }


    // Fonction centrale.
    // Toute instantiation d'entité doit passer par cette fonction.
    // Elle centralise l'initialisation de l'entité.
    public void SetPopElement(GameObject newInstantiate, Vector3 position)
    {
        newInstantiate.transform.position = position;
        newInstantiate.SetActive(true);
        Entity entity = newInstantiate.GetComponent<Entity>();

        if (entity is EntityMoveable moveable)
        {
            moveable.entityManager = this;
            if (moveable.entityData.alignment == Alignment.IA)
            {
                float m_DistanceOutpost1 = Vector3.Distance(position, outpostPlayer1.transform.position);
                float m_DistanceOutpost2 = Vector3.Distance(position, outpostPlayer2.transform.position);
                float m_DistanceTower = Vector3.Distance(position, towerPlayer.transform.position);
                if (!outpostPlayer1 && !outpostPlayer2)
                {
                    moveable.SetGlobalTarget(towerPlayer);
                }
                else if (outpostPlayer1 && !outpostPlayer2)
                {
                    if (m_DistanceOutpost1 < m_DistanceTower)
                    {
                        moveable.SetGlobalTarget(towerPlayer);
                    }
                    else
                    {
                        moveable.SetGlobalTarget(outpostPlayer1);
                    }
                }
                else if (!outpostPlayer1 && outpostPlayer2)
                {
                    if (m_DistanceOutpost2 < m_DistanceTower)
                    {
                        moveable.SetGlobalTarget(towerPlayer);
                    }
                    else
                    {
                        moveable.SetGlobalTarget(outpostPlayer2);
                    }
                }
                else
                {
                    if (m_DistanceOutpost1 < m_DistanceOutpost2)
                    {
                        moveable.SetGlobalTarget(outpostPlayer1);
                    }
                    else
                    {
                        moveable.SetGlobalTarget(outpostPlayer2);
                    }
                }
            }

            else if (moveable.entityData.alignment == Alignment.Player)
            {
                float m_DistanceOutpost1 = Vector3.Distance(position, outpostIA1.transform.position);
                float m_DistanceOutpost2 = Vector3.Distance(position, outpostIA2.transform.position);
                float m_DistanceTower = Vector3.Distance(position, towerIA.transform.position);
                if (!outpostIA1 && !outpostIA2)
                {
                    moveable.SetGlobalTarget(towerIA);
                }
                else if (outpostIA1 && !outpostIA2)
                {
                    if (m_DistanceOutpost1 < m_DistanceTower)
                    {
                        moveable.SetGlobalTarget(towerIA);
                    }
                    else
                    {
                        moveable.SetGlobalTarget(outpostIA1);
                    }
                }
                else if (!outpostIA1 && outpostIA2)
                {
                    if (m_DistanceOutpost2 < m_DistanceTower)
                    {
                        moveable.SetGlobalTarget(towerIA);
                    }
                    else
                    {
                        moveable.SetGlobalTarget(outpostIA2);
                    }
                }
                else
                {
                    if (m_DistanceOutpost1 < m_DistanceOutpost2)
                    {
                        moveable.SetGlobalTarget(outpostIA1);
                    }
                    else
                    {
                        moveable.SetGlobalTarget(outpostIA2);
                    }
                }
            }

            entity.RestartEntity();
        }
    }

    public void NewTarget(GameObject moveable)
    {

    }

    public void PoolElement(GameObject toPool)
    {
        if (towerPlayer == toPool)
        {
            OnTowerDestroy?.Invoke(Alignment.Player);
        }
        else if (towerIA == toPool)
        {
            OnTowerDestroy?.Invoke(Alignment.IA);
        }

        PoolManager.Instance.PoolElement(toPool);
    }
}
