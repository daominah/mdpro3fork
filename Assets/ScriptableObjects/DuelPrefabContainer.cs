using UnityEngine;

[CreateAssetMenu]
public class DuelPrefabContainer : ScriptableObject
{
    [Header("Duel Prefabs")]
    public GameObject cardModel;
    public GameObject duelButton;
    public GameObject duelLpText;
    [Header("Duel Phase")]
    public GameObject duelDrawPhaseNear;
    public GameObject duelDrawPhaseFar;
    public GameObject duelStandbyPhaseNear;
    public GameObject duelStandbyPhaseFar;
    public GameObject duelMain1PhaseNear;
    public GameObject duelMain1PhaseFar;
    public GameObject duelBattlePhaseNear;
    public GameObject duelBattlePhaseFar;
    public GameObject duelMain2PhaseNear;
    public GameObject duelMain2PhaseFar;
    public GameObject duelEndPhaseNear;
    public GameObject duelEndPhaseFar;
    [Header("Duel Turn Change")]
    public GameObject duelTurnChangeNear;
    public GameObject duelTurnChangeFar;

}
