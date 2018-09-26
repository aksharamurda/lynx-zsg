using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public struct Character
    {
        public bool IsValid;

        public GameObject Object;

        public PlayerMotor Motor;

        public bool IsInSight(float height, float delta)
        {
            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var position = ViewportPoint(height);
            return position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0;
        }

        public bool IsAnyInSight(float delta)
        {
            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var position = ViewportPoint();
            if (position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0)
                return true;

            position = ViewportPoint(2);
            if (position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0)
                return true;

            return false;
        }

        public Vector3 ViewportPoint(float height = 0)
        {
            if (Object == null || CameraManager.Main == null)
                return Vector2.zero;

            return CameraManager.Main.WorldToViewportPoint(Object.transform.position);
        }
    }

    public static class Characters
    {
        public static IEnumerable<Character> All
        {
            get
            {
                foreach (var character in list)
                    if (character.IsValid && character.Motor.IsAlive)
                        yield return character;
            }
        }

        public static Character MainPlayer;

        private static Dictionary<GameObject, Character> dictionary = new Dictionary<GameObject, Character>();
        private static List<Character> list = new List<Character>();

        public static void Register(PlayerMotor motor)
        {
            if (motor == null)
                return;

            var build = Build(motor);
            dictionary[motor.gameObject] = build;

            if (MainPlayer.Object == null)
                if (motor.GetComponent<PlayerController>())
                    MainPlayer = build;

            var isContained = false;

            for (int i = 0; i < list.Count; i++)
                if (list[i].Motor == motor)
                {
                    list[i] = Build(motor);
                    isContained = true;
                }

            if (!isContained)
                list.Add(build);
        }

        public static void Unregister(PlayerMotor motor)
        {
            if (motor != null && dictionary.ContainsKey(motor.gameObject))
                dictionary.Remove(motor.gameObject);

            for (int i = 0; i < list.Count; i++)
                if (list[i].Motor == motor)
                {
                    list.RemoveAt(i);
                    break;
                }
        }

        public static Character Get(GameObject gameObject)
        {
            if (!dictionary.ContainsKey(gameObject))
                dictionary[gameObject] = Build(gameObject.GetComponent<PlayerMotor>());

            return dictionary[gameObject];
        }

        public static Character Build(PlayerMotor motor)
        {
            Character character;

            if (motor != null)
            {
                character.IsValid = true;
                character.Object = motor.gameObject;
                character.Motor = motor;
            }
            else
            {
                character.IsValid = false;
                character.Object = null;
                character.Motor = null;
            }

            return character;
        }
    }
}
