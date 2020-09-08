using UnityEngine;
// This class provided by a post:
// https://answers.unity.com/questions/156698/copy-a-transform.html
 
 public class Transformer
 {
 
     public Vector3 position;
     public Quaternion rotation;
     public Vector3 localScale;
 
     public Transformer (Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale)
     {
         position = newPosition;
         rotation = newRotation;
         localScale = newLocalScale;
     }
 
     public Transformer ()
     {
         position = Vector3.zero;
         rotation = Quaternion.identity;
         localScale = Vector3.one;
     }
 
     public Transformer (Transform transform)
     {
         copyFrom (transform);
     }
 
     public void copyFrom (Transform transform)
     {
         position = transform.position;
         rotation = transform.rotation;
         localScale = transform.localScale;
     }
 
     public void copyTo (Transform transform)
     {
         transform.position = position;
         transform.rotation = rotation;
         transform.localScale = localScale;
     }
 
 }
