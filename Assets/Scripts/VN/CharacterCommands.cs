using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Yarn.Unity;

/*
 * Background
 * AnimateStage
 * Fade
 * Music
 * Sounds
 * 
 * Move
 * Outfit
 * Default Outfit
 * Animate
 * Facing
 * Emotion
 */

namespace Dialogue.VN
{
	/// <summary>
	/// Implements several character commands for the Yarn scripts to use.
	///
	/// These commands are specific to manipulating characters.
	/// General commands are in the
	/// [StageCommands](@ref Dialogue.VN.StageCommands) class.
	///
	/// Note that, whenever you have a name of an asset (e.g. an
	/// animation) which contains a space, you
	/// must wrap it in double quotes.
	/// 
	/// For any command with options such as **now**, **quickly**,
	/// and **slowly**, these options control the relative speed
	/// of the movement. Note that the exact speed depends on the
	/// command being used.
	///   * **now**: Instant; all delays and animations are skipped.
	///   * **quickly**: Snappy and quick. Good for fast or hurried motions.
	///   * (nothing): Standard speed. Should work for general situations.
	///   * **slowly**: Drags things out.
	///
	/// For commands with **wait** or **and wait** options, those
	/// will cause the dialogue system to pause until the command
	/// finishes being carried out. This does nothing if **now** is
	/// also used. Also be careful of combining this with **slowly**
	/// too often; that might make things draggy.
	/// </summary>
	public class CharacterCommands : MonoBehaviour
	{
		[Header("Setup")]

		[SerializeField]
		private DialogueRunner dialogueRunner;
		[SerializeField]
		private PuppetMaster puppetmaster;

		[Header("Design config")]

		[FormerlySerializedAs("positions")]
		[SerializeField]
		private StagePoint[] puppetPoints;

		private void Awake()
		{
			Assert.IsNotNull(puppetmaster);
			Assert.IsNotNull(dialogueRunner);

			dialogueRunner.AddCommandHandler("move", Move);
			dialogueRunner.AddCommandHandler("turn", Turn);
			dialogueRunner.AddCommandHandler("animate", Animate);
			dialogueRunner.AddCommandHandler("fade", Fade);
			dialogueRunner.AddCommandHandler("emote", Emote);
			//dialogueRunner.AddCommandHandler("face", Turn);
		}


		//private 


		/// <summary>
		/// &lt;&lt;addon CHARACTER [lingering|clear] ADDON&gt;&gt;\n 
		/// &lt;&lt;addon CHARACTER clear&gt;&gt;\n 
		/// 
		/// Applies the ADDON to CHARACTER, where ADDON is the name
		/// of an addon specified on Unity.
		/// 
		/// Addons usually have some animation tied to them.
		/// Once the animation is done playing, the addon will
		/// vanish.
		/// 
		/// However, specifying **lingering** will cause the addon
		/// stay until **clear** is used.
		///
		/// Specifying **clear** without ADDON will clear all
		/// lingering addons.
		/// 
		/// </summary>
		/// <example>
		///
		/// ## Examples
		/// 
		///     <<addon Ai !!!bubble>> 
		///     AI: The what?!
		/// Give Ai a the "!!!bubble" addon. It will vanish after
		/// the addon's animation finishes. (And can vanish while
		/// the dialogue is still being written out.)
		///
		///     <<addon Ai lingering !!!bubble>> 
		///     AI: The what?!
		///     AMI: Ai, he’s joking, but he has a point.
		///     <<addon Ai clear !!!bubble>> 
		/// Give Ai a the "!!!bubble" addon. In this case, the
		/// addon will persist until the second command is reached.
		///
		///     <<addon Ai lingering !!!bubble>> 
		///     AI: The what?!
		///     AMI: Ai, he’s joking, but he has a point.
		///     <<addon Ai clear>>
		/// Same as before.
		///
		///     <<addon Ai lingering !!!bubble>> 
		///     <<addon Ai lingering sweat>> 
		///     AI: The what?!
		///     AMI: Ai, he’s joking, but he has a point.
		///     <<addon Ai clear>>
		/// Give Ai a the "!!!bubble" addon and sweat addons.
		/// In this case, the clear command will remove all addons.
		/// </example>
		/// \warning Not implemented yet.
		public void Addon(string[] args)
		{
			Debug.LogWarning("Not implemented yet: addon");
		}

