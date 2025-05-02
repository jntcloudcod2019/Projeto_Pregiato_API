using Pregiato.API.System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    public class ModelDnaData
    {
        [JsonPropertyName("DNA")]
        public string? Dna { get; set; } = "InfoModel";

        [JsonPropertyName("APPEARANCE")]
        public Appearance? Appearance { get; set; }

        [JsonPropertyName("EYEATTRIBUTES")]
        public EyeAttributes? EyeAttributes { get; set; }

        [JsonPropertyName("HAIRATTRIBUTES")]
        public HairAttributes? HairAttributes { get; set; }

        [JsonPropertyName("SKINATTRIBUTES")]
        public SkinAttributes? SkinAttributes { get; set; }

        [JsonPropertyName("FACEATTRIBUTES")]
        public FaceAttributes? FaceAttributes { get; set; }

        [JsonPropertyName("SMILEATTRIBUTES")]
        public SmileAttributes? SmileAttributes { get; set; }

        [JsonPropertyName("BODYATTRIBUTES")]
        public BodyAttributes? BodyAttributes { get; set; }

        [JsonPropertyName("ADDITIONALATTRIBUTES")]
        public AdditionalAttributes? AdditionalAttributes { get; set; }

        [JsonPropertyName("PHYSICALCHARACTERISTICS")]
        public PhysicalCharacteristics? PhysicalCharacteristics { get; set; }

        public static implicit operator JsonDocument(ModelDnaData v)
        {
            throw new NotImplementedException();
        }
    }

    public class Appearance
    {
        public EyeAttributes?  Eyes { get; set; }
        public HairAttributes?  Hair { get; set; }
        public SkinAttributes? Skin { get; set; }
        public FaceAttributes? Face { get; set; }
        public SmileAttributes? Smile { get; set; }
        public BodyAttributes? Body { get; set; }
    }

    public class EyeAttributes
    {
        public string? Color { get; set; }
        public string? Shape { get; set; }
        public string? Spacing { get; set; }
    }

    public class HairAttributes
    {
        public string? Color { get; set; }
        public string? Texture { get; set; }
        public string? Length { get; set; }
    }

    public class SkinAttributes
    {
        public string? Tone { get; set; }
        public string? Texture { get; set; }
        public List<string>? Marks { get; set; }
    }

    public class FaceAttributes
    {
        public string? Shape { get; set; }
        public string? BoneStructure { get; set; }
        public string? Lips { get; set; }
        public string? Nose { get; set; }
    }

    public class SmileAttributes
    {
        public string? Type { get; set; }
        public string? Teeth { get; set; }
    }

    public class BodyAttributes
    {
        public string? Structure { get; set; }
        public string? Proportions { get; set; }
        public string? Posture { get; set; }
    }

    public class AdditionalAttributes
    {
        public string? Ethnicity { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Experience { get; set; }
        public string? Personality { get; set; }
        public bool? TravelAvailability { get; set; }
    }

    public class PhysicalCharacteristics
    {
        public string? Height { get; set; }
        public string? BustOrChest { get; set; }
        public string? Waist { get; set; }
        public string? Hips { get; set; }
        public int? ShoeSize { get; set; }
        public string? ClothingSize { get; set; }
        public string? LegLength { get; set; }
        public string? ArmLength { get; set; }
        public string? Neck { get; set; }
    }
}