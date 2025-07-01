using System.Collections;
using Meta.Utilities.Narrative;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LaunchGame
{
    /// <summary>
    /// VR 게임의 전체적인 흐름을 관리하는 메인 컨트롤러
    /// 씬 전환, 게임 상태 관리, 비동기 로딩 등을 담당
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        #region Singleton
        
        /// <summary>
        /// GameFlowController의 싱글톤 인스턴스
        /// </summary>
        public static GameFlowController Instance { get; private set; }
        
        #endregion

        #region Fields

        /// <summary>
        /// 각 씬별로 실행할 작업 목록
        /// </summary>
        [SerializeField] private TaskID[] m_sceneTasks;

        /// <summary>
        /// 씬 변경이 완료되었을 때 발생하는 이벤트
        /// </summary>
        public UnityEvent SceneChangeComplete;
        
        /// <summary>
        /// 게임 재시작이 요청되었을 때 발생하는 이벤트
        /// </summary>
        public UnityEvent RestartGameRequested;
        
        /// <summary>
        /// 게임 오버가 요청되었을 때 발생하는 이벤트
        /// </summary>
        public UnityEvent GameOverRequested;

        /// <summary>
        /// 게임 시작 시 실행할 첫 번째 작업 ID
        /// </summary>
        public TaskID FirstTask = TaskID.None;
        
        /// <summary>
        /// 게임이 자동으로 시작되는지 여부
        /// </summary>
        public bool StartAutomatically = false;

        /// <summary>
        /// 현재 실행 중인 비동기 로딩 작업
        /// </summary>
        private AsyncOperation m_loadOperation;
        
        /// <summary>
        /// 현재 로딩 중인 씬의 이름
        /// </summary>
        private string m_loadingSceneName;

        /// <summary>
        /// 게임이 한 번 완료되었는지 여부
        /// </summary>
        [field: SerializeField] public bool GameCompleteOnce { get; private set; }

        /// <summary>
        /// 첫 번째 실행인지 확인하는 정적 플래그
        /// </summary>
        private static bool s_firstLaunch = true;

        #endregion

        #region Properties

        /// <summary>
        /// 현재 씬이 로딩 중인지 여부를 반환
        /// </summary>
        public bool IsLoading { get; private set; }

        #endregion

        #region Unity Lifecycle

    
        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
            }
        }


        private void Start()
        {
            if (!Application.isPlaying) return;

            // 첫 실행 시 프로파일링 씬이 있으면 해당 씬 로드
            // 아니면 설정된 첫 번째 작업 시작
            if (s_firstLaunch && ProfilingSystem.SceneName != null)
            {
                LoadScene(ProfilingSystem.SceneName);
                Debug.Log($"LoadScene: {ProfilingSystem.SceneName}");
            }
            else
            {
                TaskManager.StartNarrativeFromTaskID(FirstTask);
            }

            s_firstLaunch = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 초기화 메서드 (외부에서 호출 가능)
        /// </summary>
        public void Init() { }

        /// <summary>
        /// 게임 오버 화면을 표시
        /// </summary>
        public void ShowGameOver() => GameOverRequested?.Invoke();

        /// <summary>
        /// 크레딧 화면으로 이동
        /// </summary>
        public void GoToCredits()
        {
            // LoadScreen.Instance.GoToCredits();
        }

        /// <summary>
        /// 게임 완료 상태를 설정하고 내러티브 시작
        /// </summary>
        public void SetGameComplete()
        {
            GameCompleteOnce = true;
            TaskManager.StartNarrative();
        }

        /// <summary>
        /// 지정된 씬을 로드 (기본 모드)
        /// </summary>
        /// <param name="sceneName">로드할 씬의 이름</param>
        public void LoadScene(string sceneName)
        {
            StopAllCoroutines();
            _ = StartCoroutine(ChangeSceneCoroutine(sceneName));
        }

        /// <summary>
        /// 지정된 씬을 강제로 로드 (같은 씬도 다시 로드)
        /// </summary>
        /// <param name="sceneName">로드할 씬의 이름</param>
        public void ForceLoadScene(string sceneName)
        {
            StopAllCoroutines();
            _ = StartCoroutine(ChangeSceneCoroutine(sceneName, true));
        }

        public void ResetLoadState()
        {
            IsLoading = false;
            m_loadOperation = null;
            m_loadingSceneName = null;
        }

        /// <summary>
        /// 씬을 미리 백그라운드에서 로딩 (활성화하지 않음)
        /// </summary>
        /// <param name="sceneName">미리 로드할 씬의 이름</param>
        public void PreloadScene(string sceneName)
        {
            var buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
            if (buildIndex == -1)
            {
                Debug.LogError($"Error pre-loading scene: {sceneName}, not in build");
                return;
            }
            Debug.Assert(!IsLoading, "Trying to load 2 scenes at once");
            var activeScene = SceneManager.GetActiveScene();
            Debug.Assert(sceneName != activeScene.name, $"Scene: {sceneName} already loaded");

            m_loadOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            m_loadOperation.priority = (int)ThreadPriority.Low;
            m_loadOperation.allowSceneActivation = false;
            IsLoading = true;
            m_loadingSceneName = sceneName;
        }

        /// <summary>
        /// 미리 로딩된 씬을 활성화하여 완료
        /// </summary>
        /// <param name="sceneName">활성화할 씬의 이름</param>
        public void CompleteSceneLoad(string sceneName)
        {
            if (!IsLoading)
            {
                Debug.LogError("Not loading a scene");
                PreloadScene(sceneName);
            }
            Debug.Assert(m_loadingSceneName == sceneName, "Loading the wrong scene");

            if (m_loadOperation.progress < .9f)
            {
                // 씬 로딩이 아직 완료되지 않았지만 진행 가능
                Debug.LogWarning("Scene not finished loading perhaps start loading earlier");
            }

            StopAllCoroutines();
            _ = StartCoroutine(CompleteSceneLoadCoroutine());
        }

        /// <summary>
        /// 현재 로딩 진행률을 반환 (0.0 ~ 1.0)
        /// </summary>
        /// <returns>로딩 진행률 (0.0 ~ 1.0)</returns>
        public float GetLoadProgress()
        {
            return !IsLoading ? 0f : m_loadOperation == null ? 0f : m_loadOperation.progress;
        }

        /// <summary>
        /// 게임을 재시작
        /// </summary>
        public void RestartGame() => RestartGameRequested?.Invoke();

        #endregion

        #region Private Methods

        /// <summary>
        /// 씬 변경을 처리하는 코루틴 (VR 페이드 효과 포함)
        /// </summary>
        /// <param name="sceneName">변경할 씬의 이름</param>
        /// <param name="allowReloadScene">같은 씬 재로드 허용 여부</param>
        /// <returns>코루틴 IEnumerator</returns>
        private IEnumerator ChangeSceneCoroutine(string sceneName, bool allowReloadScene = false)
        {
            // VR 화면 페이드 아웃 효과
            if (OVRScreenFade.instance)
            {
                OVRScreenFade.instance.FadeOut();
                yield return new WaitForSeconds(OVRScreenFade.instance.fadeTime);
            }

            var activeScene = SceneManager.GetActiveScene();

            // 씬 로드 조건 확인 후 로드
            if (!string.IsNullOrWhiteSpace(sceneName) && (allowReloadScene || !activeScene.IsValid() || activeScene.name != sceneName))
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (operation != null && !operation.isDone)
                    yield return null;
            }

            // 씬 변경 완료 이벤트 발생
            SceneChangeComplete?.Invoke();
        }

        /// <summary>
        /// 미리 로딩된 씬의 활성화를 완료하는 코루틴
        /// </summary>
        /// <returns>코루틴 IEnumerator</returns>
        private IEnumerator CompleteSceneLoadCoroutine()
        {
            m_loadOperation.allowSceneActivation = true;
            while (!m_loadOperation.isDone)
            {
                yield return null;
            }
            ResetLoadState();
            SceneChangeComplete?.Invoke();
        }

        #endregion
    }
}



