using System;
using System.Collections.Generic;

namespace Dialogue.VN
{
	/// <summary>
	/// This is a group of puppets which will be moved by this puppet.
	/// </summary>
	public struct MoveBatch {

		public enum BatchMode { Push, Pull };

		public Speed moveSpeed 
			{ get; private set; }
		public StagePoint destination
			{ get; private set; }
		public BatchMode mode
			{ get; private set; }

		private List<Puppet> _targets;
		public Puppet[] targets {
			get         => _targets.ToArray();
			private set => _targets = new List<Puppet>(value);
		}

		public MoveBatch(Puppet[] targets, StagePoint destination, Speed moveSpeed, BatchMode mode) {

			this.moveSpeed = moveSpeed;
			this.destination = destination;
			this.mode = mode;

			// Necessary? Yes. Stupid? Definitely.
			_targets = null;

			// This must come after all fields are initialized; otherwise, VS complains.
			this.targets = targets ?? throw new ArgumentNullException(nameof(targets));
		}

		public void RemoveTarget(Puppet target) {
			_targets.Remove(target);
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			string result = "(";

			foreach(Puppet target in targets) {
				result += target.name;
			}

			result += ") going to " + destination?.name;
			return result;
		}
	}


}
