using System.Collections.Generic;
using Engine.Core;
using Engine.Physics;
using Engine.Physics.BEPUutilities;
using OpenTK.Input;
using Vector3 = OpenTK.Vector3;

namespace HorrorOfBindings.components
{
    public class CameraRaycaster : AbstractComponent
    {
        private int cast;
        private GameObject sphereTargetMarker;
        private GameObject looker;

        public static bool ObjectUnderMouse(Vector3 cameraPosition, out KeyValuePair<Collider, RayHit> hit)
        {
            Ray r = ConstructRayFromMousePosition(cameraPosition);
            bool ret = PhysicsEngine.RayCastFirst(r, 1000, LayerManager.NameToLayer("raycast"),
                out hit);

            return ret;
        }

        public CameraRaycaster(GameObject targetmarker, GameObject looker)
        {
            cast = LayerManager.NameToLayer("raycast");
            sphereTargetMarker = targetmarker;
            this.looker = looker;
        }

        protected override void Update(float deltaTime)
        {
            Ray r = ConstructRayFromMousePosition(Owner.GetLocalPosition());
            bool ret = PhysicsEngine.RayCastFirst(r, 1000, cast,
                out KeyValuePair<Collider, RayHit> arr);
            if (ret)
            {
                Vector3 pos = arr.Value.Location;
                pos.Y = looker.LocalPosition.Y;
                sphereTargetMarker.SetLocalPosition(pos);
                looker.LookAt(sphereTargetMarker);
            }
        }


        private static Ray ConstructRayFromMousePosition(Vector3 localPosition)
        {
            Vector2 mpos = GameEngine.Instance.MousePosition;
            Vector3 mousepos = GameEngine.Instance.ConvertScreenToWorldCoords((int) mpos.X, (int) mpos.Y);
            return new Ray(localPosition, (mousepos - localPosition).Normalized());
        }
    }
}