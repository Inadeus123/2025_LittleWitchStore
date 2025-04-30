using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Movement System/Characters/Player")]
public class PlayerSO : ScriptableObject
{
   [field: SerializeField] public PlayerWithOutCarryingItemMovementData PlayerWithOutCarryingItemMovementData { get; private set; }
   [field: SerializeField] public PlayerWithCarryingItemMovementData PlayerWithCarryingItemMovementData { get; private set; }
}
