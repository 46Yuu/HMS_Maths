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
    // On definit les valeurs de base (masse, force gravitationnelle, constante de raideur et frottement de l'air)
    private float _m = 0.8f;
    private float _g = 9.81f;
    private float _k = 10f;
    private float _f2;
    
    // Coordonnées
    float dt = 0.01f;
    private float x;
    private float y;
    private float vx;
    private float vy;

    // Valeurs à donner à la fonction lancer oiseau
    public float alpha;
    public float l1;
    public int maxIterations = 1000;

    // Liste des positions que l'oiseau va parcourir
    public List<Position> positions;
    
    // Variables de reference et gameplay
    public GameObject prefab;

    private float _xPosJump;
    private bool _isPressed = false;
    public float jumpImpulse = 5.0f;
    
    public const int maxBounces = 2;
    public float energyKept = 0.5f; 
    
    private List<GameObject> trajectoryObjets;
    public LineRenderer lineRenderer;
    
    // Initilalisation des variables dont le frottement de l'air
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
            lineRenderer.positionCount = 0; 
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
                lineRenderer.SetPosition(i, new Vector3(position.x, position.y, 0));
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

    // Conversion d'un angle en degrés en radiant
    public float DegreeToRadian(float deg)
    {
        return 2 * Mathf.PI * deg / 360;
    }

    // Calcul de la vitesse initiale avec un angle et une longueur
    public float VitesseInitiale(float alpha, float l1)
    {
        return l1 * Mathf.Sqrt(_k / _m) * Mathf.Sqrt(1 - Mathf.Pow(_m * _g * Mathf.Sin(alpha) / (_k * l1), 2f));
    }

    // Fonction lancer oiseau qui prend l'angle, la longueur du lancer et la position d'origine
    public List<Position> LancerOiseauFrottementRecurrence(float _alpha, float _l1, Vector2 startPos)
    {
        // Liste pour stocker les positions successives de l'oiseau
        List<Position> positions = new List<Position>();

        // Calcul de la vitesse initiale en fonction de l'angle et de la force appliquée
        float v0 = VitesseInitiale(_alpha, _l1);
        // On définit l'origine x et y du lancer
        x = startPos.x;
        y = startPos.y;

        // Calcul des composantes de vitesse horizontale (vx) et verticale (vy)
        vx = v0 * Mathf.Cos(_alpha);
        vy = v0 * Mathf.Sin(_alpha);

        // Ajout de la position initiale à la liste
        positions.Add(new Position(x,y));
        
        bool jumped = false;
        int bounceCount = 0;
        bool wasAboveGround = true;

        // Boucle principale qui continue tant que l'oiseau n'a pas dépassé un certain nombre d'itérations ou de rebonds
        while (positions.Count < maxIterations && bounceCount <= maxBounces)
        {
            // Si l'oiseau dépasse une certaine position X, qu'il n'a pas encore sauté et qu'on appuie sur un bouton, il saute
            if (x > _xPosJump && !jumped && _isPressed && bounceCount < 1)
            {
                vy += jumpImpulse; // Ajout d'une impulsion verticale pour simuler un saut
                jumped = true; // Marque que le saut a été effectué
            }
            
            // Calcul des nouvelles positions X et Y en fonction des vitesses actuelles
            float newX = x + vx * dt;
            float newY = y + vy * dt;
            
            // Gestion de la collision avec le sol
            if (wasAboveGround && newY <= 0)
            {
                // On vérifie si on peut encore rebondir
                if (bounceCount < maxBounces)
                {
                    // Calcul de l'instant précis où l'oiseau touche le sol pour ne pas passer en dessous
                    float collisionTime = (0 - y) / vy; // Temps qu'il faut pour toucher Y = 0
                    float collisionX = x + vx * collisionTime; // Calcul de X au moment de la collision
                    newX = collisionX;
                    newY = 0; // On place l'oiseau exactement sur le sol
                    
                    // Appliquer le rebond
                    vy = -vy * energyKept;
                    bounceCount++;
                    
                    // Recalculer la position après le rebond
                    newX = newX + vx * (dt - collisionTime);
                    newY = newY + vy * (dt - collisionTime);
                }
                else
                {
                    // Si plus de rebonds possibles, on arrête et ajoute la dernière position sur le sol
                    positions.Add(new Position(newX, 0));
                    break;
                }
            }
            wasAboveGround = y > 0;
            
            // On vérifie si les nouvelles positions sont valides
            if (float.IsNaN(newX) || float.IsInfinity(newX) || 
                float.IsNaN(newY) || float.IsInfinity(newY))
            {
                break;
            }
            
            // Mise à jour des positions
            x = newX;
            y = newY;
            
            // Si le nombre de rebonds est dans la limite autorisée, on enregistre la position
            if (bounceCount <= maxBounces)
            {
                positions.Add(new Position(x, y));
            }
            
            // Appliquer le frottement de l'air et la force gravitationnelle
            vx += -_f2 * vx * dt; // Le frottement de l'air réduit la vitesse horizontale
            vy += -(_g + _f2 * vy) * dt; // La gravité agit vers le bas, combinée avec le frottement de l'air vertical
        }
        
        return positions;
    }

    // Gestion du double saut
    public void Pressed(Vector2 startPos, float xPos)
    {
        _isPressed = true;
        // On vire toutes les positions actuelles
        positions.Clear();
        // On recalcule l'angle en radian à l'aide de l'angle au lancer
        float rad = DegreeToRadian(alpha);
        // On définit la position X au moment du double saut
        _xPosJump = xPos;
        // On remet à jour la liste des positions que l'oiseau va parcourir en changeant uniquement le point d'origine
        positions = LancerOiseauFrottementRecurrence(rad, l1, startPos);
        DrawTrajectory();
    }
}
