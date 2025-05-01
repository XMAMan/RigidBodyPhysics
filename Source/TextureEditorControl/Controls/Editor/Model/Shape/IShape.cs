using GraphicPanels;
using PhysicGlobal;
using System.Drawing;
using TextureEditorControl.Controls.DrawingSettings;
using TextureEditorControl.Controls.TextureData;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    interface IShape
    {
        RectangleF BoundingBox { get; }
        TextureDataViewModel Propertys { get; }
        bool IsSelected { get; set; }
        void Draw(GraphicPanel2D panel, Camera2D camera, DrawingSettingsViewModel settings);
        bool IsPointInPhysicModel(Vec2D point);
        bool IsPointInTextureBorder(Vec2D point);
        RectanglePart GetSelectedPartFromTextureBorder(Vec2D point);
        Vec2D[] GetNormalsFromTextureBorderPoint(RectanglePart part, Vec2D point);
        Vec2D GetDistanceToTextureBorderPart(RectanglePart part, Vec2D point);
        float GetAngleDistanceToTextureCorner(RectanglePart part, Vec2D point);
    }
    enum RectanglePart { None, LeftTopCorner, RightTopCorner, RightBottomCorner, LeftBottomCorner, TopEdge, RightEdge, BottomEdge, LeftEdge, Center }
}
