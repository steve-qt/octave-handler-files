using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

public class tags{}
public class metadata{}
public class imu {public string temp;}
public class elems{public imu imu;}
public class lightelems { public string light; }
public class Accel { public double x; public double y; public double z; }
public class accelimu { public Accel accel; }
public class accelelems { public accelimu imu; }

public class body
{
    public string id;
    public string streamId;
    public string creatorId;
    public string lastEditorId;
    public metadata metadata;
    public string creationDate;
    public string lastEditDate;
    public string generatedDate;
    public string path;
    public string location;
    public string hash;
    public tags tags;
    public elems elems;
}

public class lightbody
{
    public string id;
    public string streamId;
    public string creatorId;
    public string lastEditorId;
    public metadata metadata;
    public string creationDate;
    public string lastEditDate;
    public string generatedDate;
    public string path;
    public string location;
    public string hash;
    public tags tags;
    public lightelems elems;
}

public class accelbody
{
    public string id;
    public string streamId;
    public string creatorId;
    public string lastEditorId;
    public metadata metadata;
    public string creationDate;
    public string lastEditDate;
    public string generatedDate;
    public string path;
    public string location;
    public string hash;
    public tags tags;
    public accelelems elems;
}



public class references{}

public class head
{
    public int status;
    public bool ok;
    public string[] messages;
    public string[] errors;
    public references references;
}

public class TempEvent
{
    public head head;
    public List<body> body;
}

public class LightEvent
{
    public head head;
    public List<lightbody> body;
}

public class AccelEvent 
{
    public head head;
    public List<accelbody> body;
}

public class Testing : MonoBehaviour
{
    const string BOARD1_NAME = "MangOHBoard1";
    const string BOARD1_TEMP_STREAM_ID = "event/s63c866d5db2aa0752bc4cfa3?limit=1";
    const string BOARD1_LGHT_STREAM_ID = "event/s63ffcc564426d57d795477c6?limit=1";
    const string BOARD1_ACCL_STREAM_ID = "event/s63ffcc3ef480dc55c03bd5dd?limit=2";

    const string BOARD2_NAME = "MangOHBoard2";
    const string BOARD2_TEMP_STREAM_ID = "event/s6414d12bd330ac73415c544a?limit=1";
    const string BOARD2_LGHT_STREAM_ID = "event/s6414d1cbd330ac73415c5b43?limit=1";
    const string BOARD2_ACCL_STREAM_ID = "event/s63c86818db2aa0752bc4d48b?limit=2";

    private FunctionTimer functionTimer;
    public TurbineSiteData turbineSiteData;
    public HttpClient client;

