using UnityEngine;
using UnityEngine.UI;
using SandboxPlatform.SAMANDA;

public class SAMANDA_UI : MonoBehaviour
{
	[SerializeField]
	SAMANDA_Login LoginUI;
	[SerializeField]
	SAMANDA_Account AccountUI;
	[SerializeField]
	SAMANDA_Termsofuse TermsofuseUI;

	private void Awake()
	{

	}

	public void SetActive(bool enable)
	{
		gameObject.SetActive(enable);
	}

	public void SetUIState(LOGIN_STATE state)
	{
		switch (state)
		{
			case LOGIN_STATE.NO_ACCOUNT_INFO:
			case LOGIN_STATE.LOGIN_DONE:
			case LOGIN_STATE.TERMSOFUSE:
				break;
			default:
				SetActive(false);
				return;
		}

		SetActive(true);

		LoginUI.SetUI(LOGIN_STATE.NO_ACCOUNT_INFO == state);
		AccountUI.SetUI(LOGIN_STATE.LOGIN_DONE == state);
		TermsofuseUI.SetUI(LOGIN_STATE.TERMSOFUSE == state);
	}

	public void OnSideTouch()
	{
		if (LoginUI.gameObject.activeSelf || TermsofuseUI.gameObject.activeSelf)
			return;

		SetActive(false);
	}
}
