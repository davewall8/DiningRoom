

using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 

// This implements a TCP listener based on information from a variety of sources.
// It serves only a single client at a time for the duration of that client's connection,
// and then waits for another client. A better solution would presumably be to use
// the Unity communication classes, but the needs here are simple.
public class MyTcpListener
{
    MainCamera avitar=null;     // The thing we convey messages to and from, for the client.
    CancellationTokenSource tokenSource=null;
    //CancellationToken cancellationToken;
    Task task;
    static readonly object lockObject1 = new object();
    static readonly object lockObject2 = new object();
    static readonly object lockObject3 = new object();
    String receivedMessage=null;
    String messageReply=null;
    byte[] imageReply= null;
    
    //======================================================
    // Note: constructors don't return a type (and void is a type).
    //======================================================
    public MyTcpListener()
    {
    }
    public MyTcpListener(MainCamera mainCameraObject)
    {
        avitar = mainCameraObject;
    }
    //======================================================
    // Mutex access for send and recieve messages.
    // We lock on the readonly object, lockObject.
    // The tcp listener runs in its own thread, and therefore could
    // clash with the main thread's (unity) reading and writing
    // of these variables: receivedMessage, messageReply and imageReply.
    // The locks control access, preventing simultaneous access.
    //======================================================
    public String getReceivedMessage()
    {
        String message=null;
        lock (lockObject1)
        {
            if (receivedMessage != null)
                message = (string) receivedMessage.Clone();
            receivedMessage = null;               // Clear message after reading.
        }
        return message;
    }
    public void setReceivedMessage(String message)
    {
        lock (lockObject1)
        {
            receivedMessage = (string) message.Clone();
        }
    }
    public String getMessageToSend()
    {
        String message=null;
        lock (lockObject2)
        {
            if (messageReply != null)
                message = (string) messageReply.Clone();
            messageReply = null;                  // Clear message after reading.
        }
        return message;
    }
    public void setMessageToSend(String message)
    {
        //Debug.Log("setMessageToSend(), message is " + message);
        lock (lockObject2)
        {
            messageReply = (string) message.Clone();
        }
    }
    public byte[] getImageToSend()
    {
        byte[] message=null;
        lock (lockObject3)
        {
            if (imageReply != null)
                message = (byte[]) imageReply.Clone();
            imageReply = null;                  // Clear message after reading.
        }
        return message;
    }
    public void setImageToSend(byte[] message)
    {
        lock (lockObject3)
        {
            imageReply = (byte[]) message.Clone();
        }
    }
    //======================================================
    // End of message locking.
    //======================================================
    // Helper method to concatenate 2 byte arrays.
    public static byte[] concatenate(byte[] first, byte[] second)
    {
        byte[] bytes = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
        return bytes;
    }
    // Compose the server's reply to the client.
    // The client expects either a 'T' or an 'I' (text or image),
    // followed by the 4-byte integer informing of the message bytes
    // held in the actual data, which begins after the 4-byte length.
    byte [] composeReply( char code, byte[] dataBytes)
    {
        byte[] codeBytes =  new byte[1];
        codeBytes[0] = (byte) code;
        byte[] sizeBytes = BitConverter.GetBytes(dataBytes.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sizeBytes);
        // Debug.Log("Codebytes size="+ codeBytes.Length+ "sizeBytes size="+sizeBytes.Length);
        byte[] header = concatenate(codeBytes, sizeBytes);
        return concatenate(header, dataBytes);
    }
    byte[] composeTextResponse()
    {
        byte[] data = null;
        String messageToClient = getMessageToSend();
        if (messageToClient != null)
        {
            data = System.Text.Encoding.ASCII.GetBytes(messageToClient);
            return composeReply('T', data);
        }
        else
            return null;
    }
    byte[] composeImageResponse()
    {
        byte[] data = null;
        data = getImageToSend();
        if (data != null)
            return composeReply('I', data);
        else
            return null;
    }
    byte[] getResponseToSend()
    {
        byte[] response = null;
        // wait for a response to be prepared.
        while (response == null)
        {
            response = composeTextResponse();
            if (response == null)
                response = composeImageResponse();
            if (response == null)
                Thread.Sleep(50);
        }
        return response;
    }
    // Wait for the client's next command, which is a string wrapped in a byte array.
    // The byte array begins with a 'T' (text), followed by 4-byte integer,
    // which is followed by that number of bytes, holding the message string.
    bool receiveMessage(NetworkStream stream)
    {
        byte [] bytes = new byte[256];
        // Wait for a message from the client.
        int num = stream.Read(bytes, 0, 1);
        // Debug.Log("Read() num bytes: " + num.ToString());
        string code = System.Text.Encoding.ASCII.GetString(bytes, 0, 1);
        // Debug.Log("code is " + code);

        byte[] sizeBytes = new byte[4];
        num = stream.Read(sizeBytes, 0, 4);
        // Debug.Log("Read() num bytes: " + num.ToString());
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sizeBytes);
        int  size = BitConverter.ToInt32(sizeBytes, 0);
        // Debug.Log("size is " + size.ToString());

