using UnityEngine;

[CreateAssetMenu(fileName ="New Chess", menuName = "ChessProp")]
[System.Serializable]
public class ChessProp : ScriptableObject {
    public ChessCamp camp;
    public ChessType character;
    public string chessName;
    public GameObject _gfx;
    
    public Stat damageAmout;
    public Stat maxHealth;
    public Stat buff;
    public Stat cost;

    public float attackRange;
    public float attackRate; // attack count per second
    public Vector2 posOnBoard;
    
    public void Init(ChessProp template) {
        camp = template.camp;
        character = template.character;
        chessName = template.chessName;
        _gfx = template._gfx;

        damageAmout = template.damageAmout;
        maxHealth = template.maxHealth;
        buff = template.buff;
        cost = template.cost;

        attackRange = template.attackRange;
        attackRate = template.attackRate;
        posOnBoard = template.posOnBoard;
    }
}
