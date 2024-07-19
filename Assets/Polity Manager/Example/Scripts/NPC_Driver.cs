using UnityEngine;
using UnityEngine.AI;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    public class NPC_Driver : MonoBehaviour
    {
        PolityMember member;
        Animator animator;
        NavMeshAgent agent;
        Vector3 lastPosition;
        Vector3 currentVelocity;
        Vector3 spawnPos;
        public PolityMember enemyTarget = null;
        /// <summary>
        /// This PolityMember is retrieved from an Ally's NPC_driver enemyTarget.
        /// </summary>
        public PolityMember allyEnemyTarget = null;

        void Awake()
        { enemyTarget = null; allyEnemyTarget = null; }
        void Start()
        {
            member = GetComponent<PolityMember>();
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            SearchForPolityMembers();
            agent.avoidancePriority = Random.Range(1, 99);

            spawnPos = transform.position;
        }
        void OnEnable() => OnRelationChange += OnPolityStateChanged;
        // Update is called once per frame
        void Update()
        {
            if (allyEnemyTarget != null) MoveTowardsPolityMemberTarget(allyEnemyTarget);
            else if (enemyTarget != null) MoveTowardsPolityMemberTarget(enemyTarget);
            currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
            animator.SetFloat("Blend", GetRelativeVelocity().y);
        }

        void MoveTowardsPolityMemberTarget(PolityMember polityMember)
        {
            agent.SetDestination(polityMember.transform.position);
            Vector3 direction = (polityMember.transform.position - transform.position).normalized;
            float singleStep = agent.angularSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, singleStep);

            if (agent.remainingDistance < 1.05f) animator.SetLayerWeight(1, 1);
        }

        Vector2 GetRelativeVelocity()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(currentVelocity);
            return new Vector2(localVelocity.x, localVelocity.z);
        }


        readonly float detectionRadius = 35.0f;

        void OnPolityStateChanged()
        {
            if (allyEnemyTarget != null)
            {
                PolityRelation relation = PM.GetPolityRelation(member, allyEnemyTarget);
                Debug.Log("Ally: " + relation + " " + gameObject.name + " to " + allyEnemyTarget.name, gameObject);
                switch (relation)
                {
                    case PolityRelation.Neutral:
                        Debug.Log("Ally neutral");
                        allyEnemyTarget = null;
                        SearchForPolityMembers();
                        break;
                }
            }
            else if (enemyTarget != null)
            {
                PolityRelation relation = PM.GetPolityRelation(member, enemyTarget);
                if (relation == PolityRelation.Neutral)
                {
                    Debug.Log("Enemy Neutral");
                    enemyTarget = null;
                    animator.SetLayerWeight(1, 0);
                    agent.SetDestination(spawnPos);
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
                        PolityRelation relation = PM.GetPolityRelation(member, polityMember);
                        switch (relation)
                        {
                            case PolityRelation.Allies:
                                NPC_Driver allyNPC = polityMember.GetComponent<NPC_Driver>();
                                if (allyNPC.enemyTarget != null)
                                    allyEnemyTarget = allyNPC.enemyTarget;
                                break;
                            case PolityRelation.Enemies:
                                allyEnemyTarget = null;
                                enemyTarget = polityMember;
                                agent.updateRotation = false;
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