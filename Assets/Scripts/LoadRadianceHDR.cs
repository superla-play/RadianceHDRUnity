using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using Superla.RadianceHDR;

public class LoadRadianceHDR : MonoBehaviour
{
    [SerializeField] string path;
    [SerializeField] MeshRenderer targetMesh;

    void Start()
    {
        Load();
    }

    async void Load()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, path);

        if(!File.Exists(streamingPath))
        {
            return;
        }

        var webRequest = UnityWebRequest.Get(streamingPath);
        var asyncOp = webRequest.SendWebRequest();
        while (!asyncOp.isDone) 
        {
            await Task.Yield();
        }

        if (!string.IsNullOrEmpty(webRequest.error)) 
        {
            Debug.LogErrorFormat("Error loading {0}: {1}", streamingPath, webRequest.error);
            return;
        }

        var buffer = webRequest.downloadHandler.data;

        RadianceHDRTexture texture = new RadianceHDRTexture(buffer);

        if(targetMesh)
        {
            targetMesh.material.SetTexture("_MainTex", texture.texture);
        }
    }
}
