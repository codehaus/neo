using System;

namespace Neo.Core.Qualifiers
{
	public interface IQualifierVisitor
	{
		object VisitColumnQualifier(ColumnQualifier q);
		object VisitPropertyQualifier(PropertyQualifier q);
		object VisitPathQualifier(PathQualifier q);
		object VisitAndQualifier(AndQualifier q);
		object VisitOrQualifier(OrQualifier q);
		}
}
