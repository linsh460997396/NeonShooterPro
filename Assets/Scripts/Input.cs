using UnityEngine;
using System.Linq;

namespace NeonShooter
{
    public static class Input
    {
        private static bool[] keyboardState, lastKeyboardState;
        private static Vector2 mousePosition, lastMousePosition;

        private static bool isAimingWithMouse = false;

        public static Vector2 MousePosition { get { return mousePosition; } }

        static Input()
        {
            keyboardState = new bool[256];
            lastKeyboardState = new bool[256];
        }

        public static void Update()
        {
            lastMousePosition = mousePosition;
            mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

            for (int i = 0; i < 256; i++)
            {
                lastKeyboardState[i] = keyboardState[i];
                keyboardState[i] = UnityEngine.Input.GetKey((KeyCode)i);
            }

            if (new[] { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow }.Any(x => UnityEngine.Input.GetKey(x)))
                isAimingWithMouse = false;
            else if (mousePosition != lastMousePosition)
                isAimingWithMouse = true;
        }

        public static bool WasKeyPressed(KeyCode key)
        {
            return !lastKeyboardState[(int)key] && keyboardState[(int)key];
        }

        public static Vector2 GetMovementDirection()
        {
            Vector2 direction = Vector2.zero;

            if (UnityEngine.Input.GetKey(KeyCode.A))
                direction.x -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.D))
                direction.x += 1;
            if (UnityEngine.Input.GetKey(KeyCode.W))
                direction.y -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.S))
                direction.y += 1;

            if (direction.sqrMagnitude > 1)
                direction.Normalize();

            return direction;
        }

        public static Vector2 GetAimDirection()
        {
            if (isAimingWithMouse)
                return GetMouseAimDirection();

            Vector2 direction = Vector2.zero;

            if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
                direction.x -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
                direction.x += 1;
            if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
                direction.y -= 1;
            if (UnityEngine.Input.GetKey(KeyCode.DownArrow))
                direction.y += 1;

            if (direction == Vector2.zero)
                return Vector2.zero;
            else
                return direction.normalized;
        }

        private static Vector2 GetMouseAimDirection()
        {
            Vector2 direction = mousePosition - PlayerShip.Instance.Position;

            if (direction.sqrMagnitude < 0.001f)
                return Vector2.zero;
            else
                return direction.normalized;
        }

        public static bool WasBombButtonPressed()
        {
            return WasKeyPressed(KeyCode.Space);
        }
    }
}