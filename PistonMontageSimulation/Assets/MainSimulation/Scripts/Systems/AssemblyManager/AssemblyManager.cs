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
			assemblySteps = new List<AssemblyStep>();
			SetupAssemblySteps();

		}
		private void SetupAssemblySteps()
		{
			// Create all steps
			AssemblyStep piston = new AssemblyStep("piston", false);
			AssemblyStep rod = new AssemblyStep("rod", false);
			AssemblyStep wrist_pin = new AssemblyStep("wrist_pin", false);
			AssemblyStep pin_clip_1 = new AssemblyStep("pin_clip_1", false);
			AssemblyStep pin_clip_2 = new AssemblyStep("pin_clip_2", false);
			AssemblyStep rod_bearing_cap_side = new AssemblyStep("rod_bearing_cap_side", false);
			AssemblyStep rod_bearing_rod_side = new AssemblyStep("rod_bearing_rod_side", false);
			AssemblyStep rod_bolt_1 = new AssemblyStep("rod_bolt_1", false);
			AssemblyStep rod_bolt_2 = new AssemblyStep("rod_bolt_2", false);
			AssemblyStep rod_cap = new AssemblyStep("rod_cap", false);

			// Define mandatory and forbidden assemblies
			rod.AddForbiddenAssembly(wrist_pin);

			wrist_pin.AddForbiddenAssembly(pin_clip_1);
			wrist_pin.AddForbiddenAssembly(pin_clip_2);

			pin_clip_1.AddMandatoryAssembly(wrist_pin);
			pin_clip_2.AddMandatoryAssembly(wrist_pin);

			rod_cap.AddForbiddenAssembly(rod_bolt_1);
			rod_cap.AddForbiddenAssembly(rod_bolt_2);

			rod_bolt_1.AddMandatoryAssembly(rod_cap);
			rod_bolt_2.AddMandatoryAssembly(rod_cap);

			// Add the steps to the list
			assemblySteps.Add(piston);
			assemblySteps.Add(rod);
			assemblySteps.Add(wrist_pin);
			assemblySteps.Add(pin_clip_1);
			assemblySteps.Add(pin_clip_2);
			assemblySteps.Add(rod_bearing_rod_side);
			assemblySteps.Add(rod_bearing_cap_side);
			assemblySteps.Add(rod_bolt_1);
			assemblySteps.Add(rod_bolt_2);
			assemblySteps.Add(rod_cap);
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
		public bool CanPartBeAssembled(string partIdentifier)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			if (step != null)
			{
				// Check if all mandatory parts are assembled
				foreach (var mandatory in step.AssembliesMandatory)
				{
					if (!IsPartAssembled(mandatory.PartIdentifier))
					{
						return false; // A mandatory part is not assembled
					}
				}
				// Check if any forbidden parts are assembled
				foreach (var forbidden in step.AssembliesForbidden)
				{
					if (IsPartAssembled(forbidden.PartIdentifier))
					{
						return false; // A forbidden part is assembled
					}
				}
				return true; // All checks passed, can assemble the part
			}
			return false; // Part not found
		}
		public bool CanPartBeDisassembled(string partIdentifier)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			if (step != null && step.IsCompleted)
			{
				// Check if disassembling this part would violate any forbidden rules
				foreach (var otherStep in assemblySteps)
				{
					if (otherStep.AssembliesForbidden.Contains(step) && otherStep.IsCompleted)
					{
						return false; // Cannot disassemble this part because it would violate a forbidden rule
					}
				}
				return true; // Can disassemble this part
			}
			return false; // Part not found or not assembled
		}
	}
}
