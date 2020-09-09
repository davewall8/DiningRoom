# DiningRoom
Train your image processing AI without having to actually build and maintain a robot.<br />

This Unity 3d program simulates a simple robot that moves about in a dining room, responding to client commands. Its gaze can be raised or lowered. It can turn left or right, and move forward or backward. It returns what it sees, and any collision information, whenever requested. Note that the TCP/IP protocol described below is in binary (not text).<br />

The keyboard commands are single-keys: f(orward), b(ackward), l(eft), r(ight), u(p), d(own), s(tart), which resets the camera back to its starting place.
There are similar network-based commands, using a TCP/IP client socket to port 13000. The sole socket client has the same control, but can also request the current image and the current contact points. Collision information received by the client should be used back up, and prevent the robot from passing through walls and furniture.<br />
# Client Requests
In addition to the full names above (for keyboard requests), there are also: "image", "collisions", and "quit", which disconnects the client, allowing the server to wait for a new client.<br />

# Client Request Format
About the client protocol, each client command is the full english word. Eg., "forward". Messages from the client are always text. They consist of the character T, followed by a 32-bit integer value representing the length of the ensuing string, and finally the string itself, without quotes or nulls or crs or lfs.<br>
For example, to move forward on step, the word 'forward' has length 7 (seven characters):<br />
      T<7>forward.<br />
In binary (a series of bytes), where T is an ascii 84, and length 7 is 0,0,0,7 in big endian 32-bit format:<br />
      84, 0, 0, 0, 7, 102, 111, 114, 119, 97, 114, 100 <br />
and this is the way all other client messages are encoded.<br />

# Server Response Format
Server responses are in a binary format, too.<br />
For all the movement commands and "quit", the server's response is to echo that command, but in uppercase.<br />

However, "collision" and "image" responses contain data.<br />
For a "collisions" request, the server's response is the usual T{length}{response string}.<br />
The response string is one of:
<ul>
<li>"COLLISIONS: 0" if there is no collision.</li><br />
<li>"COLLISIONS: , tableClone (UnityEngine.GameObject), 4
(0.2, 0.2, 0.4)
(0.2, 0.3, 0.4)
(-0.2, 0.3, 0.4)
(-0.2, 0.2, 0.4)" if a collision has just occured.</li> <br />
<li>"CONTACTS: , tableClone (UnityEngine.GameObject), 4
(0.2, 0.4, 0.4)
(-0.2, 0.4, 0.4)
(0.2, 0.4, 0.1)
(-0.2, 0.4, 0.1)" if still in contact.</li> <br />
<li>"EXITS: , FrontWall (UnityEngine.GameObject), 0" on "exiting" from contact.</li><br />
</ul>
The contact information indicates where on the robot's surface (a unity mask) there is a contact. In the situation above, there are 4 points of contact.<br />
      
For an "image" request, the robot returns a JPEG image as a series of bytes. The message format is an "I" (for Image) as <br />
{I}{num image bytes}{jpg image bytes}.<br />
The image presumably provides clues for your AI program to determine where the robot is, and so on. <br />

