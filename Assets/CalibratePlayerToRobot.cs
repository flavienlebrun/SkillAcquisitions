using UnityEngine;

public class CalibratePlayerToRobot : MonoBehaviour
{
    public Transform controllerRight; // Assigne le Controller Right (enfant du Player)
    public Transform robotID0;        // Assigne la sphère RobotID0
    public Vector3 offsetHauteur = new Vector3(0.0f,0.0f,0.0f); // offset vertical
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Calculer le décalage entre le Controller Right et RobotID0
            Vector3 offset = robotID0.position - controllerRight.position;

            // Déplacer le Player (et donc le Controller Right) pour annuler ce décalage
            transform.position += offset;
            
            transform.position += offsetHauteur;
            Debug.Log("Calibration terminée ! Le Controller Right est maintenant à la position de RobotID0.");
        }
    }
}
