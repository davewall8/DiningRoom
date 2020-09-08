
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

// Inspired by a listing under:
// https://forum.unity.com/threads/how-to-wait-for-capturescreen-to-complete.172194/
//
public class ImageCapture : MonoBehaviour {
 
    Texture2D texture;
    bool grab = false;
    MyTcpListener server=null;
    WaitForSeconds waitTime = new WaitForSeconds(0.1F);
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

 
    // Application.persistentDataPath + "/imagecapture.png";
    void Start()
    {
        Debug.Log("ImageCapture.Start()");
    }
    public void setServer(MyTcpListener srvr)
    {
        server = srvr;
    }
    public void startCapture()
    {
        texture = null;
        grab = true;
    } 
    // Once the grab flag is set, we capture the image bytes here, and assign them to the
    // server's image variable. The server will then send the image to its client.
    // (The Screen size has been set in the Build settings, eg., as 800x600.) The encoding
    // to JPG could be performed elsewhere if it slows down this main thread too much,
    // but this way seems okay.
    private void OnPostRender()
    {
        if (grab && (texture==null))
        {
            //Create a new texture with the width and height of the screen
            texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            // Debug.Log("OnPostRender() grab=true.");
            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
            texture.Apply();
            //Check that the display field has been assigned in the Inspector
            // Debug.Log("texture.EncodeToPNG()");
            byte[] imageBytes = texture.EncodeToJPG();
            server.setImageToSend(imageBytes);
            Debug.Log("Captured image bytes size= " + imageBytes.Length);
            //Reset the grab state
            grab = false;
        }
    }
    // The following methods are unused at the moment.
    public void saveAsPNG(string path, string filename)
    {
        //saves a PNG file to the path specified above
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path + "/" + filename, bytes);
    }
    public void saveAsPNG(string filename)
    {
        saveAsPNG(Application.persistentDataPath, filename);
    }
    public Texture2D loadImage(string path, string filename) 
    {
        Texture2D image;
        image = new Texture2D(Screen.width, Screen.height);
        byte[] bytes = System.IO.File.ReadAllBytes(path + "/" + filename);
        bool imageLoadSuccess = image.LoadImage(bytes);
        return image;
    }
    public Texture2D loadImage(string filename)
    {
        return loadImage(Application.persistentDataPath, filename);
    }
}