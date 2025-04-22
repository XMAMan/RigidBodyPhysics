using GraphicMinimal;
using GraphicPanels;
using System.Drawing;
using TextureEditorControl.Controls.DrawingSettings;
using TextureEditorControl.Controls.TextureData;
using WpfControls.Controls.CameraSetting;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    interface IShape
    {
        RectangleF BoundingBox { get; }
        TextureDataViewModel Propertys { get; }
        bool IsSelected { get; set; }
        void Draw(GraphicPanel2D panel, Camera2D camera, DrawingSettingsViewModel settings);
        bool IsPointInPhysicModel(Vector2D point);
        bool IsPointInTextureBorder(Vector2D point);
        RectanglePart GetSelectedPartFromTextureBorder(Vector2D point);
        Vector2D[] GetNormalsFromTextureBorderPoint(RectanglePart part, Vector2D point);
        Vector2D GetDistanceToTextureBorderPart(RectanglePart part, Vector2D point);
        float GetAngleDistanceToTextureCorner(RectanglePart part, Vector2D point);
    }
    enum RectanglePart { None, LeftTopCorner, RightTopCorner, RightBottomCorner, LeftBottomCorner, TopEdge, RightEdge, BottomEdge, LeftEdge, Center }
}
