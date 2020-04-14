using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace OW_TAIcheat
{
	public static class MyExtensions
	{
		public static void TAIcheat_SetTranslationalThrust(this JetpackThrusterModel jet, float newacc)
		{
			float oldtrst = jet.GetMaxTranslationalThrust();
			float oldbst = jet.GetBoostMaxThrust();
			FieldInfo fiboost = typeof(JetpackThrusterModel).GetField("_boostThrust", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo fithrust = typeof(JetpackThrusterModel).GetField("_maxTranslationalThrust", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fiboost != null && fithrust != null)
			{
				fiboost.SetValue(jet, (object)(oldbst * (newacc / oldtrst)));
				fithrust.SetValue(jet, (object)(newacc));
			}
		}
	}
	public class DebugInput : ModBehaviour
	{
		private RelativeLocationData relconstr(Vector3 body_position, Quaternion body_rotation, Vector3 body_velocity, OWRigidbody relativeBody, Transform relativeTransform = null)
		{
			if (relativeTransform == null)
			{
				relativeTransform = relativeBody.transform;
			}
			RelativeLocationData res = new RelativeLocationData(Locator.GetPlayerBody(), relativeBody);
			res.localPosition = relativeTransform.InverseTransformPoint(body_position);
			res.localRotation = Quaternion.Inverse(relativeTransform.rotation) * body_rotation;
			res.localRelativeVelocity = relativeTransform.InverseTransformDirection(body_velocity - relativeBody.GetPointVelocity(body_position));
			return res;
		}

		public static IModConsole console;
		public static GameObject player;
		public static PlayerSpacesuit playersuit;

		private void Start()
		{
			console = ModHelper.Console;
			ModHelper.Console.WriteLine("TAICheat ready!");

		}

		private void LateUpdate()
		{
			if (_playerController != null)
				this._gForce = this._playerController.GetNormalAccelerationScalar();
		}

		private void OnGUI()
		{
			if (_playerController == null || _playerForceDetector == null)
			{
				this._playerForceDetector = Locator.GetPlayerForceDetector();
				this._playerController = Locator.GetPlayerController();
				if (_playerController == null || _playerForceDetector == null) return;
			}
			float num = 400f;
			if (GUIMode.IsHiddenMode() || PlayerState.UsingShipComputer())
			{
				return;
			}
			if (DebugInput.inputHUD == 1)
			{
				GUI.Label(new Rect(10f + num, 10f, 200f, 20f), "Time Scale: " + Mathf.Round(Time.timeScale * 100f) / 100f);
				GUI.Label(new Rect(10f + num, 25f, 200f, 20f), string.Concat(new object[]
				{
				"Time Remaining: ",
				Mathf.Floor(TimeLoop.GetSecondsRemaining() / 60f),
				":",
				Mathf.Round(TimeLoop.GetSecondsRemaining() % 60f * 100f / 100f)
				}));
				GUI.Label(new Rect(10f + num, 40f, 200f, 20f), "Loop Count: " + TimeLoop.GetLoopCount());
				GUI.Label(new Rect(10f + num, 55f, 90f, 40f), "PauseFlags: ");
				GUI.Label(new Rect(100f + num, 55f, 50f, 40f), "MENU\n" + ((!OWTime.IsPaused(OWTime.PauseType.Menu)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(150f + num, 55f, 50f, 40f), "LOAD\n" + ((!OWTime.IsPaused(OWTime.PauseType.Loading)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(200f + num, 55f, 50f, 40f), "READ\n" + ((!OWTime.IsPaused(OWTime.PauseType.Reading)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(250f + num, 55f, 50f, 40f), "SLP\n" + ((!OWTime.IsPaused(OWTime.PauseType.Sleeping)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(300f + num, 55f, 50f, 40f), "INIT\n" + ((!OWTime.IsPaused(OWTime.PauseType.Initializing)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(350f + num, 55f, 50f, 40f), "STRM\n" + ((!OWTime.IsPaused(OWTime.PauseType.Streaming)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(400f + num, 55f, 50f, 40f), "SYS\n" + ((!OWTime.IsPaused(OWTime.PauseType.System)) ? "FALSE" : "TRUE "));
				GUI.Label(new Rect(10f + num, 85f, 200f, 20f), "Input Mode: " + OWInput.GetInputMode().ToString());
				this._inputModeArray = OWInput.GetInputModeStack();
				GUI.Label(new Rect(10f + num, 100f, 200f, 20f), "Input Mode Stack: ");
				int num2 = 150;
				int num3 = 0;
				while (num3 < this._inputModeArray.Length && this._inputModeArray[num3] != InputMode.None)
				{
					GUI.Label(new Rect((float)num2 + num, 100f, 200f, 20f), this._inputModeArray[num3].ToString());
					num2 += 75;
					num3++;
				}
				GUI.Label(new Rect(10f + num, 115f, 300f, 20f), "Net Force Accel: " + Mathf.Round(this._playerForceDetector.GetForceAcceleration().magnitude * 100f) / 100f);
				GUI.Label(new Rect(210f + num, 115f, 300f, 20f), "G-Force: " + Mathf.Round(this._gForce * 100f) / 100f);
				GUI.Label(new Rect(10f + num, 130f, 200f, 20f), "Load Time: " + LoadTimeTracker.GetLatestLoadTime());
				if (DynamicResolutionManager.isEnabled)
				{
					GUI.Label(new Rect(10f + num, 145f, 200f, 20f), "Resolution Scale: " + DynamicResolutionManager.currentResolutionScale);
				}
				GUI.Label(new Rect(10f + num, 160f, 200f, 20f), "Player Speed: " + (Locator.GetCenterOfTheUniverse().GetOffsetVelocity() + Locator.GetPlayerBody().GetVelocity()).magnitude.ToString());
				GUI.Label(new Rect(210f + num, 160f, 200f, 20f), "Player Accel: " + Locator.GetPlayerBody().GetAcceleration().magnitude.ToString());
				if (Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>())
				{
					GUI.Label(new Rect(10f + num, 175f, 200f, 20f), string.Concat(new object[]
					{
					"Jetpack Max Accel: ",
					Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>().GetMaxTranslationalThrust().ToString(),
					"/",
					Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>().GetBoostMaxThrust().ToString()
					}));
				}
				if (Locator.GetShipBody().GetComponent<ShipThrusterModel>())
				{
					GUI.Label(new Rect(210f + num, 175f, 200f, 20f), "Ship Max Accel: " + Locator.GetShipBody().GetComponent<ShipThrusterModel>().GetMaxTranslationalThrust().ToString());
				}
				GUI.Label(new Rect(10f + num, 190f, 400f, 20f), string.Concat(new object[]
				{
				"Inspector layer: ",
				DebugInput.rayMask,
				" ",
				LayerMask.LayerToName(DebugInput.rayMask)
				}));
				if (DebugInput.GetWarpOWRigidbody())
				{
					GUI.Label(new Rect(10f + num, 205f, 400f, 20f), string.Concat(new string[]
					{
					"Warp Body: ",
					DebugInput.GetWarpOWRigidbody().gameObject.name,
					" layer: ",
					DebugInput.GetWarpOWRigidbody().gameObject.layer.ToString(),
					" ",
					LayerMask.LayerToName(DebugInput.GetWarpOWRigidbody().gameObject.layer)
					}));
				}
				if (DebugInput.hit.collider)
				{
					GUI.Label(new Rect(10f + num, 220f, 400f, 20f), string.Concat(new string[]
					{
					"Latest hit layer: ",
					DebugInput.hit.collider.gameObject.layer.ToString(),
					" ",
					LayerMask.LayerToName(DebugInput.hit.collider.gameObject.layer)
					}));
					GUI.Label(new Rect(10f + num, 235f, 600f, 20f), "Name: " + DebugInput.hit.collider.gameObject.name + " Distance: " + (DebugInput.hit.point - Locator.GetPlayerBody().transform.position).magnitude.ToString());
				}
				/*if (PadEZ.PadManager.GetActiveController()!=null)
				{
					GUI.Label(new Rect(10f + num, 250f, 600f, 20f), PadEZ.PadManager.GetActiveController().GetIndex().ToString() + " " + PadEZ.PadManager.GetActiveController().GetPadType().ToString() +" "+ UnityEngine.Input.GetJoystickNames()[PadEZ.PadManager.GetActiveController().GetIndex()]);
				}*/
			}
			if (DebugInput.inputHUD == 2)
			{
				GUI.Label(new Rect(10f, 10f, 300f, 2500f), ReadInputManager.ReadCommandInputs(false));
			}
			if (DebugInput.inputHUD == 3)
			{
				GUI.Label(new Rect(0f, 0f, 300f, 2500f), ReadInputManager.ReadCommandInputs(false));
				GUI.Label(new Rect(300f, 0f, 300f, 2500f), ReadInputManager.ReadCommandInputs(true));
			}
			if (DebugInput.inputHUD == 4)
			{
				GUI.Label(new Rect(0f, 0f, 500f, 2500f), ReadInputManager.ReadInputAxes());
				GUI.Label(new Rect(500f, 0f, 500f, 2500f), ReadInputManager.ReadRawInputManagerButtons());
			}
		}

		private ForceDetector _playerForceDetector;
		private PlayerCharacterController _playerController;
		private InputMode[] _inputModeArray;
		private MeshRenderer[] _thrusterArrowRenderers;
		private float _gForce;

		private void FixedUpdate()
		{
			if (this._gotoWarpPointNextFrame)
			{
				this._gotoWarpPointNextFrame = false;
				Locator.GetPlayerBody().MoveToRelativeLocation(DebugInput._relativeData[DebugInput.relIndex], DebugInput._relativeBody[DebugInput.relIndex], null);
			}
			if (this._engageLudicrousSpeed)
			{
				this._engageLudicrousSpeed = false;
				Locator.GetShipBody().AddVelocityChange(Locator.GetShipBody().transform.forward * this.ludicrousMult * 25000f);
			}
		}

		private void Update()
		{
			this.shiftPressed = global::Input.GetKey(KeyCode.LeftShift) || global::Input.GetKey(KeyCode.RightShift);
			this.ctrlPressed = global::Input.GetKey(KeyCode.LeftControl) || global::Input.GetKey(KeyCode.RightControl);
			this.altPressed = global::Input.GetKey(KeyCode.LeftAlt) || global::Input.GetKey(KeyCode.RightAlt);
			if (global::Input.GetKeyDown(KeyCode.BackQuote))
			{
				DebugInput.cheatsOn = !DebugInput.cheatsOn;
				if (DebugInput.cheatsOn)
					AudioSource.PlayClipAtPoint(Locator.GetAudioManager().GetAudioClipArray(global::AudioType.NomaiPowerOn)[0], Locator.GetActiveCamera().transform.position);
				else
					AudioSource.PlayClipAtPoint(Locator.GetAudioManager().GetAudioClipArray(global::AudioType.NomaiPowerOff)[0], Locator.GetActiveCamera().transform.position);
			}

			if (playersuit == null)
				playersuit = Locator.GetPlayerSuit();
			if (playersuit == null)
				return;
			if (!playersuit.enabled)
				playersuit = Locator.GetPlayerSuit();
			if (playersuit == null || !playersuit.enabled)
				return;
			if (DebugInput.cheatsOn)
			{
				if (global::Input.GetKeyDown(KeyCode.PageDown))
				{
					if (this.shiftPressed || this.ctrlPressed)
					{
						if (Locator.GetProbe().GetAnchor().IsAnchored())
						{
							Transform transform = Locator.GetProbe().transform;
							DebugInput._relativeBody[DebugInput.relIndex] = transform.parent.GetAttachedOWRigidbody(false);
							DebugInput._relativeData[DebugInput.relIndex] = new RelativeLocationData(Locator.GetProbe().GetAnchor().GetAttachedOWRigidbody(), DebugInput._relativeBody[DebugInput.relIndex], null);
							this.COn = true;
						}
					}
					else if (this.altPressed)
					{
						OWCamera activeCamera = Locator.GetActiveCamera();
						Vector3 position = new Vector3((float)(activeCamera.pixelWidth - 1) / 2f, (float)(activeCamera.pixelHeight - 1) / 2f);
						if (!Physics.Raycast(activeCamera.ScreenPointToRay(position), out DebugInput.hit, float.PositiveInfinity, OWLayerMask.BuildPhysicalMask().value))
						{
							foreach (RaycastHit raycastHit in Physics.RaycastAll(activeCamera.ScreenPointToRay(position), float.PositiveInfinity, OWLayerMask.BuildPhysicalMask().value | 524288))
							{
								DebugInput.hit = raycastHit;
								if (raycastHit.collider.GetAttachedOWRigidbody(false))
								{
									break;
								}
							}
							if (!DebugInput.hit.collider.GetAttachedOWRigidbody(false))
							{
								foreach (RaycastHit raycastHit2 in Physics.RaycastAll(activeCamera.ScreenPointToRay(position)))
								{
									DebugInput.hit = raycastHit2;
									if (raycastHit2.collider.GetAttachedOWRigidbody(false))
									{
										break;
									}
								}
							}
						}
						if (DebugInput.hit.collider.GetAttachedOWRigidbody(false))
						{
							DebugInput._hasSetWarpPoint[DebugInput.relIndex] = true;
							DebugInput._relativeBody[DebugInput.relIndex] = DebugInput.hit.rigidbody.GetAttachedOWRigidbody(false);
							DebugInput._relativeData[DebugInput.relIndex] = relconstr(DebugInput.hit.point, Quaternion.FromToRotation(Locator.GetPlayerBody().transform.up, DebugInput.hit.normal) * Locator.GetPlayerBody().transform.rotation, DebugInput._relativeBody[DebugInput.relIndex].GetPointVelocity(DebugInput.hit.point), DebugInput._relativeBody[DebugInput.relIndex], null);
							this.COn = true;
						}
					}
					else if (Locator.GetPlayerSectorDetector().GetLastEnteredSector() != null)
					{
						DebugInput._hasSetWarpPoint[DebugInput.relIndex] = true;
						DebugInput._relativeBody[DebugInput.relIndex] = Locator.GetPlayerSectorDetector().GetLastEnteredSector().GetOWRigidbody();
						DebugInput._relativeData[DebugInput.relIndex] = new RelativeLocationData(Locator.GetPlayerBody(), DebugInput._relativeBody[DebugInput.relIndex], null);
						this.COn = true;
					}
				}
				if (global::Input.GetKeyDown(KeyCode.Home))
				{
					if (this.altPressed)
					{
						DebugInput.rayMask++;
						DebugInput.rayMask %= 32;
					}
					else
					{
						OWCamera activeCamera2 = Locator.GetActiveCamera();
						Vector3 position2 = new Vector3((float)(activeCamera2.pixelWidth - 1) / 2f, (float)(activeCamera2.pixelHeight - 1) / 2f);
						if (!Physics.Raycast(activeCamera2.ScreenPointToRay(position2), out DebugInput.hit, float.PositiveInfinity, 1 << DebugInput.rayMask) && DebugInput.hit.collider.GetAttachedOWRigidbody(false))
						{
							DebugInput._hasSetWarpPoint[DebugInput.relIndex] = true;
							DebugInput._relativeBody[DebugInput.relIndex] = DebugInput.hit.rigidbody.GetAttachedOWRigidbody(false);
							DebugInput._relativeData[DebugInput.relIndex] = relconstr(DebugInput.hit.point, Quaternion.FromToRotation(Locator.GetPlayerBody().transform.up, DebugInput.hit.normal) * Locator.GetPlayerBody().transform.rotation, DebugInput._relativeBody[DebugInput.relIndex].GetPointVelocity(DebugInput.hit.point), DebugInput._relativeBody[DebugInput.relIndex], null);
							this.COn = true;
						}
					}
				}
				if (global::Input.GetKeyDown(KeyCode.PageUp))
				{
					if (this.altPressed)
					{
						this.COn = true;
						DebugInput.relIndex++;
						DebugInput.relIndex %= 10;
					}
					else if (DebugInput._hasSetWarpPoint[DebugInput.relIndex])
					{
						this.COn = true;
						this._gotoWarpPointNextFrame = true;
					}
				}
				/*if (global::Input.GetKeyDown(DebugKeyCode.destroyTimeline))
				{
					Debug.Log("Try DestroyTimeline (Requires NomaiExperimentBlackHole)");
					GlobalMessenger.FireEvent("DebugTimelineDestroyed");
				}
				if (global::Input.GetKeyDown(DebugKeyCode.uiTestAndSuicide))
				{
					Locator.GetPlayerTransform().GetComponent<PlayerResources>().SetDebugKillResources(true);
				}
				if (global::Input.GetKeyUp(DebugKeyCode.uiTestAndSuicide))
				{
					Locator.GetPlayerTransform().GetComponent<PlayerResources>().SetDebugKillResources(false);
				}*/
				if (global::Input.GetKeyDown(KeyCode.G))
				{
					if (this.altPressed)
					{
						if (PlayerData.IsLoaded())
						{
							PlayerData.LearnLaunchCodes();
							this.COn = true;
						}
					}
					else if (!Locator.GetPlayerSuit().IsWearingSuit(true))
					{
						Locator.GetPlayerSuit().SuitUp(false, false);
						this.COn = true;
					}
					else
					{
						Locator.GetPlayerSuit().RemoveSuit(false);
						this.COff = true;
					}
				}
				if (global::Input.GetKeyDown(KeyCode.H))
				{
					if (this.altPressed)
					{
						DebugInput.hiddenHUD = !DebugInput.hiddenHUD;
						if (DebugInput.hiddenHUD)
						{
							oldmode = (int)typeof(GUIMode).GetAnyField("_renderMode").GetValue(null);
							typeof(GUIMode).GetAnyField("_renderMode").SetValue(null, 7);
							this.COn = true;
						}
						else
						{
							typeof(GUIMode).GetAnyField("_renderMode").SetValue(null, oldmode);
							this.COff = true;
						}
						GlobalMessenger.FireEvent("OnChangeGUIMode");
					}
					else if (Locator.GetPlayerSuit() && Locator.GetPlayerSuit().IsWearingSuit(true))
					{
						if (Locator.GetPlayerSuit().IsWearingHelmet())
						{
							Locator.GetPlayerSuit().RemoveHelmet();
							this.COff = true;
						}
						else
						{
							Locator.GetPlayerSuit().PutOnHelmet();
							this.COn = true;
						}
					}
				}
				if (global::Input.GetKeyDown(KeyCode.J))
				{
					if (this.altPressed && Locator.GetShipTransform())
					{
						if (this.shiftPressed)
						{
							if (Locator.GetShipTransform())
							{
								UnityEngine.Object.Destroy(Locator.GetShipTransform().gameObject);
								this.COn = true;
							}
						}
						else
						{
							ShipComponent[] componentsInChildren = Locator.GetShipTransform().GetComponentsInChildren<ShipComponent>();
							for (int k = 0; k < componentsInChildren.Length; k++)
							{
								componentsInChildren[k].SetDamaged(true);
							}
							this.COn = true;
						}
					}
					else
					{
						Locator.GetPlayerTransform().GetComponent<PlayerResources>().DebugRefillResources();
						if (Locator.GetShipTransform())
						{
							ShipComponent[] componentsInChildren2 = Locator.GetShipTransform().GetComponentsInChildren<ShipComponent>();
							for (int l = 0; l < componentsInChildren2.Length; l++)
							{
								componentsInChildren2[l].SetDamaged(false);
							}
						}
						this.COn = true;
					}
				}
				if (global::Input.GetKeyDown(KeyCode.L))
				{
					if (this.shiftPressed)
					{
						this.ludicrousMult *= 2f;
						this.COn = true;
					}
					else if (this.altPressed)
					{
						this.ludicrousMult /= 2f;
						this.COff = true;
					}
					else
					{
						this._engageLudicrousSpeed = true;
						AudioSource.PlayClipAtPoint(Locator.GetAudioManager().GetAudioClipArray(global::AudioType.ToolProbeLaunch)[0], Locator.GetPlayerBody().transform.position);
					}
				}
				if (global::Input.GetKeyDown(KeyCode.P))
				{
					if (this.altPressed)
					{
						if (Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>())
						{
							if (!this.wasBoosted)
							{
								this.jetpackStanard = Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>().GetMaxTranslationalThrust();
								Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>().TAIcheat_SetTranslationalThrust(50f);
							}
							else
							{
								Locator.GetPlayerSuit().GetComponent<JetpackThrusterModel>().TAIcheat_SetTranslationalThrust(this.jetpackStanard);
							}
							this.wasBoosted = !this.wasBoosted;
							if (this.wasBoosted)
							{
								this.COn = true;
							}
							else
							{
								this.COff = true;
							}
						}
					}
					else
					{
						Locator.GetPlayerTransform().GetComponent<PlayerResources>().ToggleInvincibility();
						Locator.GetDeathManager().ToggleInvincibility();
						Transform shipTransform = Locator.GetShipTransform();
						if (shipTransform)
						{
							shipTransform.GetComponentInChildren<ShipDamageController>().ToggleInvincibility();
							invincible = !invincible;
						}
						if (invincible)
						{
							this.COn = true;
						}
						else
						{
							this.COff = true;
						}
					}
				}
				if (global::Input.GetKeyDown(KeyCode.O))
				{
					if (this.altPressed)
					{
						if (PlayerData.KnowsMultipleFrequencies())
						{
							PlayerData.ForgetFrequency(SignalFrequency.Quantum);
							PlayerData.ForgetFrequency(SignalFrequency.EscapePod);
							PlayerData.ForgetFrequency(SignalFrequency.Statue);
							PlayerData.ForgetFrequency(SignalFrequency.WarpCore);
							PlayerData.ForgetFrequency(SignalFrequency.HideAndSeek);
							this.COff = true;
						}
						else
						{
							this.COn = true;
							for (int m = 10; m < 16; m++)
							{
								PlayerData.LearnSignal((SignalName)m);
							}
							PlayerData.LearnFrequency(SignalFrequency.Quantum);
							for (int n = 20; n < 26; n++)
							{
								PlayerData.LearnSignal((SignalName)n);
							}
							PlayerData.LearnFrequency(SignalFrequency.EscapePod);
							for (int num = 30; num < 33; num++)
							{
								PlayerData.LearnSignal((SignalName)num);
							}
							PlayerData.LearnFrequency(SignalFrequency.Statue);
							PlayerData.LearnFrequency(SignalFrequency.WarpCore);
							for (int num2 = 40; num2 < 50; num2++)
							{
								PlayerData.LearnSignal((SignalName)num2);
							}
							PlayerData.LearnFrequency(SignalFrequency.HideAndSeek);
							for (int num3 = 60; num3 < 63; num3++)
							{
								PlayerData.LearnSignal((SignalName)num3);
							}
						}
					}
					else
					{
						Locator.GetShipLogManager().RevealAllFacts(this._revealRumorsOnly);
						this._revealRumorsOnly = false;
						this.COn = true;
					}
				}
				if (global::Input.GetKeyDown(KeyCode.Backslash))
				{
					DebugInput.inputHUD++;
					DebugInput.inputHUD %= 5;
				}
				if (global::Input.GetKeyDown(KeyCode.N))
				{
					if (this.altPressed)
					{
						if (Locator.GetPlayerBody().GetRequiredComponent<Rigidbody>().detectCollisions)
						{
							Locator.GetPlayerBody().DisableCollisionDetection();
							this.COn = true;
						}
						else
						{
							Locator.GetPlayerBody().EnableCollisionDetection();
							this.COff = true;
						}
					}
					else
					{
						foreach (Collider collider in Locator.GetPlayerBody().GetComponentsInChildren<Collider>())
						{
							if (!collider.isTrigger)
							{
								collider.enabled = !collider.enabled;
								if (collider.enabled)
								{
									this.COff = true;
								}
								else
								{
									this.COn = true;
								}
							}
						}
					}
				}
				if (global::Input.GetKeyDown(KeyCode.M))
				{
					if (this.altPressed)
					{
						if (Locator.GetShipBody().GetRequiredComponent<Rigidbody>().detectCollisions)
						{
							Locator.GetShipBody().DisableCollisionDetection();
							this.COn = true;
						}
						else
						{
							Locator.GetShipBody().EnableCollisionDetection();
							this.COff = true;
						}
					}
					else
					{
						foreach (Collider collider2 in Locator.GetShipTransform().GetComponentsInChildren<Collider>())
						{
							if (!collider2.isTrigger)
							{
								collider2.enabled = !collider2.enabled;
								if (collider2.enabled)
								{
									this.COff = true;
								}
								else
								{
									this.COn = true;
								}
							}
						}
					}
				}
				if (global::Input.GetKeyDown(KeyCode.K))
				{
					foreach (AnglerfishController anglerfishController in UnityEngine.Object.FindObjectsOfType<AnglerfishController>())
					{
						if (this.altPressed)
						{
							anglerfishController.gameObject.SetActive(!anglerfishController.gameObject.activeInHierarchy);
							if (anglerfishController.gameObject.activeInHierarchy)
							{
								this.COff = true;
							}
							else
							{
								this.COn = true;
							}
						}
						else
						{
							anglerfishController.enabled = !anglerfishController.enabled;
							if (anglerfishController.enabled)
							{
								this.COff = true;
							}
							else
							{
								this.COn = true;
							}
						}
					}
				}
			}
			if (global::Input.GetKeyDown(KeyCode.Delete))
			{
				if (this.altPressed)
				{
					this.COn = true;
					FragmentIntegrity[] array2 = UnityEngine.Object.FindObjectsOfType<FragmentIntegrity>();
					for (int j = 0; j < array2.Length; j++)
					{
						array2[j].AddDamage(10000f);
					}
				}
				else
				{
					this.COn = true;
					GlobalMessenger.FireEvent("TriggerSupernova");
				}
			}
			if (global::Input.GetKey(KeyCode.E) && global::Input.GetKey(KeyCode.U))
			{
				if (PlayerData.GetWarpedToTheEye()) PlayerData.SaveEyeCompletion();
				else GlobalMessenger.FireEvent("DebugWarpVessel");
				this.COn = true;
			}
			if (global::Input.GetKeyDown(DebugKeyCode.timeLapse))
			{
				Time.timeScale = 10f;
			}
			else if (global::Input.GetKeyUp(DebugKeyCode.timeLapse))
			{
				Time.timeScale = 1f;
			}

			if (this.COn)
			{
				AudioClip[] audioClipArray = Locator.GetAudioManager().GetAudioClipArray(global::AudioType.Menu_Confirm);
				AudioSource.PlayClipAtPoint(audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], Locator.GetActiveCamera().transform.position);
				this.COn = false;
			}
			if (this.COff)
			{
				AudioClip[] audioClipArray2 = Locator.GetAudioManager().GetAudioClipArray(global::AudioType.Menu_Cancel);
				AudioSource.PlayClipAtPoint(audioClipArray2[UnityEngine.Random.Range(0, audioClipArray2.Length)], Locator.GetActiveCamera().transform.position);
				this.COff = false;
			}
		}

		public static OWRigidbody GetWarpOWRigidbody()
		{
			return DebugInput._relativeBody[DebugInput.relIndex];
		}

		private bool altPressed, shiftPressed, ctrlPressed;
		public static bool cheatsOn;
		private bool COff, COn, CMOn, CMOff;

		private bool _revealRumorsOnly = true;

		private bool _engageLudicrousSpeed;
		private float ludicrousMult = 1f;

		private bool invincible = false;

		private bool wasBoosted;
		private float jetpackStanard = 6f;


		private bool _gotoWarpPointNextFrame;
		private static RelativeLocationData[] _relativeData = new RelativeLocationData[10];
		private static OWRigidbody[] _relativeBody = new OWRigidbody[10];
		private static bool[] _hasSetWarpPoint = new bool[10];
		private static int relIndex;
		public static RaycastHit hit;
		public static int rayMask;

		private int oldmode;
		public static bool hiddenHUD;
		public static int inputHUD = 0;
	}
}
