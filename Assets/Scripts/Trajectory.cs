using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public struct Position
{
    public float x;
    public float y;

    public Position(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}
public class Trajectory : MonoBehaviour
{
    private float _m = 0.8f;
    private float _g = 9.81f;
    private float _k = 10f;
    private float _f2;
    
    float dt = 0.1f;
    private float x;
    private float y;
    private float vx;
    private float vy;

    public float alpha;
    public float l1;

    public List<Position> positions;
    
    public GameObject prefab;

    private float _timerPressed;
    private bool _isPressed = false;
    
    private List<GameObject> trajectoryObjets;
    public LineRenderer lineRenderer;
 
    // Start is called before the first frame update
    void Start()
    {
        trajectoryObjets = new List<GameObject>();
        _f2 = 0.2f / _m;
        positions = new List<Position>();
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.sortingOrder = 1;
            lineRenderer.material = new Material (Shader.Find ("Sprites/Default"));
            lineRenderer.material.color = Color.red; 
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;  // No points at the start
        }
        /*float rad = DegreeToRadian(alpha);
        positions = LancerOiseauFrottementRecurrence(rad, l1);
        Debug.Log(positions.Count);
        DrawTrajectory();
        _timerPressed = 0;*/
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isPressed)
        {
            _timerPressed += Time.deltaTime;
        }
    }

    public void DrawTrajectory()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = positions.Count;  

            int i = 0;
            foreach (var position in positions)
            {
                lineRenderer.SetPosition(i, new Vector3(position.x, position.y, -1));
                i++;
                GameObject obj = Instantiate(prefab, new Vector3(position.x, position.y, -1), Quaternion.identity);
                trajectoryObjets.Add(obj);
            }
        }
       
    }

    public void RemoveTrajectory()
    {
        positions.Clear();
        foreach (GameObject obj in trajectoryObjets)
        {
            Destroy(obj);
        }
    }

    public float DegreeToRadian(float deg)
    {
        return 2 * Mathf.PI * deg / 360;
    }
    public float VitesseInitiale(float alpha, float l1)
    {
        return l1 * Mathf.Sqrt(_k / _m) * Mathf.Sqrt(1 - Mathf.Pow(_m * _g * Mathf.Sin(alpha) / (_k * l1), 2f));
    }

    public List<Position> LancerOiseauFrottementRecurrence(float _alpha, float _l1)
    {
        List<Position> positions = new List<Position>();
        float v0 = VitesseInitiale(_alpha, _l1);
        x = transform.position.x;
        y = transform.position.y;
        vx = v0 * Mathf.Cos(_alpha);
        vy = v0 * Mathf.Sin(_alpha);
        positions.Add(new Position(x,y));
        
        float jumpImpulse = 10.0f;      // Vertical speed boost (jump strength)
        bool jumped = false;     // Duration of the jump effect (can be small)
        int maxIterations = 1000; // Prevent infinite loops

        while (positions.Count < maxIterations && positions[positions.Count-1].y > 0f)
        {
            if (x > _timerPressed && vy <= 0 && !jumped && _isPressed)  // When the object hits the threshold and is falling
            {
                vy += jumpImpulse; // Apply a vertical "jump" force
                jumped = true;
                Debug.Log("JUMP at " + _timerPressed);
            }
            
            float newX = x + vx * dt;
            float newY = y + vy * dt;
            
            // Validate numbers (crucial!)
            if (float.IsNaN(newX) || float.IsInfinity(newX) || 
                float.IsNaN(newY) || float.IsInfinity(newY))
            {
                break;
            }
            x = newX;
            y = newY;
            
            if (y < 0) break;
            positions.Add(new Position(newX, newY));
            vx += -_f2 * vx * dt;
            vy += -(_g + _f2 * vy) * dt;
        }
        return positions;
    }

    public void Pressed()
    {
        _isPressed = true;
        positions.Clear();
        foreach (var go in trajectoryObjets)
        {
            Destroy(go);
        }
        float rad = DegreeToRadian(alpha);
        positions = LancerOiseauFrottementRecurrence(rad, l1);
        DrawTrajectory();
    }
}
