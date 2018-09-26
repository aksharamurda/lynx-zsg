using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public sealed class Util
    {
        // Cache
        private static RaycastHit[] _hits = new RaycastHit[64];
        private static Collider[] _colliders = new Collider[64];

        public static float GetViewDistance(Vector3 position, float viewDistance, bool isAlerted)
        {
            return GetViewDistance(viewDistance, isAlerted);
        }

        public static float GetViewDistance(float viewDistance, bool isAlerted)
        {
            var value = viewDistance;
            return value;
        }

        public static Vector3 GetClosestHit(Vector3 origin, Vector3 target, float minDistance, GameObject ignore)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);
            var closestHit = target;

            for (int i = 0; i < Physics.RaycastNonAlloc(origin, vector, _hits); i++)
            {
                var hit = _hits[i];

                if (hit.collider.gameObject != ignore && !hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    {
                        maxDistance = hit.distance;
                        closestHit = hit.point;
                    }
            }

            return closestHit;
        }

        public static void Lerp(ref float Value, float Target, float speed)
        {
            if (Target > Value)
            {
                if (Value + speed > Target)
                    Value = Target;
                else if (speed > 0)
                    Value += speed;
            }
            else
            {
                if (Value - speed < Target)
                    Value = Target;
                else if (speed > 0)
                    Value -= speed;
            }
        }

        public static void LerpAngle(ref float Value, float Target, float speed)
        {
            var delta = Mathf.DeltaAngle(Value, Target);

            if (delta > 0)
            {
                if (Value + speed > Target)
                    Value = Target;
                else if (speed > 0)
                    Value += speed;
            }
            else
            {
                if (Value - speed < Target)
                    Value = Target;
                else if (speed > 0)
                    Value -= speed;
            }
        }

        public static bool InHiearchyOf(GameObject target, GameObject parent)
        {
            var obj = target;

            while (obj != null)
            {
                if (obj == parent)
                    return true;

                if (obj.transform.parent != null)
                    obj = obj.transform.parent.gameObject;
                else
                    obj = null;
            }

            return false;
        }

        public static float FindDeltaPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + +ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.z * ab.z;
            float t = ap_ab / ab2;

            return t;
        }

        public static Vector3 FindClosestToPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + +ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.z * ab.z;
            float t = ap_ab / ab2;

            return a + ab * Mathf.Clamp01(t);
        }

        public static float AngleOfVector(Vector3 vector)
        {
            var v = new Vector2(vector.z, vector.x);

            if (v.sqrMagnitude > 0.01f)
                v.Normalize();

            var sign = (v.y < 0) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, v) * sign;
        }

        public static float DistanceToSegment(Vector3 point, Vector3 p0, Vector3 p1)
        {
            var lengthSqr = (p1 - p0).sqrMagnitude;
            if (lengthSqr <= float.Epsilon) return Vector3.Distance(point, p0);

            var t = Mathf.Clamp01(((point.x - p0.x) * (p1.x - p0.x) +
                                   (point.y - p0.y) * (p1.y - p0.y) +
                                   (point.z - p0.z) * (p1.z - p0.z)) / lengthSqr);

            return Vector3.Distance(point, p0 + (p1 - p0) * t);
        }
    }
}