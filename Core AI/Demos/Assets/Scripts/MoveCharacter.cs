using UnityEngine;

namespace InboraStudioAIDemo
{
    public class MoveCharacter : MonoBehaviour
    {
        
        CharacterController controller;
        Animator anim;
        public float playerSpeed = 5f;
        public bool lockCursor = true;
        public bool lockYPosition = true;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();
            transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

            if (lockCursor) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        void Update()
        {
            Vector3 move = new Vector3(-Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
            controller.Move(move * Time.deltaTime * playerSpeed);


            if (lockYPosition) {
                var temp = transform.position;
                temp.y = -1.75f;
                transform.position = temp;
            }
            

            if (Input.GetKey(KeyCode.A))
            {
                transform.localRotation = Quaternion.Euler(0f, -180f, 0f);
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            //animation
            if (move.x != 0f || move.z != 0f) {
                anim.SetBool("Running", true);
            }else{
                anim.SetBool("Running", false);
            }
        }
    }
}

