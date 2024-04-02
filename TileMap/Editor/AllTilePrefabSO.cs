using UnityEngine;
using System.Collections.Generic;

// 스크립터블 오브젝트를 생성하기 위한 클래스 정의
[CreateAssetMenu(fileName = "AllTilePrefabSO", menuName = "TileMapEditor/AllTilePrefabSO", order = 1)]
public class AllTilePrefabSO : ScriptableObject
{
    public List<GameObject> prefabList = new List<GameObject>(); // 프리팹 리스트
}
