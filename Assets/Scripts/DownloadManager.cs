using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;

public class DownloadManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject waitMessage;
    public GameObject downMessage;
    public Slider downSlider;
    public TMP_Text sizeText;
    public TMP_Text downValText;

    [Header("Label")] // 다운로드받을 애셋들의 라벨
    public AssetLabelReference defaultLabel;
    public AssetLabelReference matLabel;

    // 다운받을 애셋들의 파일 크기
    private long _patchSize;
    // 각각의 애셋들의 파일 크기
    private Dictionary<string, long> _patchMap = new Dictionary<string, long>();


    // Start is called before the first frame update
    void Start()
    {
        downMessage.SetActive(false);
        waitMessage.SetActive(true);
        
        StartCoroutine(InitAddressable());
        StartCoroutine(CheckUpdateFiles());
    }

    // 어드레서블을 초기화해주는 코루틴
    private IEnumerator InitAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }

    // 업데이트할 파일을 체크하는 코루틴
    // 각 라벨별로 다운로드할 데이터를 파악한 후 추가 파일이 있으면 다운로드 버튼 활성화
    // 아니면 로딩 씬을 거쳐 샘플신을 로드하게끔 실행
    private IEnumerator CheckUpdateFiles()
    {
        var labels = new List<string>() { defaultLabel.labelString, matLabel.labelString };

        _patchSize = default;

        foreach (var label in labels)
        {
            var handle = Addressables.GetDownloadSizeAsync(label);
            yield return handle;
            _patchSize += handle.Result;

            if (_patchSize > Decimal.Zero)
            {
                waitMessage.SetActive(false);
                downMessage.SetActive(true);

                sizeText.text = "" + GetFileSize(_patchSize);
            }
            else
            {
                downValText.text = "100%";
                downSlider.value = 1f;
                yield return new WaitForSeconds(2f);
                LoadingManager.LoadScene("SampleScene");
            }
        }
    }

    public void Button_Download()
    {
        StartCoroutine(PatchFiles());
    }

    // 새 파일에 대한 패치를 실시하는 코루틴
    // 각 라벨에 대해 다운로드 여부를 파악한 후 다운로드할 파일이 있으면 다운로드 코루틴 시작
    // 직후 다운로드 상태 코루틴 시작
    private IEnumerator PatchFiles()
    {
        var labels = new List<string>() { defaultLabel.labelString, matLabel.labelString };

        _patchSize = default;

        foreach (var label in labels)
        {
            var handle = Addressables.GetDownloadSizeAsync(label);
            yield return handle;

            if (handle.Result != Decimal.Zero)
            {
                StartCoroutine(DownLoadFromLabel(label));
            }
        }

        yield return CheckDownloadStatus();
    }

    // 각각의 라벨에 대해 다운로드하게끔 하는 코루틴
    // 해당 라벨에 대해 다운로드 데이터를 받아온 후 핸들을 Release
    private IEnumerator DownLoadFromLabel(string label)
    {
        _patchMap.Add(label, 0);

        var handle = Addressables.DownloadDependenciesAsync(label);

        while (!handle.IsDone)
        {
            _patchMap[label] = handle.GetDownloadStatus().DownloadedBytes;
            yield return new WaitForEndOfFrame();
        }

        _patchMap[label] = handle.GetDownloadStatus().TotalBytes;
        Addressables.Release(handle);
    }

    // 다운로드 현황을 체크하는 코루틴
    // 로딩바와 진행도를 갱신해주고 완료시 로딩신 후 샘플신으로 넘겨주기
    private IEnumerator CheckDownloadStatus()
    {
        var total = 0f;
        downValText.text = "0 %";

        while (true)
        {
            total += _patchMap.Sum(tmp => tmp.Value);

            var perValue = total / _patchSize;
            downSlider.value = perValue;
            downValText.text = string.Format("{0:##.##}", perValue * 100) + " %";

            if (total.Equals(_patchSize))
            {
                LoadingManager.LoadScene("SampleScene");
                break;
            }

            total = 0f;
            yield return new WaitForEndOfFrame();
        }
    }

    // 파일 사이즈에 따라 용량에 대한 
    public string GetFileSize(long byteCnt)
    {
        string size = "0 Bytes";

        if (byteCnt >= 1073741824.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1073741824.0) + " GB";
        }
        else if (byteCnt >= 1048576.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1048576.0) + " MB";
        }
        else if (byteCnt >= 1024.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1024.0) + " KB";
        }
        else if (byteCnt > 0 && byteCnt < 1024.0)
        {
            size = string.Format("{0:##.##}", byteCnt) + " KB";
        }
        
        return size;
    }
}
