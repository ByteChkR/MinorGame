using Engine.Core;
using Engine.DataTypes;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Rendering;
using OpenTK;

namespace MinorGame.components.Weapons
{
    public struct Bullet
    {
        public float BulletLaunchForce { get; set; }
        public float BulletMass { get; set; }
        public bool PhysicalBullets { get; set; }
        public Mesh BulletModel { get; set; }
        public Texture BulletTexture { get; set; }
        public ShaderProgram BulletShader { get; set; }

        public GameObject CreateBullet(GameObject nozzle)
        {
            Engine.Physics.BEPUutilities.Vector3 vel =
                new Vector3(-Vector4.UnitZ * nozzle.GetWorldTransform()) * BulletLaunchForce;
            Vector3 v = vel;

            GameObject bullet =
                new GameObject(nozzle.LocalPosition + (Engine.Physics.BEPUutilities.Vector3) v.Normalized(),
                    "BulletEnemy");
            bullet.Rotation = nozzle.Rotation;
            bullet.AddComponent(new LitMeshRendererComponent(BulletShader, BulletModel, BulletTexture, 1, false));
            bullet.AddComponent(new DestroyTimer(5));
            bullet.Scale = new Vector3(0.3f, 0.3f, 1);
            Collider coll = new Collider(new Box(Vector3.Zero, 0.3f, 0.3f, 1, BulletMass),
                LayerManager.NameToLayer("physics"));
            if (!PhysicalBullets)
            {
                coll.IsTrigger = true;
            }

            bullet.AddComponent(coll);
            coll.PhysicsCollider.ApplyLinearImpulse(ref vel);
            return bullet;
        }
    }
}