using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CodeMonkey.Toolkit.TMousePosition {

    public class MousePosition2D {


        public static Vector3 GetPosition() {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePosition = Mouse.current.position.value;
#else
            Vector2 mousePosition = Input.mousePosition;
#endif
            Vector3 position = Camera.main.ScreenToWorldPoint(mousePosition);
            position.z = 0f;
            return position;
        }


    }

}