using Stride.BepuPhysics;
using Stride.BepuPhysics.Definitions.Colliders;
using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Scripts;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Engine;
using Stride.Engine.Processors;
using Stride.Extensions;
using Stride.Games;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Physics;
using Stride.Rendering;
using Stride.Rendering.Colors;
using Stride.Rendering.Compositing;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Skyboxes;
using System.Reflection;

namespace Stride.CommunityToolkit.Engine;

/// <summary>
/// Extensions for <see cref="IGame"/>
/// </summary>
public static class GameExtensions
{
    private const string SkyboxTexture = "skybox_texture_hdr.dds";
    private const string DefaultGroundName = "Ground";
    private static readonly Vector2 _default3DGroundSize = new(15f);
    private static readonly Vector3 _default2DGroundSize = new(15, 0.1f, 0);
    private static readonly Color _defaultMaterialColor = Color.FromBgra(0xFF8C8C8C);
    private static readonly Color _defaultGroundMaterialColor = Color.FromBgra(0xFF242424);

    /// <summary>
    /// Initializes the game, starts the game loop, and handles game events.
    /// </summary>
    /// <remarks>
    /// This method performs the following actions:
    /// 1. Schedules the root script for execution.
    /// 2. Initiates the game loop by calling <see cref="GameBase.Run(GameContext)"/>.
    /// 3. Invokes the provided <paramref name="start"/> and <paramref name="update"/> delegates.
    /// </remarks>
    /// <param name="game">The Game instance to initialize and run.</param>
    /// <param name="context">Optional GameContext to be used. Defaults to null.</param>
    /// <param name="start">Optional action to execute at the start of the game. Takes the root scene as a parameter.</param>
    /// <param name="update">Optional action to execute during each game loop iteration. Takes the root scene and game time as parameters.</param>
    public static void Run(this Game game, GameContext? context = null, Action<Scene>? start = null, Action<Scene, GameTime>? update = null)
    {
        game.Script.Scheduler.Add(RootScript);

        game.Run(context);

        async Task RootScript()
        {
            start?.Invoke(GetRootScene());

            if (update == null) return;

            while (true)
            {
                update.Invoke(GetRootScene(), game.UpdateTime);

                await game.Script.NextFrame();
            }
        }

        Scene GetRootScene() => game.SceneSystem.SceneInstance.RootScene;
    }

    /// <summary>
    /// Initializes the game, starts the game loop, and handles game events.
    /// </summary>
    /// <remarks>
    /// This method performs the following actions:
    /// 1. Schedules the root script for execution.
    /// 2. Initiates the game loop by calling <see cref="GameBase.Run(GameContext)"/>.
    /// 3. Invokes the provided <paramref name="start"/> and <paramref name="update"/> delegates.
    /// </remarks>
    /// <param name="game">The Game instance to initialize and run.</param>
    /// <param name="context">Optional GameContext to be used. Defaults to null.</param>
    /// <param name="start">Optional action to execute at the start of the game. Takes the game as a parameter.</param>
    /// <param name="update">Optional action to execute during each game loop iteration. Takes the game as a parameter.</param>
    public static void Run(this Game game, GameContext? context = null, Action<Game>? start = null, Action<Game>? update = null)
    {
        game.Script.Scheduler.Add(RootScript);

        game.Run(context);

        async Task RootScript()
        {
            start?.Invoke(game);

            if (update == null) return;

            while (true)
            {
                update.Invoke(game);

                await game.Script.NextFrame();
            }
        }
    }

    /// <summary>
    /// Sets up essential components for the game including a GraphicsCompositor, a camera, and a directional light.
    /// </summary>
    /// <remarks>
    /// This method performs the following operations:
    /// 1. Adds a default GraphicsCompositor to the game's SceneSystem and applies a clean UI stage.
    /// 2. Adds a camera to the game.
    /// 3. Adds a directional light to the game.
    /// </remarks>
    /// <param name="game">The Game instance that will receive the base setup.</param>
    public static void SetupBase(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add3DCamera();
        game.AddDirectionalLight();
    }

