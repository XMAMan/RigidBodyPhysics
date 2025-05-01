using GraphicMinimal;
using PhysicGlobal;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using WpfControls.Extensions;

namespace GameHelper.Simulation.RigidBodyTagging
{
    public class AnchorPoint
    {
        private IPublicRigidBody body;
        private Vec2D localAnchor;

        public AnchorPoint(ITagDataProvider tagProvider, string tagName, int index)
        {
            this.body = tagProvider.GetBodyByTagName(tagName);
            var tag = tagProvider.GetTagDataFromBody(body);
            this.localAnchor = tag.AnchorPoints[index].ToPhx();
        }

        public Vector2D GetPosition()
        {
            return this.body.GetWorldPointFromLocalDirection(this.localAnchor).ToGrx();
        }
    }
}
