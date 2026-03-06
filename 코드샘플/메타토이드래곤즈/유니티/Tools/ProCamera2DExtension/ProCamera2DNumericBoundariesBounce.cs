using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class ProCamera2DNumericBoundariesBounce : ProCamera2DNumericBoundaries, IPositionDeltaChanger, IPositionOverrider
	{
		[SerializeField]
		protected float bounceTop = 0.5f;
		[SerializeField]
		protected float bounceLeft = 0.5f;
		[SerializeField]
		protected float bounceRight = 0.5f;
		[SerializeField]
		protected float bounceBottom = 0.5f;

		int _poOrder = 0;
		public int POOrder { get { return _poOrder; } set { _poOrder = value; } }

		Vector3 _originalDelta = Vector3.zero;
		int _elapsedBoundaryDelayFrames = 0;
		bool _useSoftBoundariesEditorSetting = false;

		ProCamera2DPanAndZoom _pan = null;

		protected override void Awake()
		{
			base.Awake();

			_pan = GetComponent<ProCamera2DPanAndZoom>();
			ProCamera2D.AddPositionOverrider(this);
		}

		public Vector3 OverridePosition(float deltaTime, Vector3 originalPosition)
		{
			if (!enabled || _pan.IsPanning || _pan.IsZooming)
				return originalPosition;

			Vector3 newPos = Vector3.one * 5f;

			var newPosH = Vector3H(originalPosition);
			var newPosV = Vector3V(originalPosition);
			var halfScreenWidth = ProCamera2D.ScreenSizeInWorldCoordinates.x * .5f;
			var halfScreenHeight = ProCamera2D.ScreenSizeInWorldCoordinates.y * .5f;

			float offsetTop = TopBoundary - halfScreenHeight - bounceTop;
			float offsetLeft = LeftBoundary + halfScreenWidth + bounceLeft;
			float offsetRight = RightBoundary - halfScreenWidth - bounceRight;
			float offsetBottom = BottomBoundary + halfScreenHeight + bounceBottom;

			if (offsetLeft <= offsetRight)
			{
				if (UseLeftBoundary && newPosH - halfScreenWidth < LeftBoundary + bounceLeft)
				{
					if (Vector3H(_originalDelta) <= 0f)
					{
						if (ProCamera2D.CameraTargets != null)
						{
							for (int i = 0, count = ProCamera2D.CameraTargets.Count; i < count; ++i)
							{
								ProCamera2D.CameraTargets[i].TargetTransform.position = new Vector3(offsetLeft, ProCamera2D.TargetsMidPoint.y, ProCamera2D.TargetsMidPoint.z);
							}
						}
					}
				}

				if (UseRightBoundary && newPosH + halfScreenWidth > RightBoundary - bounceRight)
				{
					if (Vector3H(_originalDelta) >= 0f)
					{
						if (ProCamera2D.CameraTargets != null)
						{
							for (int i = 0, count = ProCamera2D.CameraTargets.Count; i < count; ++i)
							{
								ProCamera2D.CameraTargets[i].TargetTransform.position = new Vector3(offsetRight, ProCamera2D.TargetsMidPoint.y, ProCamera2D.TargetsMidPoint.z);
							}
						}
					}
				}
			}

			if (offsetBottom <= offsetTop)
			{
				if (UseBottomBoundary && newPosV - halfScreenHeight < BottomBoundary + bounceBottom)
				{
					if (Vector3V(_originalDelta) <= 0f)
					{
						if (ProCamera2D.CameraTargets != null)
						{
							for (int i = 0, count = ProCamera2D.CameraTargets.Count; i < count; ++i)
							{
								ProCamera2D.CameraTargets[i].TargetTransform.position = new Vector3(ProCamera2D.TargetsMidPoint.x, offsetBottom, ProCamera2D.TargetsMidPoint.z);
							}
						}
					}
				}

				if (UseTopBoundary && newPosV + halfScreenHeight > TopBoundary - bounceTop)
				{
					if (Vector3V(_originalDelta) >= 0f)
					{
						if (ProCamera2D.CameraTargets != null)
						{
							for (int i = 0, count = ProCamera2D.CameraTargets.Count; i < count; ++i)
							{
								ProCamera2D.CameraTargets[i].TargetTransform.position = new Vector3(ProCamera2D.TargetsMidPoint.x, offsetTop, ProCamera2D.TargetsMidPoint.z);
							}
						}
					}
				}
			}

			return originalPosition;
		}

		public new Vector3 AdjustDelta(float deltaTime, Vector3 originalDelta)
		{
			_originalDelta = originalDelta;
			if (!enabled || !UseNumericBoundaries)
				return originalDelta;

			if (UseBoundaryDelayOnEnterScene && _elapsedBoundaryDelayFrames < BoundaryDelayFrames)
			{
				UseSoftBoundaries = false;
				_elapsedBoundaryDelayFrames++;
			}
			else
				UseSoftBoundaries = _useSoftBoundariesEditorSetting;

			// Check movement in the horizontal dir
			IsCameraPositionHorizontallyBounded = false;
			ProCamera2D.IsCameraPositionLeftBounded = false;
			ProCamera2D.IsCameraPositionRightBounded = false;
			IsCameraPositionVerticallyBounded = false;
			ProCamera2D.IsCameraPositionTopBounded = false;
			ProCamera2D.IsCameraPositionBottomBounded = false;
			var newPosH = Vector3H(ProCamera2D.LocalPosition) + Vector3H(originalDelta);
			var newPosV = Vector3V(ProCamera2D.LocalPosition) + Vector3V(originalDelta);
			var halfScreenWidth = ProCamera2D.ScreenSizeInWorldCoordinates.x * .5f;
			var halfScreenHeight = ProCamera2D.ScreenSizeInWorldCoordinates.y * .5f;
			var cushionH = UseSoftBoundaries ? ProCamera2D.ScreenSizeInWorldCoordinates.x * SoftAreaSize : 0f;
			var cushionV = UseSoftBoundaries ? ProCamera2D.ScreenSizeInWorldCoordinates.y * SoftAreaSize : 0f;
			if (UseLeftBoundary && newPosH - halfScreenWidth < LeftBoundary + cushionH)
			{
                newPosH = Mathf.Max(LeftBoundary + halfScreenWidth, newPosH);

                IsCameraPositionHorizontallyBounded = true;
				ProCamera2D.IsCameraPositionLeftBounded = true;
			}

			if (UseRightBoundary && newPosH + halfScreenWidth > RightBoundary - cushionH)
			{
                newPosH = Mathf.Min(RightBoundary - halfScreenWidth, newPosH);

                IsCameraPositionHorizontallyBounded = true;
				ProCamera2D.IsCameraPositionRightBounded = true;
			}

			// Check movement in the vertical dir
			if (UseBottomBoundary && newPosV - halfScreenHeight < BottomBoundary + cushionV)
			{
                newPosV = Mathf.Max(BottomBoundary + halfScreenHeight, newPosV);

                IsCameraPositionVerticallyBounded = true;
				ProCamera2D.IsCameraPositionBottomBounded = true;
			}

			if (UseTopBoundary && newPosV + halfScreenHeight > TopBoundary - cushionV)
			{
                newPosV = Mathf.Min(TopBoundary - halfScreenHeight, newPosV);

                IsCameraPositionVerticallyBounded = true;
				ProCamera2D.IsCameraPositionTopBounded = true;
			}

			// Return the new delta
			return VectorHV(newPosH - Vector3H(ProCamera2D.LocalPosition), newPosV - Vector3V(ProCamera2D.LocalPosition));
		}
	}
}