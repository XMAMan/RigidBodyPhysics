using GraphicMinimal;
using GraphicPanels;
using System;
using System.Collections.Generic;
using System.Linq;
using GameHelper.Simulation.RigidBodyTagging;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace MoonlanderControl.Model
{
    //Diese Klasse sagt welchen Abstand das Raumschiff zum Boden hat und wo es Punkte auf dem Mond gibt, wo es landen kann
    class Ground
    {
        private const int GridSize = 50;
        private float maxX;
        private float[] height; //0 = Höhe an Stelle X=0; 1 = Höhe an Stelle X=50 (Alle 50 Punkte ein Höhenpunkt)

        public LandingArea LandingArea { get; }

        public Ground(ITagDataProvider tagProvider, Random rand)
        {
            this.height = CreateHeightArray(tagProvider);
            this.maxX = (this.height.Length - 1) * GridSize;

            var landings = GetLandingAreas();            
            this.LandingArea = landings[rand.Next(landings.Length)];
        }

        private float[] CreateHeightArray(ITagDataProvider tagProvider)
        {
            //Schritt 1: Prüfe dass alle Polygon-Punkte auf ein 50*50-Gitter liegen
            var poly = (IPublicRigidPolygon)tagProvider.GetBodyByTagName("ground");

            var yOrdered = poly.Vertex.OrderBy(x => x.Y).Reverse().ToList();
            if (yOrdered[0].Y != yOrdered[1].Y)
                throw new Exception("Ground-Line must be horizontal");

            var xOrdered = poly.Vertex.OrderBy(x => x.X).ToList();
            xOrdered.Remove(yOrdered[0]);
            xOrdered.Remove(yOrdered[1]);

            //Prüfe das alle Punkte auf den Gitterpunkten liegen und das keine zwei Punkte übereinander liegen
            float lastX = -1;
            foreach (var p in xOrdered)
            {
                int xi = (int)(p.X / GridSize);
                float xf = xi * GridSize;
                if (p.X != xf)
                    throw new Exception($"Each X-Component from each Point must be divideable with {GridSize}");

                if (p.X == lastX)
                    throw new Exception("Don't place two points above!");

                lastX = p.X;
            }

            if (xOrdered[0].X != 0)
                throw new Exception("The first point must start with X=0");

            int lastXi = (int)(xOrdered.Last().X / GridSize);
            float[] height = new float[lastXi + 1];
            height[0] = xOrdered[0].Y;

            for (int i = 1; i < xOrdered.Count; i++)
            {
                int x1i = (int)(xOrdered[i - 1].X / GridSize);
                int x2i = (int)(xOrdered[i].X / GridSize);

                height[x2i] = xOrdered[i].Y;

                //Interpoliere
                if (x2i - x1i > 1)
                {
                    float y1 = height[x1i];
                    float y2 = height[x2i];

                    int xDiff = x2i - x1i;

                    for (int j = x1i + 1; j < x2i; j++)
                    {
                        int xj = j - x1i;
                        double f = xj / (double)xDiff;
                        height[j] = (float)((1 - f) * y1 + f * y2);
                    }
                }
                else if (x2i - x1i < 1)
                {
                    throw new Exception("Don't place two points above!");
                }
            }

            return height;            
        }
        public float GetDistanceToGround(Vector2D point)
        {
            if (point.X < 0 ||point.X > this.maxX)
                return float.PositiveInfinity;

            float fDiv = point.X / GridSize;
            int i1 = (int)fDiv;
            if (i1 > this.height.Length - 2)
                i1 = this.height.Length - 2;

            float f1 = fDiv - i1;

            float height = (1-f1) * this.height[i1] + f1 * this.height[i1 +1];

            return height - point.Y;
        }

        public float GetDistanceFromShipToGround(Ship ship)
        {
            float d1 = GetDistanceToGround(ship.GetLeg1());
            float d2 = GetDistanceToGround(ship.GetLeg2());

            return Math.Min(d1, d2) / 10; //Umrechnung von Pixeln in Meter
        }

        //Gebe alle Bereiche des Bodens zurück, die horizontal sind
        private LandingArea[] GetLandingAreas()
        {
            List<LandingArea> areas = new List<LandingArea>();
            float last = height[0];
            int start = 0;
            for (int i=0; i<height.Length; i++)
            {
                float h = height[i];
                if (h != last)
                {
                    int end = i - 1;
                    if (end > start)
                    {
                        areas.Add(new LandingArea(new Vector2D(start * GridSize, height[start]), new Vector2D(end * GridSize, height[end])));
                    }

                    last = h;
                    start = i;
                }
            }

            return areas.Where(x => x.Length > 50).ToArray();
        }

        public void MoveOnStep(float dt)
        {
            this.LandingArea.MoveOnStep(dt);
        }

        public void Draw(GraphicPanel2D panel)
        {
            this.LandingArea.Draw(panel);
        }
    }

    
}