    public Testing() {
        client = new HttpClient();
        client.BaseAddress = new Uri("https://octave-api.sierrawireless.io/v5.0/hcl_technology_inc/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("X-Auth-Token", "5cecBXDf8ZAxgVNHP6TfH3x1HGTG9sNO");
        client.DefaultRequestHeaders.Add("X-Auth-User", "long_nguyen");
    }

    private void Start()
    {
        functionTimer = new FunctionTimer(TestingAction, 2f);
    }

    private void Update()
    {
        functionTimer.Update();
    }


    private void UpdateTemperature(string board_name,  double temperature) {
        string[] result = AssetDatabase.FindAssets(board_name);
        //Debug.Log("Asset Length: " + result.Length);

        if (result.Length == 1)
        {
            string path = AssetDatabase.GUIDToAssetPath(result[0]);
            WindTurbineScriptableObject obj = (WindTurbineScriptableObject)AssetDatabase.LoadAssetAtPath(path,
                typeof(WindTurbineScriptableObject));
            obj.UpdateTemp(temperature);
        }
    }

    private void UpdateLight(string board_name, double light)
    {
        string[] result = AssetDatabase.FindAssets(board_name);
        //Debug.Log("Asset Length: " + result.Length);

        if (result.Length == 1)
        {
            string path = AssetDatabase.GUIDToAssetPath(result[0]);
            WindTurbineScriptableObject obj = (WindTurbineScriptableObject)AssetDatabase.LoadAssetAtPath(path,
                typeof(WindTurbineScriptableObject));
            obj.UpdateLight(light);
        }
    }

    private bool CalculateVibration(accelbody event1, accelbody event2)
    {
        double in_range_pos = 0.03;
        double in_range_neg = -0.03;

        double currX = event1.elems.imu.accel.x;
        double currY = event1.elems.imu.accel.y;
        double currZ = event1.elems.imu.accel.z;

        double lastX = event2.elems.imu.accel.x;
        double lastY = event2.elems.imu.accel.y;
        double lastZ = event2.elems.imu.accel.z;

        if ((currX - lastX < in_range_neg || currX - lastX > in_range_pos) ||
        (currY - lastY < in_range_neg || currY - lastY > in_range_pos) ||
        (currZ - lastZ < in_range_neg || currZ - lastZ > in_range_pos))
        {
            return true;
        }
  
        return false;
    }

    private void UpdateVibration(string board_name, accelbody event1, accelbody event2)
    {
        bool isVibrated = CalculateVibration(event1,event2);

        string[] result = AssetDatabase.FindAssets(board_name);
        //Debug.Log("Asset Length: " + result.Length);

        if (result.Length == 1)
        {
            string path = AssetDatabase.GUIDToAssetPath(result[0]);
            WindTurbineScriptableObject obj = (WindTurbineScriptableObject)AssetDatabase.LoadAssetAtPath(path,
                typeof(WindTurbineScriptableObject));
            obj.UpdateVibration(isVibrated);
        }
    }

    private async void UpdateTemperatureAction(string board_name, string stream_id)
    {
        //Debug.Log("Updating Temperature of " + board_name);
        try
        {
            HttpResponseMessage response = await client.GetAsync(stream_id);
            if (response.IsSuccessStatusCode)
            {
                string res_txt = await response.Content.ReadAsStringAsync();
                //Debug.Log("Response body: " + res_txt);

                var tempEvent = JsonConvert.DeserializeObject<TempEvent>(res_txt);
                double temperature = Convert.ToDouble(tempEvent.body[0].elems.imu.temp);
                Debug.Log("Updating temp of " + board_name + " : " + temperature);
                UpdateTemperature(board_name,temperature);
            }
            else
            {
                Debug.Log("StatusCode: " + response.StatusCode);
            }
        }
        catch (HttpRequestException e)
        {
            Debug.Log("Error: " + e);
        }
    }

    private async void UpdateLightAction(string board_name, string stream_id)
    {
        //Debug.Log("Updating Light of " + board_name);
        try
        {
            HttpResponseMessage response = await client.GetAsync(stream_id);
            if (response.IsSuccessStatusCode)
            {
                string res_txt = await response.Content.ReadAsStringAsync();
                //Debug.Log("Updating light of " + boo+ res_txt);

                var tempEvent = JsonConvert.DeserializeObject<LightEvent>(res_txt);
                double light = Convert.ToDouble(tempEvent.body[0].elems.light);
                Debug.Log("Updating light of " + board_name + " : " + light);
                UpdateLight(board_name, light);
            }
            else
            {
                Debug.Log("StatusCode: " + response.StatusCode);
            }
        }
        catch (HttpRequestException e)
        {
            Debug.Log("Error: " + e);
        }
    }

    private async void UpdateAccelAction(string board_name, string stream_id)
    {
        Debug.Log("Updating Accel of " + board_name);
        try
        {
            HttpResponseMessage response = await client.GetAsync(stream_id);
            if (response.IsSuccessStatusCode)
            {
                string res_txt = await response.Content.ReadAsStringAsync();
                //Debug.Log("Updating light of " + boo+ res_txt);

                var accelEvents = JsonConvert.DeserializeObject<AccelEvent>(res_txt);
                double x = accelEvents.body[0].elems.imu.accel.x;
                double y = accelEvents.body[0].elems.imu.accel.y;
                double z = accelEvents.body[0].elems.imu.accel.z;

                Debug.Log("Updating acceleration of " + board_name 
                    + " [x : " + x + " y : " + y + " z : " + z + "]");
                
                UpdateVibration(board_name, accelEvents.body[0], accelEvents.body[1]);
            }
            else
            {
                Debug.Log("StatusCode: " + response.StatusCode);
            }
        }
        catch (HttpRequestException e)
        {
            Debug.Log("Error: " + e);
        }
    }

    private async void TestingAction()
    {
        UpdateTemperatureAction(BOARD1_NAME, BOARD1_TEMP_STREAM_ID);
        UpdateLightAction(BOARD1_NAME, BOARD1_LGHT_STREAM_ID);
        UpdateAccelAction(BOARD1_NAME, BOARD1_ACCL_STREAM_ID);

        UpdateTemperatureAction(BOARD2_NAME, BOARD2_TEMP_STREAM_ID);
        UpdateLightAction(BOARD2_NAME, BOARD2_LGHT_STREAM_ID);
        UpdateAccelAction(BOARD2_NAME, BOARD2_ACCL_STREAM_ID);


    }
}

