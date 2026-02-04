using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class JsonUtilityHelper
    {
        public static List<T> FromJsonList<T>(string json)
        {
            string wrapped = "{ \"items\": " + json + "}";
            return JsonUtility.FromJson<Wrapper<T>>(wrapped).items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
    }
}