    /// <summary>
    /// Sets up a default 3D scene for the game, similar to creating an empty project through the editor.
    /// </summary>
    /// <remarks>
    /// This method performs the following setup operations in sequence:
    /// 1. Adds a default GraphicsCompositor to the game's SceneSystem and applies a clean UI stage.
    /// 2. Adds a camera to the game and sets it up with a MouseLookCamera component.
    /// 3. Adds a directional light to the game scene.
    /// 4. Adds a skybox to the game scene.
    /// 5. Adds ground geometry to the game scene.
    /// </remarks>
    /// <param name="game">The Game instance for which the base 3D scene will be set up.</param>
    public static void SetupBase3DScene(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add3DCamera().Add3DCameraController();
        game.AddDirectionalLight();
        game.AddSkybox();
        game.Add3DGround();
    }

    public static void SetupBase2DScene(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add2DCamera().Add2DCameraController();
        //game.AddDirectionalLight();
        game.AddSkybox();
        game.Add2DGround();
    }

    public static void SetupBase3DSceneWithBepu(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add3DCamera().Add3DCameraController();
        game.AddDirectionalLight();
        game.AddSkybox();
        game.Add3DGroundWithBepu();
    }

    /// <summary>
    /// Adds a default GraphicsCompositor with enabled post-effects to the specified Game instance and sets it as the game's SceneSystem GraphicsCompositor.
    /// </summary>
    /// <param name="game">The Game instance to which the GraphicsCompositor will be added.</param>
    /// <returns>The newly configured GraphicsCompositor instance with enabled post-effects.</returns>
    public static GraphicsCompositor AddGraphicsCompositor(this Game game)
    {
        // Create a default GraphicsCompositor with enabled post-effects.
        var graphicsCompositor = GraphicsCompositorHelper.CreateDefault(true);

        // Set the GraphicsCompositor for the game's SceneSystem
        game.SceneSystem.GraphicsCompositor = graphicsCompositor;

        return graphicsCompositor;
    }

