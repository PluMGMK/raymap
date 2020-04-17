﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using OpenSpace;
using OpenSpace.Visual;
using OpenSpace.Object;
using OpenSpace.AI;
using OpenSpace.Collide;
using System.Collections;
using OpenSpace.Waypoints;
using OpenSpace.Object.Properties;
using OpenSpace.Exporter;
using System.Threading.Tasks;
using Asyncoroutine;
using Newtonsoft.Json;

public class Controller : MonoBehaviour {
	public Material baseMaterial;
	public Material baseTransparentMaterial;
	public Material baseLightMaterial;
	public Material collideMaterial;
	public Material collideTransparentMaterial;
	public SectorManager sectorManager;
	public LightManager lightManager;
	public GraphManager graphManager;
	public PortalManager portalManager;
	public LoadingScreen loadingScreen;
	public WebCommunicator communicator;
    public GameObject spawnableParent;

    public MapLoader loader = null;
	bool viewCollision_ = false; public bool viewCollision = false;
	bool viewInvisible_ = false; public bool viewInvisible = false;
	bool viewGraphs_ = false; public bool viewGraphs = false;
	bool playAnimations_ = true; public bool playAnimations = true;
	bool playTextureAnimations_ = true; public bool playTextureAnimations = true;
	bool showPersos_ = true; public bool showPersos = true;
	bool livePreview_ = false; public bool livePreview = false;
	bool levelGeometryCorruptions_ = false; public bool levelGeometryCorruptions = false;
	float livePreviewUpdateCounter = 0;

	private CinematicSwitcher cinematicSwitcher = null;
	private LevelGeometryCorruptor levelGeometryCorruptor = null;
	private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

	private bool ExportAfterLoad { get; set; }
	private bool ExportText { get; set; }
    private UnitySettings.ScreenshotAfterLoadSetting ScreenshotAfterLoad { get; set; }
    public string ExportPath { get; set; }

	public List<ROMPersoBehaviour> romPersos { get; set; } = new List<ROMPersoBehaviour>();

	public enum State {
		None,
		Downloading,
		Loading,
		Initializing,
		Error,
		Finished
	}
	private State state = State.None;
	private string detailedState = "None";
	public State LoadState {
		get { return state; }
	}

	// Use this for initialization
	async void Start() {
		// Read command line arguments
		string[] args = System.Environment.GetCommandLineArgs();
		string modeString = "";
		string gameDataBinFolder = "";
		string lvlName = "";
		Settings.Mode mode = Settings.Mode.Rayman2PC;

		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			FileSystem.mode = FileSystem.Mode.Web;
		}
		if (Application.isEditor && UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL) {
			FileSystem.mode = FileSystem.Mode.Web;
		}

		mode = UnitySettings.GameMode;
		gameDataBinFolder = UnitySettings.GameDirs.ContainsKey(mode) ? UnitySettings.GameDirs[mode] : "";
		lvlName = UnitySettings.MapName;
		ExportPath = UnitySettings.ExportPath;
		ExportAfterLoad = UnitySettings.ExportAfterLoad;
        ScreenshotAfterLoad = UnitySettings.ScreenshotAfterLoad;
		if (FileSystem.mode == FileSystem.Mode.Web) {
			gameDataBinFolder = UnitySettings.GameDirsWeb.ContainsKey(mode) ? UnitySettings.GameDirsWeb[mode] : "";
		} else {
			if (UnitySettings.LoadFromMemory) {
				lvlName = UnitySettings.ProcessName + ".exe";
			}
		}

