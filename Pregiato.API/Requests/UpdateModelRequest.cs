using Pregiato.API.Models;
using System.Text.Json;

namespace Pregiato.API.Requests
{
    public class UpdateModelRequest : Model
    {
        public string? Name { get; set; }
        public string? CPF { get; set; }
        public string? RG { get; set; }
        public string? DateofBirth { get; set; }
        public string? Email { get; set; }
        public string? PostalCode { get; set; }
        public string? Address { get; set; }
        public string? BankAccount { get; set; }
        public JsonDocument ? DNA { get; set; }
        public PhysicalCharacteristics ? PhysicalCharacteristics { get; set; }
        public Appearance ? Appearance { get; set; }
        public AdditionalAttributes ? AdditionalAttributes { get; set; }

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
        public int Age { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Experience { get; set; }
        public string Personality { get; set; }
        public bool TravelAvailability { get; set; }
    }


}
