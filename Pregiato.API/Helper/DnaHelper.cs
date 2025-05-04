using Pregiato.API.Models;

namespace Pregiato.API.Helper
{
    public static class DnaHelper
    {
        public static void EnsureDefaults(ModelDnaData attributes)
        {
            attributes.Appearance ??= new Appearance();

            attributes.Appearance.Eyes ??= new EyeAttributes();
            attributes.Appearance.Hair ??= new HairAttributes();
            attributes.Appearance.Skin ??= new SkinAttributes { Marks = new List<string>() };
            attributes.Appearance.Face ??= new FaceAttributes();
            attributes.Appearance.Smile ??= new SmileAttributes();
            attributes.Appearance.Body ??= new BodyAttributes();

            attributes.AdditionalAttributes ??= new AdditionalAttributes
            {
                Skills = new List<string>(),
                Experience = new List<string>()
            };

            attributes.PhysicalCharacteristics ??= new PhysicalCharacteristics();
        }    
    }
}
