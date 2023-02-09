using System.Dynamic;

namespace EasyOC
{
    public static class ExpandoObjectExtensions
    {
        public static Dictionary<string, object> ToDictionary(this ExpandoObject input)
        {
            if (input != null)
            {
                return input.ToJObject()?
                    .ToObject<Dictionary<string, object>>();
            }
            return null;

        }


        public static Dictionary<string, object>[] ToDictionary(this ExpandoObject[] input)
        {
            if (input != null)
            {
                return input.ToJObject()?
                    .ToObject<Dictionary<string, object>[]>();
            }
            return null;

        }
    }
}
