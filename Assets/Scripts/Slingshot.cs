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

    //  Permet de lancer l'oiseau à l'aide de la souris
    void Update()
    {
        // Si le joueur maintient le clic de souris enfoncé
        if (isMouseDown)
        {
            // Convertit la position de la souris en coordonnées du monde 2D
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;

            currentPosition = cam.ScreenToWorldPoint(mousePosition);

            // Contraint la distance max d'étirement de l'élastique
            currentPosition = center.position + Vector3.ClampMagnitude(currentPosition - center.position, maxLength); // Le clamp magnitude garde la direction mais applique une distance max
            currentPosition = ClampBoundary(currentPosition);
            SetStrips(currentPosition);

            // On met à jour la trajectoire de l'oiseau
            if (bird != null)
            {
                Trajectory trajectory = bird.GetComponent<Trajectory>();
                bird.GetComponent<Rigidbody2D>().isKinematic = true;

                // On applique une force de lancer par rapport à la distance entre le centre du lance oiseau et jusqu'ou on tire l'élastique
                float launchForce = Vector3.Distance(currentPosition, center.position) * 2f;
                // On calcule l'angle appliqué
                float angle = CalculateAngle(center.position, currentPosition);

                // On donne la force et l'angle pour la trajectoire de l'oiseau
                birdTrajectory.alpha = angle;
                birdTrajectory.l1 = launchForce;
                
                //trajectory.RemoveTrajectory();

                // On recupere l'angle en radiant
                float rad = trajectory.DegreeToRadian(birdTrajectory.alpha);
                // Appel de la fonction lancer oiseau frottement recurrence avec l'angle en radiant, la force de lancer et la position d'origine
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
    
    // Calcul de l'angle a l'aide de arc tangente
    public float CalculateAngle(Vector3 center, Vector3 point)
    {
        Vector3 direction = center - point;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
    
    // Conversion d'angle en vecteur directionnel avec cosinus et sinus (comme sur un cercle) et renvoie de la direction multiplié par la force
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
