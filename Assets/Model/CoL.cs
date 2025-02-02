using Evolia.Model;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{
    [CreateAssetMenu(fileName = "CoL", menuName = "Evolia/CoL")]
    public class CoL : ScriptableObject
    {
        public const int MAX_SPECIES = 256;

        private static CoL _instance;

        public static CoL instance {
            get 
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CoL>("CoL");
                }

                return _instance;
            }
        }

        [SerializeField]
        public List<string> names = new();

        [SerializeField]
        public Species[] species = new Species[0];

        public ref Species this[int index] => ref species[index];


        public void Clear()
        {
            names.Clear();
            species = new Species[0];
        }

        public void Add(string name)
        {
            names.Add(name);

            Species newSpecies = new Species();
            newSpecies.id = species.Length;

            Species[] newCol = new Species[species.Length + 1];
            Array.Copy(species, newCol, species.Length);
            newCol[species.Length] = newSpecies;
            species = newCol;
        }

        public ref Species this[string name] => ref species[names.IndexOf(name)];
    }


}