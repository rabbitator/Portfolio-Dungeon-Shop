using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Miscellaneous
{
    public static class Extensions
    {
        public static T Previous<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(Arr, src) - 1;
            return j < 0 ? Arr[Arr.Length - 1] : Arr[j];
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(Arr, src) + 1;
            return Arr.Length == j ? Arr[0] : Arr[j];
        }

        public static bool IsLast<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            T[] Arr = (T[])Enum.GetValues(src.GetType());

            return src.Equals(Arr[Arr.Length - 1]);
        }

        public static int Increment(this int number, int max)
        {
            int incremented = number + 1;
            return incremented > max | max < 0 ? 0 : incremented;
        }

        public static int Decrement(this int number, int max)
        {
            int decremented = number - 1;
            return decremented < 0 | max < 0 ? max : decremented;
        }

        public static bool RoughlyEqual(this float thisFloat, float anotherFloat, int precision = 3)
        {
            int factor = Mathf.RoundToInt(Mathf.Pow(10.0f, precision));
            bool equals = Mathf.RoundToInt(thisFloat * factor) == Mathf.RoundToInt(anotherFloat * factor);

            return equals;
        }

        public static bool RoughlyEqual(this Vector2 vector, Vector2 anotherVector, int precision = 3)
        {
            int factor = Mathf.RoundToInt(Mathf.Pow(10.0f, precision));

            bool x_bool = Mathf.RoundToInt(vector.x * factor) == Mathf.RoundToInt(anotherVector.x * factor);
            bool y_bool = Mathf.RoundToInt(vector.x * factor) == Mathf.RoundToInt(anotherVector.x * factor);

            return x_bool && y_bool;
        }

        public static bool RoughlyEqual(this Vector3 vector, Vector3 anotherVector, int precision = 3)
        {
            int factor = Mathf.RoundToInt(Mathf.Pow(10.0f, precision));

            bool x_bool = Mathf.RoundToInt(vector.x * factor) == Mathf.RoundToInt(anotherVector.x * factor);
            bool y_bool = Mathf.RoundToInt(vector.x * factor) == Mathf.RoundToInt(anotherVector.x * factor);
            bool z_bool = Mathf.RoundToInt(vector.x * factor) == Mathf.RoundToInt(anotherVector.x * factor);

            return x_bool && y_bool && z_bool;
        }

        public static IEnumerator ScaledTimeDelay(float delay, Action callback)
        {
            if (delay != 0.0f)
                yield return new WaitForSeconds(delay);
            else
                yield return null;

            callback.Invoke();
        }

        public static IEnumerator RealtimeDelay(float delay, Action callback)
        {
            if (delay != 0.0f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null;

            callback.Invoke();
        }

        public static IEnumerator SkipFrames(int framesCount, Action callback)
        {
            if (framesCount < 0)
                yield break;

            if (framesCount == 0)
            {
                if (callback != null)
                    callback.Invoke();

                yield break;
            }

            for (int i = 0; i < framesCount; i++)
            {
                yield return null;
            }

            if (callback != null)
                callback.Invoke();
        }

        public static IEnumerator UniversalRealtimeCoroutine(float delay, float duration, Action<float> frameWork, Action finishCallback)
        {
            if (delay != 0.0f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null;

            frameWork.Invoke(0.0f);
            yield return null;

            float startTime = Time.unscaledTime;
            while (Time.unscaledTime - startTime < duration)
            {
                float value = (Time.unscaledTime - startTime) / duration;
                frameWork.Invoke(value);

                yield return null;
            }

            frameWork.Invoke(1.0f);

            if (finishCallback != null)
                finishCallback.Invoke();
        }

        public static IEnumerator UniversalScaledTimeCoroutine(float delay, float duration, Action<float> frameWork, Action finishCallback)
        {
            if (delay != 0.0f)
                yield return new WaitForSeconds(delay);
            else
                yield return null;

            frameWork.Invoke(0.0f);
            yield return null;

            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                float value = (Time.time - startTime) / duration;
                frameWork.Invoke(value);

                yield return null;
            }

            frameWork.Invoke(1.0f);

            if (finishCallback != null)
                finishCallback.Invoke();
        }

        public static Vector3 GetClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
        {
            Vector3 AP = P - A;       //Vector from A to P   
            Vector3 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = Vector3.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;

            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

        public class TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 localScale;
        }

        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        [Serializable]
        public class FloatEvent : UnityEvent<float> { }

        [Serializable]
        public class IntEvent : UnityEvent<int> { }
    }
}