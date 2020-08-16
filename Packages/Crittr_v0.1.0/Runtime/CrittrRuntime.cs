using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Crittr {
    [Serializable]
    public struct RefAndMethod {
        public string href;
        public string method;
    }
    [Serializable]
    public struct SuccessLinks {
        public RefAndMethod get;
        public RefAndMethod update;
        public RefAndMethod upload_attachment;
    }
    [Serializable]
    public struct SuccessResponse {
        public string location;
        public string qr_code_location;
        public SuccessLinks links;
    }
    
    [Serializable]
    public struct ErrorMessage {
        public string message;
        public int status;
        public string type;
        public string field;

    }
    [Serializable]
    public struct ErrorResponse {
        public int status;
        public ErrorMessage[] errors;
    }
    public class APIProperties {
        public string scheme;
        public string host;
        public string path;
        public int port;
        public string apiKey;

        public UriBuilder BaseURI { get { return new UriBuilder(scheme, host, port); } }

        public APIProperties(string connectionURI) {
            Uri uri = new Uri(connectionURI);

            scheme = uri.Scheme;
            host = uri.Host;
            port = uri.Port;
            path = uri.LocalPath;
            apiKey = uri.UserInfo;
        }
    }


    public class CrittrRuntime {
        private static readonly Lazy<CrittrRuntime> _instance = new Lazy<CrittrRuntime>(() => new CrittrRuntime());
        public static CrittrRuntime Instance { get { return _instance.Value; } }
        private bool _isInitialized;
        private List<string> _logs = new List<string>();
        private const float timeBetween = 0.6f;
        private float _lastErrorSent = 0;

        private string _connectionURI = "";
        private int _maxLogs = 100;
        private bool _isVerbose = false;

        public event Action<Report> OnReportSend;
        public event Action<Report, SuccessResponse> OnSendReportSuccess;
        public event Action<Report, ErrorResponse> OnSendReportFailure;
        public event Action<string, string> OnExceptionError;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadMethod()
        {
            Instance.Init();
            Application.quitting += () => Instance.Destroy();
        }

        public void Init() {
            if (_isInitialized) {
                return;
            }

            _isInitialized = true;
            _logs = new List<string>();
            Application.logMessageReceived += _handleLog;
        }

        public void Destroy() {
            if (!_isInitialized) {
                return;
            }

            _isInitialized = false;
            Application.logMessageReceived -= _handleLog;
            _logs = new List<string>();
        }

        public IEnumerator CaptureScreenshot(Report report) { 
            yield return new WaitForEndOfFrame();
            var timeNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string filename = $"Crittr_Screenshot_{timeNow}.jpg";
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            texture.name = filename;
            report.screenshots.Add(texture);
        }

        private void _handleLog(string message, string stackTrace, LogType logType) {
            if (!_isInitialized) {
                return;
            }
            if (logType == LogType.Exception && Time.time - _lastErrorSent > timeBetween)
            {
                OnExceptionError?.Invoke(message, stackTrace);
                _logs.Add(message);
                _logs.Add(stackTrace);
                _lastErrorSent = Time.time;
            }
            else
            {
                _logs.Add(message);
            }
            while (_logs.Count > _maxLogs) {
                _logs.RemoveAt(0);
            }
        }

        public void SetConnectionURI(string connectionURI) {
            if (!_isInitialized) {
                return;
            }

            _connectionURI = connectionURI;
        }

        public void SetMaxLogs(int maxLogs) {
            if (!_isInitialized) {
                return;
            }

            _maxLogs = 1;
            if (maxLogs > 1) {
                _maxLogs = maxLogs;
            }
        }

        public void SetIsVerboseMode(bool isVerbose) {
            if (!_isInitialized) {
                return;
            }

            _isVerbose = isVerbose;
        }


        public Report NewReport() {
            return new Report();
        }

        public IEnumerator SendReport(Report report, bool withScreenShot) {
            if (withScreenShot) {
                yield return CaptureScreenshot(report);
            }
            _sendReport(report);
        }

        public AsyncOperation _sendReport(Report report) {
            report.SetLogs(_logs);
            APIProperties apiProperties = new APIProperties(_connectionURI);
            var builder = apiProperties.BaseURI;
            builder.Path = apiProperties.path;

            var www = new UnityWebRequest(builder.ToString()) { method = "POST" };
            www.SetRequestHeader("X-Crittr-Client-Key", apiProperties.apiKey);

            byte[] rawReport = Encoding.UTF8.GetBytes(report.ToJson());
            var uploadHandler = new UploadHandlerRaw(rawReport);
            www.uploadHandler = uploadHandler;
            www.SetRequestHeader("Content-Type", "application/json");
            www.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation wwwOp = www.SendWebRequest();
            wwwOp.completed += bindSendReportCompleted(report);

            OnReportSend?.Invoke(report);
            return wwwOp;
        }


        private Action<AsyncOperation> bindSendReportCompleted(Report report) {
            return (AsyncOperation op) =>
            {
                UnityWebRequestAsyncOperation wwwOp = (UnityWebRequestAsyncOperation)op;
                var www = wwwOp.webRequest;
                if (www.isNetworkError || www.isHttpError || www.responseCode != 200)
                {
                    if (_isVerbose)
                    {
                        Debug.LogWarning("Error sending report: " + www.downloadHandler.text);
                    }
                    ErrorResponse errorResponse = new ErrorResponse {errors = new ErrorMessage[]{}, status = 500};
                    try
                    {
                        errorResponse = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
                    }
                    catch (Exception e) {
                        if (_isVerbose)
                        {
                            Debug.LogWarning("Error generating error response" + e.ToString());
                        }
                    }
                    finally
                    {
                        OnSendReportFailure?.Invoke(report, errorResponse);
                    }
                    return;
                }

                var response = JsonUtility.FromJson<SuccessResponse>(www.downloadHandler.text);
                OnSendReportSuccess?.Invoke(report, response);
                _uploadAttachments(report, response.links.upload_attachment);
            };
        }

        private void _uploadAttachments(Report report, RefAndMethod uploadLink) {
            foreach (var texture in report.screenshots) {
                UnityWebRequestAsyncOperation wwwOp = _uploadScreenshot(texture, uploadLink);

                if (_isVerbose) {
                    wwwOp.completed += (AsyncOperation op) =>
                    {
                        var uwrOp = (UnityWebRequestAsyncOperation)op;
                        var www = uwrOp.webRequest;
                        Debug.Log($"Upload attachment response: {www.downloadHandler.text}");
                    };
                }
            }
            foreach (var filename in report.attachments) {
                UnityWebRequestAsyncOperation wwwOp = _uploadAttachment(filename, uploadLink);

                if (_isVerbose) {
                    wwwOp.completed += (AsyncOperation op) =>
                    {
                        var uwrOp = (UnityWebRequestAsyncOperation)op;
                        var www = uwrOp.webRequest;
                        Debug.Log($"Upload attachment response: {www.downloadHandler.text}");
                    };
                }
            }
        }

        private UnityWebRequestAsyncOperation _uploadAttachment(string path, RefAndMethod uploadLink) {
            var filename = Path.GetFileName(path);
            byte[] data = File.ReadAllBytes(path);
            return _uploadFile(filename, data, uploadLink);
        }
        private UnityWebRequestAsyncOperation _uploadScreenshot(Texture2D texture, RefAndMethod uploadLink) {
            byte[] data = texture.EncodeToJPG();
            return _uploadFile(texture.name, data, uploadLink);
        }

        private UnityWebRequestAsyncOperation _uploadFile(string filename, byte[] data, RefAndMethod uploadLink) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("attachment_file", data, filename, ""));

            APIProperties apiProperties = new APIProperties(_connectionURI);
            var url = apiProperties.BaseURI + uploadLink.href;
            var www = UnityWebRequest.Post(url, formData);

            // 60 Seconds timeout.
            www.timeout = 60;
            www.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation wwwOp = www.SendWebRequest();
            return wwwOp;
        }
    }
}
