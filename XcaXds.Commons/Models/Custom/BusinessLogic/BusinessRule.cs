using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using XcaXds.Commons.Models.Soap.XdsTypes;

namespace XcaXds.Commons.Models.Custom.BusinessLogic;

public class BusinessRule<T>
{
    public string Name { get; init; } = string.Empty;

    public Expression<Func<BusinessLogicParameters, bool>>? Condition { get; init; }

    public Expression<Func<IEnumerable<T>, IEnumerable<T>>>? Filter { get; init; }
}
