public class AssemblyStep
{
	public string PartIdentifier;
	public bool IsCompleted;

	public AssemblyStep(string partIdentifier, bool isCompleted)
	{
		PartIdentifier = partIdentifier;
		IsCompleted = isCompleted;
	}
}