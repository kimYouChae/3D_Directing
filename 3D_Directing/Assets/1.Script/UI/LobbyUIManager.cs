using UnityEngine;
using UnityEngine.UI;

// 로비 UI 담당: 패널 켜고 끄기 + 커서 + 버튼 연결(코드에서)
// 호출하는 쪽은 LobbyUIManager.Instance.메서드()만 부름. 순서는 FlowManager가 정함
// 버튼 Inspector의 OnClick은 비워 둘 것 (채우면 두 번 호출됨)
public class LobbyUIManager : Singleton<LobbyUIManager>
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Button resultButton;

    protected override void OnSingletonAwake()
    {
        resultButton.onClick.AddListener(OnResultButton);
        resultPanel.SetActive(false);
    }

    // 전멸 후 결과 UI
    public void ShowResult()
    {
        resultPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DashLog.Log("ResultShow");
    }

    public void HideResult()
    {
        resultPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnResultButton() => FlowManager.Instance.RequestDayEnd();
}