		/// <summary>
		/// &lt;&lt;animate CHARACTER ANIMATION [now|quickly|slowly] [and wait]&gt;&gt;\n 
		///
		/// Make CHARACTER play ANIMATION, where ANIMATION is the
		/// case-sensitive name of a character animation that has
		/// been created in Unity. These animations can either be
		/// one-off or looping; it depends on the animation.
		///
		/// The **None** animation can be used to stop all current
		/// animations. If an invalid animation is given, Unity
		/// throws a warning and continues on.
		///
		/// If **and wait** is given for a looping animation,
		/// the animation is allowed to play once before the
		/// dialogue continues.
		///
		/// The current list of animations follows. Note that this
		/// list is manually updated, so it may be out of date!
		///   * None
		///   * (Various internal animations)
		///
		/// </summary> <example>
		///
		/// ## Examples
		///
		///     <<animate Ibuki Jump>> 
		/// Make Ibuki play the "Jump" animation.
		/// (This animation may or may not exist.)
		///
		///     <<animate Ibuki Jump and wait>> 
		/// Make Ibuki play the "Jump" animation, and prevent
		/// any more dialogue boxes from playing until Ibuki
		/// finishes with the animation.
		/// 
		///     <<animate Ibuki None>> 
		/// Stop whatever animation Ibuki is playing, if any.
		///
		/// </example> 
		///
		/// \note Animations are kept in the `PuppetBase` prefab,
		///	      found in `Assets/Prefabs/UI/VN`. All animations
		///	      in its animation controller are valid.
		///	      Those with underscores are meant to be
		///	      internal-only, and should not be referenced with
		///	      this command.
		///	      (TODO: Make a video about this.)
		///	 
		/// \note Hypothetically some characters could have unique
		///       animations. However, this should not be a common
		///       occurrance and might be a pain to set up.
		///       Generally, all animations will be available on
		///       all characters.
		public void Animate(string[] args, Action onComplete)
		{
			#region Argument handling
			Assert.IsTrue(args.Length >= 2);
			Puppet character = puppetmaster.GetPuppet(args[0]);
			string animationName = args[1];

			int i = 2; // Index we were last using

			Speed speed = default(Speed);
			bool wait = false;

			CommandProcessing.ReadSpeedArgument(args, ref i, ref speed);
			CommandProcessing.ReadWaitArgument(args, ref i, ref wait);
			#endregion

			character.PlayAnim(animationName, speed, wait, onComplete);

		}

		/// <summary>
		/// &lt;&lt;emote CHARACTER EMOTION&gt;&gt;\n 
		/// 
		/// Changes CHARACTER's face to show EMOTION, where EMOTION is the
		/// (case-insensitive) name of one of the character's emotions.
		/// 
		/// Each character has their own range of emotions; some may have
		/// special emotions, and others might be missing some of the common
		/// ones. The list is configured in Unity.
		/// 
		/// This is the [list of emotions](https://docs.google.com/spreadsheets/d/1ugqnIXU1dLzg1uZWS6AMjp6IRk512xfytWhVZ9CEmfM/edit?usp=sharing)
		/// but this doesn't necessarily reflect the ones available in-game yet.
		///  
		/// Finally, there's one special emotion: **None**. This clears all
		/// emotions, resetting the character back to their default, neutral
		/// face that they use when they first come onto the stage.
		/// The **None** emotion will always be available.
		///
		/// Using an invalid emotion will print an error in the debug console
		/// and then act as though you had used **None** instead.
		/// </summary>
		/// <example>
		///
		/// ## Examples
		/// 
		///     <<emote Ibuki Laugh>> 
		/// Make Ibuki Laugh.
		///
		///     <<emote Ibuki None>> 
		/// Return Ibuki to her default emotion.
		/// </example>
		public void Emote(string[] args)
		{
			#region Argument handling
			Assert.IsTrue(args.Length >= 2);

			if (args.Length > 2) Debug.LogWarning("Emote only supports 2 args!");

			Puppet character = puppetmaster.GetPuppet(args[0]);
			string emoteName = args[1];
			#endregion

			character.SetEmote(emoteName);
		}

