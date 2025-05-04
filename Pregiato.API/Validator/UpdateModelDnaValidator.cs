namespace Pregiato.API.Validator
{
    public static class UpdateModelDnaValidator
    {
        public static bool HasAnyValue(object? obj)
        {
            if (obj == null) return false;

            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;

                if (IsSimpleType(prop.PropertyType)) return true;

                if (HasAnyValue(value)) return true; 
            }

            return false;
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type.Equals(typeof(string)) ||
                   type.Equals(typeof(DateTime)) ||
                   type.Equals(typeof(decimal)) ||
                   type.Equals(typeof(Guid));
        }
    }
}
