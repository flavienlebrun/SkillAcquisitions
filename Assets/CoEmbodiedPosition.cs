using UnityEngine;

public class CoEmbodiedPosition : MonoBehaviour
{
    // Références aux deux GameObjects dont on veut la moyenne des positions
    public Transform MasterHand;
    public Transform StudentHand;
    public bool CoEmbodiment = true;
    // La sphère dont la position sera mise à jour
    private Transform StudentVisibleHand;

    void Start()
    {
        // On suppose que ce script est attaché à la sphère
        StudentVisibleHand = transform;
    }

    void Update()
    {
        if (MasterHand != null && StudentHand != null)
        {
            Vector3 averagePosition = StudentHand.position;
            // Calcul de la position moyenne
            if(CoEmbodiment)
            {
                averagePosition = (MasterHand.position + StudentHand.position)/2f;
            }
            

            
            // Mise à jour de la position de la sphère
            StudentVisibleHand.position = averagePosition;
        }
    }
}
