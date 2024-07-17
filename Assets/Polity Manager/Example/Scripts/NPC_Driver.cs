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
        Transform currentDestination;
        private Vector3 lastPosition;
        private Vector3 currentVelocity;
        void Start()
        {
            member = GetComponent<PolityMember>();
            animator = GetComponent<Animator>();
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
            if (currentDestination != null)
            {
                agent.SetDestination(currentDestination.position);
                Vector3 direction = (currentDestination.position - transform.position).normalized;
                float singleStep = agent.angularSpeed * Time.deltaTime;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, singleStep);

                currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
                lastPosition = transform.position;
                animator.SetFloat("Blend", GetRelativeVelocity().y);
                if (agent.remainingDistance < 1.1f)
                {
                    animator.SetLayerWeight(1, 1);
                }
            }
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
            SearchForPolityMembers();
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
                            case PolityRelation.Neutral:
                                break;
                            case PolityRelation.Allies:
                                break;
                            case PolityRelation.Enemies:
                                currentDestination = polityMember.transform;
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