    /// <summary>
    /// Adds a camera entity to the game's root scene with customizable position and rotation, and default camera name "Main"
    /// </summary>
    /// <param name="game">The Game instance to which the camera entity will be added.</param>
    /// <param name="cameraName">Optional name for the camera entity and camera slot. If null, the entity will not be named.</param>
    /// <param name="initialPosition">Initial position for the camera entity. If null, the camera will be positioned at the default position (6, 6, 6).</param>
    /// <param name="initialRotation">Initial rotation for the camera entity specified in degrees. If null, the camera will be rotated to face towards the origin with default angles (Yaw: 45, Pitch: -30, Roll: 0).</param>
    /// <returns>The created Entity object representing the camera.</returns>
    /// <remarks>
    /// The camera entity will be created with a perspective projection mode and will be added to the game's root scene.
    /// It will also be assigned to the first available camera slot in the GraphicsCompositor.
    /// </remarks>
    public static Entity Add3DCamera(this Game game, string? cameraName = CameraDefaults.MainCameraName, Vector3? initialPosition = null, Vector3? initialRotation = null, CameraProjectionMode projectionMode = CameraProjectionMode.Perspective)
    {
        if (game.SceneSystem.GraphicsCompositor.Cameras.Count == 0)
        {
            throw new InvalidOperationException("Cannot add camera: The GraphicsCompositor does not have any camera slots defined.");
        }

        var cameraSlot = game.SceneSystem.GraphicsCompositor.Cameras[0];

        cameraSlot.Name = cameraName;

        initialPosition ??= CameraDefaults.Initial3DPosition;
        initialRotation ??= CameraDefaults.Initial3DRotation;

        var entity = new Entity(cameraName)
        {
            new CameraComponent
            {
                Projection = projectionMode,
                Slot =  game.SceneSystem.GraphicsCompositor.Cameras[0].ToSlotId(),
            }
        };

        entity.Transform.Position = initialPosition.Value;
        entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(
            MathUtil.DegreesToRadians(initialRotation.Value.X),
            MathUtil.DegreesToRadians(initialRotation.Value.Y),
            MathUtil.DegreesToRadians(initialRotation.Value.Z)
        );

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    public static Entity Add2DCamera(this Game game, string? cameraName = CameraDefaults.MainCameraName, Vector3? initialPosition = null, Vector3? initialRotation = null)
    {
        return game.Add3DCamera(
            cameraName,
            initialPosition ?? CameraDefaults.Initial2DPosition,
            initialRotation ?? CameraDefaults.Initial2DRotation,
            CameraProjectionMode.Orthographic);
    }

    /// <summary>
    /// Adds a directional light entity to the game's root scene with optional customization.
    /// </summary>
    /// <param name="game">The Game instance to which the directional light will be added.</param>
    /// <param name="entityName">Optional name for the new directional light entity. If null, the entity will not be named.</param>
    /// <returns>The created Entity object representing the directional light.</returns>
    /// <remarks>
    /// <para>
    /// This method creates a directional light with the following default settings:
    /// - Intensity: 20.0f
    /// - Position: (0, 2.0f, 0)
    /// - Rotation: X-axis rotated by -30 degrees and Y-axis rotated by -180 degrees.
    /// - Shadow Enabled: True
    /// - Shadow Size: Large
    /// - Shadow Filter: PCF (Percentage Closer Filtering) with a filter size of 5x5
    /// </para>
    /// <para>The entity will be added to the game's root scene. You can customize the light properties by accessing the returned Entity object.</para>
    /// </remarks>
    public static Entity AddDirectionalLight(this Game game, string? entityName = null)
    {
        var entity = new Entity(entityName)
        {
            new LightComponent
            {
                Intensity =  20.0f,
                Type = new LightDirectional
                {
                    Shadow =
                    {
                        Enabled = true,
                        Size = LightShadowMapSize.Large,
                        Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter5x5 },
                    }
                }
            }
        };

        entity.Transform.Position = new Vector3(0, 2.0f, 0);
        entity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-30.0f)) * Quaternion.RotationY(MathUtil.DegreesToRadians(-180.0f));

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    public static void AddAllDirectionLighting(this Game game, float intensity, bool showLightGizmo = true)
    {
        var position = new Vector3(7f, 2f, 0);

        CreateLightEntity(GetLight(), intensity, position);

        CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(180)));

        CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(270)));

        CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(90)));

        CreateLightEntity(GetLight(), intensity, position, Quaternion.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(270)));

        LightDirectional GetLight() => new() { Color = GetColor(Color.White) };

        static ColorRgbProvider GetColor(Color color) => new(color);

        void CreateLightEntity(ILight light, float intensity, Vector3 position, Quaternion? rotation = null)
        {
            var entity = new Entity() {
                new LightComponent {
                    Intensity =  intensity,
                    Type = light
                }};

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation ?? Quaternion.Identity;
            entity.Scene = game.SceneSystem.SceneInstance.RootScene;

            if (showLightGizmo)
            {
                entity.AddLightDirectionalGizmo(game.GraphicsDevice);
            }
        }
    }

    /// <summary>
    /// Adds a skybox to the specified game scene, providing a background texture to create a more immersive environment.
    /// </summary>
    /// <param name="game">The game instance to which the skybox will be added.</param>
    /// <param name="entityName">The name for the skybox entity. If null, a default name will be used.</param>
    /// <returns>The created skybox entity.</returns>
    /// <remarks>
    /// The skybox texture is loaded from the Resources folder, and is used to generate a skybox using the <see cref="SkyboxGenerator"/>.
    /// A new entity is created with a <see cref="BackgroundComponent"/> and a <see cref="LightComponent"/>, both configured for the skybox, and is added to the game scene.
    /// The default position of the skybox entity is set to (0.0f, 2.0f, -2.0f).
    /// </remarks>
    public static Entity AddSkybox(this Game game, string? entityName = null)
    {
        using var stream = new FileStream($"{AppContext.BaseDirectory}Resources\\{SkyboxTexture}", FileMode.Open, FileAccess.Read);

        var texture = Texture.Load(game.GraphicsDevice, stream, TextureFlags.ShaderResource, GraphicsResourceUsage.Dynamic);

        var skyboxGeneratorContext = new SkyboxGeneratorContext(game);

        var skybox = new Skybox();

        skybox = SkyboxGenerator.Generate(skybox, skyboxGeneratorContext, texture);

        var entity = new Entity(entityName) {
                new BackgroundComponent { Intensity = 1.0f, Texture = texture },
                new LightComponent {
                    Intensity = 1.0f,
                    Type = new LightSkybox() { Skybox = skybox } }
        };

        entity.Transform.Position = new Vector3(0.0f, 2.0f, -2.0f);

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    /// <summary>
    /// Adds a ground with default Size 10,10.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="entityName"></param>
    /// <param name="size"></param>
    /// <param name="includeCollider">Adds a collider</param>
    /// <returns></returns>
    public static Entity Add3DGround(this Game game, string? entityName = DefaultGroundName, Vector2? size = null, bool includeCollider = true)
        => CreateGround(game, entityName, size, includeCollider, PrimitiveModelType.Plane);

    public static Entity AddInfinite3DGround(this Game game, string? entityName = DefaultGroundName, Vector2? size = null, bool includeCollider = true)
        => CreateGround(game, entityName, size, includeCollider, PrimitiveModelType.InfinitePlane);

    public static Entity Add3DGroundWithBepu(this Game game, string? entityName = DefaultGroundName, Vector2? size = null, bool includeCollider = true)
        => CreateGroundWithBepu(game, entityName, size, includeCollider, PrimitiveModelType.Plane);

    private static Entity CreateGround(Game game, string? entityName, Vector2? size, bool includeCollider, PrimitiveModelType type)
    {
        var validSize = size ?? _default3DGroundSize;

        var material = game.CreateMaterial(_defaultGroundMaterialColor, 0.0f, 0.1f);

        var entity = game.CreatePrimitive(type, new()
        {
            EntityName = entityName,
            Material = material,
            IncludeCollider = includeCollider,
            Size = (Vector3)validSize,
            PhysicsComponent = new StaticColliderComponent()
        });

        // seems doing nothing
        //rigidBody.CcdMotionThreshold = 100;
        //rigidBody.CcdSweptSphereRadius = 100

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    private static Entity CreateGroundWithBepu(Game game, string? entityName, Vector2? size, bool includeCollider, PrimitiveModelType type)
    {
        var validSize = size ?? _default3DGroundSize;

        var material = game.CreateMaterial(_defaultGroundMaterialColor, 0.0f, 0.1f);

        var entity = game.CreatePrimitiveWithBepu(type, new Primitive3DCreationOptionsWithBepu()
        {
            EntityName = entityName,
            Material = material,
            IncludeCollider = includeCollider,
            Size = (Vector3)validSize,
            Component = new StaticComponent() { Collider = new CompoundCollider() }
        });

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    public static Entity Add2DGround(this Game game, string? entityName = DefaultGroundName, Vector2? size = null)
    {
        var validSize = size is null ? _default2DGroundSize : new Vector3(size.Value.X, size.Value.Y, 0);

        var material = game.CreateMaterial(_defaultGroundMaterialColor, 0.0f, 0.1f);

        var proceduralModel = Procedural3DModelBuilder.Build(PrimitiveModelType.Cube, validSize);
        var model = proceduralModel.Generate(game.Services);

        if (material != null)
        {
            model.Materials.Add(material);
        }

        var entity = new Entity(entityName) { new ModelComponent(model) };

        var collider = new StaticColliderComponent();

        //collider.ColliderShape = new StaticPlaneColliderShape(Vector3.UnitY, 0)
        //{
        //    LocalOffset = new Vector3(0, 0, 0),
        //};

        collider.ColliderShape = new BoxColliderShape(is2D: true, validSize)
        {
            LocalOffset = new Vector3(0, 0, 0),
        };

        entity.Add(collider);

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    public static void AddGroundGizmo(this Game game, Vector3? position = null, bool showAxisName = false, bool rotateAxisNames = true)
    {
        var groundEntity = game.SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(w => w.Name == DefaultGroundName);

        if (groundEntity is null) return;

        var gizmoEntity = new Entity("Gizmo");

        gizmoEntity.AddGizmo(game.GraphicsDevice, showAxisName: showAxisName, rotateAxisNames: rotateAxisNames);

        gizmoEntity.Transform.Position = position ?? Vector3.Zero;

        groundEntity.AddChild(gizmoEntity);
    }

    /// <summary>
    /// Gets the time elapsed since the last game update in seconds as a single-precision floating-point number.
    /// </summary>
    /// <param name="gameTime">The IGame interface providing access to game timing information.</param>
    /// <returns>The time elapsed since the last game update in seconds.</returns>
    public static float DeltaTime(this IGame gameTime)
    {
        return (float)gameTime.UpdateTime.Elapsed.TotalSeconds;
    }

    /// <summary>
    /// Gets the time elapsed since the last game update in seconds as a double-precision floating-point number.
    /// </summary>
    /// <param name="gameTime">The IGame interface providing access to game timing information.</param>
    /// <returns>The time elapsed since the last game update in seconds with double precision.</returns>
    public static double DeltaTimeAccurate(this IGame gameTime)
    {
        return gameTime.UpdateTime.Elapsed.TotalSeconds;
    }

    /// <summary>
    /// Adds a profiler to the game, which can be toggled on/off with Left Shift + Left Ctrl + P, and provides other keyboard shortcuts.
    /// Changing the filtering mode with F1, altering the sorting mode with F2, navigating result pages with F3 and F4,
    /// and adjusting the refresh interval with the plus and minus keys.
    /// </summary>
    /// <param name="game">The game to which the profiler will be added.</param>
    /// <param name="entityName">Optional name for the entity to which the <see cref="GameProfiler"/> script will be attached.</param>
    /// <returns>The entity to which the <see cref="GameProfiler"/> script was attached.</returns>
    /// <remarks>
    /// This extension method creates an entity and attaches a <see cref="GameProfiler"/> script to it, enabling in-game profiling.
    /// The profiler's behaviour can be interacted with using various keyboard shortcuts as described in the <see cref="GameProfiler"/> class.
    /// </remarks>
    public static Entity AddProfiler(this Game game, string? entityName = null)
    {
        var entity = new Entity(entityName) { new GameProfiler() };

        entity.Scene = game.SceneSystem.SceneInstance.RootScene;

        return entity;
    }

    /// <summary>
    /// Enables the visualization of collider shapes in the game scene. This feature is useful for debugging physics-related issues.
    /// </summary>
    /// <param name="game">The current game instance.</param>
    /// <remarks>
    /// This method activates the rendering of collider shapes within the physics simulation. It helps to visually inspect and debug the positioning and behaviour of colliders at runtime.
    /// </remarks>
    public static void ShowColliders(this Game game)
    {
        var simulation = game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;

        if (simulation is null) return;

        simulation.ColliderShapesRendering = true;
    }

    /// <summary>
    /// Creates a basic material with optional color, specular reflection, and microsurface smoothness values.
    /// </summary>
    /// <param name="game">The game instance used to access the graphics device.</param>
    /// <param name="color">The color of the material. Defaults to null, which will use the _defaultMaterialColor.</param>
    /// <param name="specular">The specular reflection factor of the material. Defaults to 1.0f.</param>
    /// <param name="microSurface">The microsurface smoothness value of the material. Defaults to 0.65f.</param>
    /// <returns>A new material instance with the specified or default attributes.</returns>
    public static Material CreateMaterial(this IGame game, Color? color = null, float specular = 1.0f, float microSurface = 0.65f)
    {
        var materialDescription = new MaterialDescriptor
        {
            Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color ?? _defaultMaterialColor)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Specular =  new MaterialMetalnessMapFeature(new ComputeFloat(specular)),
                    SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
                    MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(microSurface))
                }
        };

        return Material.New(game.GraphicsDevice, materialDescription);
    }

    /// <summary>
    /// Creates a primitive 3D model entity of the specified type with optional customizations.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="type">The type of primitive model to create.</param>
    /// <param name="options">The options for creating the primitive model. If null, default options are used.</param>
    /// <returns>A new entity representing the specified primitive model.</returns>
    /// <remarks>
    /// The <paramref name="options"/> parameter allows specifying various settings such as entity name, material,
    /// collider inclusion, size, render group, and 2D flag. Dimensions in the Vector3 for size are used in the order X, Y, Z.
    /// If size is null, default dimensions are used for the model. If no collider is included, the entity is returned without it.
    /// </remarks>
    public static Entity CreatePrimitive(this IGame game, PrimitiveModelType type, Primitive3DCreationOptions? options = null)
    {
        options ??= new();

        var proceduralModel = Procedural3DModelBuilder.Build(type, options.Size);

        var model = proceduralModel.Generate(game.Services);

        if (options.Material != null)
        {
            model.Materials.Add(options.Material);
        }

        var entity = new Entity(options.EntityName) { new ModelComponent(model) { RenderGroup = options.RenderGroup } };

        if (!options.IncludeCollider || options.PhysicsComponent is null) return entity;

        var colliderShape = Get3DColliderShape(type, options.Size, options.Is2D);

        if (colliderShape is null) return entity;

        options.PhysicsComponent.ColliderShapes.Add(colliderShape);

        entity.Add(options.PhysicsComponent);

        return entity;
    }

    public static Entity Create2DPrimitiveWithBepu(this IGame game, Primitive2DModelType type, Primitive2DCreationOptionsWithBepu? options = null)
    {
        options ??= new();

        var proceduralModel = Procedural2DModelBuilder.Build(type, options.Size, options.Depth);

        var model = proceduralModel.Generate(game.Services);

        if (options.Material != null)
        {
            model.Materials.Add(options.Material);
        }

        var entity = new Entity(options.EntityName) { new ModelComponent(model) { RenderGroup = options.RenderGroup } };

        if (!options.IncludeCollider) return entity;

        var colliderShape = Get2DColliderShapeWithBepu(type, options.Size);

        if (colliderShape is null) return entity;

        var compoundCollier = options.Component.Collider as CompoundCollider;

        compoundCollier.Colliders.Add(colliderShape);

        entity.Add(options.Component);

        return entity;
    }

    public static Entity CreatePrimitiveWithBepu(this IGame game, PrimitiveModelType type, Primitive3DCreationOptionsWithBepu? options = null)
    {
        options ??= new();

        var proceduralModel = Procedural3DModelBuilder.Build(type, options.Size);

        var model = proceduralModel.Generate(game.Services);

        if (options.Material != null)
        {
            model.Materials.Add(options.Material);
        }

        var entity = new Entity(options.EntityName) { new ModelComponent(model) { RenderGroup = options.RenderGroup } };

        if (!options.IncludeCollider) return entity;

        var colliderShape = Get3DColliderShapeWithBepu(type, options.Size);

        if (colliderShape is null) return entity;

        var compoundCollier = options.Component.Collider as CompoundCollider;

        compoundCollier.Colliders.Add(colliderShape);

        entity.Add(options.Component);

        return entity;
    }

    private static ColliderBase? Get2DColliderShapeWithBepu(Primitive2DModelType type, Vector2? size = null, float depth = 0)
        => type switch
        {
            Primitive2DModelType.Rectangle => size is null ? new BoxCollider() : new() { Size = new(size.Value.X, size.Value.Y, depth) },
            Primitive2DModelType.Square => size is null ? new BoxCollider() : new() { Size = new(size.Value.X, size.Value.Y, depth) },
            Primitive2DModelType.Circle => size is null ? new SphereCollider() : new() { Radius = size.Value.X },
            _ => throw new InvalidOperationException(),
        };

    private static ColliderBase? Get3DColliderShapeWithBepu(PrimitiveModelType type, Vector3? size = null)
    => type switch
    {
        PrimitiveModelType.Plane => size is null ? new BoxCollider() : new() { Size = new Vector3(size.Value.X, 0, size.Value.Y) },
        PrimitiveModelType.Cube => size is null ? new BoxCollider() : new() { Size = size ?? Vector3.One },
        _ => throw new InvalidOperationException(),
    };

    public static Entity Create2DPrimitive(this IGame game, Primitive2DModelType type, Primitive2DCreationOptions? options = null)
    {
        options ??= new();

        var proceduralModel = Procedural2DModelBuilder.Build(type, options.Size, options.Depth);

        //proceduralModel.SetMaterial("Material", options.Material);

        var model = proceduralModel.Generate(game.Services);

        //model.Add(options.Material);

        if (options.Material != null)
        {
            model.Materials.Add(options.Material);
        }

        var entity = new Entity(options.EntityName) { new ModelComponent(model) { RenderGroup = options.RenderGroup } };

        if (type == Primitive2DModelType.Circle)
        {
            entity.Transform.Rotation = Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(90));
        }

        if (!options.IncludeCollider || options.PhysicsComponent is null) return entity;

        if (type == Primitive2DModelType.Triangle)
        {
            //var a = new TriangularPrismProceduralModel() { Size = new(options.Size.Value.X, options.Size.Value.Y, options.Depth) };

            var meshData = TriangularPrismProceduralModel.New(options.Size is null ? new(1, 1, options.Depth) : new(options.Size.Value.X, options.Size.Value.Y, options.Depth));

            var points = meshData.Vertices.Select(w => w.Position).ToList();
            var uintIndices = meshData.Indices.Select(w => (uint)w).ToList();
            var collider = new ConvexHullColliderShapeDesc()
            {
                //Model = model, // seems doing nothing
                Scaling = new(0.9f),
                //LocalOffset = new(20, 20, 10),
                ConvexHulls = [],
                ConvexHullsIndices = []
            };

            collider.ConvexHulls.Add([points]);
            collider.ConvexHullsIndices.Add([uintIndices]);

            //var shapee = collider.CreateShape(game.Services);
            //var collider = new ConvexHullColliderShape(points, uintIndices, Vector3.Zero);
            //var cs = new PhysicsColliderShape(descriptions);


            List<IAssetColliderShapeDesc> descriptions = [];

            descriptions.Add(collider);

            var colliderShapeAsset = new ColliderShapeAssetDesc
            {
                Shape = new PhysicsColliderShape(descriptions)
            };

            options.PhysicsComponent.ColliderShapes.Add(colliderShapeAsset);
            //options.PhysicsComponent.ColliderShape = shapee;
            //options.PhysicsComponent.ColliderShape = collider;
        }
        else
        {
            var colliderShape = Get2DColliderShape(type, options.Size, options.Depth);

            if (colliderShape is null) return entity;

            options.PhysicsComponent.ColliderShapes.Add(colliderShape);
        }

        entity.Add(options.PhysicsComponent);

        return entity;
    }

    /// <summary>
    /// Sets the maximum frames per second (FPS) rate for the game.
    /// Set <param name="targetFPS"/> to 0 for max possible FPS.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="targetFPS"></param>
    public static void SetMaxFPS(this IGame game, int targetFPS)
    {
        var gameBase = (GameBase)game;
        gameBase.WindowMinimumUpdateRate.MinimumElapsedTime = TimeSpan.FromMilliseconds(1000 / targetFPS);
    }

    private static IInlineColliderShapeDesc? Get2DColliderShape(Primitive2DModelType type, Vector2? size = null, float depth = 0)
        => type switch
        {
            Primitive2DModelType.Rectangle => size is null ? new BoxColliderShapeDesc() { Is2D = true } : new() { Size = new(size.Value.X, size.Value.Y, 0), Is2D = true },
            Primitive2DModelType.Square => size is null ? new BoxColliderShapeDesc() { Is2D = true } : new() { Size = new(size.Value.X, size.Value.Y, 0), Is2D = true },
            Primitive2DModelType.Circle => size is null ? new SphereColliderShapeDesc() : new() { Radius = size.Value.X, Is2D = true },
            _ => throw new InvalidOperationException(),
        };

    private static IInlineColliderShapeDesc? Get3DColliderShape(PrimitiveModelType type, Vector3? size = null, bool is2D = false)
        => type switch
        {
            PrimitiveModelType.Plane => size is null ? new BoxColliderShapeDesc() : new() { Size = new Vector3(size.Value.X, 0, size.Value.Y) },
            PrimitiveModelType.InfinitePlane => new StaticPlaneColliderShapeDesc(),
            PrimitiveModelType.Sphere => size is null ? new SphereColliderShapeDesc() : new() { Radius = size.Value.X, Is2D = is2D },
            PrimitiveModelType.Cube => size is null ? new BoxColliderShapeDesc() : new() { Size = size ?? Vector3.One, Is2D = is2D },
            PrimitiveModelType.Cylinder => size is null ? new CylinderColliderShapeDesc() : new() { Radius = size.Value.X, Height = size.Value.Y },
            PrimitiveModelType.Torus => null,
            PrimitiveModelType.Teapot => null,
            PrimitiveModelType.Cone => size is null ? new ConeColliderShapeDesc() : new() { Radius = size.Value.X, Height = size.Value.Y },
            PrimitiveModelType.Capsule => size is null ? new CapsuleColliderShapeDesc() { Radius = 0.35f } : new() { Radius = size.Value.X, Length = size.Value.Y, Is2D = is2D },
            _ => throw new InvalidOperationException(),
        };

    /// <summary>
    /// Retrieves the current frames per second (FPS) rate of the running game.
    /// </summary>
    /// <param name="game">The game instance from which to obtain the FPS rate.</param>
    /// <returns>The current FPS rate of the game.</returns>
    public static float FPS(this Game game) => game.UpdateTime.FramePerSecond;
}

