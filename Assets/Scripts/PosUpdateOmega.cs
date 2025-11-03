using UnityEngine;
using ROS2;
using System.Threading;

public class SimplePointListenerWithThread : MonoBehaviour
{
    // --- Paramètres publics ---
    [Header("ROS2 Topic")]
    public string positionTopic = "robot_position";  // Topic pour recevoir la position
    public string NodeName = "SimplePointListenerNode";
    // --- Variables privées ---
    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private ISubscription<geometry_msgs.msg.Point> point_sub;

    // --- Données thread-safe ---
    private readonly object dataLock = new object();
    private Vector3 threadSafePosition;  // Position stockée de manière thread-safe
    private Thread processingThread;     // Thread pour le traitement des données
    private bool isRunning = false;       // Flag pour contrôler le thread

    // --- variable pour stocker les valeurs de position et les utiliser dans FixedUpdate ---
    private Vector3 global_pos;
    // --- Initialisation ---
    void Start()
    {
        // Récupère le composant ROS2Unity
        ros2Unity = GetComponent<ROS2UnityComponent>();

        // Crée le nœud ROS2 et s'abonne au topic "robot_position"
        if (ros2Unity.Ok())
        {
            ros2Node = ros2Unity.CreateNode(NodeName);
            point_sub = ros2Node.CreateSubscription<geometry_msgs.msg.Point>(
                positionTopic,
                PointCallback
            );
            Debug.Log($"Abonné au topic ROS2: {positionTopic}");
        }

        // Initialise la position thread-safe
        threadSafePosition = Vector3.zero;

        // Démarre le thread de traitement
        isRunning = true;
        processingThread = new Thread(ProcessingLoop);
        processingThread.Start();
    }

    // --- Callback pour recevoir la position cette fonction tourne dans le thread ros2 (createSubscription<geometry_msgs.msg.Point>(positionTopic,PointCallback)---
    private void PointCallback(geometry_msgs.msg.Point msg)
    {
        // Convertit le message ROS2 en Vector3 Unity
        Vector3 position = new Vector3(
            (float)msg.X,
            (float)msg.Y,
            (float)msg.Z
        );
        // Transformation repère ROS -> Unity
        Vector3 transform_position = new Vector3(position.y,position.z,-position.x);
        // stock les positions dans la variable globale
        global_pos = transform_position;
        // Met à jour la position thread-safe
        lock (dataLock)
        {
            threadSafePosition = position;
        }

        // Affiche la position reçue (optionnel)
        Debug.Log($"Position reçue: X={position.x}, Y={position.y}, Z={position.z}");
    }

    // --- Boucle de traitement dans un autre thread ---
    private void ProcessingLoop()
    {
        while (isRunning)
        {
            // Récupère la position thread-safe
            Vector3 currentPosition;
            lock (dataLock)
            {
                currentPosition = threadSafePosition;
            }

            // --- Traite les données ici (exemple : affichage dans la console) ---
            Debug.Log($"[Thread] Position actuelle: X={currentPosition.x}, Y={currentPosition.y}, Z={currentPosition.z}");
                  // Met à jour la position du GameObject

            // --- Simule un traitement personnalisé (exemple : calculs) ---
            // Exemple : Calculer la distance par rapport à l'origine
            float distanceFromOrigin = currentPosition.magnitude;
            Debug.Log($"[Thread] Distance depuis l'origine: {distanceFromOrigin}");
        }
    }
    void FixedUpdate()
    {
        transform.position = global_pos;
    }

    // --- Nettoyage ---
    void OnDestroy()
    {
        // Arrête le thread de traitement
        isRunning = false;
        if (processingThread != null && processingThread.IsAlive)
        {
            processingThread.Join(1000);  // Attend max 1s pour que le thread s'arrête
        }

        // Nettoie les ressources ROS2
        if (ros2Node != null)
        {
            ros2Unity.RemoveNode(ros2Node);
            Debug.Log("Nœud ROS2 supprimé.");
        }
    }
}
