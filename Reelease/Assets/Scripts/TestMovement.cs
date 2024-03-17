using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestMovement : MonoBehaviour
{
    [SerializeField]
    private Transform moveTowards;
    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();  
    }

    private void Update()
    {
        agent.destination = moveTowards.position;
    }
}
