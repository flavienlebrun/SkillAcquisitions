using UnityEngine;

public class TrajectoireSuiviObjet : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int nombrePointsMax = 100; // Nombre maximal de points à afficher
    public float tempsAffichage = 3f; // Temps d'affichage de la trajectoire (en secondes)
    private bool enContact = false;

    private void Start()
    {
        // Initialise le Line Renderer
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false; // Désactive le Line Renderer au début
    }
    private void OnTriggerEnter(Collider collider)
    {
        // Active la trajectoire quand l'objet entre en contact avec un autre GameObject

        enContact = true;
        lineRenderer.enabled = true;
    }

    private void OnTriggerExit(Collider collider)
    {

        // Désactive la trajectoire quand l'objet quitte le contact
        enContact = false;
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0; // Réinitialise la trajectoire
    }
    private void Update()
    {
        if (enContact)
        {

            // Ajoute la position actuelle de l'objet au Line Renderer
            AjouterPointTrajectoire(transform.position);

            // Limite le nombre de points
            if (lineRenderer.positionCount > nombrePointsMax)
            {
                DecalerPoints();
            }
        }
    }

    private void AjouterPointTrajectoire(Vector3 point)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);
    }

    private void DecalerPoints()
    {
        // Décale les points pour supprimer le plus ancien
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            lineRenderer.SetPosition(i, lineRenderer.GetPosition(i + 1));
        }
        lineRenderer.positionCount--;
    }
}
