using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class MultiGameManager : MonoBehaviourPunCallbacks
{
	#region Public Variables
	public GameObject playerPrefab;
	#endregion

	///���� ������ �� �÷��̾� ������ ���������� 
	#region MonoBehaviour CallBacks
	void Start()
	{
		if (playerPrefab == null)
		{
			Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
		}
		else
		{
			Debug.LogFormat("We are Instantiating LocalPlayer");
			PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position, transform.rotation);
		}
	}
    #endregion

    //�濡 ������ �κ��(0)�� �ε���
    #region Photon Callbacks

    public override void OnLeftRoom()
	{
		PhotonNetwork.Destroy(playerPrefab);
		SceneManager.LoadScene(0);

	}
	#endregion

	//���� ����
	#region Public Methods
	public void LeaveRoom()
	{
		PhotonNetwork.Destroy(playerPrefab);
		PhotonNetwork.LeaveRoom();
	}
	#endregion
}
