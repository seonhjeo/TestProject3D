using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class AddressableMananger : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject cubeObj;
    [SerializeField] private AssetReferenceGameObject[] buildingObjs;

    [SerializeField] private AssetReferenceT<AudioClip> soundBGM;
    [SerializeField] private AssetReferenceSprite flagSprite;

    private List<GameObject> _objects = new List<GameObject>();

    [SerializeField] private AudioSource bgmObj;
    [SerializeField] private Image flagImage;

    // Start is called before the first frame update
    void Start()
    {
        Button_SpawnObject();
    }


    public void Button_SpawnObject()
    {
        cubeObj.InstantiateAsync().Completed += (obj) =>
        {
            _objects.Add(obj.Result);
        };

        for (int i = 0; i < buildingObjs.Length; i++)
        {
            buildingObjs[i].InstantiateAsync().Completed += (obj) =>
            {
                _objects.Add(obj.Result);
            };
        }

        soundBGM.LoadAssetAsync().Completed += (clip) =>
        {
            bgmObj.clip = clip.Result;
            bgmObj.loop = true;
            bgmObj.Play();
        };

        flagSprite.LoadAssetAsync().Completed += (img) =>
        {
            flagImage.sprite = img.Result;
        };
    }

    public void Button_Release()
    {
        // LoadAssetAsync로 로드한 애셋을 Release해준다
        soundBGM.ReleaseAsset();
        flagSprite.ReleaseAsset();
        
        if (_objects.Count == 0)
            return;

        var index = _objects.Count - 1;

        for (int i = _objects.Count - 1; i > -1; i--)
        {
            // InstantiateAsync로 생성한 오브젝트들을 Release해준다
            Addressables.ReleaseInstance(_objects[i]);
            _objects.RemoveAt(i);
        }
    }
}
