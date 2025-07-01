# VR 프로젝트 씬 전환 설정 가이드

**작성일:** 2024년  
**목적:** Launch → Beat1 씬 전환이 안 될 때 체크리스트

---

## 🚨 필수 요소들 (하나라도 빠지면 씬 전환 안됨!)

### 1. LocalPlayerTransform GameObject 생성
```
❌ 문제: TaskHandler Inspector에서 NullReferenceException 발생
✅ 해결: Hierarchy에 LocalPlayerTransform 오브젝트 필수!
```

**생성 방법:**
1. Hierarchy 우클릭 → Create Empty
2. 이름을 `LocalPlayerTransform`으로 변경
3. `LocalPlayerTransform` 스크립트 컴포넌트 추가
4. 위치는 (0,0,0) 상관없음

### 2. Game Flow TaskHandler ID 설정
```
❌ 문제: TaskHandler들이 빈 ID("")로 등록됨
✅ 해결: 각 TaskHandler의 TaskID 필드에 정확한 ID 입력
```

**설정 목록:**
- `Game Flow/Narrative/0_Init` → TaskID = `GF_S0_Init`
- `Game Flow/Narrative/1_Load_Beat_1` → TaskID = `GF_S1_Load_Beat_1`
- `Game Flow/Narrative/2_Preload_Beat_2` → TaskID = `GF_S2_Preload_Beat_2`
- ... (나머지도 동일 패턴)

### 3. GameFlowController 설정
```
❌ 문제: FirstTask가 비어있음
✅ 해결: FirstTask = GF_S0_Init 설정
```

**Hierarchy → Game Flow 선택:**
- FirstTask.ID = `GF_S0_Init`
- StartAutomatically = true ✅

### 4. Task Sequence 연결 확인
```
❌ 문제: GF_S0.asset의 NextSequence가 자기 자신을 가리킴
✅ 해결: GF_S0 → GF_S1 → GF_S2 순서로 연결
```

**Project 창에서 확인:**
- `GF_S0.asset` → NextSequence = `GF_S1`
- `GF_S1.asset` → NextSequence = `GF_S2` (있다면)

---

## 🔧 설정 순서 (이 순서대로!)

### 1단계: Player 설정
1. LocalPlayerTransform GameObject 생성 (위 방법대로)
2. Main Camera가 "MainCamera" 태그인지 확인

### 2단계: Game Flow 프리팹 설정
1. Hierarchy에 `Game Flow.prefab` 배치
2. Game Flow → GameFlowController 컴포넌트:
   - FirstTask.ID = `GF_S0_Init`
   - StartAutomatically = true

### 3단계: TaskHandler ID 설정
1. `0_Init` 선택 → TaskHandler → TaskID = `GF_S0_Init`
2. `1_Load_Beat_1` 선택 → TaskHandler → TaskID = `GF_S1_Load_Beat_1`
3. 나머지도 동일 패턴으로...

### 4단계: Task Sequence 확인
1. `Assets/@Resource/Task Sequences/Game Flow Sequences/GF_S0.asset`
2. NextSequence가 GF_S1을 가리키는지 확인

### 5단계: Task Manager 새로고침
1. `Assets/Resources/NarrativeSequence/Task Manager.asset` 선택
2. Inspector 우클릭 → **Refresh** 실행

---

## 🐛 트러블슈팅

### TaskID 필드가 안 보일 때
```
원인: LocalPlayerTransform.Instance가 null이어서 Inspector 에러 발생
해결: 위 1단계 LocalPlayerTransform 생성하기
```

### 콘솔에 "registering with ID """가 뜰 때
```
원인: TaskHandler의 TaskID가 비어있음
해결: Debug Inspector로 TaskID.ID 필드에 직접 입력
```

### PrewarmGame 에러 뜰 때
```
원인: QualityData 설정 안됨 (현재 임시 우회 중)
해결: [DK 임시] 표시 있는 코드들 - 나중에 QualityData 설정 후 제거
```

### "Task starting with no handler" 로그
```
원인: TaskID 설정했지만 Task Manager가 인식 못함
해결: Task Manager → Refresh 실행
```

---

## ✅ 성공 확인 방법

**Play 후 콘솔에서 확인:**
```
✅ TaskHandler.Awake() on handler '0_Init'; registering with ID "GF_S0_Init"
✅ [Narrative sequence] Started task sequence 'GF_S0' with 1 initial task(s)
✅ [Narrative sequence] Task 'GF_S0_Init' completed; sequence 'GF_S0' complete!
✅ [Narrative sequence] Task sequence 'GF_S0' complete; starting next...
✅ [Narrative sequence] Started task sequence 'GF_S1' with 1 initial task(s)
```

**씬 전환 성공:** Launch → Beat1으로 자동 이동 🎉

---

## 📝 주의사항

1. **TaskID는 대소문자 구분!** `GF_S0_Init` ≠ `gf_s0_init`
2. **Task Manager Refresh 필수!** ID 설정 후 반드시 Refresh
3. **LocalPlayerTransform 필수!** 없으면 TaskHandler Inspector 깨짐
4. **저장 필수!** Ctrl+S로 씬/프리팹 저장해야 YAML에 반영
5. **[DK 임시] 코드들은 나중에 정리** 검색해서 찾기: `[DK 임시]`

---

**🔥 핵심:** LocalPlayerTransform + TaskID 설정 + Task Manager Refresh = 성공! 