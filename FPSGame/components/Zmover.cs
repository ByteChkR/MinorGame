using Engine.Core;
using Engine.Physics.BEPUutilities;

namespace EndlessRunner.components
{
    public class Zmover : AbstractComponent
    {
        protected override void Update(float deltaTime)
        {


            Owner.LocalPosition += Vector3.UnitZ * deltaTime;

        }
    }
}