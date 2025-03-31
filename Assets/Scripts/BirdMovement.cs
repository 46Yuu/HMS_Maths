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
    
    void Start()
    {
        isShooted = false;
        indexMove = 0;
        trajectory = GetComponent<Trajectory>();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;
        hasJumped = false;
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
                
                // Update target position for the new segment
                targetPosition = new Vector3(
                    trajectory.positions[indexMove].x,
                    trajectory.positions[indexMove].y,
                    -2
                );
            }
            
            // Interpolate position between current and next point
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
                    -2
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
                // Reached the end of the trajectory
                isShooted = false;
                indexMove = 0;
                trail.enabled = true;
                trail.Clear();
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
                -2
            );
            targetPosition = new Vector3(
                trajectory.positions[1].x,
                trajectory.positions[1].y,
                -2
            );
        }
    }
}
