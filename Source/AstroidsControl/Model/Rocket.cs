using DynamicObjCreation;
using GameHelper.Simulation;
using GameHelper.Simulation.RigidBodyTagging;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace AstroidsControl.Model
{
    internal class Rocket
    {
        private string dataFolder;
        private GameSimulator simulator;
        private Sounds sounds;
        private FireSprite fireSprite1, fireSprite2;
        private IPublicThruster thruster1, thruster2;
        private IPublicRigidBody rocketPolygon;
        private AnchorPoint rocketTip;
        private BodyIsInsideScreenTester bullets;
        private BulletCreator bulletCreator;

        public Rocket(string dataFolder, GameSimulator simulator, Sounds sounds, IDrawingPanel panel)
        {
            this.dataFolder = dataFolder;
            this.simulator = simulator;
            this.sounds = sounds;
            this.bullets = new BodyIsInsideScreenTester(simulator);
            this.bulletCreator = new BulletCreator(dataFolder, simulator);
            UpdateTagObjects();

            this.fireSprite1.Draw(panel); //Lade die Bilddatei in den Grafikspeicher
        }

        private void UpdateTagObjects()
        {
            this.fireSprite1 = new FireSprite(dataFolder, new AnchorPoint(simulator, "rocket", 0), new AnchorPoint(simulator, "rocket", 1));
            this.fireSprite2 = new FireSprite(dataFolder, new AnchorPoint(simulator, "rocket", 2), new AnchorPoint(simulator, "rocket", 3));
            this.thruster1 = simulator.GetThrusterByTagName("thruster1");
            this.thruster2 = simulator.GetThrusterByTagName("thruster2");
            this.rocketPolygon = simulator.GetBodyByTagName("rocketPolygon");
            this.rocketTip = new AnchorPoint(simulator, "rocket", 4);
            this.thruster1.IsEnabledChanged += Thruster1_IsEnabledChanged;
        }

        private void Thruster1_IsEnabledChanged(bool isEnabled)
        {
            if (isEnabled)
                this.sounds.StartThruster();
            else
                this.sounds.StopThruster();
        }

        public void Draw(IDrawingPanel panel)
        {
            if (this.thruster1.IsEnabled)
                this.fireSprite1.Draw(panel);

            if (this.thruster2.IsEnabled)
                this.fireSprite2.Draw(panel);
        }

        public void HandleTimerTick(float dt)
        {
            this.fireSprite1.HandleTimerTick(dt);
            this.fireSprite2.HandleTimerTick(dt);
            MoveToOtherBorderIfOutside();
            this.bullets.HandleTimerTick(dt);
        }

        private void MoveToOtherBorderIfOutside()
        {
            var box = this.simulator.GetScreenBox(); //Sichtfenster

            if (this.rocketPolygon.Center.X > box.Max.X && this.rocketPolygon.Velocity.X > 0)
            {
                MoveRocket(new Vec2D(box.Min.X, this.rocketPolygon.Center.Y));
            }

            if (this.rocketPolygon.Center.X < box.Min.X && this.rocketPolygon.Velocity.X < 0)
            {
                MoveRocket(new Vec2D(box.Max.X, this.rocketPolygon.Center.Y));
            }

            if (this.rocketPolygon.Center.Y > box.Max.Y && this.rocketPolygon.Velocity.Y > 0)
            {
                MoveRocket(new Vec2D(this.rocketPolygon.Center.X, box.Min.Y));
            }

            if (this.rocketPolygon.Center.Y < box.Min.Y && this.rocketPolygon.Velocity.Y < 0)
            {
                MoveRocket(new Vec2D(this.rocketPolygon.Center.X, box.Max.Y));
            }
        }

        private void MoveRocket(Vec2D destination)
        {
            int levelItemId = this.simulator.GetTagDataFromBody(this.rocketPolygon).LevelItemId;
            var exportData = this.simulator.GetExportDataFromLevelItem(levelItemId); //Kopie nach außen geben

            this.simulator.RemoveLevelItem(levelItemId);
            LevelItemExportHelper.MoveToPivotPoint(exportData, destination, LevelItemExportHelper.PivotOriantation.Center, 1, 0); //Kopie bearbeiten
            this.simulator.AddLevelItem(exportData);
            UpdateTagObjects();
        }

        public void HandleKeyDown(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Space)
            {
                var p1 = this.rocketPolygon.Center;
                var p2 = this.rocketTip.GetPosition();

                var shipDirection = p2 - p1;
                float dirLength = shipDirection.Length();
                shipDirection /= dirLength;

                var bullet = this.bulletCreator.CreateBullet(p2 + shipDirection * dirLength * 0.2f, this.rocketPolygon.Velocity, shipDirection);

                this.bullets.AddBody(bullet);

                this.sounds.PlayShot();
            }
        }

        public void HandleKeyUp(System.Windows.Input.Key key)
        {
            
        }
    }
}
