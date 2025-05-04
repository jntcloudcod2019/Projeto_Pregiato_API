using Pregiato.API.System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    public class ModelDnaData
    {
        [JsonPropertyName("DNA")]
        public string? Dna { get; set; } = "INFOMODEL";

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
        [JsonPropertyName("COLOR")]
        public string? Color { get; set; }

        [JsonPropertyName("SHAPE")]
        public string? Shape { get; set; }

        [JsonPropertyName("SPACING")]
        public string? Spacing { get; set; }
    }

    public class HairAttributes
    {
        [JsonPropertyName("COLOR")]
        public string? Color { get; set; }

        [JsonPropertyName("TEXTURE")]
        public string? Texture { get; set; }

        [JsonPropertyName("LENGTH")]
        public string? Length { get; set; }
    }

    public class SkinAttributes
    {
        [JsonPropertyName("TONE")]
        public string? Tone { get; set; }

        [JsonPropertyName("TEXTURE")]
        public string? Texture { get; set; }

        [JsonPropertyName("MARKS")]
        public List<string>? Marks { get; set; }
    }

    public class FaceAttributes
    {
        [JsonPropertyName("SHAPE")]

        public string? Shape { get; set; }

        [JsonPropertyName("BONESTRUCTURE")]
        public string? BoneStructure { get; set; }

        [JsonPropertyName("LIPS")]
        public string? Lips { get; set; }

        [JsonPropertyName("NOSE")]
        public string? Nose { get; set; }
    }

    public class SmileAttributes
    {
        [JsonPropertyName("TYPE")]
        public string? Type { get; set; }

        [JsonPropertyName("TEETH")]
        public string? Teeth { get; set; }
    }

    public class BodyAttributes
    {
        [JsonPropertyName("STRUCTURE")]
        public string? Structure { get; set; }

        [JsonPropertyName("PROPORTIONS")]
        public string? Proportions { get; set; }

        [JsonPropertyName("POSTURE")]
        public string? Posture { get; set; }
    }

    public class AdditionalAttributes
    {
        [JsonPropertyName("ETHNICITY")]
        public string? Ethnicity { get; set; }

        [JsonPropertyName("SKILLS")]
        public List<string>? Skills { get; set; }

        [JsonPropertyName("EXPERIENCE")]
        public List<string>? Experience { get; set; }

        [JsonPropertyName("PERSONALITY")]
        public string? Personality { get; set; }

        [JsonPropertyName("TRAVELAVAILABILITY")]
        public bool? TravelAvailability { get; set; }
    }

    public class PhysicalCharacteristics
    {
        [JsonPropertyName("HEIGHT")]
        public string? Height { get; set; }

        [JsonPropertyName("BUSTORCHEST")]
        public string? BustOrChest { get; set; }

        [JsonPropertyName("WAIST")]
        public string? Waist { get; set; }

        [JsonPropertyName("HIPS")]
        public string? Hips { get; set; }

        [JsonPropertyName("SHOESIZE")]
        public int? ShoeSize { get; set; }

        [JsonPropertyName("CLOTHINGSIZE")]
        public int? ClothingSize { get; set; }

        [JsonPropertyName("LEGLENGTH")]
        public string? LegLength { get; set; }

        [JsonPropertyName("ARMLENGTH")]
        public string? ArmLength { get; set; }

        [JsonPropertyName("NECK")]
        public string? Neck { get; set; }
    }
}