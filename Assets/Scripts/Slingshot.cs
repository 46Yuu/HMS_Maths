using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    public LineRenderer[] lineRenderers;
    public Transform[] stripPositions;
    public Transform center;
    public Transform idlePosition;

    public Vector3 currentPosition;
    public Camera cam;

    public float maxLength = 3.0f;
    public float bottomBoundary = -2.0f;

    private bool isMouseDown;
    
    public GameObject birdPrefab;
    public float birdPositionOffset = 0.2f;

    private GameObject bird;
    private Trajectory birdTrajectory;

    void Start()
    {
        lineRenderers[0].positionCount = 2;
        lineRenderers[1].positionCount = 2;
        lineRenderers[0].SetPosition(0, stripPositions[0].position);
        lineRenderers[1].SetPosition(0, stripPositions[1].position);
        
        CreateBird();
    }

    void Update()
    {
        if (isMouseDown)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;

            currentPosition = cam.ScreenToWorldPoint(mousePosition);
            currentPosition = center.position + Vector3.ClampMagnitude(currentPosition - center.position, maxLength);
            currentPosition = ClampBoundary(currentPosition);

            SetStrips(currentPosition);
        }
        else
        {
            ResetStrips();
        }
    }

    void CreateBird()
    {
        bird = Instantiate(birdPrefab, stripPositions[0].position, Quaternion.identity);
        birdTrajectory = bird.GetComponent<Trajectory>();
    }

    private void OnMouseDown()
    {
        isMouseDown = true;
    }

    private void OnMouseUp()
    {
        isMouseDown = false;
    
        if (bird != null)
        {
            Vector3 launchDirection = (center.position - currentPosition).normalized;
            float launchForce = Vector3.Distance(currentPosition, center.position) * 10f;
    
            birdTrajectory.alpha = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
            birdTrajectory.l1 = launchForce;
    
            bird = null;
            CreateBird();
        }
    
        currentPosition = idlePosition.position;
    }

    void ResetStrips()
    {
        currentPosition = idlePosition.position;
        SetStrips(currentPosition);
    }

    void SetStrips(Vector3 position)
    {
        lineRenderers[0].SetPosition(1, position);
        lineRenderers[1].SetPosition(1, position);

        if (bird)
        {
            Vector3 dir = position - center.position;
            bird.transform.position = position + dir.normalized * birdPositionOffset;
            bird.transform.right = -dir.normalized;   
        }
    }

    Vector3 ClampBoundary(Vector3 vector)
    {
        vector.y = Mathf.Clamp(vector.y, bottomBoundary, 1000);
        return vector;
    }
}
