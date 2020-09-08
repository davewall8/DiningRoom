# DiningRoom
Train your image processing AI without having to actually build and maintain a robot.
This Unity 3d program simulates a simple robot that moves about in a dining room, responding to client commands. Its gaze can be raised or lowered. It can turn left or right, and move forward or backward. It returns what it sees, and any collision information, whenever requested. 

The keyboard commands are single-keys: f(orward), b(ackward), l(eft), r(ight), u(p), d(own), s(tart), which resets the camera back to its starting place.
There are similar network-based commands, using a TCP/IP client socket to port 13000. The sole socket client has the same control, but can also request the current image and the current contact points. Collision information received by the client should be used back up, and prevent the robot from passing through walls and furniture.

# Client Requests
In addition to the full names above (for keyboard requests), there are also: "image", "collisions", and "quit", which disconnects the client, allowing the server to wait for a new client.

# Client Request Format
About the client protocol, each client command is the full english word. Eg., "forward". Messages from the client are always text. They consist of the character T, followed by a 32-bit integer value representing the length of the ensuing string, and finally the string itself, without quotes or nulls or crs or lfs. 
For example, to move forward on step, the word 'forward' has length 7 (seven characters):
      T<7>forward. 
In bytes, where T is an ascii 84, and length 7 is 0,0,0,7 in big endian 32-bit format:
      84, 0, 0, 0, 7, 102, 111, 114, 119, 97, 114, 100
and this is the way all other client messages look.

# Server Response Format
For all the movement commands and "quit", the servers response is to echo that command, but in uppercase. Eg., 
In respons to a "collisions" request, the server's response is the usual T<length><response string>. The response string is either
  "COLLISIONS: 0",
or if there is a collision, (I've removed the extra linefeeds):
"CONTACTS: , tableClone (UnityEngine.GameObject), 4
(0.2, 0.4, 0.4)
(-0.2, 0.4, 0.4)
(0.2, 0.4, 0.1)
(-0.2, 0.4, 0.1)"
The contact information indicates where on the robot's "mask" there is a contact. In this case, there are 4 contacts.
      
For an "image" request, the robot returns a JPEG image as a series of bytes. The message format is an "I" (for Image) as I, num image bytes, jpg image bytes.

