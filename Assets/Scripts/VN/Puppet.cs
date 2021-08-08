using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Nijito.Paperdoll;

namespace Dialogue.VN
{
	public class Puppet : MonoBehaviour
	{
		public enum Facing { Left, Right }

		[SerializeField] private AnimationSettings animSettings;
		[SerializeField] private Image imageRenderer;
		[SerializeField] private Facing initialFacing = Facing.Left;

		[Header("Movement")]
		[Range(0, 2)] [SerializeField] private float maxSpeed = 0.6f;
		[Range(0, 2)] [SerializeField] private float minSpeed = 0.05f;

		[Space(10)]
		
		[Range(0, 2)] [SerializeField] private float accelerationDistance = 0.2f;
		[Tooltip("Make sure this starts at 0 (slowest) and ends at 1 (fastest)!")]
		[SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Space(10)]

		[Range(0, 2)] [SerializeField] private float decelerationDistance = 0.2f;
		[Tooltip("Make sure this starts at 0 (fastest) and ends at 1 (slowest)!")]
		[SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Space(10)]

		[SerializeField] private float stoppingThreshold = 0.001f;
		[Tooltip("Range used for pushing and pulling.")]
		[Range(0f, 1f)]
		[SerializeField] private float width = 0.1f;

		[Header("Animations")]
		[SerializeField] private Animator animator;
		[SerializeField] private string fadeInAnim = "FadeIn";
		[SerializeField] private string fadeOutAnim = "FadeOut";


		public Slide ActiveSlide {
			get {
				return _activeSlide;
			}
			set {
				_activeSlide = value;
				_activeSlide.Redraw();
				imageRenderer.material = new Material(imageRenderer.material); // Otherwise it'll modify old, shared material
				imageRenderer.material.mainTexture = _activeSlide.tex;
			}
		}

		public float RendererWidth {
			get {
				return imageRenderer.rectTransform.rect.width;
			}
		}

		public float RendererHeight {
			get {
				return imageRenderer.rectTransform.rect.height;
			}
		}


		/// <summary>
		/// Sets a new destination where we want to slide to.
		/// </summary>
		/// <param name="point">Point to move to.</param>
		/// <param name="batches">Set of movement batches for this next move. (Used for pushing and pulling, mostly.)</param>
		public void SetMovementDestination(StagePoint point, List<MoveBatch> batches = null, Action onComplete = null, Speed speed = Speed.Normal) {
			this.DestinationPoint = point;
			PreviousPosition = CurrentPosition;

			if(batches != null) {
				string output = "batch:";
				foreach(MoveBatch batch in batches) {
					output += "\n" + batch.ToString();
				}
				//Debug.Log(output);
			}

			if (batches != null) {
				currentBatches = batches;
			}
			else {
				currentBatches.Clear();
			}

			moveSpeed = Speed.Normal;
			onMoveComplete = onComplete;
        }

		/// <summary>
		/// Snap to the given position.
		/// This also cancels out any movement.
		/// </summary>
		public void Warp(StagePoint point) {
			SetMovementDestination(point);
			SetPosition(DestinationPoint.GetPosition(gameObject).x);
			
			PreviousPosition = CurrentPosition;
        }

		public void SetEmote(string emoteName) {
			ActiveSlide.SetEmote(emoteName);
		}

		public void SetOutfit(string outfitName) {
			ActiveSlide.SetOutfit(outfitName);
		}

		public void SetFacing(Facing newFacing) {
			Vector3 s = rTransform.localScale;
			s.x = (newFacing == initialFacing ? 1 : -1);
			rTransform.localScale = s;
		}

		public void FocusSelf() {
			int siblingCount = transform.parent.childCount;
			transform.SetSiblingIndex(siblingCount - 1);
		}


		public void PlayAnim(string name, Speed speed = Speed.Normal, bool wait = false, Action onComplete = null) {
			StartCoroutine(animSettings.PlayAnim(animator, name, speed, wait, onComplete));
		}

		public void FadeIn(Speed speed, bool wait = false, Action onComplete = null) {
			PlayAnim(fadeInAnim, speed, wait, onComplete);
		}

		public void FadeOut(Speed speed, bool wait = false, Action onComplete = null) {
			PlayAnim(fadeOutAnim, speed, wait, onComplete + (() => DestinationPoint = null));
		}

		#region Events
		private void Awake()
		{
			//animationNames = animator.runtimeAnimatorController.animationClips.Select(c => c.name).ToArray();

			rTransform = GetComponent<RectTransform>();
			Assert.IsNotNull(rTransform,
				"Puppets should be part of the UI, not in the scene itself!"
			);
			Assert.IsNotNull(animator, "Puppet needs an animator");
			Assert.IsNotNull(animSettings, "Puppet needs animation settings");

			/*
			if(faceRenderer == null) {
				Debug.LogWarning(gameObject.name + " is missing a face!\n...must be hard for them... :(");
			}
			*/
		}

