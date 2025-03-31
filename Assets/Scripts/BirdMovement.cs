using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdMovement : MonoBehaviour
{
    public bool isShooted;
    public float timeBetweenPoints = 0.01f;
    private int indexMove;
    private Trajectory trajectory;
    private Vector3 targetPosition;
    public TrailRenderer trail;
    private float progress;
    private float startTime;
    private Vector3 startPosition;
    private bool hasJumped;
    public Vector2 shootPosition;

    public float currentScore;
    
    void Start()
    {
        isShooted = false;
        indexMove = 0;
        trajectory = GetComponent<Trajectory>();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;
        hasJumped = false;
        currentScore = 0;
    }
    
    void Update()
    {
        if (isShooted && trajectory.positions.Count > 0)
        {
            if (Input.GetMouseButtonDown(0) && !hasJumped)
            {
                trajectory.Pressed(shootPosition, transform.position.x);
                hasJumped = true;
            }
            float elapsed = Time.time - startTime;
            progress = elapsed / timeBetweenPoints;
            
            // Move to next point if time progress for current is over
            if (progress >= 1f && indexMove < trajectory.positions.Count - 1)
            {
                indexMove++;
                startTime = Time.time;
                startPosition = transform.position;
                progress = 0f;
                
                targetPosition = new Vector3(
                    trajectory.positions[indexMove].x,
                    trajectory.positions[indexMove].y,
                    0
                );
            }
            
            if (indexMove < trajectory.positions.Count - 1)
            {
                transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );
                
                // Calculate rotation
                Vector3 nextPos = new Vector3(
                    trajectory.positions[indexMove + 1].x,
                    trajectory.positions[indexMove + 1].y,
                    0
                );
                Vector3 direction = nextPos - transform.position;
                if (direction != Vector3.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
            else
            {
                isShooted = false;
                indexMove = 0;
                trail.enabled = true;
                trail.Clear();
                CalculateScore();
                Slingshot.instance.AddScore(currentScore);
                Slingshot.instance.CreateBird();
            }
        }
    }
    
    public void LaunchBird(float forceL1)
    {
        if (trajectory.positions.Count > 0)
        {
            trail.enabled = true;
            trail.Clear();
            isShooted = true;
            indexMove = 0;
            progress = 0f;
            startTime = Time.time;
            startPosition = new Vector3(
                trajectory.positions[0].x,
                trajectory.positions[0].y,
                0
            );
            targetPosition = new Vector3(
                trajectory.positions[1].x,
                trajectory.positions[1].y,
                0
            );
        }
    }


    private void CalculateScore()
    {
        float xFinal = trajectory.positions[trajectory.positions.Count-1].x;
        if (xFinal >= 26.22f && xFinal <= 27.88f)
        {
            currentScore = 50f;
        }
        else if (xFinal >= 24.54f && xFinal <= 29.56f)
        {
            currentScore = 25f;
        }
        else if (xFinal >= 21.45f && xFinal <= 32.64f)
        {
            currentScore = 10f;
        }
        else
        {
            currentScore = 0f;
        }
    }
}
