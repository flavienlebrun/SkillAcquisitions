30/10: ROS2 for unity avec la solution de robotecAI:

J'ai téléchargé la 1.3.0 release https://github.com/RobotecAI/ros2-for-unity/releases

Ros2ForUnity\_humble\_standalone\_windows11.zip



Ensuite j'ai deziper puis copier le dosser dans les assets de mon projet

Il faut modifier le fichier FastRTPS pour inclure l'adresse IP de l'ordi Windows

Il suffit ensuite d'ajouter un script à un game object pour lequel on souhaite une communication ROS2<->Unity ROS2UnityComponent



J'ai relié les deux ordis (Windows linux) en créant des variables d'environnements ROS\_DOMAIN\_ID que j'ai mis à 5 (il faut que ce soit la même pour les deux ordis) et ROS\_LOCALHOST\_ONLY qu'il faut mettre à 0.



il faut ensuite sur le PC linux avec l'installation ROS2 modifier le fichier .bashrc en ajoutant à la fin

export ROS\_DOMAIN\_ID=5

export ROS\_LOCALHOST\_ONLY=0

\#######################################################################################################################

31/10 et 01/11

j'ai pu enfin faire bouger ma sphère en fonction des mouvements du robot. Pour ça il est nécessaire d'avoir un thread qui traite la recup du msg ros (c'est un abonnement au topic robot\_position). et un autre thread qui va gérer la publication de cette position dans le main de unity. C'est fait avec deux fonction  Poincallback et processingloop. Ensuite on peut utiliser la position dans fixed update pour gérer la position. Je me suis inspiré du script d'Ali Admittance\_link. 
###################

02/11
Ajout d'un deuxième Omega. Un ID est attribué à chaque Omega en fonction du port USB à priori. Cette ID peut être utilisé comme un int ensuite pour effectuer chaque fonction sur l'appareil souhaité. L'omega7 s'initialise pareil que le 3 mais il faut ensuite actionner les 3 éléments de l'organe terminal jusqu'en butté des deux côtés pour finir et arrêter le clignotement. Il faut vérifier aussi dans unity qu'on donne des noms de nœuds différents pour chaque récupération de position de robot. 

