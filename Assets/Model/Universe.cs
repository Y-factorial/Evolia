
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Evolia.Model
{
    [Serializable]
    public class Universe
    {
        public static string SaveFile => $"{Application.persistentDataPath}/universe.json";

        [Serializable]
        public class PlanetInfo
        {
            public int id;

            public string name;

            public long age;

            public float x;

            public float y;

            public int iconType;

            public long createdAt;

            public long lastPlayedAt;

            public bool IsSaved => File.Exists(Planet.SaveFile(id));

            public void Delete()
            {
                if (File.Exists(Planet.SaveFile(id)))
                {
                    File.Delete(Planet.SaveFile(id));
                }
                if (File.Exists(Planet.ThumbnailFile(id)))
                {
                    File.Delete(Planet.ThumbnailFile(id));
                }
            }

        }

        [SerializeField]
        public List<PlanetInfo> planets = new();

        [SerializeField]
        public int lastPlayedPlanetId = -1;

        public static Universe Load()
        {
            Debug.Log($"[Universe] Load {SaveFile}");

            // Does it exist?
            if (File.Exists(SaveFile))
            {
                string fileContents = File.ReadAllText(SaveFile);
                return JsonUtility.FromJson<Universe>(fileContents);
            }
            else
            {
                return new();
            }
        }

        public void Save()
        {
            Debug.Log($"[Universe] Save {SaveFile}");

            string fileContents = JsonUtility.ToJson(this);
            File.WriteAllText(SaveFile, fileContents);
        }

        public PlanetInfo NewPlanet(float x, float y)
        {
            PlanetInfo planet = new();
            planet.id = planets.Count == 0 ? 0 : planets.Max(p => p.id) + 1;
            planet.x = x;
            planet.y = y; 
            planet.iconType = planet.id % 20;

            planet.createdAt = DateTime.Now.Ticks;
            planet.lastPlayedAt = long.MinValue;

            return planet;
        }

        public PlanetInfo GetPlanet(int id)
        {
            return planets.Where(p => p.id == id).FirstOrDefault();
        }
    }
}