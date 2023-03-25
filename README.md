# ConsoleApp17

tiny 2d game engine prototype + some games with it

Cool features:
  - a scene definition language which supports declaring entity/component hierarchies, setting properties, and archetypes (including other scene files as an entity in the current scene)
  - velcrophysics integration
  - rather extensive & useful debug gui windows with an exposed api for making more
  - Pong demo
  - SpaceGame demo
  - Space invaders
    
# Projects:
## ConsoleApp17
  - don't ask why its called this
  - main engine class lib
  - SceneLoader is the scene definition language parser. uses sprache.
  - unity style gameobject (i call it entity) system (not an ecs)
    - everything is a component (even entities), so adding a child entity in the scene heirarchy is the same operation as adding a component to an entity
# Pong
  - basic pong game
  - proves this project actually functions
  - what more can i say
  
![image](https://user-images.githubusercontent.com/45476006/227735612-a35c44cd-1fac-49d9-9dba-780213eb4a0c.png)

# SpaceGame
  - editable marching-squares-based terrain/asteroids 
  - terrain has full physics and is chucked
    - when terrain creates multiples islands the chunk is separated into multiple objects based on those islands
    - island detection done via generated collider polygons (ie multiple polygons means multiple islands)
  - very buggy
  
![image](https://user-images.githubusercontent.com/45476006/227735580-ad38e408-24a1-448c-99e6-1affc12c6f06.png)

![image](https://user-images.githubusercontent.com/45476006/227735551-cc190ad5-af02-4850-91a4-91482e6682c7.png)

has some bugs:

![image](https://user-images.githubusercontent.com/45476006/227735513-949aaaa7-5ae9-4eaf-861b-b545af1b2909.png)

# SpaceInvaders
  - not at all finished space invaders recreation
  - actually, its barely even started
  
