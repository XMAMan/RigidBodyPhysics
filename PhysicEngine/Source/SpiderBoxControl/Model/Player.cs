using GameHelper;
using GameHelper.Simulation;
using GameHelper.Simulation.RigidBodyTagging;
using GraphicPanels;
using PhysicSceneDrawing;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using SpriteEditorControl.Controls.Main.Model;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SpiderBoxControl.Model
{
    class Player : IRigidBodyDrawer
    {
        public float Power { get; private set; } = 1; //[0..1]: Mit der Kraft wird das Seil aus dem Stock geschossen

        public float RopeLength
        {
            get
            {
                if (this.rope == null) return 0;
                return this.rope.SegmentLength;
            }
        }

        private GameSimulator simulator;
        private float timerIntervallInMilliseconds;
        private Sounds sounds;
        private IPublicRigidRectangle ton;
        private IPublicRigidRectangle arm;
        private IPublicRevoluteJoint armJoint;
        private AnchorPoint tonMiddleUp;
        private AnchorPoint tonRightUp;
        private AnchorPoint tonMiddleDown;
        private AnchorPoint armTip;
        private SpriteImage sprite;
        private Rope rope = null;
        private bool isMouseDown = false;
        private MouseButtons mouseDownButton = MouseButtons.None;

        public Player(GameSimulator simulator, float timerIntervallInMilliseconds, string dataFolder, Sounds sounds)
        {
            this.simulator = simulator;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;
            this.sounds = sounds;
            this.ton = (IPublicRigidRectangle)simulator.GetBodyByTagName("Ton");
            this.arm = (IPublicRigidRectangle)simulator.GetBodyByTagName("Arm");
            this.armJoint = (IPublicRevoluteJoint)simulator.GetJointByTagName("ArmJoint");
            this.tonMiddleUp = new AnchorPoint(simulator, "Ton", 0);
            this.tonRightUp = new AnchorPoint(simulator, "Ton", 1);
            this.tonMiddleDown = new AnchorPoint(simulator, "Ton", 2);
            this.armTip = new AnchorPoint(simulator, "Arm", 0);

            var spriteData = JsonHelper.Helper.CreateFromJson<SpriteEditorExportData>(File.ReadAllText(dataFolder + "Sprite.txt"));
            this.sprite = new SpriteImage(dataFolder + "Sprite.png", spriteData.SpriteData.SpriteCount, 1, spriteData.SpriteData.SpriteCount, spriteData.PhysicItemData.AnimationData[0].AnimationData.DurrationInSeconds, true, spriteData.SpriteData.PivotX, spriteData.SpriteData.PivotY);
            simulator.UseCustomDrawingForRigidBody(this.ton, this);
            simulator.RemoveBodyFromPhysicSceneDrawer(this.arm);
        }

        public void HandleMouseMove(Vec2D mousePosition, MouseButtons button)
        {
            this.armJoint.MotorPosition = GetArmAngleFromMouse(mousePosition);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            this.Power += e.Delta / 1000f;
            this.Power = Math.Min(1, Math.Max(this.Power, 0));
        }
        public void HandleMouseClick(Vec2D mousePosition, MouseButtons button)
        {
            if (button == MouseButtons.Left && this.rope == null)
            {
                var p1 = this.arm.Center;
                var p2 = this.armTip.GetPosition().ToPhx();
                float f = 0.9f;
                var p = p1 * (1 - f) + p2 * f;
                var initialVelocity = (p2 - p1).Normalize() * this.Power * 2;
                this.rope = new Rope(this.arm, p, initialVelocity, this.simulator, this.timerIntervallInMilliseconds);

                this.sounds.PlayThrowRope();
            }
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            this.isMouseDown = true;
            this.mouseDownButton = e.Button;            

            if (this.rope != null)
            {
                this.sounds.StartRope();
            }
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.isMouseDown = false;

            this.sounds.StopRope();
        }
        public void HandleKeyDown(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Space && this.rope != null)
            {
                this.rope.RemoveFromSimulation();
                this.rope = null;
            }
        }

        public void HandleKeyUp(System.Windows.Input.Key key)
        {
            
        }

        public void HandleTimerTick(float dt)
        {
            if (this.isMouseDown && this.mouseDownButton == MouseButtons.Left && this.rope != null)
            {
                this.rope.DecreaseLength();
                this.sounds.RopeFrequency = GetRopeFrequency();
            }

            if (this.isMouseDown && this.mouseDownButton == MouseButtons.Right && this.rope != null)
            {
                this.rope.IncreaseLength();
                this.sounds.RopeFrequency = GetRopeFrequency();
            }
        }

        private float GetRopeFrequency()
        {
            float f = Math.Min(1, Math.Max(0, (this.rope.SegmentLength - Rope.MinSegmentLength) / (Rope.MaxSegmentLength - Rope.MinSegmentLength)));

            float minFreq = 400;
            float maxFreq = 800;
            return minFreq + (maxFreq - minFreq) * f;
        }

        //0 = Arm zeigt nach links; 1 = Arm zeigt nach rechts
        private float GetArmAngleFromMouse(Vec2D mousePosition)
        {
            var p1 = this.tonMiddleUp.GetPosition().ToPhx();
            var p2 = this.tonRightUp.GetPosition().ToPhx();

            var v1 = p2 - p1;
            var v2 = mousePosition - p1;

            float angle = Vec2D.Angle360(v1, v2) - 180;
            if (angle < -90) angle = 180;
            if (angle < 0) angle = 0;
            return angle / 180;
        }
        #region IRigidBodyDrawer
        public void Draw(GraphicPanel2D panel)
        {
            //Sprite
            int index = Math.Min(this.sprite.ImageCount - 1, (int)(this.armJoint.CurrentPosition * (this.sprite.ImageCount - 1) + 0.5f));

            this.sprite.RotateZAngleInDegree = this.ton.Angle / (float)Math.PI * 180;
            this.sprite.DrawSingleImage(panel, tonMiddleDown.GetPosition(), index);

            
            if (this.rope == null)
            {
                //Zielpunkt
                var tip = this.armTip.GetPosition();
                var dir = (tip.ToPhx() - this.arm.Center).Normalize();
                var targetPoint = this.arm.Center + dir * (80 + 20 * this.Power);
                //panel.DrawLine(new Pen(Color.Red, 2), targetPoint.ToGrx(), tip);
                //panel.DrawFillCircle(Color.Red, targetPoint.ToGrx(), 5);
            }else
            {
                //Seil
                this.rope.Draw(panel);
            }            
        }

        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.DrawFillRectangle(frontColor, this.ton.Center.X, this.ton.Center.Y, this.ton.Size.X, this.ton.Size.Y, this.ton.Angle / (float)Math.PI * 180);
            panel.DrawFillRectangle(frontColor, this.arm.Center.X, this.arm.Center.Y, this.arm.Size.X, this.arm.Size.Y, this.arm.Angle / (float)Math.PI * 180);
        }
        #endregion

        public void DrawPhysicBorderFromArm(GraphicPanel2D panel, Pen pen)
        {
            panel.DrawPolygon(pen, this.arm.Vertex.Select(x => x.ToGrx()).ToList());
        }
    }
}
