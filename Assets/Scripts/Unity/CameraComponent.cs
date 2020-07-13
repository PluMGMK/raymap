using OpenSpace;
using System;
using UnityEngine;

// Very simple smooth mouselook modifier for the MainCamera in Unity
// by Francis R. Griffiths-Keam - www.runningdimensions.com
// http://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/


public class CameraComponent : MonoBehaviour {
	public Controller controller;
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);
    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);
    public Quaternion? targetDirection;
    public Vector2 targetCharacterDirection;
    
    public bool mouseLookEnabled = false;
    private bool _shifted = false;
    public float flySpeed = 20f;
    public Camera cam;

    public float lerpFactor = 1f;
    Vector3? targetPos = null;
    Quaternion? targetRot = null;
	float? targetOrthoSize = null;

	private Vector3 lastMousePosition = Vector3.zero;
	private bool panning = false;

	public WebJSON.CameraPos debugCameraPos = WebJSON.CameraPos.Initial;
	private WebJSON.CameraPos lastDebugCameraPos = WebJSON.CameraPos.Initial;

	void Start() {
    }

    public void JumpTo(GameObject gao) {
        Vector3? center = null, size = null;
        BasePersoBehaviour bpb = gao.GetComponent<BasePersoBehaviour>();
        if (bpb != null) {
			switch (bpb) {
				case PersoBehaviour pb:
					//print(pb.perso.SuperObject.boundingVolume.Center + " - " + pb.perso.SuperObject.boundingVolume.Size);
					center = (pb.perso.SuperObject != null && pb.perso.SuperObject.boundingVolume != null) ? (pb.transform.position + pb.perso.SuperObject.boundingVolume.Center) : pb.transform.position;
					size = (pb.perso.SuperObject != null && pb.perso.SuperObject.boundingVolume != null) ? Vector3.Scale(pb.perso.SuperObject.boundingVolume.Size, pb.transform.lossyScale) : pb.transform.lossyScale;
					break;
				case ROMPersoBehaviour rpb:
					center = rpb.transform.position;
					size = rpb.transform.lossyScale;
					break;
				case PS1PersoBehaviour ppb:
					center = ppb.transform.position;
					size = ppb.transform.lossyScale;
					break;
			}
        } else {
            SuperObjectComponent sc = gao.GetComponent<SuperObjectComponent>();
            if (sc != null) {
				if (sc.so != null) {
					center = (gao.transform.position + sc.so.boundingVolume.Center);
					size = Vector3.Scale(sc.so.boundingVolume.Size, gao.transform.lossyScale);
				} else {
					center = gao.transform.position;
					size = gao.transform.lossyScale;
				}
			}
        }
        if (center.HasValue) {
			float objectSize = Mathf.Min(5f, Mathf.Max(size.Value.x, size.Value.y, size.Value.z));
			bool orthographic = cam.orthographic;
			if (orthographic) {
				targetOrthoSize = objectSize * 2f * 1.5f;
				Vector3 target = cam.transform.InverseTransformPoint(center.Value);
				targetPos = cam.transform.TransformPoint(new Vector3(target.x, target.y, 0f));
			} else {
				float cameraDistance = 4.0f; // Constant factor
				float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView); // Visible height 1 meter in front
				float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
				distance += objectSize; // Estimated offset from the center to the outside of the object * 2
				/*transform.position = center.Value + -transform.right * distance;
				transform.LookAt(center.Value, Vector3.up);*/
				//transform.LookAt(center.Value, Vector3.up);
				//transform.position = center.Value + Vector3.Normalize(transform.position - center.Value) * distance;
				targetPos = center.Value + Vector3.Normalize(transform.position - center.Value) * distance;
				if (center.Value - transform.position != Vector3.zero) {
					targetRot = Quaternion.LookRotation(center.Value - transform.position, Vector3.up);
				}
			}
		}
    }

	public void StopLerp() {
		targetPos = null;
		targetRot = null;
		targetOrthoSize = null;
	}

    void Update() {
		if (controller.LoadState != Controller.State.Finished && controller.LoadState != Controller.State.Error) return;
		if (!targetDirection.HasValue) {
			// Set target direction to the camera's initial orientation.
			targetDirection = transform.localRotation;
		}

		if (lastDebugCameraPos!=debugCameraPos) {
			lastDebugCameraPos = debugCameraPos;
			controller.SetCameraPosition(debugCameraPos);
        }

		bool orthographic = cam.orthographic;

		if (Input.GetKeyUp(KeyCode.LeftShift) & _shifted)
            _shifted = false;

        if ((Input.GetKeyDown(KeyCode.LeftShift) & !_shifted) |
            (Input.GetKeyDown(KeyCode.Escape) & mouseLookEnabled)) {
            _shifted = true;

            if (!mouseLookEnabled) {
                mouseLookEnabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else {
                if (Input.GetKeyDown(KeyCode.Escape))
                    _shifted = false;

                mouseLookEnabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

		if (!mouseLookEnabled) {
			if (targetPos.HasValue) {
				if (Vector3.Distance(transform.position, targetPos.Value) < 0.4f) {
					targetPos = null;
				} else {
					transform.position = Vector3.Lerp(transform.position, targetPos.Value, 0.05f * lerpFactor);
				}
			}
			if (targetRot.HasValue) {
				if (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRot.Value)) < 10) {
					targetRot = null;
				} else {
					transform.rotation = Quaternion.Lerp(transform.rotation, targetRot.Value, 0.05f * lerpFactor);
				}
			}

			Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
			lastMousePosition = Input.mousePosition;

			if (orthographic) {
				if (targetOrthoSize.HasValue) {
					if (Mathf.Abs(targetOrthoSize.Value - cam.orthographicSize) < 0.04f) {
						targetOrthoSize = null;
					} else {
						cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize.Value, 0.05f * lerpFactor);
					}
				}

				float orthoFlySpeedMult = (float)Math.Sqrt(Camera.main.orthographicSize);

				if (Input.GetMouseButton(1)) {
					StopLerp();


					Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

					if (panning) {

						float xFactor = Camera.main.orthographicSize * 2.0f / Camera.main.pixelHeight;
						float yFactor = Camera.main.orthographicSize * 2.0f / Camera.main.pixelHeight;

						transform.Translate(Vector3.right * xFactor * -mouseDelta.x, Space.Self);
						transform.Translate(Vector3.up * yFactor * -mouseDelta.y, Space.Self);
					} else if(Input.GetMouseButtonDown(1) && screenRect.Contains(Input.mousePosition)) { // Only start panning if within game window when you click
						panning = true;
                    }
				} else {
					panning = false;
                }

				if (Input.GetAxis("Mouse ScrollWheel") != 0) {
					StopLerp();
					Camera.main.orthographicSize *= (float)Math.Pow(2, (-Input.GetAxis("Mouse ScrollWheel")));
				}

				if (Input.GetAxis("Vertical") != 0) {
					StopLerp();
					transform.Translate(Vector3.up * flySpeed * orthoFlySpeedMult * Time.deltaTime * Input.GetAxis("Vertical"), Space.Self);
				}
				if (Input.GetAxis("Horizontal") != 0) {
					StopLerp();
					transform.Translate(Vector3.right * flySpeed * orthoFlySpeedMult * Time.deltaTime * Input.GetAxis("Horizontal"), Space.Self);
				}
				if (Input.GetAxis("HeightAndZoom") != 0) {
					StopLerp();
					Camera.main.orthographicSize *= (float)Math.Pow(2, (Input.GetAxis("HeightAndZoom") * Time.deltaTime));
				}

			}

		} else {
			StopLerp();
			//ensure these stay this way
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			// Allow the script to clamp based on a desired target value.
			Vector3 eulerTargetDir = targetDirection.Value.eulerAngles;
			if (eulerTargetDir.z != 0) {
				if (Mathf.Abs(eulerTargetDir.z) < 0.04f) {
					eulerTargetDir = new Vector3(eulerTargetDir.x, eulerTargetDir.y, 0f);
				} else {
					eulerTargetDir = new Vector3(eulerTargetDir.x, eulerTargetDir.y, Mathf.Lerp(eulerTargetDir.z, 0f, 0.05f * lerpFactor));
				}
				targetDirection = Quaternion.Euler(eulerTargetDir);
			}
			var targetOrientation = targetDirection.Value;
			var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

			// Get raw mouse input for a cleaner reading on more sensitive mice.
			var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

			// Scale input against the sensitivity setting and multiply that against the smoothing value.
			mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

			// Interpolate mouse movement over time to apply smoothing delta.
			_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
			_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

			// Find the absolute mouse movement value from point zero.
			_mouseAbsolute += _smoothMouse;

			// Clamp and apply the local x value first, so as not to be affected by world transforms.
			if (clampInDegrees.x < 360)
				_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

			// Then clamp and apply the global y value.
			if (clampInDegrees.y < 360)
				_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

			if (!orthographic) {
				var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
				transform.localRotation = xRotation * targetOrientation;

				// If there's a character body that acts as a parent to the camera
				var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
				transform.localRotation *= yRotation;
			}

			//movement
			if (orthographic) {

				float orthoFlySpeedMult = (float)Math.Sqrt(Camera.main.orthographicSize);

				if (Input.GetAxis("Mouse ScrollWheel") != 0) {
					Camera.main.orthographicSize *= (float)Math.Pow(2, (-Input.GetAxis("Mouse ScrollWheel")));
				}

				if (Input.GetAxis("Vertical") != 0) {
					transform.Translate(Vector3.up * flySpeed * orthoFlySpeedMult * Time.deltaTime * Input.GetAxis("Vertical"), Space.Self);
				}
				if (Input.GetAxis("Horizontal") != 0) {
					transform.Translate(Vector3.right * flySpeed * orthoFlySpeedMult * Time.deltaTime * Input.GetAxis("Horizontal"), Space.Self);
				}
				if (Input.GetAxis("HeightAndZoom") != 0) {
					Camera.main.orthographicSize *= (float)Math.Pow(2, (Input.GetAxis("HeightAndZoom") * Time.deltaTime)); 
				}

			}
			else {
				if (Input.GetAxis("Vertical") != 0) {
					transform.Translate(cam.transform.forward * flySpeed * Time.deltaTime * Input.GetAxis("Vertical"), Space.World);
				}
				if (Input.GetAxis("Horizontal") != 0) {
					transform.Translate(cam.transform.right * flySpeed * Time.deltaTime * Input.GetAxis("Horizontal"), Space.World);
				}
				if (Input.GetAxis("HeightAndZoom") != 0) {
					transform.Translate(Vector3.up * flySpeed * Time.deltaTime * Input.GetAxis("HeightAndZoom"), Space.World);
				}
			}
			if (Input.GetKey(KeyCode.Keypad8)) {
				transform.Translate(Vector3.up * flySpeed * Time.deltaTime * 0.5f, Space.World);
			} else if (Input.GetKey(KeyCode.Keypad2)) {
				transform.Translate(-Vector3.up * flySpeed * Time.deltaTime * 0.5f, Space.World);
			}
			if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) {
				flySpeed += 1f;
			} else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) {
				if (flySpeed > 0) flySpeed -= 1f;
			}
		}
    }
}