		private void Update()
		{
			if (DestinationPoint != null) {
				float destinationPosition = DestinationPoint.GetPosition(gameObject).x;

				//if (!Mathf.Approximately(CurrentPosition.x, horizontalPosition)) {
				if (Mathf.Abs(CurrentPosition.x - destinationPosition) > stoppingThreshold) {

					Vector2 oldPosition = CurrentPosition;

					/*
					SetPosition( Mathf.Lerp(
						CurrentPosition.x,
						destinationPosition,
						movementSpeed * Time.deltaTime
					) );
					*/

					SetPosition(Mathf.MoveTowards(
						CurrentPosition.x,
						destinationPosition,
						CalculateSpeed(
							PreviousPosition.x, destinationPosition, CurrentPosition.x,
							maxSpeed * animSettings.SpeedToScale(moveSpeed), minSpeed,
							accelerationDistance, accelerationCurve,
							decelerationDistance, decelerationCurve
						) * Time.deltaTime
					));



					Vector2 deltaPosition = CurrentPosition - oldPosition;
					float direction = Mathf.Sign(deltaPosition.x);
					float pushPoint = CurrentPosition.x + direction * width; // Front
					float pullPoint = CurrentPosition.x - direction * width; // Back

					foreach(MoveBatch batch in currentBatches) {
						float threshold = CurrentPosition.x;
						switch (batch.mode) {
							case MoveBatch.BatchMode.Push:
								threshold = pushPoint;
								break;
							case MoveBatch.BatchMode.Pull:
								threshold = pullPoint;
								break;
							default:
								Debug.LogWarning("Unhandled case");
								break;
						}

						// Save this because we'll be changing the source
						Puppet[] batchTargets = batch.targets;
						foreach(Puppet target in batchTargets) {
							bool passedThreshold =
								direction > 0 // Heading towards the right?
									? target.CurrentPosition.x < threshold  // Check if passed to the left
									: target.CurrentPosition.x > threshold; // Check if passed to the right

							if(passedThreshold) {
								// TODO check how this works when destination is same as current one
								//      i.e. batch destination is null
								target.SetMovementDestination(batch.destination ?? DestinationPoint);
								batch.RemoveTarget(target);
							}
						}
					}

				}
				else {
					currentBatches.Clear();

					if(onMoveComplete != null) {
						onMoveComplete.Invoke();
						onMoveComplete = null;
					}
				}
			}
		}
		#endregion

		#region Internals
		private RectTransform rTransform;

		private Slide _activeSlide;
		private StagePoint _destPoint;
		private StagePoint DestinationPoint {
			get {
				return _destPoint;
			}
			set {
				_destPoint?.RemoveInhabitant(gameObject);
				_destPoint = value;
				_destPoint?.AddInhabitant(gameObject, CurrentPosition);
			}
		}

		/// <summary>
		/// Currently active movement batches, used currently for pushing and pulling.
		/// 
		/// Never make this null; make it an empty list.
		/// </summary>
		private List<MoveBatch> currentBatches = new List<MoveBatch>();
		private Action onMoveComplete = null;
		private Speed moveSpeed = Speed.Normal;

		private Vector2 PreviousPosition {
			get; set;
		}
		private Vector2 CurrentPosition {
			get => rTransform.anchorMin;
		}

		private void SetPosition(float newHorizontalPos)
		{
			rTransform.anchorMin = new Vector2(newHorizontalPos, rTransform.anchorMin.y);
			rTransform.anchorMax = new Vector2(newHorizontalPos, rTransform.anchorMax.y);
		}

		private static float CalculateSpeed(
			float startpoint, float endpoint, float position,
			float maxSpeed, float minSpeed,
			float accelerationDistance, AnimationCurve accelerationCurve,
			float decelerationDistance, AnimationCurve decelerationCurve
		) {

			// Velocity line is in shape of
			//
			// maxSpeed       ________
			//               /        \
			//              /          \
			// minSpeed ___/            \___
			//           left         right
			//
			// If start and end are sufficiently close,
			// then the slanted lines intersect,
			// and there is no plateau.
			//
			// All this means we have 4 possible velocities:
			//  1. maxSpeed
			//  2. leftRampSpeed
			//  3. rightRampSpeed
			//  4. minSpeed
			//
			// We'll pick it in this order:
			//  1. Calculate each ramp speed, pick the lesser
			//  2. If that's more than maxSpeed, use maxSpeed
			//  3. If that's less than minSpeed, use minSpeed
			//
			// These are speeds, no velocities.
			// Caller must determine direction.

			float maxDeltaSpeed = maxSpeed - minSpeed;

			bool startedOnLeft = startpoint < endpoint;

			//float leftPoint, leftSlope, rightPoint, rightSlope;
			float leftPoint, rightPoint, leftDist, rightDist;
			AnimationCurve leftCurve, rightCurve;
			if(startedOnLeft) {
				leftPoint = startpoint;
				leftDist = accelerationDistance;
				leftCurve = accelerationCurve;

				rightPoint = endpoint;
				rightDist = decelerationDistance;
				rightCurve = decelerationCurve;
			}
			else {
				leftPoint = endpoint;
				leftDist = decelerationDistance;
				leftCurve = decelerationCurve;

				rightPoint = startpoint;
				rightDist = accelerationDistance;
				rightCurve = accelerationCurve;
			}

			float leftRampSpeed = Mathf.Lerp(minSpeed, maxSpeed, leftCurve.Evaluate((position - leftPoint) / leftDist));
			float rightRampSpeed = Mathf.Lerp(minSpeed, maxSpeed, rightCurve.Evaluate(-(position - rightPoint)/rightDist));

			float rampSpeed = Mathf.Min(leftRampSpeed, rightRampSpeed);
			float clampedSpeed = Mathf.Clamp( rampSpeed, minSpeed, maxSpeed );

			return clampedSpeed;
		}
		#endregion


	}
}
