using UnityEngine;
using UnityEngine.AI;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    public class NPC_Driver : MonoBehaviour
    {
        PolityMember member;
        NavMeshAgent agent;
        Vector3 spawnPos;
        readonly float detectionRadius = 6f;
        public PolityMember enemyTarget, allyEnemyTarget;
        /// <summary>
        /// This PolityMember is retrieved from an Ally's NPC_driver enemyTarget.
        /// </summary>
        public Transform targetArrow;
        void Awake()
        {
            targetArrow = transform.Find("TargetArrow");
            targetArrow.gameObject.SetActive(false);
            member = GetComponent<PolityMember>();
            agent = GetComponent<NavMeshAgent>();
            agent.avoidancePriority = Random.Range(1, 99);
            spawnPos = transform.position;
            enemyTarget = null; allyEnemyTarget = null;
            OnRelationChange += OnPolityStateChanged;
        }

        void Start() => SearchForPolityMembers();

        void Update()
        {
            if (allyEnemyTarget != null)
            {
                MoveTowardsPolityMemberTarget(allyEnemyTarget);
                RotateArrowTowardsTarget(allyEnemyTarget.transform);
            }
            else if (enemyTarget != null)
            {
                MoveTowardsPolityMemberTarget(enemyTarget);
                RotateArrowTowardsTarget(enemyTarget.transform);
            }
        }

        void RotateArrowTowardsTarget(Transform target)
        {
            float originalXRotation = targetArrow.eulerAngles.x;
            float originalZRotation = targetArrow.eulerAngles.z;

            targetArrow.LookAt(target);

            Quaternion additionalRotation = Quaternion.Euler(0, -90, 0);
            targetArrow.rotation *= additionalRotation;

            Vector3 currentEulerAngles = targetArrow.eulerAngles;
            currentEulerAngles.x = originalXRotation;
            currentEulerAngles.z = originalZRotation;
            targetArrow.eulerAngles = currentEulerAngles;
        }
        void MoveTowardsPolityMemberTarget(PolityMember polityMember)
        {
            agent.SetDestination(polityMember.transform.position);
            Vector3 direction = (polityMember.transform.position - transform.position).normalized;
            float singleStep = agent.angularSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, singleStep);
        }

        void OnPolityStateChanged()
        {
            if (allyEnemyTarget != null)
            {
                PolityRelation relation = PM.CheckPolityRelation(member, allyEnemyTarget);
                switch (relation)
                {
                    case PolityRelation.Allies:
                        enemyTarget = null;
                        agent.SetDestination(spawnPos);
                        targetArrow.gameObject.SetActive(false);
                        break;
                    case PolityRelation.Neutral:
                        allyEnemyTarget = null;
                        SearchForPolityMembers();
                        targetArrow.gameObject.SetActive(false);
                        break;
                }
            }
            else if (enemyTarget != null)
            {
                PolityRelation relation = PM.CheckPolityRelation(member, enemyTarget);
                if (relation == PolityRelation.Neutral)
                {
                    enemyTarget = null;
                    agent.SetDestination(spawnPos);
                    targetArrow.gameObject.SetActive(false);
                }
                else SearchForPolityMembers();
            }
            else SearchForPolityMembers();
        }

        void SearchForPolityMembers()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (var hitCollider in hitColliders)
                if (hitCollider.TryGetComponent<PolityMember>(out var polityMember))
                    if (polityMember != member)
                    {
                        PolityRelation relation = PM.CheckPolityRelation(member, polityMember);
                        switch (relation)
                        {
                            case PolityRelation.Allies:
                                NPC_Driver allyNPC = polityMember.GetComponent<NPC_Driver>();
                                if (allyNPC.enemyTarget != null)
                                    if (allyNPC.enemyTarget != null)
                                    {
                                        allyEnemyTarget = allyNPC.enemyTarget;
                                        targetArrow.gameObject.SetActive(true);
                                    }
                                break;
                            case PolityRelation.Enemies:
                                allyEnemyTarget = null;
                                enemyTarget = polityMember;
                                agent.updateRotation = false;
                                targetArrow.gameObject.SetActive(true);
                                break;
                        }
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