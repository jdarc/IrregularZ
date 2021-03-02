using System;
using IrregularZ.Graphics;
using IrregularZ.Import;
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
        private Scene _scene;
        private ShadowMapper _shadowMapper;
        private SpriteBatch _spriteBatch;
        private Texture2D _surface;
        private Renderer _renderer;

        public Game()
        {
            var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            IsMouseVisible = false;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = screenWidth,
                PreferredBackBufferHeight = screenHeight,
                IsFullScreen = true,
                SynchronizeWithVerticalRetrace = true,
                HardwareModeSwitch = true
            };
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _surface = new Texture2D(
                _graphics.GraphicsDevice,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight,
                false,
                SurfaceFormat.Color
            );

            _colorRaster = new FrameBuffer<int>(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _depthRaster = new FrameBuffer<float>(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _renderer = new Renderer(_colorRaster, _depthRaster);
            _shadowMapper = new ShadowMapper(256);

            _camera = new Camera(
                (float) Math.PI / 4,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight,
                1, 500
            );
            _camera.MoveTo(-5, 8, -22);
            _camera.LookAt(0, 0, 0);

            _firstPersonControl = new FirstPersonControl(_camera);

            var gruntMesh = ContentLoader.ReadModel("grunt.obj");
            var dinoMesh = ContentLoader.ReadModel("dinorider.obj");
            var planeMesh = BuildPlane(System.Drawing.Color.Blue);

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
                                LocalTransform = Create(new Vector3(0, -5, 0), Matrix4.Identity, 50),
                                Geometry = planeMesh
                            },
                            new BranchNode
                            {
                                Nodes =
                                {
                                    new LeafNode
                                    {
                                        LocalTransform = Matrix4.CreateScale(10, 10, 10),
                                        Geometry = gruntMesh
                                    },
                                    new LeafNode
                                    {
                                        LocalTransform = Create(new Vector3(-2, 0, 7),
                                            Matrix4.CreateRotationY(4.6F), 10),
                                        Geometry = dinoMesh
                                    },
                                    new LeafNode
                                    {
                                        LocalTransform = Create(new Vector3(7, 0, 3),
                                            Matrix4.CreateRotationY(2.3F), 10),
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

        protected override void Update(GameTime gameTime)
        {
            var keys = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keys.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            var seconds = gameTime.ElapsedGameTime.Milliseconds / 1000.0;

            if (keys.IsKeyDown(Keys.D))
            {
                _firstPersonControl.KeyDown(Keys.D);
            }
            else
            {
                _firstPersonControl.KeyUp(Keys.D);
            }

            if (keys.IsKeyDown(Keys.A))
            {
                _firstPersonControl.KeyDown(Keys.A);
            }
            else
            {
                _firstPersonControl.KeyUp(Keys.A);
            }

            if (keys.IsKeyDown(Keys.S))
            {
                _firstPersonControl.KeyDown(Keys.S);
            }
            else
            {
                _firstPersonControl.KeyUp(Keys.S);
            }

            if (keys.IsKeyDown(Keys.W))
            {
                _firstPersonControl.KeyDown(Keys.W);
            }
            else
            {
                _firstPersonControl.KeyUp(Keys.W);
            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _firstPersonControl.MouseDown();
            }
            else
            {
                _firstPersonControl.MouseUp();
            }

            _firstPersonControl.MouseMove(mouseState.X, mouseState.Y);
            _firstPersonControl.Update(seconds, 50);
            _scene.Update(seconds);

            _angle += 0.001F;

            var light = Matrix4.CreateRotationY(_angle) * new Vector3(10, 50, -70);
            _shadowMapper.LookAt(0, 0, 0);
            _shadowMapper.MoveTo(light.X, light.Y, light.Z);
            _renderer.MoveLight(light.X, light.Y, light.Z);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _renderer.ViewMatrix = _camera.ViewMatrix;
            _renderer.ProjectionMatrix = _camera.ProjectionMatrix;
            _renderer.Clear(0x5599FF);
            _scene.Render(_renderer, new Frustum(_camera.ViewMatrix, _camera.ProjectionMatrix));

            _shadowMapper.Generate(_depthRaster, _renderer.ViewportMatrix * _renderer.ProjectionMatrix * _renderer.ViewMatrix);
            _shadowMapper.Render(_scene, _colorRaster);

            _surface.SetData(_colorRaster.Data);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_surface, _graphics.GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private static Matrix4 Create(Vector3 position, Matrix4 rotation, float scale)
        {
            var translation = Matrix4.CreateTranslation(position.X, position.Y, position.Z);
            var scaler = Matrix4.CreateScale(scale, scale, scale);
            return translation * rotation * scaler;
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