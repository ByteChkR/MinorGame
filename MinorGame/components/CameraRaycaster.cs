using System.Collections.Generic;
using MinorEngine.BEPUutilities;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.physics;
using OpenTK.Input;
using Vector3 = OpenTK.Vector3;

namespace MinorGame.components
{
    public class CameraRaycaster : AbstractComponent
    {
        Layer cast;
        private GameObject sphereTargetMarker;
        private GameObject PlayerHead;
        public CameraRaycaster(GameObject targetmarker, GameObject playerHead, Layer raycast)
        {
            cast = raycast;
            sphereTargetMarker = targetmarker;
            PlayerHead = playerHead;
        }

        protected override void Update(float deltaTime)
        {
            Ray r = Physics.ConstructRayFromMousePosition(Owner.GetLocalPosition());
            bool ret = Physics.RayCastFirst(r, 1000, cast,
                out KeyValuePair<Collider, RayHit> arr);
            if (ret)
            {
                Vector3 pos = arr.Value.Location;
                pos.Y = PlayerHead.GetLocalPosition().Y;
                sphereTargetMarker.SetLocalPosition(pos);
                this.Log("Sphere Pos: " + pos, DebugChannel.Log);

                PlayerHead.LookAt(pos);
            }
        }
    }
}