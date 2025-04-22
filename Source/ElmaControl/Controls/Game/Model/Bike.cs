using GameHelper;
using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using SpriteEditorControl.Controls.Main.Model;
using PhysicSceneDrawing;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ElmaControl.Controls.Game.Model
{
    internal class Bike : ITimerHandler, IRigidBodyDrawer, IDrawable
    {
        public const int ScaleFactor = 2; //Um diesen Faktor wird die Sprite-Datei verkleinert, um das Einladen zu beschleunigen
        enum DirectionState { Right, Left }
        enum SpinState { NoSpin, SpinLeft, SpinRight }      //Zustand der Y-Drehung
        enum ArmState { NoArmMovement, ArmsUp, ArmsDown }   //Zustand, ob sich die Arme gerade bewegen
        enum KeyDownState { NoKeyDown, LeftIsDown, RightIsDown }   //Wenn die Left-Taste gedrückt ist, dann ist der KeyLeftRightState=LeftIsDown; Ist die Right-Taste gedrückt ist der KeyLeftRightState=RightIsDown

        private GameSimulator simulator;
        private Sounds sounds;
        private string dataFolder;
        private DirectionState direction = DirectionState.Right;
        private SpinState spin = SpinState.NoSpin;
        private ArmState arm = ArmState.NoArmMovement;      //Steuert die Anzeige der Sprite-Animation
        private KeyDownState keyDownState = KeyDownState.NoKeyDown;
        private float yAngleInDegree = 0; //0 = Direction = Right; 180 = Direction = Left

        private float spinImpulse = 0.00017f; //So stark wird das Motorrad um die Z-Achse gedreht, wenn der Fahrer mit den Armen wedelt
        private float spinSpeed = 0.5f; //So viele Grad pro Millisekunde dreht sich das Bild um die Y-Achse

        private IPublicRigidCircle head, wheel1, wheel2;
        private IPublicRotaryMotor motor1, motor2;

        private SpriteImage armsUpSprite, armsDownSprite;

        private bool bikeIsRemovedFromSimulation = false;

        public Vector2D HeadCenter { get => this.head.Center.ToGrx(); }
        public float HeadRadius { get => this.head.Radius; }
        public Vector2D Wheel1Center { get => this.wheel1.Center.ToGrx(); }
        public float Wheel1Radius { get => this.wheel1.Radius; }
        public Vector2D Wheel2Center { get => this.wheel2.Center.ToGrx(); }
        public float Wheel2Radius { get => this.wheel2.Radius; }

        public event Action HeadIsTouchingTheGround;

        public Bike(GameSimulator simulator, Sounds sounds, string dataFolder, GraphicPanel2D panel)
        {
            this.dataFolder = dataFolder;
            this.simulator = simulator;
            this.sounds = sounds;
            this.head = (IPublicRigidCircle)simulator.GetBodyByTagName("head");
            this.wheel1 = (IPublicRigidCircle)simulator.GetBodyByTagName("wheel1");
            this.wheel2 = (IPublicRigidCircle)simulator.GetBodyByTagName("wheel2");

            this.motor1 = simulator.GetMotorByTagName("motor1");
            this.motor2 = simulator.GetMotorByTagName("motor2");

            var spriteData = JsonHelper.Helper.CreateFromJson<SpriteEditorExportData>(File.ReadAllText(dataFolder + "Motorbike_Sprite.txt"));
            this.armsUpSprite = new SpriteImage(dataFolder + "Sprite1.png", spriteData.SpriteData.SpriteCount, 1, spriteData.SpriteData.SpriteCount, spriteData.PhysicItemData.AnimationData[0].AnimationData.DurrationInSeconds, true, spriteData.SpriteData.PivotX / ScaleFactor, spriteData.SpriteData.PivotY / ScaleFactor);
            this.armsDownSprite = new SpriteImage(dataFolder + "Sprite2.png", spriteData.SpriteData.SpriteCount, 1, spriteData.SpriteData.SpriteCount,  spriteData.PhysicItemData.AnimationData[1].AnimationData.DurrationInSeconds, true, spriteData.SpriteData.PivotX / ScaleFactor, spriteData.SpriteData.PivotY / ScaleFactor - 63 / ScaleFactor); //Die Bilddatei von der zweiten Animation ist 63 Pixel weniger hoch
  
            this.armsUpSprite.Zoom = this.armsDownSprite.Zoom = ScaleFactor;

            this.armsUpSprite.FinishIsReached += ArmAnimationIsFinished;
            this.armsDownSprite.FinishIsReached += ArmAnimationIsFinished;

            //Übertrage mit diesen Befehl die Bilddaten in den Grafikkartenspeicher
            //Verhindere so kurze Wartezeit bei ertmaliger Nutzung der jeweiligen Sprite
            this.armsUpSprite.Draw(panel, new Vector2D(0, 0));
            this.armsDownSprite.Draw(panel, new Vector2D(0, 0));


            simulator.UseCustomDrawingForRigidBody(this.head, this);
            simulator.RemoveBodyFromPhysicSceneDrawer(this.wheel1);
            simulator.RemoveBodyFromPhysicSceneDrawer(this.wheel2);

            simulator.CollisonOccured += Simulator_CollisonOccured;
        }


        public void RemoveBikeFromSimulation()
        {
            this.simulator.RemoveRigidBody(this.head);
            this.simulator.RemoveRigidBody(this.wheel1);
            this.simulator.RemoveRigidBody(this.wheel2);
            this.bikeIsRemovedFromSimulation = true;
        }

        private void Simulator_CollisonOccured(RigidBodyPhysics.PhysicScene sender, RigidBodyPhysics.CollisionDetection.PublicRigidBodyCollision[] collisions)
        {
            foreach (var collision in collisions)
            {
                byte color1 = this.simulator.GetTagDataFromBody(collision.Body1).Color;
                byte color2 = this.simulator.GetTagDataFromBody(collision.Body2).Color;

                //0 = Ground
                //1 = Wheels
                //2 = Head
                //3 = Apple
                //4 = Flower

                //Head is touching the ground
                if (color1 == 2 && color2 == 0)
                {
                    HeadIsTouchingTheGround?.Invoke();
                }
            }
        }

        private void ArmAnimationIsFinished()
        {
            this.arm = ArmState.NoArmMovement;
        }

        public float GetMotorSpeed()
        {
            if (this.motor1.IsEnabled)
            {
                return this.wheel1.AngularVelocity;
            }

            if (this.motor2.IsEnabled)
            {
                return this.wheel2.AngularVelocity;
            }

            return 0;
        }

        //Tonhöhe des Motors
        private float GetMotorFrequency()
        {
            float minFreq = 50;
            float maxFreq = 200;
            float maxSpeed = this.motor1.MaxSpeed * 3;

            float speed = Math.Abs(GetMotorSpeed());
            if (speed < maxSpeed)
            {
                float f = speed / maxSpeed;
                return (1 - f) * minFreq + f * maxFreq;
            }
            else
            {
                return maxFreq;
            }
        }

        public void TurnMotorOn()
        {
            if (this.bikeIsRemovedFromSimulation) return;

            if (this.direction == DirectionState.Right)
            {
                this.motor1.IsEnabled = true;
                this.motor2.IsEnabled = false;
            }
            else
            {
                this.motor1.IsEnabled = false;
                this.motor2.IsEnabled = true;
            }

            this.sounds.StartMotor();
        }

        public void TurnMotorOff()
        {
            this.motor1.IsEnabled = false;
            this.motor2.IsEnabled = false;

            this.sounds.StopMotor();
        }

        public void ActivateBrake()
        {
            this.motor1.BrakeIsEnabled = true;
            this.motor2.BrakeIsEnabled = true;
        }

        public void ReleaseBrake()
        {
            this.motor1.BrakeIsEnabled = false;
            this.motor2.BrakeIsEnabled = false;
        }

        public void HandleLeftIsDown()
        {
            if (this.bikeIsRemovedFromSimulation) return;

            this.keyDownState = KeyDownState.LeftIsDown;
        }
        public void HandleLeftIsUp()
        {
            this.keyDownState = KeyDownState.NoKeyDown;
        }
        public void HandleRightIsDown()
        {
            if (this.bikeIsRemovedFromSimulation) return;

            this.keyDownState = KeyDownState.RightIsDown;
        }
        public void HandleRightIsUp()
        {
            this.keyDownState = KeyDownState.NoKeyDown;
        }

        private void RotateLeft()
        {
            if (this.arm == ArmState.NoArmMovement)
            {
                if (this.direction == DirectionState.Right)
                {
                    MoveArmsUp();
                }
                else
                {
                    MoveArmsDown();
                }
                var upDir = (this.wheel1.Center - this.wheel2.Center).Spin90();
                this.wheel1.Force = -upDir * spinImpulse;
                this.wheel2.Force = upDir * spinImpulse;
            }
        }

        private void RotateRight()
        {
            if (this.arm == ArmState.NoArmMovement)
            {
                if (this.direction == DirectionState.Right)
                {
                    MoveArmsDown();
                }
                else
                {
                    MoveArmsUp();
                }

                var upDir = (this.wheel1.Center - this.wheel2.Center).Spin90();
                this.wheel1.Force = upDir * spinImpulse;
                this.wheel2.Force = -upDir * spinImpulse;
            }
        }

        private void MoveArmsUp()
        {
            this.armsUpSprite.Reset();
            this.arm = ArmState.ArmsUp;
            this.sounds.PlayRotateArms();
        }

        private void MoveArmsDown()
        {
            this.armsDownSprite.Reset();
            this.arm = ArmState.ArmsDown;
            this.sounds.PlayRotateArms();
        }

        public void SpinDirection()
        {
            if (this.bikeIsRemovedFromSimulation) return;

            if (this.direction == DirectionState.Right)
                this.direction = DirectionState.Left;
            else
                this.direction = DirectionState.Right;

            if (this.direction == DirectionState.Right && this.yAngleInDegree > 0)
            {
                this.spin = SpinState.SpinLeft;
            }

            if (this.direction == DirectionState.Left && this.yAngleInDegree < 180)
            {
                this.spin = SpinState.SpinRight;
            }

            if (this.yAngleInDegree == 0 || this.yAngleInDegree == 180)
            {
                this.sounds.PlaySpinDirection();
            }
        }

        public Rectangle GetBoundingBox()
        {
            List<Vector2D> points = new List<Vector2D>();

            var c1 = this.head.Center;
            float r1 = this.head.Radius;
            points.Add(new Vector2D(c1.X - r1, c1.Y - r1));
            points.Add(new Vector2D(c1.X + r1, c1.Y + r1));

            var c2 = this.wheel1.Center;
            float r2 = this.wheel1.Radius;
            points.Add(new Vector2D(c2.X - r2, c2.Y - r2));
            points.Add(new Vector2D(c2.X + r2, c2.Y + r2));

            var c3 = this.wheel2.Center;
            float r3 = this.wheel2.Radius;
            points.Add(new Vector2D(c3.X - r3, c3.Y - r3));
            points.Add(new Vector2D(c3.X + r3, c3.Y + r3));

            float minX = points.Min(x => x.X);
            float minY = points.Min(y => y.Y);
            float maxX = points.Max(x => x.X);
            float maxY = points.Max(y => y.Y);
            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        public void Draw(GraphicPanel2D panel)
        {
            if (this.bikeIsRemovedFromSimulation) return;

            var pivot = this.head.Center.ToGrx();
            var w1 = this.wheel1.Center.ToGrx();
            var w2 = this.wheel2.Center.ToGrx();
            float angleInDegree = Vector2D.Angle360(new Vector2D(1, 0), w2 - w1);

            this.armsUpSprite.RotateZAngleInDegree = this.armsDownSprite.RotateZAngleInDegree = angleInDegree;
            this.armsUpSprite.RotateYAngleInDegree = this.armsDownSprite.RotateYAngleInDegree = this.yAngleInDegree;

            panel.DisableDepthTesting();
            panel.ZValue2D = 0;
            DrawSticks(panel, pivot, w1, w2);        //Zeichne die Linien von den Rädern zum Gehäuse
            DrawSprite(panel, pivot);                //Zeichne die Sprite-Datei
            DrawWheels(panel, w1, w2);               //Zeichne die Räder

            panel.EnableDepthTesting();
        }

        private SpriteImage GetActiveSprite()
        {
            SpriteImage sprite = null;

            switch (this.arm)
            {
                case ArmState.NoArmMovement:
                    sprite = this.armsUpSprite;
                    break;

                case ArmState.ArmsUp:
                    sprite = this.armsUpSprite;
                    break;

                case ArmState.ArmsDown:
                    sprite = this.armsDownSprite;
                    break;
            }

            return sprite;
        }

        private void DrawSprite(GraphicPanel2D panel, Vector2D pivot)
        {
            //Ermittle, welche Sprite-Datei gezeichnet werden soll
            var sprite = GetActiveSprite();

            if (this.arm == ArmState.NoArmMovement)
            {
                //Zeichne nur Frame 0 von der aktiven Sprite
                sprite.DrawSingleImage(panel, pivot, 0);
            }
            else
            {
                //Zeichne die Armbewegung (hoch oder runter)
                sprite.Draw(panel, pivot);
            }
        }

        private void DrawSticks(GraphicPanel2D panel, Vector2D pivot, Vector2D w1, Vector2D w2)
        {
            var localToWorld = armsUpSprite.GetLocalToWorldMatrix(pivot);

            var a1 = Matrix4x4.MultPosition(localToWorld, new Vector3D(235, 400, 0) / ScaleFactor).XY; //An diesen Punkt ist das Hinterrad befestigt, wenn Direction==Right
            var a2 = Matrix4x4.MultPosition(localToWorld, new Vector3D(330, 295, 0) / ScaleFactor).XY; //An diesen Punkt ist das Vorderrad befestigt, wenn Direction==Right

            Vector2D b1, b2;
            if (this.direction == DirectionState.Right)
            {
                b1 = w1;
                b2 = w2;
            }
            else
            {
                b1 = w2;
                b2 = w1;
            }

            if (this.spin != SpinState.NoSpin)
            {
                b1 = Matrix4x4.MultPosition(localToWorld, new Vector3D(-93, 442, 0) / ScaleFactor).XY; //Diese ist die Hinterradposition wenn Direction==Right
                b2 = Matrix4x4.MultPosition(localToWorld, new Vector3D(417, 442, 0) / ScaleFactor).XY;  //Diese ist die Vorderradposition wenn Direction==Right
            }

            panel.DrawLineWithTexture(dataFolder + "GrayColors.png", a1, b1, 15);
            panel.DrawLineWithTexture(dataFolder + "GrayColors.png", a2, b2, 15);
        }

        private void DrawWheels(GraphicPanel2D panel, Vector2D w1, Vector2D w2)
        {
            float size = Matrix4x4.GetSizeFactorFromMatrix(panel.GetTransformationMatrix());

            var wheelDir1 = Vector2D.DirectionFromPhi(this.wheel1.Angle);
            float r1 = this.wheel1.Radius;
            panel.DrawLine(new Pen(Color.Gray, 10 * size), w1 + wheelDir1 * r1, w1 - wheelDir1 * r1);
            wheelDir1 = wheelDir1.Spin90();
            panel.DrawLine(new Pen(Color.Gray, 10 * size), w1 + wheelDir1 * r1, w1 - wheelDir1 * r1);

            panel.DrawCircle(new Pen(Color.Black, 15 * size), w1, this.wheel1.Radius);

            var wheelDir2 = Vector2D.DirectionFromPhi(this.wheel2.Angle);
            float r2 = this.wheel2.Radius;
            panel.DrawLine(new Pen(Color.Gray, 10 * size), w2 + wheelDir2 * r2, w2 - wheelDir2 * r2);
            wheelDir2 = wheelDir2.Spin90();
            panel.DrawLine(new Pen(Color.Gray, 10 * size), w2 + wheelDir2 * r2, w2 - wheelDir2 * r2);

            panel.DrawCircle(new Pen(Color.Black, 15 * size), w2, this.wheel2.Radius);
        }

        public void HandleTimerTick(float dt)
        {
            if (this.bikeIsRemovedFromSimulation) return;

            if (this.spin == SpinState.SpinLeft)
            {
                this.yAngleInDegree -= this.spinSpeed * dt;
                if (this.yAngleInDegree < 0)
                {
                    this.yAngleInDegree = 0;
                    this.spin = SpinState.NoSpin;
                }
            }

            if (this.spin == SpinState.SpinRight)
            {
                this.yAngleInDegree += this.spinSpeed * dt;
                if (this.yAngleInDegree > 180)
                {
                    this.yAngleInDegree = 180;
                    this.spin = SpinState.NoSpin;
                }
            }

            if (this.keyDownState == KeyDownState.LeftIsDown)
            {
                RotateLeft();
            }
            else if (this.keyDownState == KeyDownState.RightIsDown)
            {
                RotateRight();
            }


            if (this.arm == ArmState.ArmsUp)
            {
                this.armsUpSprite.HandleTimerTick(dt);
            }

            if (this.arm == ArmState.ArmsDown)
            {
                this.armsDownSprite.HandleTimerTick(dt);
            }

            this.sounds.MotorFrequency = GetMotorFrequency();
        }

        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.DrawFillCircle(frontColor, this.HeadCenter, this.HeadRadius);
            panel.DrawFillCircle(frontColor, this.Wheel1Center, this.Wheel1Radius);
            panel.DrawFillCircle(frontColor, this.Wheel2Center, this.Wheel2Radius);
        }
    }
}
