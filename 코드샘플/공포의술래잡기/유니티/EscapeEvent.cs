using SBSocketSharedLib;
using UnityEngine;

public class EscapeEvent : MonoBehaviour
{
    public float width = 0;
    public float height = 0;

    public Vector2 GoalCeremonyOffsetPos = Vector2.zero;

    float minX = 0;
    float minY = 0;
    float maxX = 0;
    float maxY = 0;

    GameObject guide = null;
    public static EscapeEvent Instance { get; private set; }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void Init()
    {
        Instance = this;

        minX = -(width / 2);
        maxX = (width / 2);
        minY = -(height / 2);
        maxY = (height / 2);

        if (guide == null)
        {
            var hudObj = Game.Instance.HudNode.CreateHudPosEscapeDoor(gameObject, new Vector2(transform.position.x, transform.position.y));
            if (hudObj)
            {
                guide = hudObj.gameObject;
                guide.name = "GuideEscape";
            }
        }
    }

    public void Show(bool isShow)
    {
        gameObject.SetActive(isShow);
        guide.gameObject.SetActive(isShow);
    }

    //private void Update()
    //{
    //    var myCharacterController = Game.Instance.PlayerController;
    //    if (myCharacterController == null) return;
    //    if (myCharacterController.ObserverMode) return;     // 옵저버 모드일때는 탈출 루틴을 돌리지 않는다
    //    if (myCharacterController.Character.Escaped) return;        
    //    var myCharacter = myCharacterController.Character;
    //    if (myCharacter.IsChaser) return;       // 추격자면 Escape 시키지 않는다
    //    var subPos = transform.position - myCharacter.transform.position;

    //    if (minX <= subPos.x &&
    //        maxX >= subPos.x &&
    //        minY <= subPos.y &&
    //        maxY >= subPos.y)
    //    {
    //        //Managers.GameServer.SendEscapeRoom(Game.Instance.PlayerController.Character.Id);

    //        Vector2 CharacterPos = myCharacter.transform.position;
    //        if (GoalCeremonyOffsetPos.y > 0)
    //        {
    //            CharacterPos.y = transform.position.y + GoalCeremonyOffsetPos.y;
    //        }
    //        if(GoalCeremonyOffsetPos.x > 0)
    //        {
    //            CharacterPos.x = transform.position.x + GoalCeremonyOffsetPos.x;
    //        }


    //        myCharacterController.Character.SetEscaped(CharacterPos);
    //        Show(false);
    //    }
    //}
}
