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
        // Vérifie si l'oiseau a été lancé et s'il y a une trajectoire disponible
        if (isShooted && trajectory.positions.Count > 0)
        {
            // Permet un saut supplémentaire si le joueur appuie sur la souris
            if (Input.GetMouseButtonDown(0) && !hasJumped)
            {
                trajectory.Pressed(shootPosition, transform.position.x);
                hasJumped = true;
            }
            // Calcul du temps écoulé depuis le dernier mouvement
            float elapsed = Time.time - startTime;
            progress = elapsed / timeBetweenPoints;
            
            // Si l'oiseau doit passer au point suivant sur la trajectoire après 1 seconde de temps écoulé
            if (progress >= 1f && indexMove < trajectory.positions.Count - 1)
            {
                indexMove++; // Avancer d'un point
                startTime = Time.time; // Remettre le temps à 0
                startPosition = transform.position; // Sauvegarde de la position actuelle
                progress = 0f; // Réinitialisation de la progression
                
                // Mise à jour de la nouvelle position cible
                targetPosition = new Vector3(
                    trajectory.positions[indexMove].x,
                    trajectory.positions[indexMove].y,
                    0
                );
            }
            
            // Si l'oiseau est toujours en mouvement
            if (indexMove < trajectory.positions.Count - 1)
            {
                // Déplacement progressif de l'oiseau entre les points avec interpolation linéaire
                transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );
                
                // Rotation de l'oiseau en fonction de la direction du mouvement
                Vector3 nextPos = new Vector3(
                    trajectory.positions[indexMove + 1].x,
                    trajectory.positions[indexMove + 1].y,
                    0
                );
                Vector3 direction = nextPos - transform.position;
                // Si la direction est valide, on applique une rotation
                if (direction != Vector3.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
            // Dans le cas contraire, l'oiseau a atterri
            else
            {
                // Fin du mouvement
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
    
    // Fonction pour lancer l'oiseau avec une force donnée
    public void LaunchBird(float forceL1)
    {
        // Vérifie si la trajectoire contient au moins un point
        if (trajectory.positions.Count > 0)
        {
            trail.enabled = true;
            trail.Clear();
            isShooted = true;
            indexMove = 0;
            progress = 0f;
            startTime = Time.time;

            // Définit la position d'origine
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

    // Fonction pour calculer le score en fonction de la position finale
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
