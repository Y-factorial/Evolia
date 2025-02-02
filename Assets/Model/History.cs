using System;
using UnityEngine;

namespace Evolia.Model
{

    [Serializable]
    public class History
    {
        [SerializeField]
        public DataSeries n2Mass = new DataSeries();
        [SerializeField]
        public DataSeries o2Mass = new DataSeries();
        [SerializeField]
        public DataSeries co2Mass = new DataSeries();
        [SerializeField]
        public DataSeries h2oMass = new DataSeries();

        [SerializeField]
        public DataSeries temperature = new DataSeries();

        [SerializeField]
        public DataSeries photosynthesisMass = new DataSeries();

        [SerializeField]
        public DataSeries respirationMass = new DataSeries();

        [SerializeField]
        public DataSeries microbePopulation = new DataSeries();
        [SerializeField]
        public DataSeries plantPopulation = new DataSeries();
        [SerializeField]
        public DataSeries animalPopulation = new DataSeries();
    }
}