namespace Neo.Core.Qualifiers
{
	/// <summary>
	/// Callback interface for objects that can visit a <c>Qualifier</c> objects.
	/// </summary>
	/// <remarks>
	/// The value returned from the individual methods is returned from the
	/// <c>AcceptQualifier</c> method.
	/// </remarks>
	public interface IQualifierVisitor
	{
		object VisitColumnQualifier(ColumnQualifier q);
		object VisitPropertyQualifier(PropertyQualifier q);
		object VisitPathQualifier(PathQualifier q);
		object VisitAndQualifier(AndQualifier q);
		object VisitOrQualifier(OrQualifier q);
		}
}
