using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MyUtils
{
    public static Dictionary<string, object> ToDictionary(this object obj)
    {
        var dictionary = new Dictionary<string, object>();

        if (obj == null)
            return dictionary;

        PropertyInfo[] properties = obj.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            dictionary[property.Name] = property.GetValue(obj);
        }

        return dictionary;
    }
    public static bool ContainsObject<T>(this T[] array, T obj)
    {
        foreach (T item in array)
        {
            if (item.Equals(obj)) // Implement Equals() method in your custom object or struct
            {
                return true;
            }
        }
        return false;
    }
    public static void AddArray<T>(ref T[] array, T[] newArray)
    {
        if (newArray == null || newArray.Length == 0)
        {
            Debug.LogWarning("New array is null or empty.");
            return;
        }

        if (array == null || array.Length == 0)
        {
            array = new T[newArray.Length];
            newArray.CopyTo(array, 0);
        }
        else
        {
            int originalLength = array.Length;
            System.Array.Resize(ref array, originalLength + newArray.Length);
            newArray.CopyTo(array, originalLength);
        }
    }
    public static T[] ConcatArrays<T>(params T[][] arrays)
    {
        int totalLength = 0;
        foreach (var array in arrays)
        {
            totalLength += array.Length;
        }

        T[] result = new T[totalLength];
        int offset = 0;
        foreach (var array in arrays)
        {
            array.CopyTo(result, offset);
            offset += array.Length;
        }

        return result;
    }
    
    public static async Task WaitForSomeTime(float duration)
    {
        await Task.Delay(TimeSpan.FromSeconds(duration));
    }
    public static async void Execute(float delay, Action OnCallback)
    {
        await WaitForSomeTime(delay);
#if UNITY_EDITOR
        // Check if Unity is in Play mode
        if (!EditorApplication.isPlaying)
        {
            LogWarning("Game is not running in Play mode. Callback will not be invoked.");
            return;
        }
#endif

        OnCallback?.Invoke();
    }

#if UNITY_EDITOR
    private static readonly string SavePath = Application.dataPath;
#else
    private static readonly string SavePath = Application.persistentDataPath;
