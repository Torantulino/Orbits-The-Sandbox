using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crittr
{
    [Serializable]
    public class Device
    {
        public string name;
        public string family;
        public string model;
        public string type;
        public string processor_type;
        public int battery_level;
        public string battery_status;
        public int memory_size;
        public string orientation;
    }

    [Serializable]
    public class GPU
    {
        public string name;
        public string family;
        public string model;
        public string type;
        public string version;
        public string vendor_id;
        public string vendor_name;
        public int memory_size;
        public bool is_multi_threaded;
    }

    [Serializable]
    public class OS
    {
        public string name;
        public string family;
    }

    [Serializable]
    public class System
    {
        public Device device;
        public GPU gpu;
        public OS os;

        public System()
        {
            device = new Device();
            device.name = SystemInfo.deviceName;
            device.model = SystemInfo.deviceModel;
            device.type = SystemInfo.deviceType.ToString();
            device.processor_type = SystemInfo.processorType;
            device.battery_level = (int)Math.Floor(SystemInfo.batteryLevel) * 100;
            device.battery_status = SystemInfo.batteryStatus.ToString().ToLower();
            device.memory_size = SystemInfo.systemMemorySize;
            device.orientation = Input.deviceOrientation.ToString().ToLower();

            gpu = new GPU();
            gpu.name = SystemInfo.graphicsDeviceName;
            gpu.type = SystemInfo.graphicsDeviceType.ToString();
            gpu.vendor_id = SystemInfo.graphicsDeviceVendorID.ToString();
            gpu.vendor_name = SystemInfo.graphicsDeviceVendor;
            gpu.memory_size = SystemInfo.graphicsMemorySize;
            gpu.is_multi_threaded = SystemInfo.graphicsMultiThreaded;
            gpu.version = SystemInfo.graphicsDeviceVersion;

            os = new OS();
            os.name = SystemInfo.operatingSystem;
            os.family = SystemInfo.operatingSystemFamily.ToString();
        }
    }

    [Serializable]
    public class App
    {
        public string version;
        public string env;
        public string platform;

        public App()
        {
            version = Application.version;
            env = "release";
            if (Debug.isDebugBuild)
            {
                env = "development";
            }
            platform = Application.platform.ToString().ToLower();
        }
    }

    [Serializable]
    public class User
    {
        public string email;
    }

    [Serializable]
    public class SDK
    {
        public string version = "0.1.0";
        public string platform = "unity";
    }

    [Serializable]
    public class Report
    {
        public System system;
        public App app;
        public List<string> logs;
        public User user;
        public SDK sdk;

        public string title = "";
        public string description = "";
        public string category = "bug";
        public string tags;
        public string extras;

        [NonSerialized]
        public List<Texture2D> screenshots;

        [NonSerialized]
        public List<string> attachments;

        [NonSerialized]
        private Dictionary<string, string> _tags = new Dictionary<string, string>();

        public Report()
        {
            attachments = new List<string>();
            screenshots = new List<Texture2D>();
            system = new System();
            app = new App();
            user = new User();
            sdk = new SDK();
        }

        public void AddTag(string key, string value)
        {
            _tags.Add(key, value);
        }

        public void DeleteTag(string key)
        {
            _tags.Remove(key);
        }


        public void SetTags(Dictionary<string, string> newTags)
        {
            _tags = newTags;
        }

        public void SetExtras(Dictionary<string, object> _extras)
        {
            extras = CrittrUtils.Json.Serialize(_extras);
        }

        public void SetLogs(List<string> _logs)
        {
            logs = _logs;
        }

        public void SetUserEmail(string email)
        {
            user.email = email;
        }

        public string ToJson()
        {
            tags = CrittrUtils.Json.Serialize(_tags);
            return JsonUtility.ToJson(this);
        }
    }
}