		/// <summary>
		/// &lt;&lt;fade in CHARACTER POSITION [now|quickly|slowly] [and wait]&gt;&gt;\n 
		/// &lt;&lt;fade out CHARACTER [now|quickly|slowly] [and wait]&gt;&gt;\n 
		///
		/// In the first form, causes CHARACTER to fade into view
		/// at POSITION.
		/// 
		/// In the second form, causes CHARACTER to fade out of view.
		/// For the purpose of stacking,
		/// this removes CHARACTER after they finish fading.
		///
		/// </summary> <example>
		///
		/// ## Examples
		///
		///     <<fade in Nimura Right slowly>>
		///     NIMURA: I'm fading in right now!
		/// Fades Nimura in at the right position.
		/// %Dialogue starts printing as the fade goes on.
		/// "slowly" is optional, and makes her linger longer.
		///
		///     <<fade in Ami Left>>
		///     <<fade in Nimura Right and wait>>
		///     NIMURA: We just faded in.
		/// Fades in Ami and Nimura at the same time and wait for them.
		/// Note that only the last one to get faded in needs the wait argument.
		/// %Dialogue doesn't print until the fade finishes.
		///
		///     <<fade out Nimura quickly>>
		///     NIMURA: Gotta run!
		/// Fades Nimura out quickly.
		///
		/// </example>
		public void Fade(string[] args, Action onComplete)
		{
			#region Argument handling
			string fadeMode = args[0];
			Puppet character = puppetmaster.GetPuppet(args[1]);

			int i = 2; // Index we were last using

			StagePoint fadeDest = null;
			if(fadeMode.Equals("in", StringComparison.OrdinalIgnoreCase)) {
				fadeDest = GetNamedPoint(args[2]);
				++i;
			}

			Speed speed = default(Speed);
			bool wait = false;

			CommandProcessing.ReadSpeedArgument(args, ref i, ref speed);
			CommandProcessing.ReadWaitArgument(args, ref i, ref wait);
			#endregion

			//Debug.Log(fadeMode);

			switch(fadeMode.ToLower()) {
				case "in":
					character.Warp(fadeDest);
					character.FadeIn(speed, wait, onComplete);
					break;

				case "out":
					character.FadeOut(speed, wait, onComplete);
					break;

				default:
					Debug.LogError("Bad fade mode: " + fadeMode);
					break;
			}
		}

