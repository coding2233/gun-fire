using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WebTest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        string filePath = @"C:\Users\wanderer\Pictures\Uplay\xmas_FINAL COMP_v2-3 FINAL-for-Wallpaper.mp4";
        var stream =  File.OpenRead(filePath);
        int readSize = 1;
        int offset = 0;
        while (readSize>0)
        {
            byte[] buffer=new byte[4096];
            readSize = stream.Read(buffer, 0,4096);
            UnityWebRequest webRequest = UnityWebRequest.Post($"http://localhost:18000/upload?name=tttt.mp4&offset={offset}","");
            webRequest.downloadHandler =new  DownloadHandlerBuffer();
            webRequest.uploadHandler = new UploadHandlerRaw(buffer);
            yield return webRequest.SendWebRequest();
            offset += readSize;
            //if (webRequest.result)
            //{
                
            //}
            Debug.Log(webRequest.downloadHandler.text);
        }
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
