using System.Linq.Expressions;

namespace XcaXds.BusinessLogic.Models.Custom.BusinessLogic;

public class BusinessRule<T>
{
    public string Name { get; init; } = string.Empty;

    public Expression<Func<BusinessLogicParameters, bool>>? Condition { get; init; }

    public Expression<Func<IEnumerable<T>, IEnumerable<T>>>? Filter { get; init; }
}
