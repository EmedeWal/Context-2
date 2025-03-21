namespace Context
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System;
    using UnityEngine;

    public static class Layers
    {
        private const string _controller = "Controller";

        public static LayerMask GetControllerLayer() => LayerMask.NameToLayer(_controller);
    }

    public static class Collections
    {
        public static void SortByGameObjectName<T>(T[] array) where T : MonoBehaviour
        {
            Array.Sort(array, (a, b) => GetIndexOnGameObjectName(a.gameObject.name)
                             .CompareTo(GetIndexOnGameObjectName(b.gameObject.name)));
        }

        public static T GetRandomEntry<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogError("Array is null or empty!");
                return default;
            }

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static bool TryGetUniqueEntry<T>(List<T> list, Func<T, string> selector, string searchValue, out T result)
        {
            result = default;

            var matchingItems = list.FindAll(item => selector(item) == searchValue);

            if (matchingItems.Count == 0)
            {
                Debug.LogWarning($"{typeof(T).Name} entry '{searchValue}' not found!");
                return false;
            }
            if (matchingItems.Count > 1)
            {
                Debug.LogWarning($"{typeof(T).Name} list contains duplicate entries for '{searchValue}'!");
                return false;
            }

            result = matchingItems[0];
            return true;
        }

        public static bool TryGetUniqueEntry<T>(T[] array, Func<T, string> selector, string searchValue, out T result)
        {
            result = default;

            var matchingItems = array.Where(item => selector(item) == searchValue).ToArray();

            if (matchingItems.Length == 0)
            {
                Debug.LogError($"{typeof(T).Name} entry '{searchValue}' not found!");
                return false;
            }
            if (matchingItems.Length > 1)
            {
                Debug.LogError($"{typeof(T).Name} array contains duplicate entries for '{searchValue}'!");
                return false;
            }

            result = matchingItems[0];
            return true;
        }

        public static void ShuffleArray<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public static bool AreArraysEqual<T>(T[] array1, T[] array2) where T : IEquatable<T>
        {
            if (array1 == null || array2 == null || array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].Equals(array2[i]))
                    return false;
            }
            return true;
        }

        public static bool AreScriptableObjectArraysEqual<T>(T[] array1, T[] array2) where T : ScriptableObject
        {
            if (array1 == null || array2 == null || array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] == null || array2[i] == null)
                    return false;

                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }

        public static int SafeIncrement(int index, int length) => (index + 1) % length;

        private static int GetIndexOnGameObjectName(string name)
        {
            Match match = Regex.Match(name, @"\((\d+)\)"); // Find number in parentheses

            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
                return index;

            Debug.LogError($"Object '{name}' does not have a valid index format!");
            return -1;
        }
    }
}