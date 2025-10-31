// Copyright 2019-2021 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEngine;

namespace ROS2
{

/// <summary>
/// A class for listening omega position
/// </summary>
public class ListenerPosition : MonoBehaviour
{
    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private ISubscription<geometry_msgs.msg.Point> point_sub;
    // Facteur d'échelle pour adapter les coordonnées ROS2 à l'échelle de Unity
    public float scaleFactor = 1.0f;
    void Start()
    {
        ros2Unity = GetComponent<ROS2UnityComponent>();
    }

    void Update()
    {    
        // Récupère la position (x, y, z) du GameObject
        Vector3 position = transform.position;
        // Affiche la position dans la console

        Debug.Log($"Position : X={position.x}, Y={position.y}, Z={position.z}");
        if (ros2Node == null && ros2Unity.Ok())
        {
            ros2Node = ros2Unity.CreateNode("ROS2UnityListenerNode");
            point_sub = ros2Node.CreateSubscription<geometry_msgs.msg.Point>(
                    "robot_position",
                    msg => UpdatePosition(msg));
        }
    }
    // Méthode pour mettre à jour la position du GameObject
    private void UpdatePosition(geometry_msgs.msg.Point msg)
    {
        // Applique le facteur d'échelle si nécessaire
        float x = (float)msg.X * scaleFactor;
        float y = (float)msg.Y * scaleFactor;
        float z = (float)msg.Z * scaleFactor;

        // Met à jour la position du GameObject
        transform.position = new Vector3(x, y, z);

        // Log pour vérifier les valeurs
        Debug.Log($"Position mise à jour : X={x}, Y={y}, Z={z}");
    }
}

}  // namespace ROS2
