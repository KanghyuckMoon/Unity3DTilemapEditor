using UnityEngine;
using System.Collections.Generic;

// ��ũ���ͺ� ������Ʈ�� �����ϱ� ���� Ŭ���� ����
[CreateAssetMenu(fileName = "AllTilePrefabSO", menuName = "TileMapEditor/AllTilePrefabSO", order = 1)]
public class AllTilePrefabSO : ScriptableObject
{
    public List<GameObject> prefabList = new List<GameObject>(); // ������ ����Ʈ
}
