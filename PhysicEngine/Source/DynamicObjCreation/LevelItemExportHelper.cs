using GraphicMinimal;
using LevelEditorGlobal;
using LevelToSimulatorConverter;
using RigidBodyPhysics.MathHelper;
using System.Drawing;

namespace DynamicObjCreation
{
    //Verändert die Position/Rotation von ein PhysikLevelItemExportData
    public static class LevelItemExportHelper
    {
        public enum PivotOriantation { Center, TopLeft, BottomCenter };

        //Bewegt das Zentrem/LinkeObereRecke/UntenMitte vom LevelItem zum pivotPoint und skaliert dabei das Objekt im size und dreht es um angleInDegree
        public static void MoveToPivotPoint(PhysikLevelItemExportData levelItem, Vector2D pivotPoint, PivotOriantation oriantation, float size, float angleInDegree)
        {
            var box = PhysicSceneExportDataHelper.GetBoundingBoxFromScene(levelItem.PhysicSceneData);

            //An dieser Stelle liegt der Pivot-Punkt vom LevelItem
            var itemPivot = new Vector2D(box.Min.X, box.Min.Y) + GetOriantationPoint(new SizeF(box.Width, box.Height), oriantation);

            var m = Matrix4x4.Ident();

            m *= Matrix4x4.Translate(-itemPivot.X, -itemPivot.Y, 0);     //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= Matrix4x4.Scale(size, size, size);                      //Schritt 2: Skaliere
            m *= Matrix4x4.Rotate(angleInDegree, 0, 0, 1);               //Schritt 3: Rotiere um Z
            m *= Matrix4x4.Translate(pivotPoint.X, pivotPoint.Y, 0);     //Schritt 4: Gehe zum Zielpunkt

            Transform(levelItem, m);
        }

        public static void SetVelocityFromAllBodies(PhysikLevelItemExportData levelItem, Vec2D velocity)
        {
            foreach (var body in levelItem.PhysicSceneData.Bodies)
            {
                body.Velocity = new Vec2D(velocity);
            }
        }

        private static Vector2D GetOriantationPoint(SizeF size, PivotOriantation oriantation)
        {
            switch(oriantation)
            {
                case PivotOriantation.Center:
                    return new Vector2D(size.Width / 2, size.Height / 2);

                case PivotOriantation.TopLeft:
                    return new Vector2D(0, 0);

                case PivotOriantation.BottomCenter:
                    return new Vector2D(size.Width / 2, size.Height);
            }

            throw new NotImplementedException();
        }

        private static void Transform(PhysikLevelItemExportData levelItem, Matrix4x4 matrix)
        {
            PhysicSceneExportDataHelper.TranslateScene(levelItem.PhysicSceneData, matrix);

            float sizeFactor = Matrix4x4.GetSizeFactorFromMatrix(matrix);
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
