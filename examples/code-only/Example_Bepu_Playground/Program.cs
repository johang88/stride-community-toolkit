using Example_Bepu_Playground;
using Stride.BepuPhysics;
using Stride.BepuPhysics._2D;
using Stride.BepuPhysics.Definitions.Colliders;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Input;
using Stride.Physics;
using Stride.Rendering;

var shift = new Vector3(0, 0, 0);
var boxSize = new Vector3(0.2f, 0.2f, 0.04f);
var groundSize = new Vector3(20, 0.01f, 20);
var box2DSize = new Vector2(0.2f, 0.2f);
var rectangleSize = new Vector3(0.2f, 0.3f, 0);
int cubes = 0;
int debugX = 5;
int debugY = 30;
var bgImage = "JumpyJetBackground.jpg";
const string ShapeName = "Shape";
const float Depth = 0.04f;
Model? model = null;
Scene scene = new();

List<Shape2DModel> shapes = [
    new() { Type = Primitive2DModelType.Square, Color = Color.Green, Size = (Vector2)boxSize },
    new() { Type = Primitive2DModelType.Rectangle, Color = Color.Orange, Size = (Vector2)rectangleSize },
    new() { Type = Primitive2DModelType.Circle, Color = Color.Red, Size = (Vector2)boxSize / 2 },
    new() { Type = Primitive2DModelType.Triangle, Color = Color.Purple, Size = (Vector2)boxSize }
];

using var game = new Game();

game.Run(start: (Action<Scene>?)((Scene rootScene) =>
{
    scene = rootScene;

    game.Window.AllowUserResizing = true;
    game.Window.Title = "2D Example";

    game.SetupBase3DSceneWithBepu();
    game.AddProfiler();
    game.AddAllDirectionLighting(intensity: 5f, true);
    //game.ShowColliders();

    //var simulation = game.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;
    //simulation.FixedTimeStep = 1f / 90;

    var simulation2DEntity = new Entity("Simulation2D")
    {
        new Simulation2DComponent()
    };
    simulation2DEntity.Scene = rootScene;
    simulation2DEntity.AddGizmo(game.GraphicsDevice, showAxisName: true);

    var entity = game.CreatePrimitive(PrimitiveModelType.Capsule);
    entity.Transform.Position = new Vector3(0, 20, 0);
    entity.Scene = rootScene;

    //var groundProceduralModel = Procedural3DModelBuilder.Build(PrimitiveModelType.Cube, groundSize);
    //var groundModel = groundProceduralModel.Generate(game.Services);

    //var ground = new Entity("Ground")
    //{
    //    new ModelComponent(groundModel) { RenderGroup = RenderGroup.Group0 },
    //    new StaticComponent() {
    //        Collider = new CompoundCollider {
    //            Colliders = { new BoxCollider() { Size = groundSize } }
    //        }
    //    }
    //};

    //ground.Transform.Position = new Vector3(0, 2, 0) + shift;
    //ground.AddGizmo(game.GraphicsDevice, showAxisName: true);
    //ground.Scene = rootScene;

    //var shape = Procedural2DModelBuilder.Build(Primitive2DModelType.Circle, box2DSize, Depth);
    var shape = Procedural2DModelBuilder.Build(Primitive2DModelType.Square, box2DSize, Depth);
    model = shape.Generate(game.Services);

    GenerateCubes(rootScene, shift, model);

}), update: Update);

void Update(Scene scene, GameTime time)
{
    if (game.Input.IsKeyDown(Keys.Space))
    {
        GenerateCubes(scene, shift, model);

        SetCubeCount(scene);
    }

    if (game.Input.IsKeyPressed(Keys.M))
    {
        Add2DShapes(Primitive2DModelType.Square, 10);

        SetCubeCount(scene);
    }
    else if (game.Input.IsKeyPressed(Keys.R))
    {
        Add2DShapes(Primitive2DModelType.Rectangle, 10);

        SetCubeCount(scene);
    }
    else if (game.Input.IsKeyPressed(Keys.C))
    {
        Add2DShapes(Primitive2DModelType.Circle, 10);

        SetCubeCount(scene);
    }
    else if (game.Input.IsKeyReleased(Keys.X))
    {
        foreach (var entity in scene.Entities.Where(w => w.Name == "BepuCube").ToList())
        {
            entity.Remove();
        }

        SetCubeCount(scene);
    }

    RenderNavigation();
}

