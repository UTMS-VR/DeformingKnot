using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    class PhysicalButtonFromStick : IPhysicalButton
    {
        public enum Direction
        {
            Up, Down, Right, Left
        }

        private enum State
        {
            Down, Hold, Up, None
        }

        private IPhysicalStick stick;
        private float threshold;
        private Direction direction;
        private State state = State.None;

        public PhysicalButtonFromStick(IPhysicalStick stick, Direction direction, float threshold = 0.8f)
        {
            this.stick = stick;
            this.direction = direction;
            this.threshold = threshold;
        }

        public bool Get()
        {
            switch (this.direction)
            {
                case Direction.Up:
                    return this.stick.Get().y > this.threshold;
                case Direction.Down:
                    return this.stick.Get().y < -this.threshold;
                case Direction.Right:
                    return this.stick.Get().x > this.threshold;
                case Direction.Left:
                    return this.stick.Get().x < -this.threshold;
                default:
                    throw new System.Exception("This can't happen!");
            }
        }

        public bool GetDown()
        {
            return this.state == State.Down;
        }

        public bool GetUp()
        {
            return this.state == State.Up;
        }

        public void UpdateFirst()
        {
            switch (this.state)
            {
                case State.None:
                    if (this.Get())
                    {
                        this.state = State.Down;
                    }
                    break;
                case State.Down:
                    this.state = State.Hold;
                    break;
                case State.Hold:
                    if (!this.Get())
                    {
                        this.state = State.Up;
                    }
                    break;
                case State.Up:
                    this.state = State.None;
                    break;
            }
        }
    }
}
