using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using SpriteEditorControl.Controls.Main.Model;
using SpriteEditorControl.Controls.Sprite.Model;
using PhysicItemEditorControl.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Windows.Forms;
using WpfControls.Model;

namespace SpriteEditorControl.Controls.Sprite
{
    internal class SpriteViewModel : ReactiveObject, IGraphicPanelHandler, ITimerHandler, IStringSerializable, IObjectSerializable
    {
        private GraphicPanel2D panel;
        private SpriteRowImage spriteImage = null;
        private PhysicItemExportData physicData;

        [Reactive] public int SpriteCount { get; set; } = 10;
        [Reactive] public SpriteExportData.PivotOriantation Oriantation { get; set; } = SpriteExportData.PivotOriantation.None;
        [Reactive] public int TimeStepsPerFrame { get; set; } = 10; //So viele Timesteps bleibt das PhysicModel an einer Position, bevor der nächste Frame angesprungen wird 
        [Reactive] public int IterationCount { get; set; } = 100; //So viel PGS-Iterationen wird zur Phyisksimulation verwendet. Entspricht this.physicScene.IterationCount

        public ObservableCollection<string> Animations { get; set; } = new ObservableCollection<string>(); //Menge der Animation-Tab-Namen
        [Reactive] public int SelectedAnimationIndex { get; set; } = 0; //Aus diesen ausgewählten Animation-Tab wird das Sprite erzeugt

        public IEnumerable<SpriteExportData.PivotOriantation> OriantationValues
        {
            get
            {
                return Enum.GetValues(typeof(SpriteExportData.PivotOriantation))
                    .Cast<SpriteExportData.PivotOriantation>();
            }
        }

        [Reactive] public bool ShowBoundingBox { get; set; } = true;

        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }


        [Reactive] public int PivotX { get; set; } = 0; //Zentrum innerhalb eines einzelne Sprite-Bildes
        [Reactive] public int PivotY { get; set; } = 0; //Zentrum innerhalb eines einzelne Sprite-Bildes
        [Reactive] public float Zoom { get; set; } = 1;
        [Reactive] public float RotateZ { get; set; } = 0;
        [Reactive] public float RotateY { get; set; } = 0;

        public ReactiveCommand<object, Unit> ZoomRightClick { get; private set; }
        public ReactiveCommand<object, Unit> RotateZRightClick { get; private set; }
        public ReactiveCommand<object, Unit> RotateYRightClick { get; private set; }