        num = stream.Read(bytes, 0, size);
        // Debug.Log("Read() num bytes: " + num.ToString());
        // Translate data bytes to a ASCII string.
        if (size > bytes.Length)
            bytes = new byte[size];
        String data = System.Text.Encoding.ASCII.GetString(bytes, 0, size);
        Debug.Log("Received: " + data);
        bool isWorking = (data != "quit");
        if ( isWorking )
            setReceivedMessage( data );
        return isWorking;
    }
    void sendResponse(NetworkStream stream, byte[] reply )
    {
        // Send back a response.
        // Debug.Log("Sending: " + reply.Length.ToString());
        stream.Write(reply, 0, reply.Length);
    }
    void workWithClient(NetworkStream stream )
    {
        // Loop with this client socket until they send "quit".
        bool isWorking = true;
        bool isReceiving = true;
        while( isWorking )
        {
            if ( isReceiving )
            {
                isWorking = receiveMessage(stream);
                isReceiving = false;
            }
            else
            {
                byte[] msg = getResponseToSend();
                sendResponse(stream, msg);
                isReceiving = true;
            }
        }
    }
    public static String determineLocalIP()
    {
        // See https://www.csharp-examples.net/local-ip/
        string localComputerName = Dns.GetHostName();
        Debug.Log("Computer name =" + localComputerName);
        Regex regex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");                
        IPAddress[] localIPs = Dns.GetHostAddresses(localComputerName);
        foreach (IPAddress localIP in localIPs)
        {
            String ipString = localIP.ToString();
            Debug.Log("local IP = " + ipString);
            if (ipString != "127.0.0.1")
                if (regex.IsMatch(ipString))
                    return ipString;
        }
        Debug.Log("Not found.");
        // If we're here, we couldn't find a match, so try localhost.
        return "localhost";
    }
    // This server loop waits for a client to connect, ane then begins
    // a series of client-request->server response messages. The server
    // is passive; it waits for a client request, and returns a response.
    // Each request must be one of the following:
    //      1. command to move the camera (up, down, left, right, backward, forward).
    //      2. command to return current collision data, if any (collisions).
    //      3. command to return the current camera image (image).
    //      4. command to reset the camera to the starting position (start).
    //      5. command to quit (quit).
    // Since there is only one camera, only a single client is served at any
    // time, and this continues until that client quits. After that, the
    // server waits for a new client, and so on. There is a way to stop the
    // server (although it may be untested), see cancelServer().
    public void Main()
    {
        Debug.Log("Main");
        TcpListener tServer=null;
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 13000;
            string localIPString = determineLocalIP();
            IPAddress localAddr = IPAddress.Parse(localIPString);
            Debug.Log( "localAddr = "+localAddr.ToString());

            tServer = new TcpListener(localAddr, port);

            // Start listening for client requests.
            tServer.Start();
            Debug.Log("Started Server");

            // Buffer for reading data
            Byte[] bytes = new Byte[256];

            // Enter the listening loop.
            while(true)
            {
                if (tokenSource.Token.IsCancellationRequested)
                {
                    Debug.Log("Operation is going to be cancelled.");
                    // do some clean up
                    throw new OperationCanceledException();
                }
                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                TcpClient client = tServer.AcceptTcpClient();
                Debug.Log("Connected!");

                // data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                Debug.Log("Got Stream.");
                // Block here, (on this thread) working with single client.
                // Eventually, the client will quit, and then we'll loop,
                // waiting for the next client.
                workWithClient( stream );

                // Shutdown and end the client's connection.
                client.Close();
            }
        }
        catch(SocketException e)
        {
            Debug.Log("SocketException: " + e.ToString());
        }
        catch(OperationCanceledException e)
        {
            Debug.Log("OperationCanceledException: " + e.ToString());
        }
        finally
        {
            // Stop listening for new clients.
            tServer.Stop();
        }

        Debug.Log("\nHit enter to continue...");
        // Console.Read();
    }

    public void startServer()
    {
        tokenSource = new CancellationTokenSource();
        //CancellationToken wtoken = tokenSource.Token;
        Action main=this.Main;
        task = Task.Factory.StartNew( main, TaskCreationOptions.LongRunning);
    }

    public void cancelServer()
    {
        tokenSource.Cancel(true);
    }
}

