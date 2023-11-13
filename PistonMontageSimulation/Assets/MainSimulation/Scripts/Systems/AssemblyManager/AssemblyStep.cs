using System.Collections.Generic;

public class AssemblyStep
{
	public string PartIdentifier;
	public bool IsCompleted;
	public List<AssemblyStep> AssembliesForbidden = new List<AssemblyStep>();
	public List<AssemblyStep> AssembliesMandatory = new List<AssemblyStep>();
	public List<AssemblyStep> DisassembliesForbidden = new List<AssemblyStep>();

	public AssemblyStep(string partIdentifier, bool isCompleted)
	{
		PartIdentifier = partIdentifier;
		IsCompleted = isCompleted;
	}

	public void AddForbiddenAssembly(AssemblyStep part)
	{
		AssembliesForbidden.Add(part);
	}

	public void AddMandatoryAssembly(AssemblyStep part)
	{
		AssembliesMandatory.Add(part);
	}
	public void AddForbiddenDisassembly(AssemblyStep part)
	{
		DisassembliesForbidden.Add(part);
	}
}