		/// <summary>
		/// &lt;&lt;move CHARACTER [to] TO_POINT [from FROM_POINT] [now|slowly|quickly] [and wait]&gt;&gt;
		/// <br>
		/// (Continued; see subsection)
		///
		/// Causes CHARACTER to slide over to TO_POINT. If FROM_POINT is
		/// specified, the character will warp there before moving.
		/// If trying to move a character into view, please make sure to
		/// specify FROM_POINT. Otherwise, your character may start
		/// somewhere you weren't expecting!
		/// 
		/// Points are created in Unity and are listed in the "puppetPoints"
		/// variable. For this reason, the number and names of points is
		/// flexible, and could possibly change. Current points PROBABLY are:
		///		offleft, left, middle, right, offright
		///
		///
		/// ### Stacking
		/// 
		/// If two or more characters end up on the same point, then
		/// they'll "stack," with their positions being staggered
		/// so they aren't all on the same position.
		/// 
		/// Note that this is still in development. More details on
		/// how this looks/works once that's done.
		/// 
		/// 
		/// ### Pushing and Pulling
		/// 
		///     <<move CHARACTER [to] TO_POINT [from FROM_POINT] [now|slowly|quickly] [and wait]
		///            [and pull|push TARGET1 [, TARGET2, TARGET3, ...] [to TARGET_POSITION]]>>
		///
		/// If **pull** or **push** is given, then every TARGET gets
		/// pulled/pushed by CHARACTER. It's possible to push/pull 
		/// several targets at a time, separating their names with
		/// commas.
		///
		/// By default, each TARGET ends up on the same spot as
		/// CHARACTER. However, if TARGET_POSITION is also specified,
		/// then all TARGETs end up there.
		/// 
		/// The assumption is that CHARACTER will move past every
		/// TARGET. Any TARGETs which CHARACTER does not go past
		/// won't be pushed. (This is subject to change?)
		/// 
		/// 
		/// </summary> <example>
		///
		/// ## Examples
		///
		///     <<move Ibuki Middle and wait>> 
		/// Move Ibuki to the middle (from wherever she was before).
		/// The VN will wait until Ibuki finishes moving.
		/// 
		///     <<move Ibuki to Middle>>
		/// Same as before, except without the waiting. (And it looks
		/// a little more like English.)
		///
		///     <<move Ibuki Middle now>>
		/// Same as before, but Ibuki will "teleport" to the middle.
		/// 
		///     <<move Ibuki to Left from OffLeft>>
		/// Move Ibuki from the off the left edge of the screen to the left position.
		/// Use something like this if the character was off screen.
		///
		///     <<move Ibuki to Middle and push Nimura to Left and push Ami to OffLeft>>
		/// Move Ibuki to the middle.<br>
		/// When Ibuki reaches Nimura, Nimura will be forced over to the left.<br>
		/// When Ibuki reaches Ami, Ami will be forced off the left edge.
		///
		///     <<move Ibuki to Middle and push Nimura, Ami to Left>>
		/// Move Ibuki to the middle.<br>
		/// When Ibuki reaches Nimura and/or Ami, they will both be to the Left position.
		/// 
		/// 
		/// </example>
		/// \warning Pushing, pulling, and stacking all work.
		///          However, all three of these things still need some tuning.
		///          Movement speed parameters still need to be implemented.
		public void Move(string[] args, Action onComplete)
		{

			#region Argument handling
			Assert.IsTrue(args.Length >= 2);
			//string charName = args[0];
			Puppet character = puppetmaster.GetPuppet(args[0]);
			//string destName = "";
			//string fromName = "";
			StagePoint endpoint = null;
			StagePoint startpoint = null;
			bool wait = false;
			Speed moveSpeed = Speed.Normal;
			List<Puppet.MoveBatch> batches = new List<Puppet.MoveBatch>();

			for(int i = 1; i < args.Length; i++)
			{
				switch(args[i])
				{
					case "to":
						i++;
						//destName = args[i];
						endpoint = GetNamedPoint(args[i]);
						break;

					case "from":
						i++;
						//fromName = args[i];
						startpoint = GetNamedPoint(args[i]);
						break;

					case "push":
					case "pull":
						// [and pull|push TARGET1 [, TARGET2, TARGET3, ...] [to TARGET_POSITION]]
						StagePoint batchDestination = null;
						Puppet.MoveBatch.BatchMode mode =
							args[i].Equals("push")
								? Puppet.MoveBatch.BatchMode.Push
								: Puppet.MoveBatch.BatchMode.Pull;
						i++;

						List<Puppet> targets = new List<Puppet>();
						bool finished = false;
						while(!finished) {
							bool trailingComma = (args[i].EndsWith(","));
							string nextTargetName =
								trailingComma
									? (args[i].Substring(0, args[i].Length - 1))
									: (args[i]);
							Puppet nextTarget = puppetmaster.GetPuppet(nextTargetName);
							i++;

							if(!trailingComma) {
								// There was no comma immediately following the last name. We will check for one by itself.
								// But first we must make sure that we are staying in bounds.
								if(i >= args.Length) {
									finished = true;
								}
								else if (args[i].Equals(",")) {
									i++;
								}
								else {
									finished = true;
									if (args[i].Equals("to")) {
										i++;
										batchDestination = GetNamedPoint(args[i]);
									}
								}

							}

							targets.Add(nextTarget);

						}

						// TODO Handle speeds

						batches.Add(new Puppet.MoveBatch(targets.ToArray(), batchDestination, Speed.Normal, mode));
						break;

					default:
						if (CommandProcessing.ReadWaitArgument(args, ref i, ref wait) ||
							CommandProcessing.ReadSpeedArgument(args, ref i, ref moveSpeed)
						) {
							// The functions above increment i upon a successful read,
							// but the for loop does this for us. Thus, we must decrement i
							// in order to prevent any skips.
							i--;
						}
						else {

							// Only take an unlabeled parameter if we don't
							// already have a destination.
							if (endpoint == null) {
								endpoint = GetNamedPoint(args[i]);
							}
							else {
								ReportInvalidArgument("move", args[i]);
							}
						}
						break;
				}
			}
			#endregion


			if(startpoint != null)
			{
				character.Warp(startpoint);
			}

			if(!wait) {
				onComplete.Invoke();
				onComplete = null;
			}

			character.SetMovementDestination(endpoint, batches, onComplete, moveSpeed);
		}

