using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdMovement : MonoBehaviour
{
    public bool isShooted;
    public float movementSpeed;
    private int indexMove;
    private Trajectory trajectory;
    private Vector3 targetPosition;
    public TrailRenderer trail;
    
    void Start()
    {
        isShooted = false;
        indexMove = 0;
        trajectory = GetComponent<Trajectory>();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;
    }
    
    void Update()
    {
        if (isShooted && trajectory.positions.Count > 0)
        {
            targetPosition = new Vector3(
                trajectory.positions[indexMove].x,
                trajectory.positions[indexMove].y,
                -2
            );
            
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movementSpeed * Time.deltaTime
            );
            
            if (indexMove < trajectory.positions.Count - 1)
            {
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
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                indexMove++;
                
                if (indexMove >= trajectory.positions.Count-1)
                {
                    isShooted = false;
                    indexMove = 0;
                    trail.enabled = true;
                    trail.Clear();
                }
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
            movementSpeed = forceL1*2.5f;
            transform.position = new Vector3(
                trajectory.positions[0].x,
                trajectory.positions[0].y,
                transform.position.z
            );
        }
    }
}