#endif
    public static void SaveData(object data, string subPath, string jsonFileName, bool overrideData = false)
    {
        string directoryPath = Path.Combine(SavePath, subPath);

        // Ensure the directory exists, creating it if needed
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, $"{jsonFileName}.json");

        if (!File.Exists(filePath) || overrideData)
        {
            Log("Saving Data.");
            using StreamWriter writer = new(filePath);
            //string dataToWrite = JsonUtility.ToJson(data);
            string dataToWrite = JsonConvert.SerializeObject(data, Formatting.Indented);
            writer.Write(dataToWrite);
        }
    }
    public static T LoadData<T>(string subPath, string jsonFileName)
    {
        string directoryPath = Path.Combine(SavePath, subPath);
        string filePath = Path.Combine(directoryPath, $"{jsonFileName}.json");
        if (File.Exists(filePath))
        {
            using StreamReader reader = new(filePath);
            if (reader == null)
            {
                return default;
            }

            string dataToLoad = reader.ReadToEnd();
            //return JsonUtility.FromJson<T>(dataToLoad);
            return JsonConvert.DeserializeObject<T>(dataToLoad);
        }
        else
        {
            LogWarning("No saved data found.");
            T defaultData = Activator.CreateInstance<T>();
            SaveData(defaultData, subPath, jsonFileName);
            return defaultData;
        }
    }

    public static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        else
        {
            LogWarning("Invalid hex color code: " + hex);
            return Color.white;
        }
    }
    public static GameObject GetFarthestTarget(Vector3 _origin, List<GameObject> targetList)
    {
        GameObject farthestTarget = null;
        float largestDistance = 0f;

        if (targetList.Count != 0)
        {
            foreach (GameObject _enemy in targetList)
            {
                Vector3 targetPos = new Vector3(_enemy.transform.position.x, _origin.y, _enemy.transform.position.z);
                float distance = (targetPos - _origin).sqrMagnitude;

                if (distance > largestDistance)
                {
                    farthestTarget = _enemy;
                    largestDistance = distance;
                }
            }
        }

        return farthestTarget;
    }
    public static T GetNearestTarget<T>(Vector3 origin, T[] targetList, float smallestDistance = float.MaxValue) where T : Component
    {
        T nearestTarget = null;
        if (targetList.Length > 0)
        {
            foreach (T target in targetList)
            {
                Vector3 targetPos = new Vector3(target.transform.position.x, origin.y, target.transform.position.z);
                float distance = (targetPos - origin).sqrMagnitude;
                if (distance < smallestDistance)
                {
                    nearestTarget = target;
                    smallestDistance = distance;
                }
            }
        }

        return nearestTarget;
    }

    public static float GetDistanceXZ(Vector3 vectorA, Vector3 vectorB)
    {
        vectorA.y = vectorB.y = 0f;
        //Log(Vector3.Distance(vectorA, vectorB));
        return Vector3.Distance(vectorA, vectorB);
    }

    public static Quaternion LookTowards(Vector3 _origin, Vector3 _target)
    {
        Vector3 MoveDistance = _target - _origin;
        MoveDistance.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(MoveDistance);
        return targetRotation;
    }

    public static void ValidateInteraction(this Animator anim, string message)
    {
        // less childrens
        TextMeshProUGUI validTxt = anim.gameObject.FindChildByName("validationTxt").GetComponent<TextMeshProUGUI>();
        validTxt.text = message;
        anim.Play("Validate", -1, 0f);
    }

    public static GameObject FindChildByName(this GameObject parent, string name)
    {
        //Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    public static Color SetColor(int red, int green, int blue, int alpha)
    {
        float normalizedRed = red / 255f;
        float normalizedGreen = green / 255f;
        float normalizedBlue = blue / 255f;
        float normalizedAlpha = alpha / 255f;

        return new Color(normalizedRed, normalizedGreen, normalizedBlue, normalizedAlpha);
    }

    public static void LogWarning(string log)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"{log}\n{StackTraceUtility.ExtractStackTrace()}");
#endif
    }
    public static void LogError(string log)
    {
#if UNITY_EDITOR
        Debug.LogError($"{log}\n{StackTraceUtility.ExtractStackTrace()}");
#endif
    }
    public static void Log(object log)
    {
#if UNITY_EDITOR
        Debug.Log($"{log}\n{StackTraceUtility.ExtractStackTrace()}");
#endif
    }

    public static string ConvertTimeToString(float timeInSeconds)
    {
        if (timeInSeconds < 60)
        {
            return string.Format("{0}s", Mathf.RoundToInt(timeInSeconds));
        }
        else if (timeInSeconds < 3600) // less than an hour
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            if (seconds == 0)
            {
                return string.Format("{0}m", minutes);
            }
            else
            {
                return string.Format("{0}m {1}s", minutes, seconds);
            }
        }
        else // an hour or more
        {
            int hours = Mathf.FloorToInt(timeInSeconds / 3600);
            int remainingSeconds = Mathf.FloorToInt(timeInSeconds % 3600);
            int minutes = Mathf.FloorToInt(remainingSeconds / 60);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60);
            if (minutes == 0 && seconds == 0)
            {
                return string.Format("{0}h", hours);
            }
            else if (minutes == 0)
            {
                return string.Format("{0}h {1}s", hours, seconds);
            }
            else if (seconds == 0)
            {
                return string.Format("{0}h {1}m", hours, minutes);
            }
            else
            {
                return string.Format("{0}h {1}m {2}s", hours, minutes, seconds);
            }
        }
    }

    public static string GetNumberExtension(int number)
    {
        if (number == 1)
            return "st";
        else if (number == 2)
            return "nd";
        if (number == 3)
            return "rd";

        return "th";
    }
}