		/// <summary>
		/// &lt;&lt;outfit CHARACTER COSTUME&gt;&gt;\n 
		///
		/// Make CHARACTER where COSTUME, where COSTUME is a
		/// (case-insensitive) name for one of CHARACTER's costumes.
		/// 
		/// Costumes are specified within Unity, so their names aren't
		/// fixed. However, every character will have a default
		/// **VR** and **RL** costumes. Normally, characters will
		/// enter the stage with their RL costume, but this may
		/// be configured with
		/// [outfit-all](@ref Dialogue.VN.CharacterCommands.OutfitAll).
		///
		/// If COSTUME is **None** or an invalid costume, the
		/// character's default costume is used instead.
		/// (Again, default is controlled via
		/// [outfit-all](@ref Dialogue.VN.CharacterCommands.OutfitAll).)
		///
		/// Note that changing costumes is instant.
		/// Making characters change while they're
		/// on screen might be strange; get them off the screen
		/// first. (If you need characters changing on screen with
		/// some animation involved, let a coder know!)
		///
		/// </summary> <example>
		///
		/// ## Examples
		///
		///     <<outfit Ibuki VR>> 
		/// Make Ibuki wear her default VR outfit.
		///
		///		<<outfit Ibuki None>>
		/// Make Ibuki wear her current default costume.
		///
		/// </example>
		/// \warning Not implemented yet.
		public void Outfit(string[] args)
		{
			Debug.LogWarning("Not implemented yet: Outfit");
		}

		/// <summary>
		/// &lt;&lt;outfit-all [VR|RL] [default-only]&gt;&gt;\n 
		///
		/// Make all characters wear either their VR or RL costumes.
		/// This also changes all incoming characters to have on
		/// their VR/RL costume.
		/// 
		/// If **default-only** is specified, loaded characters will
		/// not have their costume changed. (Note: all characters
		/// who have been used in the scene are loaded, even if
		/// they're not visible!)
		///
		/// </summary> <example>
		///
		/// ## Examples
		///
		///     <<outfit-all VR>> 
		/// Make all characters wear their virtual reality outfits.
		///
		///		<<outfit-all RL default-only>>
		/// Make all future characters wear their real-life outfits.
		///
		/// </example>
		/// \warning Not implemented yet.
		public void OutfitAll(string[] args)
		{
			Debug.LogWarning("Not implemented yet: OutfitAll");
		}

		/// <summary>
		/// &lt;&lt;turn CHARACTER DIRECTION&gt;&gt;\n 
		/// 
		/// Causes CHARACTER to face in DIRECTION, where DIRECTION is
		/// either "left" or "right" (DIRECTION is case-insensitive).
		///
		/// </summary> <example>
		///
		/// ## Examples
		///
		///     <<turn Ibuki Right>> 
		/// Make Ibuki face towards the right.
		/// </example>
		public void Turn(string[] args)
		{
			#region Argument handling
			Assert.IsTrue(args.Length >= 2);
			string charName = args[0];
			string facingName = args[1];
			#endregion

			Puppet charPuppet = puppetmaster.GetPuppet(charName);
			Puppet.Facing newFacing;
			if(facingName.Equals("left", StringComparison.OrdinalIgnoreCase))
			{
				newFacing = Puppet.Facing.Left;
			}
			else if(facingName.Equals("right", StringComparison.OrdinalIgnoreCase))
			{
				newFacing = Puppet.Facing.Right;
			}
			else
			{
				throw new InvalidEnumArgumentException("Invalid facing: " + facingName);
			}

			Debug.Log("Changing facing to " + newFacing.ToString());

			charPuppet.SetFacing(newFacing);
		}

		/// <summary>
		/// Internal use only; this doesn't implement a command.
		/// </summary>
		/// <param name="name"></param>
		public void Focus(string name) {
			puppetmaster.GetPuppet(name).FocusSelf();
		}

		private StagePoint GetNamedPoint(string posName)
		{
			return puppetPoints.FirstOrDefault(
				(rt) => posName.Equals(rt.name, System.StringComparison.OrdinalIgnoreCase)
			); 
		}

		private void ReportInvalidArgument(string command, string arg)
		{
			Debug.LogWarning("Invalid argument for " + command + ": " + arg);
		}
	}
}
