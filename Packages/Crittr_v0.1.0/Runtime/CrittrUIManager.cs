using System.Collections;
using UnityEngine;
using Crittr;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

[Serializable]
public class ClearScreensEvent : UnityEvent { };

public class CrittrUIManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject formScreen;
    public GameObject successScreen;
    public GameObject failureScreen;
    public GameObject crittrReporter;
    public GameObject successQRCodeGO;

    [SerializeField]
    public ClearScreensEvent OnClearedScreens;


    [NonSerialized]
    private Report _currentReport = null;
    private CrittrSDK _crittrSDK;
    private string _reportLocation;
    private RawImage _qrCodeRawImage;
    private Dropdown _categoryDropdown;
    private float timeAtPause;

    void Awake()
    {
        _currentReport = null;
        _crittrSDK = crittrReporter.GetComponent<CrittrSDK>();
        _qrCodeRawImage = successQRCodeGO.GetComponent<RawImage>();

        _categoryDropdown = formScreen.GetComponentInChildren<Dropdown>();
        _categoryDropdown.onValueChanged.AddListener(delegate
        {
            _handleDropdownChange(_categoryDropdown);
        });
    }

    // Takes note of the current simulation speed before pausing the physics simulation
    public void pauseSimulation()
    {
        timeAtPause = Time.timeScale;
        Time.timeScale = 0;
    }

    // Resumes simulating physics at the previous speed
    public void resumeSimulation()
    {
        Time.timeScale = timeAtPause;
    }

    public void HandleShowForm(Report report)
    {
        _currentReport = report;
        _currentReport.category = _categoryDropdown.options[_categoryDropdown.value].text;
        StartCoroutine(_screenShotAndDisplayScreen(report));
        pauseSimulation();
    }

    public void HandleTitleChange(string value)
    {
        if (_currentReport == null) return;
        _currentReport.title = value;
    }

    public void HandleDescriptionChange(string value)
    {
        if (_currentReport == null) return;
        _currentReport.description = value;
    }

    private void _handleDropdownChange(Dropdown change)
    {
        if (_currentReport == null) return;
        _currentReport.category = change.options[change.value].text;
    }

    private IEnumerator _screenShotAndDisplayScreen(Report report)
    {
        // We want to capture the screen before we display the report screen.
        yield return CrittrRuntime.Instance.CaptureScreenshot(report);
        ShowScreen(formScreen);
    }

    public void HandleSendReport()
    {
        if (_currentReport == null) return;
        _crittrSDK.SendReport(_currentReport, false);
        ClearScreens();
    }

    public void HandleShowSuccess(Report report, SuccessResponse successResponse)
    {
        if (report.category != "error")
        {
            StartCoroutine(_setQRCodeImage(successResponse.qr_code_location));
            ShowScreen(successScreen);
            _reportLocation = successResponse.location;
        }
    }

    private IEnumerator _setQRCodeImage(string qrCodeLocation)
    {
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(qrCodeLocation);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
            yield return null;
        }

        _qrCodeRawImage.texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
    }

    public void HandleReportLinkClick()
    {
        Application.OpenURL(_reportLocation);
    }

    public void HandleShowFailure(Report _, ErrorResponse errorResponse)
    {
        var text = failureScreen.GetComponent<Text>();
        if (errorResponse.errors.Length > 0)
        {
            text.text = errorResponse.errors[0].message;
        }
        ShowScreen(failureScreen);
    }

    private void ShowScreen(GameObject screen)
    {
        panel.SetActive(true);
        screen.SetActive(true);
    }

    public void ClearScreens()
    {
        resumeSimulation();

        panel.SetActive(false);
        formScreen.SetActive(false);
        successScreen.SetActive(false);
        failureScreen.SetActive(false);
        _currentReport = null;
        _qrCodeRawImage.texture = null;
        // Trigger event.
        OnClearedScreens?.Invoke();
    }
}
