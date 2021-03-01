using System;
using System.Diagnostics;
using IrregularZ.Graphics;
using IrregularZ.Import;
using IrregularZ.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Model = IrregularZ.Graphics.Model;
using Vector3 = IrregularZ.Graphics.Vector3;

namespace IrregularZ
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private float _angle;
        private Camera _camera;
        private FrameBuffer<int> _colorRaster;
        private FrameBuffer<float> _depthRaster;
        private FirstPersonControl _firstPersonControl;
        private Scene.Scene _scene;
        private ShadowMapper _shadowMapper;
        private SpriteBatch _spriteBatch;
        private Texture2D _surface;
        private Renderer _renderer;

        public Game()
        {
            var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width ;
            var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height ;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = screenWidth,
                PreferredBackBufferHeight = screenHeight,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = true
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _colorRaster = new FrameBuffer<int>(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _depthRaster = new FrameBuffer<float>(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _renderer = new Renderer(_colorRaster, _depthRaster);

            _camera = new Camera(
                (float) Math.PI / 4,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight, 1, 10000
            );
            _camera.MoveTo(-5, 8, -22);
            _camera.LookAt(0, 0, 0);

            _firstPersonControl = new FirstPersonControl(_camera);

            _shadowMapper = new ShadowMapper(256);
            _shadowMapper.MoveTo(10, 80, -100);
            _shadowMapper.LookAt(0, 0, 0);

            _renderer.MoveLight(10, 280, -100);

            var gruntMesh = ContentLoader.ReadModel("grunt.obj");
            var dinoMesh = ContentLoader.ReadModel("dinorider.obj");

            var planeMesh = BuildPlane(System.Drawing.Color.Blue);

            _scene = new Scene.Scene(new BranchNode
            {
                Nodes =
                {
                    new BranchNode
                    {
                        Nodes =
                        {
                            new LeafNode
                            {
                                LocalTransform = Matrix4.CreateTranslation(0, -5, 0) * Matrix4.CreateScale(50, 50, 50),
                                Geometry = planeMesh
                            },
                            new BranchNode
                            {
                                Nodes =
                                {
                                    new LeafNode {LocalTransform = Matrix4.CreateScale(10, 10, 10), Geometry = gruntMesh},
                                    new LeafNode
                                    {
                                        LocalTransform =
                                            Matrix4.CreateTranslation(-2, 0, 7) * Matrix4.CreateScale(10, 10, 10) *
                                            Matrix4.CreateRotationY(4.6F),
                                        Geometry = dinoMesh
                                    },
                                    new LeafNode
                                    {
                                        LocalTransform =
                                            Matrix4.CreateTranslation(7, 0, 3) * Matrix4.CreateScale(10, 10, 10) *
                                            Matrix4.CreateRotationY(2.3F),
                                        Geometry = gruntMesh
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
                _firstPersonControl.KeyDown(Keys.D);
            else
                _firstPersonControl.KeyUp(Keys.D);

            if (keys.IsKeyDown(Keys.A))
                _firstPersonControl.KeyDown(Keys.A);
            else
                _firstPersonControl.KeyUp(Keys.A);

            if (keys.IsKeyDown(Keys.S))
                _firstPersonControl.KeyDown(Keys.S);
            else
                _firstPersonControl.KeyUp(Keys.S);

            if (keys.IsKeyDown(Keys.W))
                _firstPersonControl.KeyDown(Keys.W);
            else
                _firstPersonControl.KeyUp(Keys.W);

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
                _firstPersonControl.MouseDown();
            else
                _firstPersonControl.MouseUp();

            _firstPersonControl.MouseMove(mouseState.X, mouseState.Y);

            _firstPersonControl.Update(seconds, 50);
            _scene.Update(seconds);

            _angle += 0.001f;

            var light = Matrix4.CreateRotationY(_angle) * new Vector3(10, 50, -70);
            _shadowMapper.MoveTo(light.X, light.Y, light.Z);
            _shadowMapper.LookAt(0, 0, 0);

            _renderer.MoveLight(light.X, light.Y, light.Z);

            base.Update(gameTime);
        }

        private Stopwatch watch = new Stopwatch();
        protected override void Draw(GameTime gameTime)
        {
            _renderer.ViewMatrix = _camera.ViewMatrix;
            _renderer.ProjectionMatrix = _camera.ProjectionMatrix;
            _renderer.Clear(0x5599FF);
            _scene.Render(_renderer, new Frustum(_camera.ViewMatrix, _camera.ProjectionMatrix));
            
            watch.Restart();
            var combinedMatrix = _renderer.ViewportMatrix * _renderer.ProjectionMatrix * _renderer.ViewMatrix;
            _shadowMapper.Shadow(_scene, _colorRaster, _depthRaster, combinedMatrix);
            Console.WriteLine(watch.ElapsedMilliseconds);
            
            _surface.SetData(_colorRaster.Data);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_surface, _graphics.GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private static Model BuildPlane(System.Drawing.Color material)
        {
            var assembler = new Assembler {Color = material};
            assembler.AddVertex(-1, 0, 1);
            assembler.AddVertex(1, 0, 1);
            assembler.AddVertex(1, 0, -1);
            assembler.AddVertex(-1, 0, -1);
            assembler.CreateTriangle(0, 1, 2);
            assembler.CreateTriangle(2, 3, 0);
            return assembler.Compile();
        }
    }
}