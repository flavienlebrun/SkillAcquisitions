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

