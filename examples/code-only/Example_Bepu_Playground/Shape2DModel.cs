using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Rendering;

namespace Example_Bepu_Playground;

public class Shape2DModel
{
    public required Primitive2DModelType Type { get; set; }
    public required Color Color { get; set; }
    public required Vector2 Size { get; set; }
    public Model? Model { get; set; }
}