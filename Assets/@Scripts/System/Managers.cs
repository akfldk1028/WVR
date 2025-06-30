using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LaunchGame;

public class Managers : MonoBehaviour
{
	public static bool Initialized { get; set; } = false;

	private static Managers s_instance;
	public static Managers Instance { get { Init(); return s_instance; } }

	#region Contents


    // private MessageManager<ActionType> _action_message = new MessageManager<ActionType>();

	// public static GameManager Game { get { return Instance?._game; } }
	// public static PlacementManager Placement { get { return Instance?._placement; } }

	// public static ObjectManager Object { get { return Instance?._object; } }
	// public static MapManager Map { get { return Instance?._map; } }
	// public static MessageManager<ActionType> ActionMessage { get { return Instance?._action_message; } }
	// public static InputManager Input { get { Instance?._input.Init();  return Instance?._input; } }
	// private GameFlowController _gameFlow = new GameFlowController();
    // public static GameFlowController GameFlow { get { Instance?._gameFlow.Init(); return Instance?._gameFlow; } }

	#endregion

	#region Core
	// private DataManager _data = new DataManager();
	// private ResourceManager _resource = new ResourceManager();
	// private SceneManagerEx _scene = new SceneManagerEx();
	// private UIManager _ui = new UIManager();

	// public static DataManager Data { get { return Instance?._data; } }
	// public static ResourceManager Resource { get { return Instance?._resource; } }
	// public static SceneManagerEx Scene { get { return Instance?._scene; } }
	// public static UIManager UI { get { return Instance?._ui; } }
	#endregion

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void AutoInit()
	{
		Init();
	}

	public static void Init()
	{
		// Debug.Log("<color=yellow>[Managers]</color> Init");
		if (s_instance == null && Initialized == false)
		{
			Initialized = true;

			GameObject go = GameObject.Find("@Managers");
			if (go == null)
			{
				go = new GameObject { name = "@Managers" };
				go.AddComponent<Managers>();
			}

			DontDestroyOnLoad(go);

			// 초기화
			s_instance = go.GetComponent<Managers>();
		}
	}
	//  public static void PublishAction(ActionType actionType)
    // {
    //     ActionMessage?.Publish(actionType);
    // }


    // public static IDisposable Subscribe(ActionType actionType, Action handler)
    // {
    //     return ActionMessage?.Subscribe(type => 
    //     {
    //         if (type == actionType)
    //             handler?.Invoke();
    //     });
    // }


}
