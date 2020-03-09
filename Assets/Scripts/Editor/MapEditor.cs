using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    // Ref instance editée
    private MapManager m_CurrentTarget = null;

    // Variable Edit Mode
    private bool m_IsInEditSquareMode = false;
    private bool m_IsInEditEdgeMode = false;
    private SquareState m_CurrentEditState = SquareState.Normal;
    private bool m_CurrentEditEdge = false;
    private BrushType m_CurrentEditBrushType = BrushType.One;

    private bool m_ShowMapView = false;

    // Global edit constraint
    private bool m_CanEditMouseAndKeyConstraints = true;

    private void OnEnable()
    {
        // Recupération de l'instance editée
        m_CurrentTarget = (MapManager)target;

        m_ShowMapView = m_CurrentTarget.navContainer.activeSelf;

        LoadLastEditorState();
    }

    private void OnDisable()
    {
        SaveCurrentEditorState();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("============ EDITOR MAP =============", EditorStyles.boldLabel);
        // Affichage Bouton Initialize Map And Generate.
        if (GUILayout.Button("Initialize Map Randomly"))
        {
            m_CurrentTarget.InitializeMapRandomly();
        }

        // Affichage Bouton de Reset de la map.
        if (GUILayout.Button("Initialize Empty Map"))
        {
            m_CurrentTarget.InitiliazeEmptyMap();
        }

        GUILayout.Space(10);
        m_ShowMapView = GUILayout.Toggle(m_ShowMapView, "Show View");
        m_CurrentTarget.navContainer.SetActive(m_ShowMapView);


        GUILayout.Label("============ EDIT SQUARE =============", EditorStyles.boldLabel);
        m_IsInEditSquareMode = GUILayout.Toggle(m_IsInEditSquareMode, "Edit Square Mode");
        if(m_IsInEditSquareMode)
        {
            m_IsInEditEdgeMode = false;
            // Affichage Bouton d'edition du state d'un square de la map
            m_CurrentEditState = (SquareState)EditorGUILayout.EnumPopup(m_CurrentEditState);
            m_CurrentEditBrushType = (BrushType)EditorGUILayout.EnumPopup(m_CurrentEditBrushType);
        }



        GUILayout.Label("============ EDIT EDGE =============", EditorStyles.boldLabel);
        m_IsInEditEdgeMode = GUILayout.Toggle(m_IsInEditEdgeMode, "Edit Edge Mode");
        if(m_IsInEditEdgeMode)
        {
            m_IsInEditSquareMode = false;

            m_CurrentEditEdge = GUILayout.Toggle(m_CurrentEditEdge, "Add/Remove");
        }

        GUILayout.Label("============ MAP PROPERTIES =============", EditorStyles.boldLabel);
        base.OnInspectorGUI();

    }

    // Ici on affiche dans la Scene les elements necessaire.
    // Ici on recupère les inputs qui ont été fait dans la Scene.
    private void OnSceneGUI()
    {
        // On valide si on peut editer
        UpdateGlobalEditState();

        // Si on peut editer
        if (m_CanEditMouseAndKeyConstraints)
        {
            // Si on est en edit square mode
            if (m_IsInEditSquareMode)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // Calculate Interact Coordonnee
                Vector3 intersectPos = CalculateInteractPosition();
                intersectPos.y = 0;

                Vector3 intersectPosInt = Vector3.zero;
                intersectPosInt.x = Mathf.FloorToInt(intersectPos.x);
                intersectPosInt.z = Mathf.FloorToInt(intersectPos.z);

                //Debug.Log("Scene GUI is painted");
                DisplayGizmoEditSquareInScene(intersectPosInt);

                EditCurrentSquareState(intersectPosInt);
            }


            // Si on est en edit edge mode
            else if (m_IsInEditEdgeMode)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // Calculate Interact Coordonnee
                Vector3 intersectPos = CalculateInteractPosition();
                intersectPos.y = 0.5f ;

                Vector3 intersectPosInt = Vector3.zero;
                intersectPosInt.x = Mathf.FloorToInt(intersectPos.x );
                intersectPosInt.z = Mathf.FloorToInt(intersectPos.z + 0.5f);

                //Debug.Log("Scene GUI is painted");
                DisplayGizmoEditEdgeInScene(intersectPosInt);

                EditCurrentSquareState(intersectPosInt);
            }
        }
    }

    private Vector3 CalculateInteractPosition()
    {
        Vector3 mousePosition = Event.current.mousePosition;

        // recupération d'un Ray (rayon) à partir de la position de la mouse sur l'ecran
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Creation d'un plan dans l'espace
        // Il n'y a pas de plan créer dans la scene
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);

        // On envoi le rayon par rapport à la scene
        if (hPlane.Raycast(ray, out float distance))
        {
            // get the hit point:
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    #region DISPLAY EDIT SQUARE GIZMO
    private void DisplayGizmoEditSquareInScene(Vector3 intersectPosInt)
    {
        // Affichage du gizmo uniquement si on est dans la grille
        if (TestIfPositionIsInLimit(intersectPosInt))
        {
            DrawSquareSelectedGizmo(intersectPosInt);

        }
        SceneView.RepaintAll();
    }

    private void DisplayGizmoEditEdgeInScene(Vector3 intersectPosInt)
    {
        // Affichage du gizmo uniquement si on est dans la grille
        if (TestIfPositionIsInLimit(intersectPosInt))
        {
            DrawEdgeSelectedGizmo(intersectPosInt);

        }
        SceneView.RepaintAll();
    }

    private bool TestIfPositionIsInLimit(Vector3 position)
    {
        return position.x >= 0 && position.z >= 0 && position.x < m_CurrentTarget.mapData.width && position.z < m_CurrentTarget.mapData.height;
    }

    private void DrawSquareSelectedGizmo(Vector3 intersectPos)
    {
        Handles.color = Color.cyan;

        Vector3 pos1 = intersectPos;
        Vector3 pos2 = intersectPos;
        pos2.x = intersectPos.x + 1;
        float screenSpace = 20;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
        pos1.x += 1;
        pos1.z += 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);

        pos1 = intersectPos;
        pos2.x = intersectPos.x;
        pos2.z = intersectPos.z + 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
        pos1.x += 1;
        pos1.z += 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
    }

    private void DrawEdgeSelectedGizmo(Vector3 intersectPos)
    {
        Handles.color = Color.cyan;

        Vector3 pos1 = intersectPos;
        Vector3 pos2 = intersectPos;
        pos2.x = intersectPos.x + 1;
        float screenSpace = 20;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
        pos1.x += 1;
        pos1.z += 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);

        pos1 = intersectPos;
        pos2.x = intersectPos.x;
        pos2.z = intersectPos.z + 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
        pos1.x += 1;
        pos1.z += 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
    }
    #endregion DISPLAY EDIT SQUARE GIZMO

    #region EDIT SQUARE
    private void EditCurrentSquareState(Vector3 intersectPos)
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    m_CurrentTarget.SetSquareState(intersectPos, m_CurrentEditState);
                    m_CurrentTarget.CreateMapViewFromData();
                    SetObjectDirty(m_CurrentTarget);
                    break;
            }
        }
    }

    private void EditCurrentEdgeState(Vector3 intersectPos)
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    m_CurrentTarget.SetSquareState(intersectPos, m_CurrentEditState);
                    m_CurrentTarget.CreateMapViewFromData();
                    SetObjectDirty(m_CurrentTarget);
                    break;
            }
        }
    }
    


    // Skip Update si en train d'appuyer sur Alt
    private void UpdateGlobalEditState()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.LeftAlt)
                {
                    m_CanEditMouseAndKeyConstraints = false;
                }
                break;
            case EventType.KeyUp:
                if (e.keyCode == KeyCode.LeftAlt)
                {
                    m_CanEditMouseAndKeyConstraints = true;
                }
                break;
        }
    }
    #endregion

    #region LOAD / SAVE EDITOR STATE
    private void SetObjectDirty(Object objectDirty)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(objectDirty);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void LoadLastEditorState()
    {
        m_IsInEditSquareMode = EditorPrefs.GetBool(nameof(m_IsInEditSquareMode));
        m_IsInEditEdgeMode = EditorPrefs.GetBool(nameof(m_IsInEditEdgeMode));
        m_CurrentEditState = (SquareState)EditorPrefs.GetInt(nameof(m_CurrentEditState));
    }
    private void SaveCurrentEditorState()
    {
        EditorPrefs.SetBool(nameof(m_IsInEditEdgeMode), m_IsInEditEdgeMode);
        EditorPrefs.SetBool(nameof(m_IsInEditSquareMode), m_IsInEditSquareMode);
        EditorPrefs.SetInt(nameof(m_CurrentEditState), (int)m_CurrentEditState);
    }
    #endregion
}