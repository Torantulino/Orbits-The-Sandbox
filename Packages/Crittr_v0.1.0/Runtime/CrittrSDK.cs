using System.Collections.Generic;
using UnityEngine;
using Crittr;
using System;
using UnityEngine.Events;

// Bug in several Unity distributions. Extending and serializing the UnityEvent
// class allows it to show up in the editor GUI.
[Serializable]
public class CrittrEventReport : UnityEvent<Report> { }
[Serializable]
public class CrittrEventReportSuccess : UnityEvent<Report, SuccessResponse> { }
[Serializable]
public class CrittrEventReportFailure : UnityEvent<Report, ErrorResponse> { }


public class CrittrSDK : MonoBehaviour
{
    [SerializeField]
    [Header("Connection URI with API Key")]
    public string ConnectionURI;

    [Header("Options")]
    public bool sendAutomaticReports = false;
    [Range(1,100)]
    public int maxLogs = 100;
    public bool isVerbose = true;

    [Header("Inputs to trigger manual reports", order=1)]
    public KeyCode keyboardInputTrigger = KeyCode.F8;
    public List<KeyCode> controllerInputTriggers = new List<KeyCode> { KeyCode.JoystickButton0, KeyCode.JoystickButton4, KeyCode.JoystickButton5 };

    [Header("Events triggered during the report lifecycle")]
    [SerializeField]
    public CrittrEventReport OnShowForm;
    [SerializeField]
    public CrittrEventReport OnReportSend;
    [SerializeField]
    public CrittrEventReportSuccess OnReportSuccess;
    [SerializeField]
    public CrittrEventReportFailure OnReportFailure;

    private bool holdingTriggerKey = false;


    void Awake()
    {
        CrittrRuntime.Instance.SetConnectionURI(ConnectionURI);
        CrittrRuntime.Instance.SetMaxLogs(maxLogs);
        CrittrRuntime.Instance.SetIsVerboseMode(isVerbose);

        CrittrRuntime.Instance.OnReportSend += HandleReportSend;
        CrittrRuntime.Instance.OnSendReportSuccess += HandleReportSuccess;
        CrittrRuntime.Instance.OnSendReportFailure += HandleReportFailure;
        CrittrRuntime.Instance.OnExceptionError += HandleExceptionError;
    }

    public virtual void TriggeredManualReport()
    {
        var report = CrittrRuntime.Instance.NewReport();
        if (OnShowForm.GetPersistentEventCount() > 0)
        {
            OnShowForm?.Invoke(report);
            return;
        }

        _defaultReportTriggered(report);
    }

    private void _defaultReportTriggered(Report report) { 
        SendReport(report, true);
    }

    public virtual void PopulateReport(Report report)
    {
        // Expects to be overridden.
        // Add custom data to the report.
    }

    public void SendReport(Report report, bool withScreenshot) {
        PopulateReport(report);
        StartCoroutine(CrittrRuntime.Instance.SendReport(report, withScreenshot));
    }

    public virtual void HandleReportSend(Report report)
    {
        if (isVerbose)
        {
            Debug.Log("After constructing the report request");
        }
        // A good place to show a loading screen...
        OnReportSend?.Invoke(report);
    }

    public virtual void HandleReportSuccess(Report report, SuccessResponse response)
    {
        if (isVerbose)
        {
            Debug.Log("The report was sent successfully: " + response.location);
        }

        OnReportSuccess?.Invoke(report, response);
    }

    public virtual void HandleReportFailure(Report report, ErrorResponse error)
    {
        if (isVerbose)
        {
            Debug.Log("The report was not sent: " + error.status);
        }
        OnReportFailure?.Invoke(report, error);
    }

    public virtual void HandleExceptionError(string message, string stackTrace)
    {
        if (!sendAutomaticReports) return;
        var report = CrittrRuntime.Instance.NewReport();
        report.category = "error";
        report.title = message;
        report.description = stackTrace;
        SendReport(report, true);

        if (isVerbose)
        {
            Debug.Log("Sending automatic exception");
        }
    }

    public virtual void Update()
    {

        if (!holdingTriggerKey && Input.GetKeyDown(keyboardInputTrigger))
        {
            TriggeredManualReport();
            holdingTriggerKey = true;
        }
        if (Input.GetKeyUp(keyboardInputTrigger))
        {
            holdingTriggerKey = false;
        }

        var hasPressedControllerKeys = true;
        foreach (KeyCode input in controllerInputTriggers)
        {
            var isKeyPressed = Input.GetKey(input);
            hasPressedControllerKeys = hasPressedControllerKeys && isKeyPressed;
        }
        if (!hasPressedControllerKeys) holdingTriggerKey = false;
        if (!holdingTriggerKey && hasPressedControllerKeys)
        {
            TriggeredManualReport();
            holdingTriggerKey = hasPressedControllerKeys;
        }

    }
}