		// Override loaded settings with args
		for (int i = 0; i < args.Length; i++) {
			switch (args[i]) {
				case "--lvl":
				case "-l":
					lvlName = args[i + 1];
					i++;
					break;
				case "--folder":
				case "--directory":
				case "-d":
				case "-f":
					gameDataBinFolder = args[i + 1];
					i++;
					break;
				case "--mode":
				case "-m":
					modeString = args[i + 1];
					i++;
					break;
				case "--export":
					ExportPath = args[i + 1];
					if (!string.IsNullOrEmpty(ExportPath)) {
						ExportAfterLoad = true;
					}
					i++;
					break;
                case "--screenshot":
                    UnitySettings.ScreenshotPath = args[i + 1];
                    if (!string.IsNullOrEmpty(UnitySettings.ScreenshotPath)) {
                        ScreenshotAfterLoad = UnitySettings.ScreenshotAfterLoadSetting.TopDownAndOrthographic;
                    }
                    i++;
                    break;
                case "--screenshot-topdown":
                    UnitySettings.ScreenshotPath = args[i + 1];
                    if (!string.IsNullOrEmpty(UnitySettings.ScreenshotPath)) {
                        ScreenshotAfterLoad = UnitySettings.ScreenshotAfterLoadSetting.TopDownAndOrthographic;
                    }
                    i++;
                    break;
            }
		}
		Application.logMessageReceived += Log;

		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			//Application.logMessageReceived += communicator.WebLog;
			UnityEngine.Debug.unityLogger.logEnabled = false; // We don't need prints here
			string url = Application.absoluteURL;
			if (url.IndexOf('?') > 0) {
				string urlArgsStr = url.Split('?')[1].Split('#')[0];
				if (urlArgsStr.Length > 0) {
					string[] urlArgs = urlArgsStr.Split('&');
					foreach (string arg in urlArgs) {
						string[] argKeyVal = arg.Split('=');
						if (argKeyVal.Length > 1) {
							switch (argKeyVal[0]) {
								case "lvl":
									lvlName = argKeyVal[1]; break;
								case "mode":
									modeString = argKeyVal[1]; break;
								case "folder":
								case "directory":
								case "dir":
									gameDataBinFolder = argKeyVal[1]; break;
							}
						}
					}
				}
			}
		}
		if (Settings.cmdModeNameDict.ContainsKey(modeString)) {
			mode = Settings.cmdModeNameDict[modeString];
		}
		loadingScreen.Active = true;
		Settings.Init(mode);
		loader = MapLoader.Loader;
		loader.controller = this;
		loader.gameDataBinFolder = gameDataBinFolder;
		loader.lvlName = lvlName;
		loader.baseMaterial = baseMaterial;
		loader.baseTransparentMaterial = baseTransparentMaterial;
		loader.collideMaterial = collideMaterial;
		loader.collideTransparentMaterial = collideTransparentMaterial;
		loader.baseLightMaterial = baseLightMaterial;
		
		loader.allowDeadPointers = UnitySettings.AllowDeadPointers;
		loader.forceDisplayBackfaces = UnitySettings.ForceDisplayBackfaces;
		loader.blockyMode = UnitySettings.BlockyMode;
		loader.exportTextures = UnitySettings.SaveTextures;
		ExportText = UnitySettings.ExportText;

		await Init();
	}

    async Task Init() {
        state = State.Loading;
        await loader.LoadWrapper();
        if (state == State.Error) return;
        stopwatch.Start();
        state = State.Initializing;
        detailedState = "Initializing sectors";
        await WaitIfNecessary();
        sectorManager.Init();
        detailedState = "Initializing graphs";
        await WaitIfNecessary();
        graphManager.Init();
        detailedState = "Initializing lights";
        await WaitIfNecessary();
        lightManager.Init();
        detailedState = "Initializing persos";
        await InitPersos();
        sectorManager.InitLights();
        detailedState = "Initializing camera";
        await WaitIfNecessary();
        InitCamera();
        detailedState = "Initializing portals";
        await WaitIfNecessary();
        portalManager.Init();

        /*if (viewCollision)*/
        UpdateViewCollision();
        if (loader.cinematicsManager != null) {
            detailedState = "Initializing cinematics";
            await new WaitForEndOfFrame();
            InitCinematics();
        }
        detailedState = "Finished";
        stopwatch.Stop();
        state = State.Finished;
        loadingScreen.Active = false;

		if (ExportText) {
			MapExporter.ExportText();
		}

        if (ExportAfterLoad) {
            MapExporter e = new MapExporter(this.loader, ExportPath);
            e.Export();

            Application.Quit();
        }

        if (ScreenshotAfterLoad!=UnitySettings.ScreenshotAfterLoadSetting.None) {

            Resolution res = TransparencyCaptureBehaviour.GetCurrentResolution();
            System.DateTime dateTime = System.DateTime.Now;
            TransparencyCaptureBehaviour pb = new GameObject("Dummy").AddComponent<TransparencyCaptureBehaviour>();
            lightManager.enableFog = false;
            Camera.main.orthographic = true;

            var filledSectors = sectorManager.sectors.Where(s => s.sector?.SuperObject?.children?.Count > 0 ? true : false);

            float minX = filledSectors.Min(v => v.SectorBorder.boxMin.x);
            float minY = filledSectors.Min(v => v.SectorBorder.boxMin.y);
            float minZ = filledSectors.Min(v => v.SectorBorder.boxMin.z);

            float maxX = filledSectors.Max(v => v.SectorBorder.boxMax.x);
            float maxY = filledSectors.Max(v => v.SectorBorder.boxMax.y);
            float maxZ = filledSectors.Max(v => v.SectorBorder.boxMax.z);

            Vector3 worldMin = new Vector3(minX, minY, minZ);
            Vector3 worldMax = new Vector3(maxX, maxY, maxZ);

            Vector3 worldSize = (worldMax - worldMin);
            Vector3 center = worldMin + worldSize * 0.5f;

            sectorManager.displayInactiveSectors = true;
            lightManager.luminosity = 1.0f;
            spawnableParent?.SetActive(false);

            byte[] screenshotBytes;

            if (ScreenshotAfterLoad == UnitySettings.ScreenshotAfterLoadSetting.TopDownAndOrthographic || ScreenshotAfterLoad == UnitySettings.ScreenshotAfterLoadSetting.TopDownOnly) {

                Camera.main.transform.position = center + new Vector3(0, Camera.main.farClipPlane * 0.5f, 0);
                Camera.main.transform.rotation = Quaternion.Euler(90, worldSize.z <= worldSize.x ? 90 : 0, 0);

                Camera.main.orthographicSize = (worldSize.z > worldSize.x ? worldSize.z : worldSize.x) * 0.5f;

                screenshotBytes = await pb.Capture(res.width * 8, res.height * 8);
                OpenSpace.Util.ByteArrayToFile(UnitySettings.ScreenshotPath + "/" + loader.lvlName + "_top_" + dateTime.ToString("yyyy_MM_dd HH_mm_ss") + ".png", screenshotBytes);

            }

            if (ScreenshotAfterLoad == UnitySettings.ScreenshotAfterLoadSetting.TopDownAndOrthographic || ScreenshotAfterLoad == UnitySettings.ScreenshotAfterLoadSetting.OrthographicOnly) {

                var pitch = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(Mathf.Deg2Rad * 45));
                for (int i = 0; i < 360; i += 45) {
                    Camera.main.transform.rotation = Quaternion.Euler(pitch, i, 0);
                    Camera.main.transform.position = center - Camera.main.transform.rotation * Vector3.forward * Camera.main.farClipPlane * 0.5f;

                    Camera.main.orthographicSize = (worldSize.x + worldSize.y + worldSize.z) / 6.0f;
                    screenshotBytes = await pb.Capture(res.width * 8, res.height * 8);
                    OpenSpace.Util.ByteArrayToFile(UnitySettings.ScreenshotPath + "/" + loader.lvlName + "_iso_" + i + dateTime.ToString("yyyy_MM_dd HH_mm_ss") + ".png", screenshotBytes);

                }
            }

            Destroy(pb.gameObject);
        }
	}

	// Update is called once per frame
	void Update() {
		if (loadingScreen.Active) {
			if (state == State.Error) {
				loadingScreen.LoadingText = detailedState;
				loadingScreen.LoadingtextColor = Color.red;
			} else {
				if (state == State.Loading) {
					loadingScreen.LoadingText = loader.loadingState;
				} else {
					loadingScreen.LoadingText = detailedState;
				}
				loadingScreen.LoadingtextColor = Color.white;
			}
		}
		if (Input.GetKeyDown(KeyCode.C)) {
			viewCollision = !viewCollision;
		}
		if (Input.GetKeyDown(KeyCode.I)) {
			viewInvisible = !viewInvisible;
		}
		if (Input.GetKeyDown(KeyCode.G)) {
			viewGraphs = !viewGraphs;
		}
		if (Input.GetKeyDown(KeyCode.P)) {
			playAnimations = !playAnimations;
		}
		if (Input.GetKeyDown(KeyCode.T)) {
			playTextureAnimations = !playTextureAnimations;
		}
		if (Input.GetKeyDown(KeyCode.U)) {
			showPersos = !showPersos;
		}
		bool updatedSettings = false;
		if (loader != null) {
			if (viewInvisible != viewInvisible_) {
				updatedSettings = true;
				UpdateViewInvisible();
			}
			if (viewCollision != viewCollision_) {
				updatedSettings = true;
				UpdateViewCollision();
			}
			if (viewGraphs != viewGraphs_) {
				updatedSettings = true;
				UpdateViewGraphs();
			}
			if (showPersos != showPersos_) {
				updatedSettings = true;
				UpdateShowPersos();
			}
			if (livePreview != livePreview_) {
				livePreview_ = livePreview;
				//updatedSettings = true;
			}
			if (levelGeometryCorruptions != levelGeometryCorruptions_) {
				levelGeometryCorruptions_ = levelGeometryCorruptions;
			}
			if (playAnimations != playAnimations_ || playTextureAnimations != playTextureAnimations_) {
				playTextureAnimations_ = playTextureAnimations;
				playAnimations_ = playAnimations;
				updatedSettings = true;
			}


		}
		if (updatedSettings) {
			communicator.SendSettings();
		}

		if (livePreview) {
			livePreviewUpdateCounter += Time.deltaTime;
			if (livePreviewUpdateCounter > 1.0f / 60.0f) {
				UpdateLivePreview();
				livePreviewUpdateCounter = 0.0f;
			}
		}

		if (levelGeometryCorruptions) {
			if (levelGeometryCorruptor == null) {
				levelGeometryCorruptor = new LevelGeometryCorruptor(this);
			}

			levelGeometryCorruptor.DoCorruptions();
		}
	}

	async Task InitPersos() {
		if (loader != null) {
			for (int i = 0; i < loader.persos.Count; i++) {
				detailedState = "Initializing persos: " + i + "/" + loader.persos.Count;
				await WaitIfNecessary();
				Perso p = loader.persos[i];
				PersoBehaviour unityBehaviour = p.Gao.AddComponent<PersoBehaviour>();
				unityBehaviour.controller = this;
				if (loader.globals != null && loader.globals.spawnablePersos != null) {
					if (loader.globals.spawnablePersos.IndexOf(p) > -1) {
						unityBehaviour.IsAlways = true;
						unityBehaviour.transform.position = new Vector3(i * 10, -1000, 0);
					}
				}
				if (!unityBehaviour.IsAlways) {
					if (p.sectInfo != null && p.sectInfo.off_sector != null) {
						unityBehaviour.sector = sectorManager.sectors.FirstOrDefault(s => s.sector != null && s.sector.SuperObject.offset == p.sectInfo.off_sector);
					} else {
						SectorComponent sc = sectorManager.GetActiveSectorWrapper(p.Gao.transform.position);
						unityBehaviour.sector = sc;
					}
				} else unityBehaviour.sector = null;
				unityBehaviour.perso = p;
				unityBehaviour.Init();

				// Scripts
				if (p.Gao) {
					if (p.brain != null && p.brain.mind != null && p.brain.mind.AI_model != null) {
						if (p.brain.mind.AI_model.behaviors_normal != null) {
							GameObject intelParent = new GameObject("Rule behaviours");
							intelParent.transform.parent = p.Gao.transform;
							Behavior[] normalBehaviors = p.brain.mind.AI_model.behaviors_normal;
							int iter = 0;
							foreach (Behavior behavior in normalBehaviors) {
								string shortName = behavior.GetShortName(p.brain.mind.AI_model, Behavior.BehaviorType.Intelligence, iter);
								GameObject behaviorGao = new GameObject(shortName);
								behaviorGao.transform.parent = intelParent.transform;
								foreach (Script script in behavior.scripts) {
									GameObject scriptGao = new GameObject("Script");
									scriptGao.transform.parent = behaviorGao.transform;
									ScriptComponent scriptComponent = scriptGao.AddComponent<ScriptComponent>();
									scriptComponent.SetScript(script, p);
								}
								if (behavior.firstScript != null) {
									ScriptComponent scriptComponent = behaviorGao.AddComponent<ScriptComponent>();
									scriptComponent.SetScript(behavior.firstScript, p);
								}
								if (iter == 0) {
									behaviorGao.name += " (Init)";
								}
								if ((behavior.scripts == null || behavior.scripts.Length == 0) && behavior.firstScript == null) {
									behaviorGao.name += " (Empty)";
								}
								iter++;
							}
						}
						if (p.brain.mind.AI_model.behaviors_reflex != null) {
							GameObject reflexParent = new GameObject("Reflex behaviours");
							reflexParent.transform.parent = p.Gao.transform;
							Behavior[] reflexBehaviors = p.brain.mind.AI_model.behaviors_reflex;
							int iter = 0;
							foreach (Behavior behavior in reflexBehaviors) {
								string shortName = behavior.GetShortName(p.brain.mind.AI_model, Behavior.BehaviorType.Reflex, iter);
								GameObject behaviorGao = new GameObject(shortName);
								behaviorGao.transform.parent = reflexParent.transform;
								foreach (Script script in behavior.scripts) {
									GameObject scriptGao = new GameObject("Script");
									scriptGao.transform.parent = behaviorGao.transform;
									ScriptComponent scriptComponent = scriptGao.AddComponent<ScriptComponent>();
									scriptComponent.SetScript(script, p);
								}
								if (behavior.firstScript != null) {
									ScriptComponent scriptComponent = behaviorGao.AddComponent<ScriptComponent>();
									scriptComponent.SetScript(behavior.firstScript, p);
								}
								if ((behavior.scripts == null || behavior.scripts.Length == 0) && behavior.firstScript == null) {
									behaviorGao.name += " (Empty)";
								}
								iter++;
							}
						}
						if (p.brain.mind.AI_model.macros != null) {
							GameObject macroParent = new GameObject("Macros");
							macroParent.transform.parent = p.Gao.transform;
							Macro[] macros = p.brain.mind.AI_model.macros;
							int iter = 0;

							foreach (Macro macro in macros) {
								GameObject behaviorGao = new GameObject(macro.GetShortName(p.brain.mind.AI_model, iter));
								behaviorGao.transform.parent = macroParent.transform;
								ScriptComponent scriptComponent = behaviorGao.AddComponent<ScriptComponent>();
								scriptComponent.SetScript(macro.script, p);
								iter++;
							}
						}
					}
				}
			}
			// Initialize DSGVars after all persos have their perso behaviours
			for (int i = 0; i < loader.persos.Count; i++) {
				Perso p = loader.persos[i];
				Moddable mod = null;
				if (p.SuperObject != null && p.SuperObject.Gao != null) {
					mod = p.SuperObject.Gao.GetComponent<Moddable>();
					if (mod != null) {
						mod.persoBehaviour = p.Gao.GetComponent<PersoBehaviour>();
					}
				}
				if (p.Gao && p.brain != null && p.brain.mind != null && p.brain.mind.AI_model != null) {
					// DsgVars
					if (p.brain.mind.dsgMem != null || p.brain.mind.AI_model.dsgVar != null) {
						DsgVarComponent dsgVarComponent = p.Gao.AddComponent<DsgVarComponent>();
						dsgVarComponent.SetPerso(p);
						if (mod != null) mod.dsgVarComponent = dsgVarComponent;
					}
					// Dynam
					if (p.dynam != null && p.dynam.dynamics != null) {
						DynamicsMechanicsComponent dynamicsBehaviour = p.Gao.AddComponent<DynamicsMechanicsComponent>();
						dynamicsBehaviour.SetDynamics(p.dynam.dynamics);
					}
					// Mind
					if (p.brain != null && p.brain.mind != null) {
						MindComponent mindComponent = p.Gao.AddComponent<MindComponent>();
						mindComponent.Init(p, p.brain.mind);
						if (mod != null) mod.mindComponent = mindComponent;
					}
					// Custom Bits
					if (p.stdGame != null) {
						CustomBitsComponent c = p.Gao.AddComponent<CustomBitsComponent>();
						c.stdGame = p.stdGame;
						if (Settings.s.engineVersion == Settings.EngineVersion.R3) c.hasAiCustomBits = true;
						c.Init();
					}
				}
			}
		}
		if (loader is OpenSpace.Loader.R2ROMLoader) {
			OpenSpace.Loader.R2ROMLoader romLoader = loader as OpenSpace.Loader.R2ROMLoader;
			if (romPersos.Count > 0) {
				for (int i = 0; i < romPersos.Count; i++) {
					detailedState = "Initializing persos: " + i + "/" + romPersos.Count;
					await WaitIfNecessary();
					ROMPersoBehaviour unityBehaviour = romPersos[i];
					unityBehaviour.controller = this;
					/*if (loader.globals != null && loader.globals.spawnablePersos != null) {
						if (loader.globals.spawnablePersos.IndexOf(p) > -1) {
							unityBehaviour.IsAlways = true;
							unityBehaviour.transform.position = new Vector3(i * 10, -1000, 0);
						}
					}*/
					if (!unityBehaviour.IsAlways) {
						SectorComponent sc = sectorManager.GetActiveSectorWrapper(unityBehaviour.transform.position);
						unityBehaviour.sector = sc;
					} else unityBehaviour.sector = null;
					/*Moddable mod = null;
					if (p.SuperObject != null && p.SuperObject.Gao != null) {
						mod = p.SuperObject.Gao.GetComponent<Moddable>();
						if (mod != null) {
							mod.persoBehaviour = unityBehaviour;
						}
					}*/
					unityBehaviour.Init();

					var iteratorPerso = unityBehaviour.perso;

					// Of sound brain and AI model?
					if (iteratorPerso.brain?.Value?.aiModel?.Value != null) {
						var aiModel = iteratorPerso.brain.Value.aiModel.Value;

						// DsgVars
						if (iteratorPerso.brain?.Value?.dsgMem?.Value != null || aiModel.dsgVar?.Value != null) {
							DsgVarComponent dsgVarComponent = unityBehaviour.gameObject.AddComponent<DsgVarComponent>();
							dsgVarComponent.SetPerso(iteratorPerso);
						}

						// Comports
						if (aiModel.comportsIntelligence.Value != null) {
							aiModel.comportsIntelligence.Value.CreateGameObjects("Rule", unityBehaviour.gameObject, iteratorPerso);
						}
						if (aiModel.comportsReflex.Value != null) {
							aiModel.comportsReflex.Value.CreateGameObjects("Reflex", unityBehaviour.gameObject, iteratorPerso);
						}
					}
				}
			}
			if (romLoader.level != null && romLoader.level.spawnablePersos.Value != null && romLoader.level.num_spawnablepersos > 0) {
				spawnableParent = new GameObject("Spawnable persos");
				for (int i = 0; i < romLoader.level.num_spawnablepersos; i++) {
					detailedState = "Initializing spawnable persos: " + i + "/" + romLoader.level.num_spawnablepersos;
					await WaitIfNecessary();
					OpenSpace.ROM.SuperObjectDynamic sod = romLoader.level.spawnablePersos.Value.superObjects[i];
					GameObject sodGao = sod.GetGameObject();
					if (sodGao != null) {
						ROMPersoBehaviour unityBehaviour = sodGao.GetComponent<ROMPersoBehaviour>();
						unityBehaviour.controller = this;
						unityBehaviour.IsAlways = true;
						unityBehaviour.transform.SetParent(spawnableParent.transform);
						unityBehaviour.transform.position = new Vector3(i * 10, -1000, 0);
						unityBehaviour.transform.rotation = Quaternion.identity;
						unityBehaviour.transform.localScale = Vector3.one;
						if (!unityBehaviour.IsAlways) {
							SectorComponent sc = sectorManager.GetActiveSectorWrapper(unityBehaviour.transform.position);
							unityBehaviour.sector = sc;
						} else unityBehaviour.sector = null;
						unityBehaviour.Init();

						var iteratorPerso = unityBehaviour.perso;

						// Of sound brain and AI model?
						if (iteratorPerso.brain?.Value?.aiModel?.Value != null) {
							var aiModel = iteratorPerso.brain.Value.aiModel.Value;

							// DsgVars
							if (iteratorPerso.brain?.Value?.dsgMem?.Value != null || aiModel.dsgVar?.Value != null) {
								DsgVarComponent dsgVarComponent = unityBehaviour.gameObject.AddComponent<DsgVarComponent>();
								dsgVarComponent.SetPerso(iteratorPerso);
							}

							// Comports
							if (aiModel.comportsIntelligence.Value != null) {
								aiModel.comportsIntelligence.Value.CreateGameObjects("Rule", unityBehaviour.gameObject, iteratorPerso);
							}
							if (aiModel.comportsReflex.Value != null) {
								aiModel.comportsReflex.Value.CreateGameObjects("Reflex", unityBehaviour.gameObject, iteratorPerso);
							}
						}
					}
				}
			}
		}
	}

	public void InitCamera() {
		if (loader != null) {
			if (loader is OpenSpace.Loader.R2ROMLoader) {
				ROMPersoBehaviour camera = romPersos.FirstOrDefault(p => p.perso.camera != null);
				if (camera != null) {
					Camera.main.transform.position = camera.transform.position;
					Camera.main.transform.rotation = camera.transform.rotation * Quaternion.Euler(0, 180, 0);
				}
			} else {
				Perso camera = loader.persos.FirstOrDefault(p => p != null && p.namePerso.Equals("StdCamer"));
				if (camera == null && loader.globals != null && loader.globals.off_camera != null) {
					camera = loader.persos.FirstOrDefault(p => p != null && p.stdGame != null && p.stdGame.off_superobject == loader.globals.off_camera);
				}
				if (camera != null) {
					Camera.main.transform.position = camera.Gao.transform.position;
					Camera.main.transform.rotation = camera.Gao.transform.rotation * Quaternion.Euler(0, 180, 0);
				}
			}
		}
	}

	public void UpdateViewGraphs() {
		if (loader != null) {
			viewGraphs_ = viewGraphs;
			graphManager.UpdateViewGraphs();
		}
	}

	public void UpdateViewInvisible() {
		if (loader != null) {
			viewInvisible_ = viewInvisible;
			foreach (SuperObject so in loader.superObjects) {
				if (so.type == SuperObject.Type.Perso) {
					UpdatePersoActive(so.data as Perso);
				} else {
					if (so.Gao != null) {
						if (so.flags.HasFlag(OpenSpace.Object.Properties.SuperObjectFlags.Flags.Invisible)) {
							so.Gao.SetActive(viewCollision || viewInvisible);
						}
					}
				}
			}
		}
	}

	public void UpdateShowPersos() {
		if (loader != null) {
			showPersos_ = showPersos;
			foreach (Perso perso in loader.persos) {
				UpdatePersoActive(perso);
			}
		}
	}

	public void UpdatePersoActive(Perso perso) {
		if (perso != null && perso.Gao != null) {
			PersoBehaviour pb = perso.Gao.GetComponent<PersoBehaviour>();
			if (pb != null) {
				bool isVisible = true;
				if (perso.SuperObject != null) {
					isVisible = !perso.SuperObject.flags.HasFlag(OpenSpace.Object.Properties.SuperObjectFlags.Flags.Invisible);
				}
				perso.Gao.SetActive(showPersos && pb.IsEnabled && (isVisible || (viewCollision || viewInvisible)));
			}
		}
	}

	public void UpdateViewCollision() {
		if (loader != null) {
			viewCollision_ = viewCollision;
			foreach (PhysicalObject po in loader.physicalObjects) {
				if (po != null) po.UpdateViewCollision(viewCollision);
			}
			foreach (Perso perso in loader.persos) {
				if (perso != null && perso.Gao != null) {
					PersoBehaviour pb = perso.Gao.GetComponent<PersoBehaviour>();
					pb.UpdateViewCollision(viewCollision);
				}
			}
			foreach (ROMPersoBehaviour perso in romPersos) {
				if (perso != null) { perso.UpdateViewCollision(viewCollision); }
			}
			foreach (SuperObject so in loader.superObjects) {
				if (so.Gao != null) {
					if (so.flags.HasFlag(OpenSpace.Object.Properties.SuperObjectFlags.Flags.Invisible)) {
						if (so.type == SuperObject.Type.Perso) {
							UpdatePersoActive(so.data as Perso);
						} else {
							so.Gao.SetActive(viewCollision || viewInvisible);
						}
					}
				}
			}
			if (loader is OpenSpace.Loader.R2ROMLoader) {
				PhysicalObjectComponent[] pos = FindObjectsOfType<PhysicalObjectComponent>();
				foreach (PhysicalObjectComponent po in pos) {
					po.UpdateViewCollision(viewCollision);
				}
			}
		}
	}

	public void InitCinematics() {
		if (loader.cinematicsManager != null && loader.cinematicsManager.cinematics.Count > 0) {
			cinematicSwitcher = new GameObject("Cinematic Switcher").AddComponent<CinematicSwitcher>();
			cinematicSwitcher.Init();
		}
	}

	public void UpdateLivePreview() {
		Reader reader = MapLoader.Loader.livePreviewReader;

		foreach (SuperObject so in MapLoader.Loader.superObjects) {

			if (!(so.data is Perso)) {
				continue;
			}

			if (so.off_matrix == null) {
				continue;
			}
			Pointer.Goto(ref reader, so.off_matrix);
			so.matrix = Matrix.Read(MapLoader.Loader.livePreviewReader, so.off_matrix);
			if (so.data != null && so.data.Gao != null) {
				so.data.Gao.transform.localPosition = so.matrix.GetPosition(convertAxes: true);
				so.data.Gao.transform.localRotation = so.matrix.GetRotation(convertAxes: true);
				so.data.Gao.transform.localScale = so.matrix.GetScale(convertAxes: true);

				if (so.data is Perso) {
					Perso perso = (Perso)so.data;

					PersoBehaviour pb = perso.Gao.GetComponent<PersoBehaviour>();
					if (pb != null) {

						Pointer.Goto(ref reader, perso.p3dData.offset);
						perso.p3dData.UpdateCurrentState(reader);

						// State offset changed?
						if (perso.p3dData.stateCurrent != null) {
							pb.SetState(perso.p3dData.stateCurrent.index);
							pb.autoNextState = true;
						}
					}

					MindComponent mc = perso.Gao.GetComponent<MindComponent>();
					if (mc != null) {
						Mind mind = mc.mind;
						Pointer.DoAt(ref reader, mind.Offset, () => {
							mind.UpdateCurrentBehaviors(reader);
						});
					}

					DsgVarComponent dsgVarComponent = perso.Gao.GetComponent<DsgVarComponent>();
					if (dsgVarComponent != null) {
						dsgVarComponent.SetPerso(perso);
					}

					CustomBitsComponent customBitsComponent = perso.Gao.GetComponent<CustomBitsComponent>();
					if (customBitsComponent != null) {
						Pointer.Goto(ref reader, perso.off_stdGame);
						perso.stdGame = StandardGame.Read(reader, perso.off_stdGame);
						customBitsComponent.stdGame = perso.stdGame;
						customBitsComponent.Init();
					}

					DynamicsMechanicsComponent dnComponent = perso.Gao.GetComponent<DynamicsMechanicsComponent>();
					if (dnComponent != null) {
						Pointer.DoAt(ref reader, perso.off_dynam, () => {
							perso.dynam = Dynam.Read(reader, perso.off_dynam);
						});

						dnComponent.SetDynamics(perso.dynam.dynamics);
					}
				}
			}
		}

		Perso camera = loader.persos.FirstOrDefault(p => p != null && p.namePerso.Equals("StdCamer"));
		if (camera != null) {

			SuperObject cameraSO = camera.SuperObject;
			Pointer.Goto(ref reader, cameraSO.off_matrix);
			cameraSO.matrix = Matrix.Read(reader, cameraSO.off_matrix);
			camera.Gao.transform.localPosition = cameraSO.matrix.GetPosition(convertAxes: true);
			camera.Gao.transform.localRotation = cameraSO.matrix.GetRotation(convertAxes: true);
			camera.Gao.transform.localScale = cameraSO.matrix.GetScale(convertAxes: true);

			Camera.main.transform.position = camera.Gao.transform.position;
			Camera.main.transform.rotation = camera.Gao.transform.rotation * Quaternion.Euler(0, 180, 0);
		}
	}

	public void SaveChanges() {
		if (loader != null) loader.Save();
	}

	public void Log(string condition, string stacktrace, LogType type) {
		switch (type) {
			case LogType.Exception:
			case LogType.Error:
				if (state != State.Finished) {
					// Allowed exceptions
					if (condition.Contains("cleaning the mesh failed")) break;
					if (condition.Contains("desc.isValid() failed!")) break;

					// Go to error state
					state = State.Error;
					if (loadingScreen.Active) {
						detailedState = condition;
					}
				}
				break;
		}
	}
	public async Task WaitFrame() {
		await new WaitForEndOfFrame();
		if (stopwatch.IsRunning) {
			stopwatch.Restart();
		}
	}
	public async Task WaitIfNecessary() {
		if (stopwatch.ElapsedMilliseconds > 16) {
			await WaitFrame();
		}
	}
}
