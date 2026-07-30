namespace XcaXds.Commons.Commons;

public static class ObjectMerger
{
    /// <summary>
    /// Merges two objects of the same type together
    /// </summary>
    /// <param name="source">The object that will be merged with target</param>
    /// <param name="target">The target object. Ie. In a PATCH-operation, this will be the input</param>
    public static void MergeObjects<T>(T source, T target)
    {
        if (target == null) return;

        var props = target.GetType().GetProperties().ToList();

        foreach (var property in props)
        {
            if (property.GetIndexParameters().Length > 0 || property.CanRead == false)
            {
                continue;
            }

            var sourceValue = property.GetValue(source);
            var targetValue = property.GetValue(target);

            if (sourceValue?.Equals(targetValue) == true)
                continue;

            if (targetValue == null ||
               (sourceValue == null && targetValue == null) ||
               (sourceValue == null && targetValue == null))
                continue;


            if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) == false)
            {
                if (sourceValue == null)
                {
                    sourceValue = Activator.CreateInstance(property.PropertyType);
                    if (property.CanWrite == false)
                    {
                        continue;
                    }
                    property.SetValue(source, sourceValue);
                }

                MergeObjects(sourceValue, targetValue);
            }
            else
            {
                if (targetValue != null && property.CanWrite)
                {
                    property.SetValue(source, targetValue);
                }
            }
        }
    }

}
