using Engine.Core;
using Engine.Physics;
using Engine.Physics.BEPUphysics.CollisionTests;
using Engine.Physics.BEPUphysics.NarrowPhaseSystems.Pairs;
using OpenTK;
using Vector3 = Engine.Physics.BEPUutilities.Vector3;

namespace MinorGame.components
{
    public class BouncePad : AbstractComponent
    {
        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if ((other.Owner.Name == "Player"|| other.Owner.Name == "Enemy") && handler.Contacts.Count == 1) //Only if thats the first contact
            {
                Vector3 force = Vector3.UnitY*5000;
                other.PhysicsCollider.ApplyLinearImpulse(ref force);
            }
        }
    }
}