using LevelEditorExports.Simulator;
using LevelToSimulatorConverter.Helper;
using PhysicGlobal;
using System.Drawing;

namespace DynamicObjCreation
{
    //Verändert die Position/Rotation von ein PhysikLevelItemExportData
    public static class LevelItemExportHelper
    {
        public enum PivotOriantation { Center, TopLeft, BottomCenter };

        //Bewegt das Zentrem/LinkeObereRecke/UntenMitte vom LevelItem zum pivotPoint und skaliert dabei das Objekt im size und dreht es um angleInDegree
        public static void MoveToPivotPoint(PhysikLevelItemExportData levelItem, Vec2D pivotPoint, PivotOriantation oriantation, float size, float angleInDegree)
        {
            var box = PhysicSceneExportDataHelper.GetBoundingBoxFromScene(levelItem.PhysicSceneData);

            //An dieser Stelle liegt der Pivot-Punkt vom LevelItem
            var itemPivot = new Vec2D(box.Min.X, box.Min.Y) + GetOriantationPoint(new SizeF(box.GetWidth(), box.GetHeight()), oriantation);

            var m = PhxMatrix.Ident();

            m *= PhxMatrix.Translate(-itemPivot.X, -itemPivot.Y, 0);     //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= PhxMatrix.Scale(size, size, size);                      //Schritt 2: Skaliere
            m *= PhxMatrix.Rotate(angleInDegree, 0, 0, 1);               //Schritt 3: Rotiere um Z
            m *= PhxMatrix.Translate(pivotPoint.X, pivotPoint.Y, 0);     //Schritt 4: Gehe zum Zielpunkt

            Transform(levelItem, m);
        }

        public static void SetVelocityFromAllBodies(PhysikLevelItemExportData levelItem, Vec2D velocity)
        {
            foreach (var body in levelItem.PhysicSceneData.Bodies)
            {
                body.Velocity = new Vec2D(velocity);
            }
        }

        private static Vec2D GetOriantationPoint(SizeF size, PivotOriantation oriantation)
        {
            switch(oriantation)
            {
                case PivotOriantation.Center:
                    return new Vec2D(size.Width / 2, size.Height / 2);

                case PivotOriantation.TopLeft:
                    return new Vec2D(0, 0);

                case PivotOriantation.BottomCenter:
                    return new Vec2D(size.Width / 2, size.Height);
            }

            throw new NotImplementedException();
        }

        private static void Transform(PhysikLevelItemExportData levelItem, PhxMatrix matrix)
        {
            PhysicSceneExportDataHelper.TranslateScene(levelItem.PhysicSceneData, matrix);

            float sizeFactor = PhxMatrix.GetSizeFactorFromMatrix(matrix);
            foreach (var tex in levelItem.TextureData.Textures)
            {
                tex.Width = tex.Width * sizeFactor;
                tex.Height = tex.Height * sizeFactor;
                tex.DeltaX = tex.DeltaX * sizeFactor;
                tex.DeltaY = tex.DeltaY * sizeFactor;
            }

            foreach (var tag in levelItem.TagdataEntries)
            {
                foreach (var point in tag.AnchorPoints)
                {
                    point.X *= sizeFactor; 
                    point.Y *= sizeFactor;
                }
            }
        }
    }
}
