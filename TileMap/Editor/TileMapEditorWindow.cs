using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TileMapEditorWindow : EditorWindow
{
    public enum CreationMode
    {
        None, // 배치 안 함
        Basic, // 기본 생성 모드
        TileBased, // 타일 기반 생성 모드
        ColliderBased, // 콜라이더 기반 생성 모드
        Eraser, // 지우기

    }

    private AllTilePrefabSO allTilePrefabSO;
    private Vector2 scrollPosition;
    private int selectedIndex = -1; // 선택된 프리팹의 인덱스
    private GameObject parentObject; // 프리팹을 배치할 부모 오브젝트
    private float tileSize = 1.0f; // 타일 크기, 필요에 따라 조정 가능
    private float angle = 0f;
    private CreationMode creationMode = CreationMode.Basic; // 생성 모드를 enum으로 변경

    //미리보기 기능
    private GameObject ghost;
    private Material ghostMaterial;

    //모델 단축키
    private int hotModel1;
    private int hotModel2;
    private int hotModel3;
    private int hotModel4;
    private int hotModel5;

    //드래그 기능
    private Texture2D dragImage;
    private int dragIndex;
    private bool isDrag;

    [MenuItem("Tools/TileMapEditor")]
    public static void ShowWindow()
    {
        GetWindow<TileMapEditorWindow>("TileMap Editor");
    }
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        //프리펩 로드 및 디스플레이 코드
        if (GUILayout.Button("Load Tile Prefabs"))
        {
            LoadTilePrefabs();
        }

        if (allTilePrefabSO == null) return;

        if (allTilePrefabSO != null && allTilePrefabSO.prefabList.Count > 0)
        {
            // 가로 스크롤 뷰를 생성
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150), GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < allTilePrefabSO.prefabList.Count; i++)
            {
                DrawPrefabPreview(i);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        // 프리팹 생성 모드 선택 UI
        creationMode = (CreationMode)GUILayout.Toolbar((int)creationMode, new string[] { "배치 안함", "기본", "타일 기반", "콜라이더 기반", "지우개" });

        //부모 오브젝트 선택
        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

        // 타일 크기 입력 UI (타일 기반 및 콜라이더 기반 생성 모드에서 사용)
        if (creationMode == CreationMode.TileBased || creationMode == CreationMode.ColliderBased)
        {
            tileSize = EditorGUILayout.FloatField("Tile Size", tileSize);
        }

        if (creationMode != CreationMode.Eraser && creationMode != CreationMode.None)
        {
            angle = EditorGUILayout.FloatField("Angle", angle);
        }

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 5; ++i)
        {
            int index = -1;

            switch (i)
            {
                case 0:
                    index = hotModel1;
                    break;
                case 1:
                    index = hotModel2;
                    break;
                case 2:
                    index = hotModel3;
                    break;
                case 3:
                    index = hotModel4;
                    break;
                case 4:
                    index = hotModel5;
                    break;
            }

            Texture2D previewImage = null;
            if (index != -1)
            {
                GameObject prefab = allTilePrefabSO.prefabList[index];
                previewImage = AssetPreview.GetAssetPreview(prefab);
            }

            GUIStyle style = new GUIStyle(GUI.skin.button);
            if (GUILayout.Button(new GUIContent(previewImage, $"Preset {i + 1}"), style, GUILayout.Width(100), GUILayout.Height(120)))
            {
                switch (i)
                {
                    case 0:
                        hotModel1 = selectedIndex;
                        break;
                    case 1:
                        hotModel2 = selectedIndex;
                        break;
                    case 2:
                        hotModel3 = selectedIndex;
                        break;
                    case 3:
                        hotModel4 = selectedIndex;
                        break;
                    case 4:
                        hotModel5 = selectedIndex;
                        break;
                }
            }
        }
        EditorGUILayout.EndHorizontal();


        if (isDrag && dragImage != null)
        {
            Vector2 mousePosition = Event.current.mousePosition;
            GUI.DrawTexture(new Rect(mousePosition.x, mousePosition.y, 100, 120), dragImage);
            Repaint();
        }

    }

    private void MiniSceneGUI()
    {
        //프리펩 로드 및 디스플레이 코드
        if (GUILayout.Button("Load Tile Prefabs"))
        {
            LoadTilePrefabs();
        }

        if (allTilePrefabSO != null && allTilePrefabSO.prefabList.Count > 0)
        {
            // 가로 스크롤 뷰를 생성
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150), GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < allTilePrefabSO.prefabList.Count; i++)
            {
                DrawPrefabPreview(i);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        // 프리팹 생성 모드 선택 UI
        creationMode = (CreationMode)GUILayout.Toolbar((int)creationMode, new string[] { "배치 안함", "기본", "타일 기반", "콜라이더 기반", "지우개" });
    }

    private void HotKey()
    {
        Event e = Event.current;

        //isInputAlt == e.type == EventType.KeyDown 

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Q && (e.control || e.command))
        {
            angle -= 90;
            e.Use();

            Repaint(); // 에디터 윈도우를 강제로 다시 그리도록 합니다.
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.E && (e.control || e.command))
        {
            angle += 90;
            e.Use();

            Repaint(); // 에디터 윈도우를 강제로 다시 그리도록 합니다.
        }

    }

    private void DrawPrefabPreview(int index)
    {
        GameObject prefab = allTilePrefabSO.prefabList[index];
        Texture2D previewImage = AssetPreview.GetAssetPreview(prefab);

        // 선택된 프리팹에 대한 시각적인 변화를 위한 GUIStyle
        GUIStyle style = new GUIStyle(GUI.skin.button);
        if (index == selectedIndex)
        {
            style.normal.background = MakeTex(2, 2, new Color(0.8f, 0.8f, 0.8f, 1.0f)); // 선택된 프리팹에 대한 배경 색상 변경
        }

        // 프리팹 미리보기와 이름을 버튼으로 표시
        if (GUILayout.Button(new GUIContent(previewImage, prefab.name), style, GUILayout.Width(100), GUILayout.Height(120)))
        {
            isDrag = true;
            dragImage = previewImage;
            ChangeIndex(index); // 프리팹 선택 시 인덱스 업데이트
        }

        // 프리팹 이름 표시
        EditorGUILayout.LabelField(prefab.name, GUILayout.Width(100));
    }

    private void LoadTilePrefabs()
    {
        string path = "Assets/Prefabs/Tile/AllTilePrefabSO.asset";
        allTilePrefabSO = AssetDatabase.LoadAssetAtPath<AllTilePrefabSO>(path);

        if (allTilePrefabSO == null)
        {
            Debug.LogError("Selected asset is not an AllTilePrefabSO.");
        }

        string path2 = "Assets/1.@Art/1.3D/BaseMap/2rd_Map_Tiles_Walls/GhostMaterial.mat";
        ghostMaterial = AssetDatabase.LoadAssetAtPath<Material>(path2);

        if (ghostMaterial == null)
        {
            Debug.LogError("Selected asset is not an ghostMaterial.");
        }
    }

    // 배경색 변경을 위한 Texture2D 생성 함수
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }


    private void OnSceneGUI(SceneView sceneView)
    {
        Event _event = Event.current;

        if (_event.alt)
        {

        }
        else
        {
            if (selectedIndex < 0 || selectedIndex >= allTilePrefabSO.prefabList.Count)
                return;

            if (_event.shift)
            {
                Shift(_event);
            }
            else
            {
                NotShift(_event);
            }

            HotKey();
        }
    }

    private void Shift(Event _event)
    {
        if (_event.type == EventType.KeyDown)
        {
            switch (_event.keyCode)
            {
                case KeyCode.Q:
                    creationMode = CreationMode.None;
                    break;
                case KeyCode.W:
                    creationMode = CreationMode.Basic;
                    break;
                case KeyCode.E:
                    creationMode = CreationMode.TileBased;
                    break;
                case KeyCode.R:
                    creationMode = CreationMode.ColliderBased;
                    break;
                case KeyCode.T:
                    creationMode = CreationMode.Eraser;
                    break;

                case KeyCode.Alpha1:
                    ChangeIndex(hotModel1);
                    break;
                case KeyCode.Alpha2:
                    ChangeIndex(hotModel2);
                    break;
                case KeyCode.Alpha3:
                    ChangeIndex(hotModel3);
                    break;
                case KeyCode.Alpha4:
                    ChangeIndex(hotModel4);
                    break;
                case KeyCode.Alpha5:
                    ChangeIndex(hotModel5);
                    break;
            }
            Repaint();
        }
        MiniSceneGUI();
    }

    private void NotShift(Event _event)
    {
        if (_event.type == EventType.MouseDown && _event.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(_event.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 position = hit.point;

                if (creationMode == CreationMode.Eraser)
                {
                    EraseObject(hit.collider.gameObject);
                }

                // 타일 기반 및 콜라이더 기반 생성 모드에서 타일 크기를 고려하여 위치 조정
                if (creationMode == CreationMode.TileBased || creationMode == CreationMode.ColliderBased)
                {
                    position = AdjustPositionToTileGrid(position, hit.normal);
                }

                if (creationMode != CreationMode.ColliderBased || hit.collider != null) // 콜라이더 기반 모드에서는 콜라이더가 있을 때만 생성
                {
                    if (creationMode != CreationMode.None && creationMode != CreationMode.Eraser)
                    {
                        CreatePrefabInstance(position);
                    }
                    else if (creationMode == CreationMode.Eraser)
                    {
                        Selection.activeGameObject = null;
                    }
                }

                Event.current.Use();
            }
            else
            {
                if (creationMode == CreationMode.Eraser && Selection.activeGameObject != null)
                {
                    EraseObject(Selection.activeGameObject);
                    Selection.activeGameObject = null;
                }
                else if (creationMode != CreationMode.ColliderBased || hit.collider != null) // 콜라이더 기반 모드에서는 콜라이더가 있을 때만 생성
                {
                    if (creationMode != CreationMode.None && creationMode != CreationMode.Eraser)
                    {
                        float rayLength = (0 - ray.origin.y) / ray.direction.y;
                        Vector3 hitPointAtY0 = ray.origin + ray.direction * rayLength;

                        if (creationMode == CreationMode.TileBased || creationMode == CreationMode.ColliderBased)
                        {
                            hitPointAtY0 = AdjustPositionToTileGrid(hitPointAtY0, Vector3.zero);
                        }

                        CreatePrefabInstance(hitPointAtY0);
                    }
                }
                _event.Use();
            }
        }
        else
        {
            //미리보기 기능

            Ray ray = HandleUtility.GUIPointToWorldRay(_event.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 position = hit.point;

                // 타일 기반 및 콜라이더 기반 생성 모드에서 타일 크기를 고려하여 위치 조정
                if (creationMode == CreationMode.TileBased || creationMode == CreationMode.ColliderBased)
                {
                    position = AdjustPositionToTileGrid(position, hit.normal);
                }

                SetPositionGhost(position);

            }
            else
            {
                if (creationMode != CreationMode.ColliderBased || hit.collider != null) // 콜라이더 기반 모드에서는 콜라이더가 있을 때만 생성
                {
                    if (creationMode != CreationMode.None && creationMode != CreationMode.Eraser)
                    {
                        float rayLength = (0 - ray.origin.y) / ray.direction.y;
                        Vector3 hitPointAtY0 = ray.origin + ray.direction * rayLength;

                        if (creationMode == CreationMode.TileBased || creationMode == CreationMode.ColliderBased)
                        {
                            hitPointAtY0 = AdjustPositionToTileGrid(hitPointAtY0, Vector3.zero);
                        }

                        SetPositionGhost(hitPointAtY0);
                    }
                }
            }
        }
    }

    private void EraseObject(GameObject obj)
    {
        DestroyImmediate(obj);
    }

    private Vector3 AdjustPositionToTileGrid(Vector3 originalPosition, Vector3 hitNormal)
    {
        Vector3 adjustedPosition = originalPosition;

        // 타일 기반 생성 모드에서는 클릭 위치를 타일 크기에 맞게 정렬
        if (creationMode == CreationMode.TileBased)
        {
            adjustedPosition.x = Mathf.Floor(adjustedPosition.x / tileSize) * tileSize + tileSize / 2;
            adjustedPosition.y = Mathf.Floor(adjustedPosition.y / tileSize) * tileSize + tileSize / 2;
            adjustedPosition.z = Mathf.Floor(adjustedPosition.z / tileSize) * tileSize + tileSize / 2;
        }
        // 콜라이더 기반 생성 모드에서는 콜라이더 면에서 타일 크기만큼 떨어진 위치에 생성
        else if (creationMode == CreationMode.ColliderBased)
        {
            adjustedPosition += hitNormal * tileSize;
            adjustedPosition.x = AdjustCordinate(adjustedPosition.x, hitNormal.x);
            adjustedPosition.y = AdjustCordinate(adjustedPosition.y, hitNormal.y);
            adjustedPosition.z = AdjustCordinate(adjustedPosition.z, hitNormal.z);
        }

        return adjustedPosition;
    }

    private float AdjustCordinate(float pos, float normal)
    {
        pos = Mathf.Floor(pos / tileSize) * tileSize + tileSize / 2;
        if (normal > 0)
        {
            pos = Mathf.Floor(pos / tileSize) * tileSize - tileSize / 2;
            //pos += Mathf.Floor(normal);
        }
        else if (normal < 0)
        {
            pos = Mathf.Floor(pos / tileSize) * tileSize + tileSize / 2;
            //pos += normal;
        }

        return pos;
    }

    private void CreatePrefabInstance(Vector3 position)
    {
        GameObject prefab = allTilePrefabSO.prefabList[selectedIndex];
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        Undo.RegisterCreatedObjectUndo(instance, "Create " + prefab.name); // Undo 기능을 위한 등록

        instance.transform.position = position;
        instance.transform.eulerAngles = new Vector3(0, angle, 0);
        if (parentObject != null)
        {
            instance.transform.SetParent(parentObject.transform, true);
        }
    }

    private void ChangeIndex(int index)
	{
        selectedIndex = index;
        SetGhost();
	}

	#region 미리보기 기능

    private void SetGhost()
    {
        if(ghost != null)
        {
            DestroyImmediate(ghost);
        }

        if (selectedIndex == -1) return;

        GameObject prefab = allTilePrefabSO.prefabList[selectedIndex];
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(instance, "Create " + prefab.name); // Undo 기능을 위한 등록
        ghost = instance;
        MeshRenderer[] meshRenderers = ghost.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer renderer in meshRenderers)
        {
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = ghostMaterial;
            }

            renderer.sharedMaterials = newMaterials;
        }
    }

    private void SetPositionGhost(Vector3 pos)
	{
        if (ghost == null) return;
        ghost.transform.position = pos;
        ghost.transform.eulerAngles = new Vector3(0, angle, 0);
    }

	#endregion
}
