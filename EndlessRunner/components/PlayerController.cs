using Engine.Core;
using Engine.DataTypes;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.CollisionTests;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.NarrowPhaseSystems.Pairs;
using Engine.Physics.BEPUutilities;
using Engine.Rendering;
using EndlessRunner.scenes;
using OpenTK.Input;
using Mesh = Engine.DataTypes.Mesh;

namespace EndlessRunner.components
{
    public class PlayerController : AbstractComponent
    {
        private static bool _init = false;
        private static int _physicsLayer, _raycastLayer;
        private static Mesh _playerMesh;
        private static Texture _playerTexture;

        public static GameObject CreatePlayer(Vector3 position, BasicCamera c)
        {
            if (!_init)
            {
                _init = true;
                _physicsLayer = LayerManager.RegisterLayer("physics", new Layer(1));
                _raycastLayer = LayerManager.RegisterLayer("raycast", new Layer(2));
                _playerMesh = MeshLoader.FileToMesh("models/cube_flat.obj");
                _playerTexture = TextureLoader.FileToTexture("textures/playerTexture.png");
            }

            OffsetConstraint off = new OffsetConstraint {Damping = 0, MoveSpeed = 10};
            c.AddComponent(off);

            GameObject playerObject = new GameObject(position, "Player");
            LitMeshRendererComponent playerRenderer =
                new LitMeshRendererComponent(GameScene.TextureShader, _playerMesh, _playerTexture, 1);
            Collider collider = new Collider(new Box(Vector3.Zero, 0.6f, 1f, 0.4f, 1f), _physicsLayer);
            PlayerController controller = new PlayerController(10, collider);
            playerObject.AddComponent(playerRenderer);
            playerObject.AddComponent(collider);
            playerObject.AddComponent(controller);


            off.Attach(playerObject, new OpenTK.Vector3(0, 5, 7));

            return playerObject;
        }

        private float _jumpForce = 20f;
        private bool _isSwitchingLanes = false;
        private Vector3 _switchDir = Vector3.Zero;
        private Vector3 _initSwitchPosition = Vector3.Zero;
        private Collider _collider;
        private bool _isJumping = true;
        private float _switchTimer;
        private float _switchTime = 1f;
        private float _laneDistance = 1f;


        private PlayerController(float jumpForce, Collider collider)
        {
            _jumpForce = jumpForce;
            _collider = collider;
        }

        private void Jump()
        {
            Vector3 impulse = Vector3.Up * _jumpForce;
            _collider.PhysicsCollider.ApplyLinearImpulse(ref impulse);
        }

        protected override void Awake()
        {
            _initSwitchPosition = Owner.LocalPosition;
        }

        protected override void Update(float deltaTime)
        {
            if (_isSwitchingLanes)
            {
                SwitchLanes(deltaTime);
            }
        }

        private void SwitchLanes(float deltaTime)
        {
            _switchTimer += deltaTime;
            if (_switchTimer < _switchTime)
            {
                float t = _switchTimer / _switchTime;
                Vector3 delta = _initSwitchPosition + _switchDir * _laneDistance - _initSwitchPosition;
                Vector2 positionAdd = new Vector2(delta.X, delta.Z);
                Vector3 v = Vector3.Lerp(_initSwitchPosition, _initSwitchPosition + _switchDir * _laneDistance, t);
                _collider.PhysicsCollider.Position = new Vector3(v.X, _collider.PhysicsCollider.Position.Y, v.Z);
            }
            else
            {
                _isSwitchingLanes = false;
                _switchTimer = 0;
            }
        }

        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!_isSwitchingLanes)
            {
                if (e.Key == Key.A)
                {
                    _isSwitchingLanes = true;
                    _switchDir = Vector3.Left;
                    _initSwitchPosition = Owner.LocalPosition;
                }
                else if (e.Key == Key.D)
                {
                    _isSwitchingLanes = true;
                    _initSwitchPosition = Owner.LocalPosition;
                    _switchDir = Vector3.Right;
                }
            }

            if (!_isJumping)
            {
                if (e.Key == Key.Space)
                {
                    Jump();
                }
            }
        }

        protected override void OnContactRemoved(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (other.Owner.Name == "Ground" && handler.Contacts.Count == 0)
            {
                _isJumping = true;
            }
        }


        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (other.Owner.Name == "Ground")
            {
                _isJumping = false;
            }
        }
    }
}