void Add2DShapes(Primitive2DModelType? type = null, int count = 5)
{
    //var entity = new Entity();

    for (int i = 1; i <= count; i++)
    {
        var shapeModel = GetShape(type);

        if (shapeModel == null) return;

        var entity = game.Create2DPrimitiveWithBepu(shapeModel.Type, new() { Size = shapeModel.Size, Material = game.CreateMaterial(shapeModel.Color) });

        //if (type == null || i == 1)
        //{
        //    entity = game.Create2DPrimitiveWithBepu(shapeModel.Type, new() { Size = shapeModel.Size, Material = game.CreateMaterial(shapeModel.Color) });
        //}
        //else
        //{
        //    entity = entity.Clone();
        //}

        entity.Name = ShapeName;
        entity.Transform.Position = GetRandomPosition();
        entity.Scene = scene;

        AddAngularAndLinearFactor(shapeModel.Type, entity);
    }

    static void AddAngularAndLinearFactor(Primitive2DModelType? type, Entity entity)
    {
        if (type != Primitive2DModelType.Triangle) return;

        var rigidBody = entity.Get<RigidbodyComponent>();
        rigidBody.AngularFactor = new Vector3(0, 0, 1);
        rigidBody.LinearFactor = new Vector3(1, 1, 0);

        // seems doing nothing
        //rigidBody.CcdMotionThreshold = 10000;
        //rigidBody.CcdSweptSphereRadius = 10000;
    }
}

Shape2DModel? GetShape(Primitive2DModelType? type = null)
{
    if (type == null)
    {
        int randomIndex = Random.Shared.Next(shapes.Count);

        return shapes[randomIndex];
    }

    return shapes.Find(x => x.Type == type);
}

void GenerateCubes(Scene rootScene, Vector3 shift, Model model)
{
    var rotationLock = new Vector3(0, 0, 1);

    for (int i = 0; i < 10; i++)
    {
        var entity2 = new Entity("BepuCube") {
            new ModelComponent(model) { RenderGroup = RenderGroup.Group0 }
        };
        entity2.Transform.Position = new Vector3(Random.Shared.Next(-5, 5), Random.Shared.Next(10, 30), 0) + shift;
        //entity2.Transform.Rotation = Quaternion.Multiply(
        //    Quaternion.RotationZ(MathUtil.PiOverTwo),
        //    Quaternion.RotationY(MathUtil.PiOverTwo)
        //);

        var component = new Body2DComponent()
        {
            Collider = new CompoundCollider() { Colliders = { new BoxCollider() { Size = boxSize } } },
            //Collider = new CompoundCollider()
            //{
            //    Colliders = { new CylinderCollider() {
            //        Radius = boxSize.X,
            //        Length = Depth,
            //        Mass = 1,
            //        //RotationLocal = Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(90)),
            //        PositionLocal = new Vector3(box2DSize.X, box2DSize.X, 0)

            //        }
            //    }
            //},
            //SimulationIndex = 2,
            //SpringFrequency = 30,
            //SpringDampingRatio = 3.0f,
            //FrictionCoefficient = 1,
            //MaximumRecoveryVelocity = 1000,
            //Kinematic = false,
            //IgnoreGlobalGravity = false,
            //SleepThreshold = 0.01f,
            //MinimumTimestepCountUnderThreshold = 32,
            //InterpolationMode = InterpolationMode.None,
            //ContinuousDetectionMode = ContinuousDetectionMode.Discrete,
        };

        entity2.Add(component);
        //entity2.Add(new LinearAxisLimitConstraintComponent()
        //{
        //    Enabled = true,
        //    TargetVelocity = new Vector3(0, 10, 0),
        //    MotorDamping = 50,
        //    MotorMaximumForce = 10000000,
        //});
        entity2.Scene = rootScene;

        //component.LinearVelocity *= new Vector3(1, 1, 0);
        ////component.Position *= new Vector3(1, 1, 0);
        //component.AngularVelocity *= new Vector3(0, 0, 1);
    }
}

void SetCubeCount(Scene scene) => cubes = scene.Entities.Where(w => w.Name == "BepuCube").Count();

void RenderNavigation()
{
    var space = 0;
    game.DebugTextSystem.Print($"Cubes: {cubes}", new Int2(x: debugX, y: debugY));
    space += 30;
    game.DebugTextSystem.Print($"X - Delete all cubes and shapes", new Int2(x: debugX, y: debugY + space), Color.Red);
    space += 20;
    game.DebugTextSystem.Print($"M - generate 2D squares", new Int2(x: debugX, y: debugY + space));
    space += 20;
    game.DebugTextSystem.Print($"R - generate 2D rectangles", new Int2(x: debugX, y: debugY + space));
    space += 20;
    game.DebugTextSystem.Print($"C - generate 2D circles", new Int2(x: debugX, y: debugY + space));
    //space += 20;
    //game.DebugTextSystem.Print($"T - generate 2D triangles", new Int2(x: debugX, y: debugY + space));
    //space += 20;
    //game.DebugTextSystem.Print($"P - generate random 2D shapes", new Int2(x: debugX, y: debugY + space));
}

static Vector3 GetRandomPosition() => new(Random.Shared.Next(-5, 5), 3 + Random.Shared.Next(0, 7), 0);