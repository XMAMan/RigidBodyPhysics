using DynamicObjCreation;
using GameHelper.Simulation;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System.Drawing;
using TextureEditorGlobal;

namespace AstroidsControl.Model
{
    class BulletCreator
    {
        private string dataFolder;
        private GameSimulator simulator;

        public BulletCreator(string dataFolder, GameSimulator simulator)
        {
            this.dataFolder = dataFolder;
            this.simulator = simulator;
        }

        public IPublicRigidBody CreateBullet(Vec2D position, Vec2D velocity, Vec2D shipDirection)
        {
            var bullet = CreateBulletData(position, velocity, shipDirection);
            var body = this.simulator.AddRigidBody(bullet);
            return body;
        }

        private BodyWithTexture CreateBulletData(Vec2D position, Vec2D velocity, Vec2D shipDirection)
        {
            float size = 5;
            float vel = 1;

            var body = new RectangleExportData()
            {
                Size = new Vec2D(5 * size, 15 * size),
                Center = position,
                AngleInDegree = Vec2D.Angle360(new Vec2D(0, 1), shipDirection),
                Velocity = velocity + shipDirection * vel,
                AngularVelocity = 0,
                MassData = new MassData(MassData.MassType.Density, 1, 0.001f),
                Friction = 1.5f,
                Restituion = 0.5f,
                CollisionCategory = 1 //0=Ship;1=Bullet;2=Astroid
            };

            var texture = new TextureExportData()
            {
                TextureFile = this.dataFolder + "FireBullet.png",
                MakeFirstPixelTransparent = true,
                ColorFactor = Color.White,
                DeltaX = 0,
                DeltaY = 0,
                Width = (int)body.Size.X,
                Height = (int)body.Size.Y,
                DeltaAngle = 0,
                ZValue = 0
            };

            return new BodyWithTexture(body, texture, 1, new string[] {"Bullet1"});
        }
    }
}
