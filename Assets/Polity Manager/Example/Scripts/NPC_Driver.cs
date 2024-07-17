using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    public class NPC_Driver : MonoBehaviour
    {
        PolityMember member;
        NavMeshAgent agent;
        void Start()
        {
            member = GetComponent<PolityMember>();
            agent = GetComponent<NavMeshAgent>();
            SearchForPolityMembers();
            agent.avoidancePriority = Random.Range(1, 99);
        }
        void OnEnable()
        {
            OnPolityRelationChange += OnPolityStateChanged;
        }
        // Update is called once per frame
        void Update()
        {

        }
        public float detectionRadius = 5.0f;

        void OnPolityStateChanged()
        {
            SearchForPolityMembers();
        }

        void SearchForPolityMembers()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

            foreach (var hitCollider in hitColliders)
                if (hitCollider.TryGetComponent<PolityMember>(out var polityMember))
                    if (polityMember != member)
                    {
                        PM.ComparePolityRelation(member, polityMember);
                    }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        void OnPolityMemberChanged()
        {

        }
    }
}