        public SpriteViewModel(GraphicPanel2D panel, string dataFolder) 
        {
            this.panel = panel;

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = dataFolder;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.spriteImage.Image.SaveWithCorrectFormat(saveFileDialog.FileName);
                }
            });

            this.ZoomRightClick = ReactiveCommand.Create<object, Unit>((args) =>
            {
                this.Zoom = 1;
                return Unit.Default;
            });
            this.RotateZRightClick = ReactiveCommand.Create<object, Unit>((args) =>
            {
                this.RotateZ = 0;
                return Unit.Default;
            });
            this.RotateYRightClick = ReactiveCommand.Create<object, Unit>((args) =>
            {
                this.RotateY = 0;
                return Unit.Default;
            });

            this.WhenAnyValue(x => x.SpriteCount).Subscribe(x => { Update(); });
            this.WhenAnyValue(x => x.Oriantation).Subscribe(x => { Update(); });
            this.WhenAnyValue(x => x.SelectedAnimationIndex).Subscribe(x => { Update(); });
            this.WhenAnyValue(x => x.TimeStepsPerFrame).Subscribe(x => { Update(); });
            this.WhenAnyValue(x => x.IterationCount).Subscribe(x => { Update(); });
        }

        private void Update()
        { 
            if (this.physicData == null ||this.physicData.AnimationData == null) return;

            if (this.Animations.Count != this.physicData.AnimationData.Length)
            {
                this.Animations.Clear();
                for (int i=0;i<this.physicData.AnimationData.Length;i++)
                {
                    this.Animations.Add("Animation " + i);
                }
            }


            var spriteData = GetSpriteData();

            string textureName = "sprite";
            this.panel.CreateOrUpdateNamedBitmapTexture(textureName, spriteData.Image);
            this.spriteImage = new SpriteRowImage(textureName, spriteData, this.physicData.AnimationData[this.SelectedAnimationIndex].AnimationData.DurrationInSeconds);
        }

        public void UpdatePhysicData(PhysicItemExportData data)
        {
            this.physicData = data;
            Update();
        }

        public SpriteData GetSpriteData()
        {
            var spriteCreator = new PhysicSceneSpriteCreator(this.physicData.PhysicSceneData, this.physicData.TextureData, this.physicData.AnimationData[this.SelectedAnimationIndex]);
            spriteCreator.PhysicSceneIterationCount = this.IterationCount;
            var spriteData = spriteCreator.GetSpriteImage(this.SpriteCount, this.Oriantation, this.TimeStepsPerFrame);

            return spriteData;
        }

        #region IGraphicPanelHandler, ITimerHandler
        public void HandleTimerTick(float dt)
        {
            this.panel.ClearScreen(Color.White);
            if (this.spriteImage != null)
            {
                this.spriteImage.HandleTimerTick(dt);

                int x = panel.Width / 2;
                int y = panel.Height / 2;
                this.spriteImage.Draw(this.panel, x, y, this.PivotX, this.PivotY, this.Zoom, this.RotateZ, this.RotateY, this.ShowBoundingBox);
                
                panel.DrawFillCircle(Color.Green, new Vector2D(x, y), 5); //Pivotpunkt

                //this.panel.DrawImage("sprite", 0, 0, this.spriteImage.Image.Width, this.spriteImage.Image.Height, 0, 0, this.spriteImage.Image.Width, this.spriteImage.Image.Height, false, Color.White);
            }
            
            this.panel.FlipBuffer();
        }
        public void HandleMouseClick(MouseEventArgs e)
        {
        }
        public void HandleMouseWheel(MouseEventArgs e)
        {
        }
        public void HandleMouseMove(MouseEventArgs e)
        {
        }
        public void HandleMouseDown(MouseEventArgs e)
        {
        }
        public void HandleMouseUp(MouseEventArgs e)
        {
        }
        public void HandleMouseEnter()
        {
        }
        public void HandleMouseLeave()
        {
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }
        public void HandleSizeChanged(int width, int height)
        {
        }

        #endregion

        #region IObjectSerializable
        public object GetExportObject()
        {
            return GetExportData();
        }

        public void LoadFromExportObject(object exportObject)
        {
            if (exportObject == null) return;

            var data = (SpriteExportData)exportObject;
           
            this.SpriteCount = data.SpriteCount;
            this.Oriantation = data.Oriantation;
            this.TimeStepsPerFrame = data.TimeStepsPerFrame;
            this.IterationCount = data.IterationCount;
            this.PivotX = data.PivotX;
            this.PivotY = data.PivotY;
            this.Zoom = data.Zoom;
            this.RotateZ = data.RotateZ;
            this.RotateY = data.RotateY;
        }

        private SpriteExportData GetExportData()
        {
            return new SpriteExportData()
            {
                SpriteCount = this.SpriteCount,
                Oriantation = this.Oriantation,
                TimeStepsPerFrame = this.TimeStepsPerFrame,
                IterationCount = this.IterationCount,
                PivotX = this.PivotX,
                PivotY = this.PivotY,
                Zoom = this.Zoom,
                RotateZ = this.RotateZ,
                RotateY = this.RotateY,
            };
        }


        #endregion

        #region IStringSerializable
        public string GetExportString()
        {
            return JsonHelper.Helper.ToJson(GetExportData());
        }

        public void LoadFromExportString(string exportString)
        {
            var data = JsonHelper.Helper.CreateFromJson<SpriteExportData>(exportString);
            LoadFromExportObject(data);
        }
        #endregion
    }
}