/// <summary>
/// Temporary workaround for BoxColliderShape not working with 2D
/// </summary>
public class BoxColliderShapeX4 : ColliderShape
{
    public readonly Vector3 BoxSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoxColliderShape"/> class.
    /// </summary>
    /// <param name="is2D">If this cube is a 2D quad</param>
    /// <param name="size">The size of the cube</param>
    public BoxColliderShapeX4(bool is2D, Vector3 size)
    {
        Type = ColliderShapeTypes.Box;
        BoxSize = size;

        // Use reflection to set internal properties
        SetInternalProperties(is2D, size);

        cachedScaling = Is2D ? new Vector3(1, 1, 0.001f) : Vector3.One;
    }

    private void SetInternalProperties(bool is2D, Vector3 size)
    {
        // Set the Is2D property using reflection
        //var is2DProperty = typeof(ColliderShape).GetProperty("Is2D", BindingFlags.NonPublic | BindingFlags.Instance);
        var is2DProperty = typeof(ColliderShape).GetProperty("Is2D");

        if (is2DProperty != null)
        {
            is2DProperty.SetValue(this, is2D);
        }

        // Set the InternalShape property using reflection
        var internalShapeField = typeof(ColliderShape).GetField("InternalShape", BindingFlags.NonPublic | BindingFlags.Instance);
        if (internalShapeField != null)
        {
            BulletSharp.CollisionShape internalShape;

            if (is2D)
            {
                size.Z = 0.001f; // Adjust the Z size for 2D
                internalShape = new BulletSharp.Convex2DShape(new BulletSharp.Box2DShape(size / 2) { LocalScaling = Vector3.One }) { LocalScaling = cachedScaling };
            }
            else
            {
                internalShape = new BulletSharp.BoxShape(size / 2) { LocalScaling = cachedScaling };
            }

            internalShapeField.SetValue(this, internalShape);
        }
    }

    public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
    {
        return GeometricPrimitive.Cube.New(device).ToMeshDraw();
    }
}