using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseMapHUD : MonoBehaviour
{
    [SerializeField] Transform p1MapIcon;
    [SerializeField] Transform p2MapIcon;

    [SerializeField] Transform p1StandbyPosition;
    [SerializeField] Transform p1ActPhasePos;
    [SerializeField] Transform p1NumPhasePos;
    [SerializeField] Transform p1BattlePhasePos;

    [SerializeField] Transform p2StandbyPosition;
    [SerializeField] Transform p2ActPhasePos;
    [SerializeField] Transform p2NumPhasePos;
    [SerializeField] Transform p2BattlePhasePos;

    public void MovePlayerToPhasePosition(PlayerCode player, GamePhase phase)
    {
        Transform playerTransform = (player == PlayerCode.P1 ? p1MapIcon : p2MapIcon);
        Vector3 phasePosition = Vector3.zero;
        switch (phase)
        {
            case GamePhase.ACTION:
                phasePosition = (player == PlayerCode.P1 ? p1ActPhasePos.position : p2ActPhasePos.position);
                break;
            case GamePhase.NUMBER:
                phasePosition = (player == PlayerCode.P1 ? p1NumPhasePos.position : p2NumPhasePos.position);
                break;
            case GamePhase.BATTLE:
                phasePosition = (player == PlayerCode.P1 ? p1BattlePhasePos.position : p2BattlePhasePos.position);
                break;
            default:
                phasePosition = (player == PlayerCode.P1 ? p1StandbyPosition.position : p2StandbyPosition.position);
                break;
        }
        StartCoroutine(TransformToPosition(playerTransform, phasePosition));
    }

    private IEnumerator TransformToPosition(Transform playerTrnsfm, Vector3 phasePos)
    {
        int shipBobbing = 0;
        float lerpValue = 0f;
        float startX = playerTrnsfm.position.x;
        float startY = playerTrnsfm.position.y;
        float endX = phasePos.x;
        while (lerpValue < 1f)
        {
            lerpValue += Time.deltaTime;
            if (lerpValue > 1f)
            {
                lerpValue = 1f;
            }

            playerTrnsfm.position = new Vector3(Mathf.Lerp(startX, endX, lerpValue), startY + ((shipBobbing % 15 < 7) ? (-1f/32f) : 0f), playerTrnsfm.position.z);
            shipBobbing++;
            yield return null;
        }
        playerTrnsfm.position = new Vector3(endX, startY, playerTrnsfm.position.z);
        yield return null;
    }
}
