using Evolia.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Evolia.Model.DataSeries;

namespace Evolia.GameScene
{
    /// <summary>
    /// 履歴グラフ
    /// </summary>
    public class HistoryGraph : MonoBehaviour
    {
        [SerializeField]
        private GraphBackground background;

        [SerializeField]
        private List<LineGraph> graphs = new();

        [SerializeField]
        public HistoryGraphMode mode;

        [SerializeField]
        private float forcedMinValue;

        [SerializeField]
        private float forcedMaxValue;

        [NonSerialized]
        public long xRange;

        private Planet planet => GameController.planet;

        public void Awake()
        {
            switch (mode)
            {
                case HistoryGraphMode.Atmosphere:
                    
                    graphs[0].points =AtmosphereToPoints(planet.history.n2Mass) ;
                    graphs[1].points =AtmosphereToPoints(planet.history.o2Mass) ;
                    graphs[2].points =AtmosphereToPoints(planet.history.co2Mass);
                    graphs[3].points = AtmosphereToPoints(planet.history.h2oMass);
                    break;
                case HistoryGraphMode.Temperature:
                    graphs[0].points = TemperatureToPoints(planet.history.temperature);
                    break;
                case HistoryGraphMode.Metabolism:
                    graphs[0].points = MetabolismToPoints(planet.history.photosynthesisMass);
                    graphs[1].points = MetabolismToPoints(planet.history.respirationMass);
                    break;
                case HistoryGraphMode.Population:
                    graphs[0].points = PopulationToPoints(planet.history.microbePopulation);
                    graphs[1].points = PopulationToPoints(planet.history.plantPopulation);
                    graphs[2].points = PopulationToPoints(planet.history.animalPopulation);
                    break;
                default:
                    throw new NotImplementedException($"HistoryGraphMode {mode} is not implemented");
            }

        }
        private IEnumerable<Vector2> AtmosphereToPoints(DataSeries series)
        {
            foreach (TimedValue v in series.data)
            {
                yield return new Vector2(v.tick, v.value / EarthConstants.AtmosphereMass);
            }
        }

        private IEnumerable<Vector2> TemperatureToPoints(DataSeries series)
        {
            foreach (TimedValue v in series.data)
            {
                yield return new Vector2(v.tick, v.value - PhysicalConstants.CelsiusZero);
            }
        }

        private IEnumerable<Vector2> MetabolismToPoints(DataSeries series)
        {
            foreach (TimedValue v in series.data)
            {
                // 単位は ppm にしたい
                yield return new Vector2(v.tick, v.value * 1000000 / EarthConstants.AtmosphereMass);
            }
        }

        private IEnumerable<Vector2> PopulationToPoints(DataSeries series)
        {
            foreach (TimedValue v in series.data)
            {
                yield return new Vector2(v.tick, v.value);
            }
        }

        public void Start()
        {
            UpdateGraphMax();
        }

        public void Update()
        {
            UpdateGraphMax(true);
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateContent(true);
        }

        private float prevYMax;

        public void UpdateContent(bool smoothing = false)
        {
            UpdateGraphMax(smoothing);
        }

        private void UpdateGraphMax(bool smoothing = false)
        {
            (float xMin, float xMax) = GetXRange();
            (float yMin, float yMax) = GetYRange(xMin, xMax);

            if (smoothing)
            {
                yMax = Mathf.Lerp(prevYMax, yMax, 0.1f);
            }
            prevYMax = yMax;

            foreach (LineGraph graph in graphs)
            {
                graph.xMin = xMin;
                graph.xMax = xMax;
                graph.yMin = yMin;
                graph.yMax = yMax;
                graph.SetVerticesDirty();
            }

            background.SetMax(yMax);
        }

        private (float, float) GetXRange()
        {
            float xMax = float.MinValue;
            float xMin = float.MaxValue;
            if (graphs.Count != 0 && graphs[0].points!=null)
            {
                foreach (Vector2 p in graphs[0].points)
                {
                    xMax = Mathf.Max(xMax, p.x);
                    xMin = Mathf.Min(xMin, p.x);
                }
            }
            if (xRange != 0)
            {
                xMin = xMax - xRange;
            }

            return (xMin, xMax);
        }

        private (float, float) GetYRange(float xMin, float xMax)
        {

            // 全データの最大値を取得
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            foreach (LineGraph graph in graphs)
            {
                if (graph.points != null)
                {
                    foreach (Vector2 timedValue in graph.points)
                    {
                        if (timedValue.x >= xMin && timedValue.x <= xMax)
                        {
                            yMin = Mathf.Min(yMin, timedValue.y);
                            yMax = Mathf.Max(yMax, timedValue.y*1.05f);
                        }
                    }
                }
            }

            if (yMin > forcedMinValue)
            {
                yMin = forcedMinValue;
            }
            if (yMax < forcedMaxValue)
            {
                yMax = forcedMaxValue;
            }


            if (yMin == yMax)
            {
                yMin = 0;
                yMax = 1;
            }

            return (yMin, yMax);
        }

    }
}