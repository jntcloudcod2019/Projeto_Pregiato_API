using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    [NotMapped]
    public class ModelDnaData
    {
        [JsonPropertyName("dna")]
        public string Dna { get; set; } = "InfoModel";

        [JsonPropertyName("appearance")]
        public Appearance Appearance { get; set; }

        [JsonPropertyName("eyeAttributes")]
        public EyeAttributes EyeAttributes { get; set; }

        [JsonPropertyName("hairAttributes")]
        public HairAttributes HairAttributes { get; set; }

        [JsonPropertyName("skinAttributes")]
        public SkinAttributes SkinAttributes { get; set; }

        [JsonPropertyName("faceAttributes")]
        public FaceAttributes FaceAttributes { get; set; }

        [JsonPropertyName("smileAttributes")]
        public SmileAttributes SmileAttributes { get; set; }

        [JsonPropertyName("bodyAttributes")]
        public BodyAttributes BodyAttributes { get; set; }

        [JsonPropertyName("additionalAttributes")]
        public AdditionalAttributes AdditionalAttributes { get; set; }

        [JsonPropertyName("physicalCharacteristics")]
        public PhysicalCharacteristics PhysicalCharacteristics { get; set; }

        public static implicit operator JsonDocument(ModelDnaData v)
        {
            throw new NotImplementedException();
        }
    }

    public class Appearance
    {
        public EyeAttributes Eyes { get; set; }
        public HairAttributes Hair { get; set; }
        public SkinAttributes Skin { get; set; }
        public FaceAttributes Face { get; set; }
        public SmileAttributes Smile { get; set; }
        public BodyAttributes Body { get; set; }
    }

    public class EyeAttributes
    {
        public string Color { get; set; }
        public string Shape { get; set; }
        public string Spacing { get; set; }
    }

    public class HairAttributes
    {
        public string Color { get; set; }
        public string Texture { get; set; }
        public string Length { get; set; }
    }

    public class SkinAttributes
    {
        public string Tone { get; set; }
        public string Texture { get; set; }
        public List<string> Marks { get; set; }
    }

    public class FaceAttributes
    {
        public string Shape { get; set; }
        public string BoneStructure { get; set; }
        public string Lips { get; set; }
        public string Nose { get; set; }
    }

    public class SmileAttributes
    {
        public string Type { get; set; }
        public string Teeth { get; set; }
    }

    public class BodyAttributes
    {
        public string Structure { get; set; }
        public string Proportions { get; set; }
        public string Posture { get; set; }
    }

    public class AdditionalAttributes
    {
        public string Ethnicity { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Experience { get; set; }
        public string Personality { get; set; }
        public bool TravelAvailability { get; set; }
    }

    public class PhysicalCharacteristics
    {
        public string Height { get; set; }
        public string BustOrChest { get; set; }
        public string Waist { get; set; }
        public string Hips { get; set; }
        public int ShoeSize { get; set; }
        public int ClothingSize { get; set; }
        public string LegLength { get; set; }
        public string ArmLength { get; set; }
        public string Neck { get; set; }
    }
}