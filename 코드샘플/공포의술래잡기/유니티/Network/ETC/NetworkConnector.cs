using UnityEngine;
using MessagePack;
using MessagePack.Resolvers;

public class NetworkConnector : MonoBehaviour
{
    NetworkManager networkManager;

    private void Start()
    {
        networkManager = Managers.Network;
        if (networkManager == null)
        {
            SBDebug.LogError("networkManager is Null");
            return;
        }
    }

    static bool serializerRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (!serializerRegistered)
        {
            StaticCompositeResolver.Instance.Register(
                 GeneratedResolver.Instance,
                 StandardResolver.Instance
            );

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

            MessagePackSerializer.DefaultOptions = option;
            serializerRegistered = true;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void EditorInitialize()
    {
        Initialize();
    }

#endif

    public void OnConnect()
    {
        networkManager.OnConnect();
    }

    //public void EnterLobbyComplete()
    //{
    //    SBDebug.Log("==============================================EnterLobbyComplete()");
    //}

    //public void OnLeaveLobby()
    //{
    //    var sendPacket = new CSLeaveLobby();
    //    networkManager.SendMessage(PacketId.CSEnterLobby, sendPacket, 13, LeaveLobbyComplete);
    //}

    //public void LeaveLobbyComplete()
    //{
    //    SBDebug.Log("==============================================LeaveLobbyComplete()");
    //}
}
