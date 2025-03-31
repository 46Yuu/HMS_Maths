using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Slingshot : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private Transform camBasePosition;
    
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

    public float scoreTotal;
    
    public static Slingshot instance;

    private int counterBird = 0;
    private int maxBirds = 3;
    
    public TMP_Text scoreTextInGame;

    public Canvas canvasEndScreen;
    public TMP_Text scoreText;
    public TMP_Text birdsLeftText;

    void Start()
    {
        lineRenderers[0].positionCount = 2;
        lineRenderers[1].positionCount = 2;
        lineRenderers[0].SetPosition(0, stripPositions[0].position);
        lineRenderers[1].SetPosition(0, stripPositions[1].position);
        camBasePosition = vcam.transform;
        scoreTotal = 0;
        canvasEndScreen.gameObject.SetActive(false);
        scoreTextInGame.text = "Current Score : " + scoreTotal.ToString("0");
        birdsLeftText.text = "Birds Left: " + (maxBirds-counterBird).ToString("0");

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
            if (bird != null)
            {
                Trajectory trajectory = bird.GetComponent<Trajectory>();
                bird.GetComponent<Rigidbody2D>().isKinematic = true;
                float launchForce = Vector3.Distance(currentPosition, center.position) * 2f;
                float angle = CalculateAngle(center.position, currentPosition);
                birdTrajectory.alpha = angle;
                birdTrajectory.l1 = launchForce;
                
                //trajectory.RemoveTrajectory();
                float rad = trajectory.DegreeToRadian(birdTrajectory.alpha);
                trajectory.positions = trajectory.LancerOiseauFrottementRecurrence(rad, birdTrajectory.l1,
                    new Vector2(currentPosition.x, currentPosition.y));
                trajectory.DrawTrajectory();
            }
        }
        else
        {
            if(bird != null && !bird.GetComponent<BirdMovement>().isShooted)
                ResetStrips();
        }
    }

    public void CreateBird()
    {
        if (counterBird < maxBirds)
        {
            birdsLeftText.text = "Birds Left: " + (maxBirds-counterBird).ToString("0");
            counterBird++;
        }
        else
        {
            ShowEndScreen();
            return;
        }
        bird = null;
        bird = Instantiate(birdPrefab, stripPositions[0].position, Quaternion.identity);
        bird.GetComponent<CircleCollider2D>().enabled = false;
        birdTrajectory = bird.GetComponent<Trajectory>();
        GetComponent<BoxCollider2D>().enabled = true;
        vcam.Follow = bird.transform;
    }

    private void OnMouseDown()
    {
        if (!bird.GetComponent<BirdMovement>().isShooted)
        {
            isMouseDown = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void OnMouseUp()
    {
        isMouseDown = false;
    
        if (bird != null)
        {
            ShootBird();
        }
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
    
    /*void Shoot()
    {
        bird.GetComponent<Rigidbody2D>().isKinematic = true;
        Vector2 launchDirection = ConvertToVector2(birdTrajectory.alpha , birdTrajectory.l1);
        bird.GetComponent<Rigidbody2D>().velocity = launchDirection;

        bird = null;
        Invoke("CreateBird", 2);
    }*/

    void ShootBird()
    {
        bird.GetComponent<BirdMovement>().shootPosition = new Vector2(currentPosition.x, currentPosition.y);
        bird.GetComponent<BirdMovement>().LaunchBird(birdTrajectory.l1);
    }
    
    public float CalculateAngle(Vector3 center, Vector3 point)
    {
        Vector3 direction = center - point;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
    
    public Vector2 ConvertToVector2(float angleInRadians, float force)
    {
        float xDirection = Mathf.Cos(angleInRadians);
        float yDirection = Mathf.Sin(angleInRadians);
        
        Vector2 forceVector = new Vector2(xDirection, yDirection) * force;

        return forceVector;
    }

    private void ShowEndScreen()
    {
        scoreTextInGame.gameObject.SetActive(false);
        birdsLeftText.gameObject.SetActive(false);
        canvasEndScreen.gameObject.SetActive(true);
        scoreText.text = scoreTotal.ToString("0");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void AddScore(float scoreToAdd)
    {
        scoreTotal+= scoreToAdd;
        scoreTextInGame.text = "Current Score : " + scoreTotal.ToString("0");
    }
}
