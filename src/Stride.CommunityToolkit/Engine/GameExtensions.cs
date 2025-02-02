using Stride.CommunityToolkit.Rendering.Compositing;
using Stride.CommunityToolkit.Rendering.DebugShapes;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.CommunityToolkit.Scripts;
using Stride.Engine;
using Stride.Engine.Processors;
using Stride.Games;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering;
using Stride.Rendering.Colors;
using Stride.Rendering.Compositing;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;

namespace Stride.CommunityToolkit.Engine;

/// <summary>
/// Provides extension methods for the <see cref="Game"/> class to simplify common game setup tasks,
/// such as adding cameras, lights, and ground entities, as well as configuring scenes
/// and running the game with custom logic.
/// </summary>
public static class GameExtensions
{
    private const string DefaultGroundName = "Ground";
    private const string GraphicsCompositorNotSet = "GraphicsCompositor is not set.";

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
    /// Sets up essential components for the game, including a GraphicsCompositor, a camera, and a directional light.
    /// </summary>
    /// <remarks>
    /// This method performs the following operations:
    /// <list type="bullet">
    /// <item><description>Adds a default GraphicsCompositor to the game's SceneSystem and applies a clean UI stage.</description></item>
    /// <item><description>Adds a camera to the game.</description></item>
    /// <item><description>Adds a directional light to the game.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="game">The Game instance that will receive the base setup.</param>
    public static void SetupBase(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add3DCamera();
        game.AddDirectionalLight();
    }

    public static void SetupBase2DScene(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add2DCamera().Add2DCameraController();
        //game.AddDirectionalLight();
        game.Add2DGround();
    }

