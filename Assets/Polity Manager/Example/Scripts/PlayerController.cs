using UnityEngine;

namespace KhiemLuong
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 4.0f; // Movement speed
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float gravityValue = -9.81f;

        private void Start()
        {
            controller = gameObject.GetComponent<CharacterController>();
        }

        void Update()
        {
            groundedPlayer = controller.isGrounded;
            if (groundedPlayer && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }

            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            controller.Move(move * Time.deltaTime * speed);

            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }
}