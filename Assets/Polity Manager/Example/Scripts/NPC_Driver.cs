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
        public PolityMember closestTarget = null;
        void Start()
        {
            member = GetComponent<PolityMember>();
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            SearchForPolityMembers();
            agent.avoidancePriority = Random.Range(1, 99);

            spawnPos = transform.position;
        }
        void OnEnable()
        {
            OnPolityRelationChange += OnPolityStateChanged;
        }
        // Update is called once per frame
        void Update()
        {
            if (closestTarget != null)
            {
                agent.SetDestination(closestTarget.transform.position);
                Vector3 direction = (closestTarget.transform.position - transform.position).normalized;
                float singleStep = agent.angularSpeed * Time.deltaTime;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, singleStep);

                if (agent.remainingDistance < 1.05f) animator.SetLayerWeight(1, 1);
            }
            currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
            animator.SetFloat("Blend", GetRelativeVelocity().y);
        }

        public Vector2 GetRelativeVelocity()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(currentVelocity);
            return new Vector2(localVelocity.x, localVelocity.z);
        }

        public Vector2 GetRelativeVelocity2()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            float forwardVelocity = Vector3.Dot(currentVelocity, forward);
            float rightVelocity = Vector3.Dot(currentVelocity, right);

            return new Vector2(rightVelocity, forwardVelocity);
        }
        readonly float detectionRadius = 35.0f;

        void OnPolityStateChanged()
        {
            if (closestTarget != null)
            {
                PolityRelation relation = PM.ComparePolityRelation(member, closestTarget);
                Debug.LogError("!relation! " + relation + " " + gameObject.name + " to " + closestTarget.name, gameObject);
                //If there was a closestTarget but the relation was neutral, we know that the target was an enemy of an ally
                if (relation == PolityRelation.Neutral)
                {
                    Debug.LogError("Neutral");
                    SearchForPolityMembers();

                }
                else if (relation == PolityRelation.Enemies)
                {
                    Debug.LogError("Enemies");

                }
                else
                {
                    SearchForPolityMembers();
                }
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
                        PolityRelation relation = PM.ComparePolityRelation(member, polityMember);
                        switch (relation)
                        {
                            case PolityRelation.Allies:
                                NPC_Driver allyNPC = polityMember.GetComponent<NPC_Driver>();
                                if (allyNPC.closestTarget != null)
                                    closestTarget = allyNPC.closestTarget;
                                // PM.ModifyPolityRelation(member, closestTarget.polityName, PolityRelation.Enemies);
                                break;
                            case PolityRelation.Enemies:
                                Debug.LogError("Enemy " + gameObject.name, gameObject);
                                closestTarget = polityMember;
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