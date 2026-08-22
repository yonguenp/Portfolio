using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 구버전 <see cref="StandaloneInputModule"/>을 걷어낸다.
///
/// 이 프로젝트는 Active Input Handling이 Input System 전용이라, 구버전 모듈이
/// 하나라도 살아 있으면 <c>EventSystem.Update()</c>가 매 프레임
/// <c>InvalidOperationException</c>을 던진다 (실측 303회/3000줄). 로그가 폭주하고
/// 에디터가 느려진다. EnhancedTouch 때와 같은 종류의 함정이다.
///
/// <b>저장된 씬에는 없다.</b> 전 씬을 검사했지만 모두 InputSystemUIInputModule
/// 하나뿐이었는데, 실행하면 EventSystem이 하나 더 생겨 있고 그게 구버전 모듈을
/// 달고 있었다. 어떤 패키지가 만드는지 특정하지 않고 <b>생기면 치우는</b> 쪽으로
/// 막는다 — "누가 알아서 해주겠거니" 하면 안 되는 자리다.
/// </summary>
public static class InputModuleGuard
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        Sweep();
        SceneManager.sceneLoaded += (_, __) => Sweep();
    }

    static void Sweep()
    {
        var all = Object.FindObjectsByType<EventSystem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (all.Length == 0) return;

        // 새 Input System 모듈을 가진 걸 정본으로 삼는다. 없으면 첫 번째에 붙인다.
        var keeper = all.FirstOrDefault(e => e.GetComponent<InputSystemUIInputModule>() != null)
                  ?? all[0];
        if (keeper.GetComponent<InputSystemUIInputModule>() == null)
            keeper.gameObject.AddComponent<InputSystemUIInputModule>();

        foreach (var legacy in keeper.GetComponents<StandaloneInputModule>())
        {
            // Destroy는 프레임 끝에 처리되므로 먼저 꺼야 그 사이 예외가 안 난다.
            legacy.enabled = false;
            Object.Destroy(legacy);
        }

        foreach (var es in all)
        {
            if (es == keeper) continue;
            // EventSystem이 둘이면 서로 입력을 뺏어 클릭이 씹힌다. 정본만 남긴다.
            es.gameObject.SetActive(false);
            Object.Destroy(es.gameObject);
        }
    }
}
