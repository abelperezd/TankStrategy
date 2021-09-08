using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint = null; //position of the prefab where we shot at

    public Transform GetAimPoint()
    {
        return aimAtPoint;
    }
}
