# TaskHandler 및 Narrative 시스템 사용법 가이드

## 🎯 개요

Unity Meta Utilities Narrative 시스템은 VR 게임의 스토리 진행과 작업(Task) 관리를 위한 시스템입니다.

## 📋 주요 구성 요소

### 1. TaskManager (ScriptableObject)
- 모든 작업과 시퀀스를 관리하는 중앙 관리자
- 위치: `Assets/Resources/NarrativeSequence/Task Manager.asset`
- 싱글톤 패턴으로 동작

### 2. TaskSequence (ScriptableObject)
- 여러 작업들을 순서대로 묶은 시퀀스
- 각 작업의 전제조건과 완료조건 정의

### 3. TaskHandler (MonoBehaviour)
- 실제 게임 오브젝트에 붙어서 작업의 시작/완료 로직을 처리
- 씬의 GameObject에 컴포넌트로 추가

### 4. TaskCondition
- 작업 완료 조건들 (시간 지연, 이벤트 대기, 시선 추적 등)

## 🚀 기본 설정 방법

### 1. TaskManager 설정
```yaml
DebugLogging: 1  # 디버그 로그 활성화 (개발 중 권장)
Loop: 0          # 시퀀스 루프 여부
Sequences:       # 실행할 TaskSequence 목록
  - 시퀀스1
  - 시퀀스2
  - ...
```

### 2. TaskSequence 생성
1. Assets 우클릭 → Create → Data → Narrative Sequencing → Task Sequence
2. TaskDefinitions에 작업들 추가
3. 각 작업의 ID와 전제조건 설정

### 3. TaskHandler 설정
1. 씬의 GameObject에 TaskHandler 컴포넌트 추가
2. **TaskID**: TaskSequence에서 정의한 작업 ID와 정확히 일치해야 함
3. **CompletionConditions**: 작업 완료 조건 설정
4. **Events**: 작업 시작/완료시 실행할 이벤트

## 🔗 TaskID 매칭 시스템

### 매칭 방식
TaskHandler는 **GameObject 이름이 아닌 TaskID 필드로만 매칭**됩니다:

```csharp
// TaskSequence에서 정의
Task Definition ID: "GF_S0_Init" 

// TaskHandler에서 설정 (정확히 일치해야 함!)
TaskID: "GF_S0_Init"    
```

### 매칭 과정
1. **TaskHandler.Awake()**: `TaskManager.RegisterHandler(this)` 호출
2. **TaskManager**: `Dictionary[TaskID] = TaskHandler` 형태로 등록
3. **Task 실행 시**: TaskManager가 Dictionary에서 TaskID로 검색
4. **매칭 성공**: 해당 TaskHandler의 메서드들 호출

### ✅ 올바른 설정 예제
```
TaskSequence Inspector:
├── Task Definition #0
│   └── ID: "MoveTo_Kitchen"

씬의 GameObject: "Kitchen_Handler" (이름은 자유)
├── TaskHandler Component
│   └── TaskID: "MoveTo_Kitchen"  ← TaskSequence ID와 동일!
```

### ❌ 자주 하는 실수들
```
TaskSequence ID: "GF_S0_Init"
TaskHandler ID: "GF_S0_init"     ← 대소문자 다름 (실패)
TaskHandler ID: "GF-S0-Init"     ← 언더스코어 vs 하이픈 (실패)
TaskHandler ID: ""               ← 빈 문자열 (등록 안됨)
TaskHandler ID: " GF_S0_Init "   ← 앞뒤 공백 (실패)
```

### 🔍 매칭 확인 방법
1. **에디터에서**: TaskSequence Inspector에서 "Task Handler" 필드 확인
2. **플레이 모드**: Console에서 등록 로그 확인
3. **디버그**: TaskManager의 DebugLogging 활성화

## 🔄 작업 흐름

```
게임 시작
    ↓
TaskManager.StartNarrative() 호출
    ↓
첫 번째 TaskSequence 시작
    ↓
조건을 만족하는 Task들이 Task.Start() 호출
    ↓
해당하는 TaskHandler들의 TaskStarted() 호출
    ↓
TaskHandler가 CompletionConditions 체크
    ↓
조건 만족시 Task.Complete() 호출
    ↓
다음 Task 시작 또는 시퀀스 완료
```

## ⚙️ TaskHandler 자동 등록 과정

### 등록 시점
- **Awake()**: TaskHandler가 씬에 로드될 때 자동 등록
- **RefreshAll()**: 씬 로드나 에디터에서 수동 새로고침 시

### 등록 조건
- TaskID가 설정되어 있어야 함 (빈 문자열이면 등록 안됨)
- GameObject가 활성화되어 있어야 함
- TaskHandler 컴포넌트가 활성화되어 있어야 함

### 디버그 로그 확인
TaskManager의 DebugLogging을 켜면 다음 로그들을 볼 수 있습니다:
```
TaskHandler.Awake() on handler 'ObjectName'; registering with ID 'TaskID'
TaskManager registered handler 'ObjectName' for task 'TaskID'
Task Manager refreshing all tasks and task handlers
```

## 🎮 실제 사용 예제

### 시나리오: 플레이어가 특정 위치로 이동하는 작업

1. **TaskSequence 설정**
```
Task ID: "MoveTo_Kitchen"
Starting Prerequisites: (없음 - 첫 번째 작업)
Completion Prerequisites: (없음)
```

2. **씬 설정**
```
GameObject: "Kitchen_Trigger"
├── BoxCollider (IsTrigger: true)
└── TaskHandler
    ├── TaskID: "MoveTo_Kitchen"
    ├── CompletionConditions:
    │   └── WaitForEventCondition (플레이어 충돌 감지)
    └── Events:
        ├── OnTaskStarted: UI 안내 표시
        └── OnTaskCompleted: 효과음 재생
```

## 🐛 문제 해결

### TaskHandler가 등록되지 않을 때
1. TaskID가 올바르게 설정되었는지 확인
2. GameObject와 컴포넌트가 활성화되어 있는지 확인
3. TaskManager의 DebugLogging을 켜서 로그 확인
4. TaskManager.RefreshAll() 수동 호출

### Task가 시작되지 않을 때
1. 전제조건(Prerequisites)이 만족되었는지 확인
2. 이전 작업이 완료되었는지 확인
3. TaskSequence가 TaskManager에 등록되었는지 확인

### 일반적인 오류들
- `Could not load Task Manager from resources!`
  → Task Manager.asset이 올바른 경로에 있는지 확인
- `Handler not found for task 'TaskID'`
  → TaskHandler의 TaskID와 TaskSequence의 ID가 일치하는지 확인

## 📁 파일 경로 정리

```
Assets/
├── Resources/
│   └── NarrativeSequence/
│       └── Task Manager.asset
├── @Resource/
│   └── Task Sequences/
│       ├── Game Flow Sequences/
│       └── NarrativeSequence/
└── @Scripts/
    └── Docs/
        └── TaskHandler_사용법.md (이 파일)
```

## 💡 팁

1. **개발 중에는 DebugLogging을 켜두세요** - 어떤 작업이 언제 시작/완료되는지 추적 가능
2. **TaskID는 명확하게 작성** - "Start", "Move_To_Kitchen", "Talk_To_NPC_1" 등
3. **전제조건을 활용** - 복잡한 분기나 순서 제어 가능
4. **Context Menu 활용** - TaskHandler에서 우클릭으로 "Start Narrative", "Skip" 등 사용 가능

---
*이 문서는 Meta Utilities Narrative 시스템 v1.0 기준으로 작성되었습니다.* 