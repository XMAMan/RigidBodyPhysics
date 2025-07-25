using RigidBodyPhysics;
using TextureEditorGlobal;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.Joints;
using PhysicGlobal;

namespace PhysicSceneDrawing
{
    public class PhysicSceneDrawer
    {
        private List<ITexturedRigidBody> textures;
        private List<IPublicDistanceJoint> distanceJoints;

        public PhysicSceneDrawer(PhysicScene physicScene, VisualisizerOutputData textureData)
        {
            textures = ConvertPhysicScene(physicScene, textureData).ToList();
            distanceJoints = physicScene.GetAllJoints().Where(x => x is IPublicDistanceJoint).Cast<IPublicDistanceJoint>().ToList();

            physicScene.BodyWasDeletedHandler += RigidBodyWasDeleted;
            physicScene.JointWasDeletedHandler += JointWasDeleted;
        }

        private void RigidBodyWasDeleted(PhysicScene physicScene, IPublicRigidBody body)
        {
            var deleteList = textures.Where(x => x.AssociatedBody == body).ToList();
            foreach (var del in deleteList)
            {
                textures.Remove(del);
            }
        }

        private void JointWasDeleted(PhysicScene physicScene, IPublicJoint joint)
        {
            if (joint is IPublicDistanceJoint)
                distanceJoints.Remove((IPublicDistanceJoint)joint);
        }

        public PhysicGlobal.BoundingBox GetPhysicBoundingBoxFromScene()
        {
            return PhysicGlobal.BoundingBox.GetBoxFromBoxes(textures.Select(x => x.PhysicBoundingBox));
        }

        public PhysicGlobal.BoundingBox GetTextureBoundingBoxFromScene()
        {
            return PhysicGlobal.BoundingBox.GetBoxFromBoxes(textures.Select(x => x.TextureBoundingBox));
        }

        public PhysicGlobal.BoundingBox GetPhysicBoundingBoxFromBodies(IEnumerable<IPublicRigidBody> bodies)
        {
            return PhysicGlobal.BoundingBox.GetBoxFromBoxes(bodies.Select(body => textures.First(tex => tex.AssociatedBody == body).PhysicBoundingBox));
        }

        public PhysicGlobal.BoundingBox GetTextureBoundingBoxFromBodies(IEnumerable<IPublicRigidBody> bodies)
        {
            return PhysicGlobal.BoundingBox.GetBoxFromBoxes(bodies.Select(body => textures.First(tex => tex.AssociatedBody == body).TextureBoundingBox));
        }

        private static ITexturedRigidBody[] ConvertPhysicScene(PhysicScene physicScene, VisualisizerOutputData textureData)
        {
            var bodys = physicScene.GetAllBodys().ToList();
            if (bodys.Count != textureData.Textures.Length) throw new ArgumentException("Body.Count != Textures.Count");

            List<ITexturedRigidBody> textures = new List<ITexturedRigidBody>();
            for (int i = 0; i < bodys.Count; i++)
            {
                textures.Add(Convert(bodys[i], textureData.Textures[i]));
            }

            return textures.ToArray();
        }

        private static ITexturedRigidBody Convert(IPublicRigidBody body, TextureExportData textureData)
        {
            if (body is IPublicRigidRectangle)
                return new TexturedRectangle((IPublicRigidRectangle)body, textureData);

            if (body is IPublicRigidPolygon)
                return new TexturedPolygon((IPublicRigidPolygon)body, textureData);

            if (body is IPublicRigidCircle)
                return new TexturedCircle((IPublicRigidCircle)body, textureData);

            throw new ArgumentException("Unknown type " + body.GetType());
        }

        public void Draw(IDrawingPanel panel)
        {
            panel.EnableDepthTesting();
            foreach (var tex in textures)
            {
                if (tex.IsInvisible) continue;
                panel.ZValue2D = tex.ZValue;
                tex.Draw(panel);
            }
        }

        public void DrawDistanceJoints(IDrawingPanel panel)
        {
            DrawDistanceJoints(panel, new Pen(Color.Blue, 2));
        }

        public void DrawDistanceJoints(IDrawingPanel panel, Pen borderPen)
        {
            foreach (var disJoint in distanceJoints)
            {
                panel.DrawLine(borderPen, disJoint.Anchor1, disJoint.Anchor2);
            }
        }

        public void DrawPhysicBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.EnableDepthTesting();
            foreach (var tex in textures)
            {
                panel.ZValue2D = tex.ZValue;
                tex.DrawPhysicBorder(panel, borderPen);
            }            
        }
        public void DrawTextureBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.EnableDepthTesting();
            foreach (var tex in textures)
            {
                panel.ZValue2D = tex.ZValue;
                tex.DrawTextureBorder(panel, borderPen);
            }
        }

        public void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor)
        {
            panel.EnableDepthTesting();
            foreach (var tex in textures)
            {
                if (tex.IsInvisible) continue;

                panel.ZValue2D = tex.ZValue;
                tex.DrawWithTwoColors(panel, frontColor, backColor);
            }
        }

        //Überschreibt für den Körper "body" die Draw-Methode
        public void UseCustomDrawingForRigidBody(IPublicRigidBody body, IRigidBodyDrawer bodyDrawer)
        {
            var tex = this.textures.First(x => x.AssociatedBody == body);
            this.textures.Remove(tex);
            this.textures.Add(new TexturedRigidBodyWithCustomDrawing(tex, bodyDrawer));
        }

        public void AddBody(IPublicRigidBody body, TextureExportData texturData)
        {
            this.textures.Add(Convert(body, texturData));
        }

        //Return: Diese Texturdaten wurden entfertn
        public TextureExportData RemoveBody(IPublicRigidBody body)
        {
            var tex = this.textures.First(x => x.AssociatedBody == body);
            this.textures.Remove(tex);
            return tex.TextureExportData;
        }

        public void AddDistanceJoint(IPublicDistanceJoint joint)
        {
            this.distanceJoints.Add(joint);
        }

        //Wird für die Voronoi-Zerlegung benötigt
        public TextureExportData GetTextureDataFromBody(IPublicRigidBody body)
        {
            var tex = this.textures.FirstOrDefault(x => x.AssociatedBody == body);
            if (tex == null)
            {
                throw new Exception("No TextureData available");
            }
            return tex?.TextureExportData;
        }
    }
}