    /// <summary>
    /// Sets up a default 3D scene for the game, similar to creating an empty project through the editor.
    /// </summary>
    /// <remarks>
    /// This method performs the following setup operations in sequence:
    /// 1. Adds a default GraphicsCompositor to the game's SceneSystem and applies a clean UI stage.
    /// 2. Adds a camera to the game and sets it up with a MouseLookCamera component.
    /// 3. Adds a directional light to the game scene.
    /// 4. Adds ground geometry to the game scene.
    /// </remarks>
    /// <param name="game">The Game instance for which the base 3D scene will be set up.</param>
    public static void SetupBase3DScene(this Game game)
    {
        game.AddGraphicsCompositor().AddCleanUIStage();
        game.Add3DCamera().Add3DCameraController();
        game.AddDirectionalLight();
        game.Add3DGround();
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
    /// Adds a 2D camera entity to the game's root scene with customizable position and rotation, defaulting to orthographic projection.
    /// </summary>
    /// <param name="game">The Game instance to which the camera entity will be added.</param>
    /// <param name="cameraName">Optional name for the camera entity and camera slot. Defaults to "MainCamera" if not provided. If null, the entity will not be named.</param>
    /// <param name="initialPosition">Initial position for the camera entity. If not provided, the camera will be positioned at a default 2D position.</param>
    /// <param name="initialRotation">Initial rotation for the camera entity specified in degrees. If not provided, the camera will be rotated to the default 2D orientation.</param>
    /// <returns>The created Entity object representing the 2D camera.</returns>
    /// <remarks>
    /// The camera entity will be created with an orthographic projection mode and added to the game's root scene. It will also be assigned to the first available camera slot in the GraphicsCompositor.
    /// </remarks>
    public static Entity Add2DCamera(this Game game, string? cameraName = CameraDefaults.MainCameraName, Vector3? initialPosition = null, Vector3? initialRotation = null)
    {
        return game.Add3DCamera(
            cameraName,
            initialPosition ?? CameraDefaults.Initial2DPosition,
            initialRotation ?? CameraDefaults.Initial2DRotation,
            CameraProjectionMode.Orthographic);
    }

    /// <summary>
    /// Adds a 3D camera entity to the game's root scene with customizable position, rotation, projection mode and default camera name "Main".
    /// </summary>
    /// <param name="game">The Game instance to which the camera entity will be added.</param>
    /// <param name="cameraName">Optional name for the camera entity and camera slot. Defaults to "MainCamera" if not provided. If null, the entity will not be named.</param>
    /// <param name="initialPosition">Initial position for the camera entity. If not provided, the camera will be positioned at a default 3D position (6, 6, 6).</param>
    /// <param name="initialRotation">Initial rotation for the camera entity specified in degrees. If not provided, the camera will be rotated to face towards the origin with default angles (Yaw: 45, Pitch: -30, Roll: 0).</param>
    /// <param name="projectionMode">The projection mode for the camera (Perspective or Orthographic). Defaults to Perspective.</param>
    /// <returns>The created Entity object representing the 3D camera.</returns>
    /// <remarks>
    /// The camera entity will be created with the specified projection mode and added to the game's root scene. It will also be assigned to the first available camera slot in the GraphicsCompositor.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the GraphicsCompositor does not have any camera slots defined.</exception>
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
    public static Entity AddDirectionalLight(this Game game, string? entityName = "Directional Light")
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

    /// <summary>
    /// Adds directional lighting from multiple angles to the current scene, enhancing scene illumination.
    /// </summary>
    /// <param name="game">The game instance to which the lighting will be added.</param>
    /// <param name="intensity">The intensity of the light sources.</param>
    /// <param name="showLightGizmo">Specifies whether to display a gizmo for the light in the editor. Default is true.</param>
    /// <remarks>
    /// This method creates six directional lights positioned around a central point, each aiming from a unique angle to simulate uniform lighting from all directions.
    /// The lights are added at predefined positions and rotations to cover the scene evenly.
    /// </remarks>
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

    /// <summary>
    /// Adds a 3D ground entity to the game with a default size of 10x10 units. The ground is created as a plane, and a collider can be optionally added.
    /// </summary>
    /// <param name="game">The Game instance to which the ground entity will be added.</param>
    /// <param name="entityName">The optional name for the ground entity. If not provided, it defaults to "Ground".</param>
    /// <param name="size">The size of the ground, specified as a 2D vector. If not provided, it defaults to (10, 10) units.</param>
    /// <param name="includeCollider">Specifies whether to add a collider to the ground. Defaults to true.</param>
    /// <returns>The created Entity object representing the 3D ground.</returns>
    public static Entity Add3DGround(this Game game, string? entityName = DefaultGroundName, Vector2? size = null, bool includeCollider = true)
        => CreateGround(game, entityName, size, includeCollider, PrimitiveModelType.Plane);

    /// <summary>
    /// Adds an infinite 3D ground entity to the game. The visible part of the ground is defined by the <paramref name="size"/> parameter,
    /// while the collider is infinite and extends beyond the visible ground.
    /// </summary>
    /// <param name="game">The Game instance to which the infinite ground entity will be added.</param>
    /// <param name="entityName">The optional name for the ground entity. If not provided, it defaults to "Ground".</param>
    /// <param name="size">Defines the visible part of the ground, specified as a 2D vector. If not provided, it defaults to (10, 10) units.</param>
    /// <param name="includeCollider">Specifies whether to add a collider to the ground. The collider is infinite, extending beyond the visible part. Defaults to true.</param>
    /// <returns>The created Entity object representing the infinite 3D ground.</returns>
    public static Entity AddInfinite3DGround(this Game game, string? entityName = DefaultGroundName, Vector2? size = null, bool includeCollider = true)
        => CreateGround(game, entityName, size, includeCollider, PrimitiveModelType.InfinitePlane);

    private static Entity CreateGround(Game game, string? entityName, Vector2? size, bool includeCollider, PrimitiveModelType type)
    {
        var validSize = size ?? _default3DGroundSize;

        var material = game.CreateMaterial(_defaultGroundMaterialColor, 0.0f, 0.1f);

        var entity = game.Create3DPrimitive(type, new()
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

    /// <summary>
    /// Adds a ground gizmo to the game's root scene, attached to an existing ground entity.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> instance in which the ground gizmo will be added.</param>
    /// <param name="position">
    /// The position of the gizmo in 3D space. If null, the gizmo will be placed at the origin (0, 0, 0).
    /// </param>
    /// <param name="showAxisName">
    /// A boolean indicating whether the axis names (X, Y, Z) should be displayed on the gizmo. Default is false.
    /// </param>
    /// <param name="rotateAxisNames">
    /// A boolean indicating whether the axis names should rotate to always face the camera. Default is true.
    /// </param>
    /// <remarks>
    /// The gizmo is added as a child to an existing ground entity. If the ground entity is not found, the method will return without adding the gizmo.
    /// The gizmo helps visualize the ground with axis indicators in 3D space.
    /// </remarks>
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
    public static Entity AddProfiler(this Game game, string? entityName = "Game Profiler")
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

    public static Entity Create2DPrimitive(this IGame game, Primitive2DModelType type, Primitive2DCreationOptions? options = null)
    {
        options ??= new();

        var modelBase = Procedural2DModelBuilder.Build(type, options.Size, options.Depth);

        //proceduralModel.SetMaterial("Material", options.Material);

        var model = modelBase.Generate(game.Services);

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
    public static Entity Create3DPrimitive(this IGame game, PrimitiveModelType type, Primitive3DCreationOptions? options = null)
    {
        options ??= new();

        var modelBase = Procedural3DModelBuilder.Build(type, options.Size);

        var model = modelBase.Generate(game.Services);

        if (options.Material != null)
        {
            model.Materials.Add(options.Material);
        }

        var entity = new Entity(options.EntityName) { new ModelComponent(model) { RenderGroup = options.RenderGroup } };

        if (!options.IncludeCollider || options.PhysicsComponent is null) return entity;

        var colliderShape = Get3DColliderShape(type, options.Size);

        if (colliderShape is null) return entity;

        options.PhysicsComponent.ColliderShapes.Add(colliderShape);

        entity.Add(options.PhysicsComponent);

        return entity;
    }

    /// <summary>
    /// Saves a screenshot of the current frame to the specified file path.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="fileName">creates a file with this name</param>
    /// <param name="fileType"></param>
    public static void TakeScreenShot(this IGame game, string fileName, ImageFileType fileType = ImageFileType.Png)
    {
        using (var stream = File.Create(fileName))
        {
            var commandList = game.GraphicsContext.CommandList;
            commandList.RenderTarget.Save(commandList, stream, fileType);
        }
    }

    /// <summary>
    /// Adds a scene renderer to the game's <see cref="GraphicsCompositor"/>.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> instance to add the renderer to.</param>
    /// <param name="renderer">The scene renderer to be added, inheriting from <see cref="SceneRendererBase"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GraphicsCompositor"/> is not set in the game's <see cref="SceneSystem"/>.</exception>
    public static void AddSceneRenderer(this Game game, SceneRendererBase renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        var graphicsCompositor = game.SceneSystem.GraphicsCompositor ?? throw new InvalidOperationException(GraphicsCompositorNotSet);

        graphicsCompositor.AddSceneRenderer(renderer);
    }

    /// <summary>
    /// <para>Adds <see cref="ImmediateDebugRenderFeature"/> and <see cref="ImmediateDebugRenderSystem"/> to the game.</para>
    /// <para>Registers the system to the service registry for easy access.</para>
    /// </summary>
    /// <param name="game"></param>
    /// <param name="debugShapeRenderGroup"></param>
    public static void AddDebugShapes(this Game game, RenderGroup debugShapeRenderGroup = RenderGroup.Group1)
    {
        game.SceneSystem.GraphicsCompositor.AddImmediateDebugRenderFeature();

        var debugDraw = new ImmediateDebugRenderSystem(game.Services, debugShapeRenderGroup);
#if DEBUG
        debugDraw.Visible = true;
#endif
        game.Services.AddService(debugDraw);
        game.GameSystems.Add(debugDraw);
    }

    public static void AddParticleRenderer(this Game game)
    {
        game.SceneSystem.GraphicsCompositor.AddParticleStagesAndFeatures();
    }

    /// <summary>
    /// Adds an <see cref="EntityDebugSceneRenderer"/> to the game's <see cref="GraphicsCompositor"/> for rendering entity debug information.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> instance to which the entity debug renderer will be added.</param>
    /// <param name="options">Optional settings to customize the appearance of the debug renderer. If not provided, default options will be used.</param>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GraphicsCompositor"/> is not set in the game's <see cref="SceneSystem"/>.</exception>
    /// <remarks>
    /// This method adds a custom <see cref="EntityDebugSceneRenderer"/> to the game's graphics compositor, allowing the display of debug information
    /// such as entity names and positions in a 3D scene. The renderer can be customized using the <paramref name="options"/> parameter,
    /// which allows the user to define font size, color, and other settings.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to add an entity debug renderer with default settings:
    /// <code>
    /// game.EntityDebugSceneRenderer();
    /// </code>
    /// You can also specify custom options:
    /// <code>
    /// var options = new EntityDebugRendererOptions { FontSize = 16, FontColor = Color.Red };
    /// game.EntityDebugSceneRenderer(options);
    /// </code>
    /// </example>
    public static void AddEntityDebugSceneRenderer(this Game game, EntityDebugSceneRendererOptions? options = null)
    {
        var graphicsCompositor = game.SceneSystem.GraphicsCompositor ?? throw new InvalidOperationException(GraphicsCompositorNotSet);

        graphicsCompositor.AddEntityDebugRenderer(options);
    }

    private static IInlineColliderShapeDesc? Get2DColliderShape(Primitive2DModelType type, Vector2? size = null, float depth = 0)
        => type switch
        {
            Primitive2DModelType.Rectangle => size is null ? new BoxColliderShapeDesc() { Is2D = true } : new() { Size = new(size.Value.X, size.Value.Y, 0), Is2D = true },
            Primitive2DModelType.Square => size is null ? new BoxColliderShapeDesc() { Is2D = true } : new() { Size = new(size.Value.X, size.Value.Y, 0), Is2D = true },
            Primitive2DModelType.Circle => size is null ? new SphereColliderShapeDesc() : new() { Radius = size.Value.X, Is2D = true },
            Primitive2DModelType.Capsule => size is null ? new CapsuleColliderShapeDesc() : new() { Radius = size.Value.X, Is2D = true },
            _ => throw new InvalidOperationException(),
        };

    private static IInlineColliderShapeDesc? Get3DColliderShape(PrimitiveModelType type, Vector3? size = null)
        => type switch
        {
            PrimitiveModelType.Plane => size is null ? new BoxColliderShapeDesc() : new() { Size = new Vector3(size.Value.X, 0, size.Value.Y) },
            PrimitiveModelType.InfinitePlane => new StaticPlaneColliderShapeDesc(),
            PrimitiveModelType.Sphere => size is null ? new SphereColliderShapeDesc() : new() { Radius = size.Value.X },
            PrimitiveModelType.Cube => size is null ? new BoxColliderShapeDesc() : new() { Size = size ?? Vector3.One },
            PrimitiveModelType.Cylinder => size is null ? new CylinderColliderShapeDesc() : new() { Radius = size.Value.X, Height = size.Value.Y },
            PrimitiveModelType.Torus => null,
            PrimitiveModelType.Teapot => null,
            PrimitiveModelType.Cone => size is null ? new ConeColliderShapeDesc() : new() { Radius = size.Value.X, Height = size.Value.Y },
            PrimitiveModelType.Capsule => size is null ? new CapsuleColliderShapeDesc() { Radius = 0.35f } : new() { Radius = size.Value.X, Length = size.Value.Y },
            _ => throw new InvalidOperationException(),
        };
}