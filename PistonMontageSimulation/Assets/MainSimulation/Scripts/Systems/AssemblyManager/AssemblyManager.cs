using System.Collections.Generic;
using UnityEngine;

namespace PistonProject.Managers
{
	public class AssemblyManager : Singleton<AssemblyManager>
	{
		[SerializeField] private List<AssemblyStep> assemblySteps;

		private void Awake()
		{
			InitializeAssemblySteps();
		}

		private void InitializeAssemblySteps()
		{
			assemblySteps = new List<AssemblyStep>()
			{
				new AssemblyStep("piston", false), 
                new AssemblyStep("rod", false), 
                new AssemblyStep("wrist_pin", false), 
				new AssemblyStep("pin_clip_1", false),
				new AssemblyStep("pin_clip_2", false),
				new AssemblyStep("rod_bearing_rod_side", false),
				new AssemblyStep("rod_bearing_cap_side", false),
				new AssemblyStep("rod_cap", false),
				new AssemblyStep("rod_bolt_1", false),
				new AssemblyStep("rod_bolt_2", false)
            };
		}
		public void SetPartAssembled(string partIdentifier, bool isAssembled)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			if (step != null)
			{
				step.IsCompleted = isAssembled;
			}
		}
		public bool IsPartAssembled(string partIdentifier)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			return step != null && step.IsCompleted;
		}
	}
}
