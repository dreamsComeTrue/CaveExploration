using System;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int id;
        public string userName;
        public Vector3 position;
        public Quaternion rotation;

        private float moveSpeed = 6.0f / Constants.TICKS_PER_SEC;
        private bool[] inputs;

        public Player(int id, string userName, Vector3 position)
        {
            this.id = id;
            this.userName = userName;
            this.position = position;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 inputDirection = Vector2.Zero;

            if (inputs[0])
            {
                inputDirection.Y += 1;
            }

            if (inputs[1])
            {
                inputDirection.Y -= 1;
            }

            if (inputs[2])
            {
                inputDirection.X += 1;
            }

            if (inputs[3])
            {
                inputDirection.X -= 1;
            }

            Move(inputDirection);
        }

        private void Move(Vector2 inputDirection)
        {
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;
            position += moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] inputs, Quaternion rotation)
        {
            this.inputs = inputs;
            this.rotation = rotation;
        }
    }
}
