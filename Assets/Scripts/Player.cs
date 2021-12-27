using UnityEngine;

namespace Assets.Scripts
{
    public class Player : MonoBehaviour
    {
        public int Id { get; set; }
        public string Username { get; set; }

        private readonly float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] inputs;

        public void Initialize(int id, string username)
        {
            Id = id;
            Username = username;

            inputs = new bool[4];
        }

        public void FixedUpdate()
        {
            Vector2 moveDirection = Vector2.zero;

            if (inputs[0])
            {
                moveDirection.y += 1;
            }
            if (inputs[1])
            {
                moveDirection.y -= 1;
            }
            if (inputs[2])
            {
                moveDirection.x -= 1;
            }
            if (inputs[3])
            {
                moveDirection.x += 1;
            }

            Move(moveDirection);
        }

        private void Move(Vector2 _inputDirection)
        {
            Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
            transform.position += _moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            transform.rotation = _rotation;
        }
    }
}
