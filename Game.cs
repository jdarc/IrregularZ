using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IrregularZ
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private float _angle;
        private Camera _camera;
        private Raster _colorRaster;
        private Raster _depthRaster;
        private FpsCamera _fpsCamera;
        private Scene _scene;
        private ShadowMapper _shadowMapper;
        private SpriteBatch _spriteBatch;
        private Texture2D _surface;
        private Visualizer _visualizer;

        public Game()
        {
            var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = screenWidth >> 1,
                PreferredBackBufferHeight = screenHeight >> 1
            };
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _colorRaster = new Raster(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _depthRaster = new Raster(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _visualizer = new Visualizer(_colorRaster, _depthRaster);

            _camera = new Camera(
                (float) Math.PI / 4.0f,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight, 1f, 10000f
            );
            _camera.MoveTo(-5, 8, -22);
            _camera.LookAt(0, 0, 0);

            _fpsCamera = new FpsCamera(_camera);

            _shadowMapper = new ShadowMapper(256);
            _shadowMapper.MoveTo(10, 80, -100);
            _shadowMapper.LookAt(0, 0, 0);

            _visualizer.MoveLight(10, 280, -100);

            var loader = new ResourceLoader {Path = "Content"};
            var objMesh = loader.LoadModel("grunt.obj");
            objMesh.Compile(true);

            IGeometry planeMesh = BuildPlane(new Material
            {
                Diffuse = ColorF.FromXyz(0.1f, 0.125f, 0.9f),
                Ambient = ColorF.FromXyz(0.1f, 0.2f, 0.3f)
            });

            _scene = new Scene(new BranchNode
            {
                Nodes =
                {
                    new BranchNode
                    {
                        Nodes =
                        {
                            new LeafNode
                            {
                                LocalTransform = Matrix4F.CreateTranslation(0, -5, 0) * Matrix4F.CreateScaler(50),
                                Geometry = planeMesh
                            },
                            new BranchNode
                            {
                                Nodes =
                                {
                                    new LeafNode
                                    {
                                        LocalTransform = Matrix4F.CreateScaler(10), Geometry = objMesh
                                    },
                                    new LeafNode
                                    {
                                        LocalTransform =
                                            Matrix4F.CreateTranslation(-2, 0, 7) * Matrix4F.CreateScaler(10) *
                                            Matrix4F.CreateRotationAboutY(4.67134f),
                                        Geometry = objMesh
                                    },
                                    new LeafNode
                                    {
                                        LocalTransform =
                                            Matrix4F.CreateTranslation(7, 0, 3) * Matrix4F.CreateScaler(10) *
                                            Matrix4F.CreateRotationAboutY(2.334f),
                                        Geometry = objMesh
                                    }
                                }
                            }
                        }
                    }
                }
            });

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _surface = new Texture2D(
                _graphics.GraphicsDevice,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight,
                false,
                SurfaceFormat.Color
            );
        }

        protected override void Update(GameTime gameTime)
        {
            var seconds = gameTime.ElapsedGameTime.Milliseconds / 1000.0;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            var keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.D))
                _fpsCamera.KeyDown(Keys.D);
            else
                _fpsCamera.KeyUp(Keys.D);

            if (keys.IsKeyDown(Keys.A))
                _fpsCamera.KeyDown(Keys.A);
            else
                _fpsCamera.KeyUp(Keys.A);

            if (keys.IsKeyDown(Keys.S))
                _fpsCamera.KeyDown(Keys.S);
            else
                _fpsCamera.KeyUp(Keys.S);

            if (keys.IsKeyDown(Keys.W))
                _fpsCamera.KeyDown(Keys.W);
            else
                _fpsCamera.KeyUp(Keys.W);

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
                _fpsCamera.MouseDown();
            else
                _fpsCamera.MouseUp();

            _fpsCamera.MouseMove(mouseState.X, mouseState.Y);

            _fpsCamera.Update(seconds, 50);
            _scene.Update(seconds);

            var light = new Vector3F(10, 50, -70);

            _angle += 0.001f;
            var rot = Matrix4F.CreateRotationAboutY(_angle);

            light.Transform(ref rot);
            _shadowMapper.MoveTo(light.X, light.Y, light.Z);
            _shadowMapper.LookAt(0, 0, 0);

            _visualizer.MoveLight(light.X, light.Y, light.Z);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _visualizer.ViewMatrix = _camera.ViewMatrix;
            _visualizer.ProjectionMatrix = _camera.ProjectionMatrix;
            _visualizer.Clear(0x5599FF, float.PositiveInfinity);
            _scene.Render(_visualizer, _camera.Frustum);
            _shadowMapper.Shadow(_scene, _colorRaster, _depthRaster, _visualizer.CombinedMatrix);

            _colorRaster.ReOrder();
            _surface.SetData(_colorRaster.Buffer.Data);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_surface, _graphics.GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public static Model BuildPlane(Material material)
        {
            var model = new Model();

            model.AddVertex(-1, 0, 1);
            model.AddVertex(1, 0, 1);
            model.AddVertex(1, 0, -1);
            model.AddVertex(-1, 0, -1);

            model.CreateTriangle(0, 1, 2);
            model.CreateTriangle(2, 3, 0);

            model.ChangeMaterial(material);

            return model.Compile(false);
        }
    }
}