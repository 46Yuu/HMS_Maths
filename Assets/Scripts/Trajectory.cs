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
    
    float dt = 0.01f;
    private float x;
    private float y;
    private float vx;
    private float vy;

    public float alpha;
    public float l1;
    public int maxIterations = 1000;

    public List<Position> positions;
    
    public GameObject prefab;

    private float _xPosJump;
    private bool _isPressed = false;
    public float jumpImpulse = 5.0f;
    
    public const int maxBounces = 2;
    public float energyKept = 0.5f; 
    
    private List<GameObject> trajectoryObjets;
    public LineRenderer lineRenderer;
 
    // Start is called before the first frame update
    void Start()
    {
        trajectoryObjets = new List<GameObject>();
        _f2 = 0.2f / _m;
        positions = new List<Position>();
        _xPosJump = 0;
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
                /*GameObject obj = Instantiate(prefab, new Vector3(position.x, position.y, -1), Quaternion.identity);
                trajectoryObjets.Add(obj);*/
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

    public List<Position> LancerOiseauFrottementRecurrence(float _alpha, float _l1, Vector2 startPos)
    {
        List<Position> positions = new List<Position>();
        float v0 = VitesseInitiale(_alpha, _l1);
        x = startPos.x;
        y = startPos.y;
        vx = v0 * Mathf.Cos(_alpha);
        vy = v0 * Mathf.Sin(_alpha);
        positions.Add(new Position(x,y));
        
        bool jumped = false;
        int bounceCount = 0;
        bool wasAboveGround = true;

        while (positions.Count < maxIterations && bounceCount <= maxBounces)
        {
            // Check for jump input
            if (x > _xPosJump && !jumped && _isPressed && bounceCount < 1)
            {
                vy += jumpImpulse;
                jumped = true;
                Debug.Log("JUMP at " + _xPosJump);
            }
            
            // Calculate new position
            float newX = x + vx * dt;
            float newY = y + vy * dt;
            
            // Check for ground collision
            if (wasAboveGround && newY <= 0)
            {
                if (bounceCount < maxBounces)
                {
                    // Calculate exact collision point
                    float collisionTime = (0 - y) / vy;
                    float collisionX = x + vx * collisionTime;
                    
                    // Snap to ground at collision point
                    newX = collisionX;
                    newY = 0;
                    
                    // Apply bounce physics
                    vy = -vy * energyKept;
                    bounceCount++;
                    
                    // Recalculate position after bounce
                    newX = newX + vx * (dt - collisionTime);
                    newY = newY + vy * (dt - collisionTime);
                }
                else
                {
                    // Final position at ground level
                    positions.Add(new Position(newX, 0));
                    break;
                }
            }
            
            // Track if we were above ground last frame
            wasAboveGround = y > 0;
            
            // Validate numbers
            if (float.IsNaN(newX) || float.IsInfinity(newX) || 
                float.IsNaN(newY) || float.IsInfinity(newY))
            {
                break;
            }
            
            x = newX;
            y = newY;
            
            // Add position if still simulating
            if (bounceCount <= maxBounces)
            {
                positions.Add(new Position(x, y));
            }
            
            // Apply air resistance and gravity
            vx += -_f2 * vx * dt;
            vy += -(_g + _f2 * vy) * dt;
            
            // Early exit if we've lost all energy after last bounce
            if (bounceCount >= maxBounces && Mathf.Abs(vy) < 0.01f)
            {
                positions.Add(new Position(x, 0));
                break;
            }
        }
        
        return positions;
    }

    public void Pressed(Vector2 startPos, float xPos)
    {
        _isPressed = true;
        positions.Clear();
        float rad = DegreeToRadian(alpha);
        _xPosJump = xPos;
        positions = LancerOiseauFrottementRecurrence(rad, l1, startPos);
        DrawTrajectory();
    }
}
